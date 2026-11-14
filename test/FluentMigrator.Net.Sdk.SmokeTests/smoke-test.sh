#!/usr/bin/env bash
#
# Smoke tests for FluentMigrator.Net.Sdk and FluentMigrator.Net.Sdk.Host.
#
# These SDKs are MSBuild payload — props, targets and RoslynCodeTaskFactory
# tasks — so there is no assembly to unit test. What there is to test is the
# build itself: does the classification pipeline produce the manifest we
# claim, does content land embedded vs copied, do the diagnostics fire, and
# does the whole thing stay incremental. That is what this script asserts.
#
# Requires a .NET 8+ SDK on PATH. Deliberately no python3/jq dependency: the
# manifests are byte-for-byte deterministic by design, which makes plain grep
# a legitimate assertion tool and keeps CI free of extra prerequisites.
#
# Usage:  ./test/FluentMigrator.Net.Sdk.SmokeTests/smoke-test.sh

set -euo pipefail
cd "$(dirname "$0")/../.."
REPO_ROOT=$(pwd)

SAMPLES=samples/FluentMigrator.Net.Sdk
ADVENTURE=$SAMPLES/AdventureLite
COMPOSITE=$SAMPLES/composite
FLYWAY=$SAMPLES/flyway-layout
TFM=netstandard2.0

FEED=$(mktemp -d)
SCRATCH=$(mktemp -d)
SHARED=$SAMPLES/shared-smoke
FAILURES=0
BACKUPS=()

# Files a section edits in place are backed up first and put back on exit,
# whether or not the section got as far as restoring them itself.
backup() { cp "$1" "$1.smokebak"; BACKUPS+=("$1"); }
restore() { if [ -f "$1.smokebak" ]; then mv "$1.smokebak" "$1"; fi; }

cleanup() {
  rm -rf "$FEED" "$SCRATCH" "$SHARED"
  rm -rf "$ADVENTURE/Schema/Views/legacy" "$ADVENTURE/Schema/Functions/sales.open_orders.sql"
  rm -f "$ADVENTURE/AdventureLite.sourcemodel.lock.json" "$ADVENTURE/Schema/Views/sales.tmp_lock.sql"
  if [ -f "$COMPOSITE/AdventureHost/AdventureHost.csproj.bak" ]; then
    mv "$COMPOSITE/AdventureHost/AdventureHost.csproj.bak" "$COMPOSITE/AdventureHost/AdventureHost.csproj"
  fi
  for f in ${BACKUPS[@]+"${BACKUPS[@]}"}; do restore "$f"; done
}
trap cleanup EXIT

# MSBuild needs a native path; under Git Bash on Windows that isn't /c/...
native_path() { if command -v cygpath > /dev/null; then cygpath -m "$1"; else printf '%s' "$1"; fi; }

# Item and property edits that arrive from outside the project file, the way a
# shared .props or .targets file would bring them. MSBuild imports this file
# after the project body; the manifest's up-to-date check sees neither the
# file nor the global property naming it, only what they change.
EXTRA_TARGETS=$SCRATCH/extra.targets
extra_targets() { printf '<Project>\n%s\n</Project>\n' "$1" > "$EXTRA_TARGETS"; }
build_extra() { build "$@" -p:CustomAfterMicrosoftCommonTargets="$(native_path "$EXTRA_TARGETS")"; }

checksum_of() { # manifest path-value
  object_block "$1" "$2" | sed -n 's/.*"checksum": "\(sha256:[0-9a-f]*\)".*/\1/p'
}

pass() { printf '  ok   %s\n' "$1"; }
fail() { printf '  FAIL %s\n' "$1"; FAILURES=$((FAILURES + 1)); }
section() { printf '\n== %s ==\n' "$1"; }

assert_contains() { # file pattern message
  if grep -qa -- "$2" "$1"; then pass "$3"; else fail "$3 (expected '$2' in $1)"; fi
}

assert_not_contains() { # file pattern message
  if grep -qa -- "$2" "$1"; then fail "$3 (unexpected '$2' in $1)"; else pass "$3"; fi
}

assert_file() { # path message
  if [ -f "$1" ]; then pass "$2"; else fail "$2 (missing $1)"; fi
}

assert_no_file() { # path message
  if [ -f "$1" ]; then fail "$2 (unexpected $1)"; else pass "$2"; fi
}

assert_count() { # file pattern expected message
  local actual
  actual=$(grep -ca -- "$2" "$1" || true)
  if [ "$actual" = "$3" ]; then pass "$4"; else fail "$4 (expected $3, got $actual)"; fi
}

# Extracts the one manifest object whose "path" is $2, relying on the
# manifest's fixed indentation — which the task guarantees, since a
# deterministic diffable document is the whole point of the format.
object_block() { # file path-value
  awk -v needle="\"path\": \"$2\"" '
    /^    \{/       { buf = ""; inblk = 1 }
    inblk           { buf = buf $0 "\n" }
    /^    \}/       { if (inblk && index(buf, needle)) printf "%s", buf; inblk = 0 }
  ' "$1"
}

assert_object() { # file path field-pattern message
  local blk
  blk=$(object_block "$1" "$2")
  if [ -z "$blk" ]; then
    fail "$4 (no object with path $2)"
  elif printf '%s' "$blk" | grep -q -- "$3"; then
    pass "$4"
  else
    fail "$4 (object $2 lacks '$3')"
  fi
}

build() { dotnet build "$@" -v:minimal; }

# ---------------------------------------------------------------- source model

section "FluentMigrator.Net.Sdk: source model manifest"
build "$ADVENTURE"
OUT=$ADVENTURE/bin/Debug/$TFM
MANIFEST=$OUT/AdventureLite.sourcemodel.json
cat "$MANIFEST"

assert_contains "$MANIFEST" '"dialect": "sqlserver"' "dialect stamped from the project body"
assert_contains "$MANIFEST" '"defaultSchema": "dbo"'  "DefaultSchema stamped"
assert_contains "$MANIFEST" '"convention": "fluentmigrator"' "default convention recorded"
assert_contains "$MANIFEST" '"role": "root"' "root module self-declaration recorded"
assert_count    "$MANIFEST" '"provider": "sql"' 14 "14 objects classified"
assert_count    "$MANIFEST" '"checksum": "sha256:' 14 "every object checksummed"

assert_object "$MANIFEST" "Schema/Views/sales.open_orders.sql"            '"type": "view"'      "view classified"
assert_object "$MANIFEST" "Schema/Views/sales.open_orders.sql"            '"schema": "sales"'   "SchemaDotObject splits the schema segment"
assert_object "$MANIFEST" "Schema/Functions/sales.fn_order_total.sql"     '"type": "function"'  "function classified"
assert_object "$MANIFEST" "Schema/StoredProcedures/sales.usp_ship_order.sql" '"type": "procedure"' "procedure classified"
assert_object "$MANIFEST" "Schema/Triggers/sales.trg_orders_audit.sql"    '"type": "trigger"'   "trigger classified"
assert_object "$MANIFEST" "Schema/Types/ZipCode.sql"                      '"schema": "dbo"'     "schema-less filename falls back to DefaultSchema"
assert_object "$MANIFEST" "Migrations/M20260101000000_CreateOrders.cs"    '"type": "migration"' "migration classified"
assert_object "$MANIFEST" "Maintenance/RebuildIndexes.cs"                 '"type": "maintenance-migration"' "maintenance migration classified"
assert_object "$MANIFEST" "Seed/DemoDataSeed.cs"                          '"type": "seed"'      "seed source classified"
assert_object "$MANIFEST" "Seed/seed_customers.csv"                       '"type": "seed-data"' "seed data classified"
assert_object "$MANIFEST" "Migrations/Data/order_statuses.csv"            '"type": "migration-data"' "migration data classified"
assert_object "$MANIFEST" "Tools/CreateDatabase.sql"                      '"role": "create-database"' "CreateDatabase role recorded"
assert_object "$MANIFEST" "Tools/DatabaseSchema.sql"                      '"role": "baseline"'  "baseline role recorded"

section "execution facets (normalized across conventions)"
assert_object "$MANIFEST" "Schema/Views/sales.open_orders.sql"         '"execution": "onChange"' "Schema/* is onChange"
assert_object "$MANIFEST" "Migrations/M20260101000000_CreateOrders.cs" '"execution": "once"'     "migrations are once"
assert_object "$MANIFEST" "Maintenance/RebuildIndexes.cs"              '"execution": "always"'   "maintenance is always"
assert_object "$MANIFEST" "Maintenance/RebuildIndexes.cs"              '"stage": "maintenance"'  "maintenance carries its stage"
assert_object "$MANIFEST" "Tools/CreateDatabase.sql"                   '"stage": "provision"'    "tools carry the provision stage"

# ------------------------------------------------------------ content mapping

section "embed vs copy"
assert_object   "$MANIFEST" "Migrations/Scripts/V001_backfill_status.sql" '"contentType": "embedded"' "default script is embedded"
assert_object   "$MANIFEST" "Migrations/Scripts/V002_rebuild_stats.sql"   '"contentType": "external"' "body-level Update flips a script to external"
assert_file     "$OUT/Migrations/Scripts/V002_rebuild_stats.sql" "external script copied beside the assembly"
assert_no_file  "$OUT/Migrations/Scripts/V001_backfill_status.sql" "embedded script not copied to output"
assert_contains "$OUT/AdventureLite.dll" "V001_backfill_status" "embedded script is in the assembly"
assert_contains "$OUT/AdventureLite.dll" "order_statuses"       "embedded migration data is in the assembly"
assert_contains "$OUT/AdventureLite.dll" "seed_customers"       "embedded seed data is in the assembly"
assert_not_contains "$OUT/AdventureLite.dll" "V002_rebuild_stats" "external script is not embedded"

# --------------------------------------------------------------- diagnostics

section "FMSDK001 duplicate object identity"
# Same file name under a different type is a different logical object.
cp "$ADVENTURE/Schema/Views/sales.open_orders.sql" "$ADVENTURE/Schema/Functions/sales.open_orders.sql"
if build "$ADVENTURE" > /dev/null 2>&1; then
  pass "same name across types does not collide"
else
  fail "same name across types should not collide"
fi
rm -f "$ADVENTURE/Schema/Functions/sales.open_orders.sql"

# Same type, same schema, same name in two files is a genuine duplicate.
mkdir -p "$ADVENTURE/Schema/Views/legacy"
cp "$ADVENTURE/Schema/Views/sales.open_orders.sql" "$ADVENTURE/Schema/Views/legacy/sales.open_orders.sql"
DUP_OUT=$(build "$ADVENTURE" 2>&1 || true)
if printf '%s' "$DUP_OUT" | grep -q "FMSDK001"; then
  pass "FMSDK001 raised for a duplicate view identity"
else
  fail "FMSDK001 expected for a duplicate view identity"
fi
rm -rf "$ADVENTURE/Schema/Views/legacy"
build "$ADVENTURE" > /dev/null

# Regression: a timestamp-only up-to-date check cannot see a deleted input,
# which would leave the manifest describing an object that no longer exists.
assert_count "$MANIFEST" '"provider": "sql"' 14 "manifest drops an object again once its file is deleted"

# -------------------------------------------------------------- incremental

section "incrementality"
build "$ADVENTURE" > /dev/null
INC_OUT=$(dotnet build "$ADVENTURE" -v:normal 2>&1)
if printf '%s' "$INC_OUT" | grep -qiE "BuildSourceModelManifest.*(skipping|up-to-date)"; then
  pass "manifest target skipped on an unchanged rebuild"
else
  fail "manifest target should be skipped on an unchanged rebuild"
fi

section "the manifest follows properties and metadata, not only files"
# Regression: the up-to-date check only saw the file set and the project
# file, so a -p: property, or a metadata edit from an imported file, left the
# previous manifest in place.
build "$ADVENTURE" -p:DatabaseDialect=postgres > /dev/null
assert_contains "$MANIFEST" '"dialect": "postgres"' "a -p: property rebuilds the manifest"
build "$ADVENTURE" > /dev/null
assert_contains "$MANIFEST" '"dialect": "sqlserver"' "dropping the -p: property rebuilds it again"

extra_targets '<ItemGroup><SqlObject Update="Schema/Views/sales.open_orders.sql" Execution="always" /></ItemGroup>'
build_extra "$ADVENTURE" > /dev/null
assert_object "$MANIFEST" "Schema/Views/sales.open_orders.sql" '"execution": "always"' "metadata set in an imported file rebuilds the manifest"
# Same global properties, same project file: only the imported file changed.
extra_targets '<ItemGroup><SqlObject Update="Schema/Views/sales.open_orders.sql" Execution="once" /></ItemGroup>'
build_extra "$ADVENTURE" > /dev/null
assert_object "$MANIFEST" "Schema/Views/sales.open_orders.sql" '"execution": "once"' "editing only that file rebuilds it again"
build "$ADVENTURE" > /dev/null
assert_object "$MANIFEST" "Schema/Views/sales.open_orders.sql" '"execution": "onChange"' "and back without it"

INC_OUT=$(dotnet build "$ADVENTURE" -v:normal 2>&1)
if printf '%s' "$INC_OUT" | grep -qiE "BuildSourceModelManifest.*(skipping|up-to-date)"; then
  pass "still skipped on the next unchanged rebuild"
else
  fail "the manifest target should be skipped again on an unchanged rebuild"
fi

section "checksums don't depend on line endings or a BOM"
# Regression: the checksum was over raw bytes, so a Windows checkout with
# core.autocrlf and Linux CI disagreed about an unchanged file, and with them
# the onChange facet.
VIEW_PATH=Schema/Views/sales.open_orders.sql
VIEW=$ADVENTURE/$VIEW_PATH
backup "$VIEW"
tr -d '\r' < "$VIEW.smokebak" > "$VIEW"
build "$ADVENTURE" > /dev/null
LF_SUM=$(checksum_of "$MANIFEST" "$VIEW_PATH")
EXPECTED_SUM="sha256:$(sha256sum "$VIEW" | cut -d' ' -f1)"
if [ "$LF_SUM" = "$EXPECTED_SUM" ]; then pass "checksum is SHA-256 of the LF text"; else fail "checksum $LF_SUM, expected $EXPECTED_SUM"; fi

sed 's/$/\r/' "$VIEW" > "$SCRATCH/crlf.sql" && cp "$SCRATCH/crlf.sql" "$VIEW"
build "$ADVENTURE" > /dev/null
CRLF_SUM=$(checksum_of "$MANIFEST" "$VIEW_PATH")
if [ "$CRLF_SUM" = "$LF_SUM" ]; then pass "CRLF gives the same checksum"; else fail "CRLF changed the checksum: $LF_SUM -> $CRLF_SUM"; fi

{ printf '\357\273\277'; tr -d '\r' < "$VIEW.smokebak"; } > "$VIEW"
build "$ADVENTURE" > /dev/null
BOM_SUM=$(checksum_of "$MANIFEST" "$VIEW_PATH")
if [ "$BOM_SUM" = "$LF_SUM" ]; then pass "a UTF-8 BOM gives the same checksum"; else fail "a BOM changed the checksum: $LF_SUM -> $BOM_SUM"; fi

printf 'SELECT 1;\n' >> "$VIEW"
build "$ADVENTURE" > /dev/null
EDIT_SUM=$(checksum_of "$MANIFEST" "$VIEW_PATH")
if [ "$EDIT_SUM" != "$LF_SUM" ]; then pass "a content change still changes it"; else fail "a content change kept the checksum"; fi
restore "$VIEW"
build "$ADVENTURE" > /dev/null

section "FMSDK103 paths outside SourceModelRoot"
# Regression: a file outside the project was recorded by its absolute path,
# so the committed manifest differed per machine and leaked local paths.
mkdir -p "$SHARED"
printf 'CREATE VIEW sales.v_shared AS SELECT 1 AS one;\n' > "$SHARED/sales.v_shared.sql"
extra_targets '<ItemGroup><SqlObject Include="../shared-smoke/sales.v_shared.sql" ObjectType="view" NamePolicy="SchemaDotObject" UniqueIdentity="true" Execution="onChange" /></ItemGroup>'
OUTSIDE_OUT=$(build_extra "$ADVENTURE" 2>&1 || true)
assert_object "$MANIFEST" "../shared-smoke/sales.v_shared.sql" '"name": "v_shared"' "outside file recorded relative to the root"
if grep -qaE '"path": "(/|[A-Za-z]:)' "$MANIFEST"; then fail "an absolute path reached the manifest"; else pass "no absolute path in the manifest"; fi
if printf '%s' "$OUTSIDE_OUT" | grep -q "FMSDK103"; then pass "FMSDK103 raised for a file outside the root"; else fail "FMSDK103 expected for a file outside the root"; fi

extra_targets '<ItemGroup><SqlObject Include="../shared-smoke/sales.v_shared.sql" Link="Schema/Views/sales.v_shared.sql" ObjectType="view" NamePolicy="SchemaDotObject" UniqueIdentity="true" Execution="onChange" /></ItemGroup>'
LINK_OUT=$(build_extra "$ADVENTURE" 2>&1 || true)
assert_object "$MANIFEST" "Schema/Views/sales.v_shared.sql" '"name": "v_shared"' "Link metadata gives the in-project path"
if printf '%s' "$LINK_OUT" | grep -q "FMSDK103"; then fail "FMSDK103 should not fire for a linked item"; else pass "no FMSDK103 for a linked item"; fi
rm -rf "$SHARED"
build "$ADVENTURE" > /dev/null
assert_count "$MANIFEST" '"provider": "sql"' 14 "back to 14 objects without the shared file"

section "FMSDK104 execution facet spelling"
# Regression: Execution was copied verbatim, so "OnChange" or a typo produced
# a manifest that no longer compared with others.
extra_targets '<ItemGroup>
  <SqlObject Update="Schema/Views/sales.open_orders.sql" Execution="OnChange" />
  <SqlObject Update="Schema/Functions/sales.fn_order_total.sql" Execution="sometimes" />
</ItemGroup>'
EXEC_OUT=$(build_extra "$ADVENTURE" 2>&1 || true)
assert_object "$MANIFEST" "Schema/Views/sales.open_orders.sql" '"execution": "onChange"' "a known value in another case becomes the canonical spelling"
if printf '%s' "$EXEC_OUT" | grep "FMSDK104" | grep -q "fn_order_total"; then pass "FMSDK104 raised for an unknown value"; else fail "FMSDK104 expected for Execution=sometimes"; fi
if printf '%s' "$EXEC_OUT" | grep "FMSDK104" | grep -q "open_orders"; then fail "FMSDK104 should not fire for OnChange"; else pass "no FMSDK104 for a known value in another case"; fi
build "$ADVENTURE" > /dev/null

section "content changes behind a newer manifest"
# Regression: the up-to-date check compared timestamps, so a manifest newer
# than a changed input (a checkout, a stash pop, a restored CI cache) kept the
# old checksum. The stamp now carries each input's content hash.
INTERMEDIATE=$ADVENTURE/obj/Debug/$TFM/AdventureLite.sourcemodel.json
backup "$VIEW"
BEFORE_SUM=$(checksum_of "$MANIFEST" "$VIEW_PATH")
printf -- '-- edited\n' >> "$VIEW"
sleep 1
touch "$INTERMEDIATE"
build "$ADVENTURE" > /dev/null
AFTER_SUM=$(checksum_of "$MANIFEST" "$VIEW_PATH")
if [ "$AFTER_SUM" != "$BEFORE_SUM" ]; then pass "an edit is seen even when the manifest is newer"; else fail "manifest kept $BEFORE_SUM after an edit behind a newer manifest"; fi
restore "$VIEW"
build "$ADVENTURE" > /dev/null

section "clean and generation off remove the manifest"
# Regression: IncrementalClean runs inside CoreBuild, before the manifest
# targets, so their FileWrites were never recorded and nothing removed them.
STAMP=$ADVENTURE/obj/Debug/$TFM/sourcemodel.inputs.txt
build "$ADVENTURE" > /dev/null
assert_file "$MANIFEST" "an unchanged rebuild keeps the manifest"
assert_file "$INTERMEDIATE" "and its intermediate copy"
dotnet clean "$ADVENTURE" -v:minimal > /dev/null
assert_no_file "$MANIFEST" "dotnet clean removes the manifest"
assert_no_file "$INTERMEDIATE" "and its intermediate copy"
assert_no_file "$STAMP" "and the inputs stamp"
build "$ADVENTURE" > /dev/null
build "$ADVENTURE" -p:GenerateSourceModelManifest=false > /dev/null
assert_no_file "$MANIFEST" "turning generation off removes the old manifest"
build "$ADVENTURE" > /dev/null
assert_file "$MANIFEST" "and turning it back on regenerates it"

section "FMSDK003 a path in SourceModelManifestFileName"
# The manifest is a build output. A path would put it in the source tree,
# where an up-to-date check can't be trusted; the lock file is for that.
PATH_OUT=$(build "$ADVENTURE" -p:SourceModelManifestFileName=../../../AdventureLite.sourcemodel.json 2>&1 || true)
if printf '%s' "$PATH_OUT" | grep -q "FMSDK003"; then pass "FMSDK003 raised for a path"; else fail "FMSDK003 expected for a path"; fi
assert_no_file "$ADVENTURE/AdventureLite.sourcemodel.json" "nothing written into the source tree"

section "lock file"
LOCK=$ADVENTURE/AdventureLite.sourcemodel.lock.json
LOCK_ON=-p:GenerateSourceModelLockFile=true
LOCKED=-p:SourceModelLockedMode=true
same_as_manifest() { # message
  if [ "$(tr -d '\r' < "$LOCK")" = "$(tr -d '\r' < "$MANIFEST")" ]; then pass "$1"; else fail "$1 (lock and manifest differ)"; fi
}
tamper_lock() { sed -i '0,/"execution": "onChange"/s//"execution": "once"/' "$LOCK"; touch "$LOCK"; }

build "$ADVENTURE" "$LOCK_ON" > /dev/null
assert_file "$LOCK" "the lock is created next to the project"
same_as_manifest "with the manifest's content"

# A hand edit (or a mis-resolved merge) makes the lock the newest file in the
# tree. An up-to-date check would skip; the lock is compared every build.
tamper_lock
build "$ADVENTURE" "$LOCK_ON" > /dev/null
same_as_manifest "a hand-edited lock is corrected on the next build"

tamper_lock
LOCKED_OUT=$(build "$ADVENTURE" "$LOCK_ON" "$LOCKED" 2>&1 || true)
if printf '%s' "$LOCKED_OUT" | grep "FMSDK004" | grep -q "changed"; then
  pass "locked mode fails with FMSDK004 and says what changed"
else
  fail "FMSDK004 expected for a changed lock in locked mode"
fi
if [ "$(tr -d '\r' < "$LOCK")" != "$(tr -d '\r' < "$MANIFEST")" ]; then pass "locked mode doesn't write the lock"; else fail "locked mode wrote the lock"; fi

dotnet build "$ADVENTURE" -v:minimal -t:RewriteSourceModelLock "$LOCK_ON" "$LOCKED" > /dev/null
same_as_manifest "-t:RewriteSourceModelLock writes it, even in locked mode"

# A checkout with core.autocrlf gives the committed lock CRLF line endings.
sed -i 's/$/\r/' "$LOCK"
if build "$ADVENTURE" "$LOCK_ON" "$LOCKED" > /dev/null 2>&1; then pass "a CRLF lock passes locked mode"; else fail "a CRLF lock should pass locked mode"; fi
build "$ADVENTURE" "$LOCK_ON" > /dev/null
# Counted with tr: grep under Git for Windows ignores carriage returns.
if [ "$(tr -cd '\r' < "$LOCK" | wc -c)" -gt 0 ]; then pass "and isn't rewritten, so git sees no change"; else fail "a CRLF lock was rewritten"; fi

rm -f "$LOCK"
MISSING_OUT=$(build "$ADVENTURE" "$LOCK_ON" "$LOCKED" 2>&1 || true)
if printf '%s' "$MISSING_OUT" | grep -q "FMSDK004"; then pass "locked mode fails when the lock is missing"; else fail "FMSDK004 expected for a missing lock"; fi
assert_no_file "$LOCK" "and doesn't create it"

# Each configuration has its own obj/ stamp, but there is one lock. Comparing
# every build keeps it right whichever configuration built last.
build "$ADVENTURE" "$LOCK_ON" > /dev/null
build "$ADVENTURE" "$LOCK_ON" -c Release > /dev/null
cp "$VIEW" "$ADVENTURE/Schema/Views/sales.tmp_lock.sql"
build "$ADVENTURE" "$LOCK_ON" > /dev/null
assert_contains "$LOCK" "sales.tmp_lock.sql" "a Debug build adds a new view to the lock"
rm -f "$ADVENTURE/Schema/Views/sales.tmp_lock.sql"
build "$ADVENTURE" "$LOCK_ON" -c Release > /dev/null
assert_not_contains "$LOCK" "sales.tmp_lock.sql" "a Release build removes it again"

# Several target frameworks: one writer, and only if they agree. Set in the
# project file, as a real project would: a -p: value can't carry the ';'.
ADVENTURE_PROJ=$ADVENTURE/AdventureLite.csproj
backup "$ADVENTURE_PROJ"
sed -i 's|<IncludeFluentMigratorReference>false</IncludeFluentMigratorReference>|&<TargetFrameworks>netstandard2.0;net11.0</TargetFrameworks>|' "$ADVENTURE_PROJ"
rm -f "$LOCK"
build "$ADVENTURE" "$LOCK_ON" > /dev/null
same_as_manifest "with two target frameworks the outer build writes one lock"
extra_targets '<ItemGroup Condition="'"'"'$(TargetFramework)'"'"' == '"'"'net11.0'"'"'"><SqlObject Remove="Schema/Views/sales.open_orders.sql" /></ItemGroup>'
TFM_OUT=$(build_extra "$ADVENTURE" "$LOCK_ON" 2>&1 || true)
if printf '%s' "$TFM_OUT" | grep -q "FMSDK005"; then pass "FMSDK005 when the frameworks' manifests differ"; else fail "FMSDK005 expected when the frameworks' manifests differ"; fi
restore "$ADVENTURE_PROJ"

rm -f "$LOCK"
build "$ADVENTURE" > /dev/null

# -------------------------------------------------------------- host manifest

section "FluentMigrator.Net.Sdk.Host: host manifest"
build "$COMPOSITE/AdventureHost"
HOST=$COMPOSITE/AdventureHost/bin/Debug/$TFM/AdventureHost.host.json
cat "$HOST"

# Targets in document order, modules root-first, per-module VersionInfo.
TARGET_ORDER=$(grep -a '"name": "primary"\|"name": "reporting"' "$HOST" | tr -d ' ,' | tr '\n' ' ')
if [ "$TARGET_ORDER" = '"name":"primary" "name":"reporting" ' ]; then
  pass "DeploymentTarget document order preserved"
else
  fail "DeploymentTarget document order violated: $TARGET_ORDER"
fi

MODULE_ORDER=$(grep -a '"name": "Core"\|"name": "Sales"' "$HOST" | tr -d ' ,' | tr '\n' ' ')
if [ "$MODULE_ORDER" = '"name":"Core" "name":"Sales" "name":"Core" "name":"Sales" ' ]; then
  pass "modules ordered root-first in both discovery and explicit modes"
else
  fail "module order wrong: $MODULE_ORDER"
fi

assert_count    "$HOST" '"role": "root"' 2 "root module marked in each target"
assert_contains "$HOST" '"versionTable": {"schema": "core", "name": "VersionInfo"}'  "Core VersionInfo resolves against its DefaultSchema"
assert_contains "$HOST" '"versionTable": {"schema": "sales", "name": "VersionInfo"}' "Sales VersionInfo resolves against its DefaultSchema"
assert_contains "$HOST" '"sourceModel": "Core.sourcemodel.json"' "module source model manifest referenced"
assert_contains "$HOST" '"assembly": "Core.dll"' "module assembly referenced"

section "hosting contexts and runners (the two axes)"
assert_count    "$HOST" '"name": "dotnet-cli"' 1 "dotnet-cli context recorded"
assert_contains "$HOST" '"referenceKind": "tool"' "a CLI context is a tool, not a reference"
assert_contains "$HOST" '"name": "msbuild"' "msbuild context recorded"
assert_contains "$HOST" '"name": "aspire"'  "planned context is legal to select"
assert_contains "$HOST" '"availability": "planned"' "planned availability recorded rather than failing the build"
assert_contains "$HOST" '"availability": "shipping"' "shipping availability recorded"
assert_contains "$HOST" '"discovery": true' "Discover()-driven contexts flagged"
assert_contains "$HOST" '"runner": "FluentMigrator.Runner.SqlServer"' "sqlserver target resolves its runner"
assert_contains "$HOST" '"runner": "FluentMigrator.Runner.Postgres"'  "postgres target resolves a different runner"

section "FMSDK201/203 on a declared order that contradicts the graph"
cp "$COMPOSITE/AdventureHost/AdventureHost.csproj" "$COMPOSITE/AdventureHost/AdventureHost.csproj.bak"
sed -i 's|<Modules>Core;Sales</Modules>|<Modules>Sales;Core</Modules>|' "$COMPOSITE/AdventureHost/AdventureHost.csproj"
BAD_OUT=$(build "$COMPOSITE/AdventureHost" 2>&1 || true)
mv "$COMPOSITE/AdventureHost/AdventureHost.csproj.bak" "$COMPOSITE/AdventureHost/AdventureHost.csproj"
if printf '%s' "$BAD_OUT" | grep -q "FMSDK203"; then
  pass "FMSDK203 raised when the root module is not deployed first"
else
  fail "FMSDK203 expected when the root module is not deployed first"
fi
if printf '%s' "$BAD_OUT" | grep -q "FMSDK201"; then
  pass "FMSDK201 raised when declared order contradicts the reference graph"
else
  fail "FMSDK201 expected when declared order contradicts the reference graph"
fi

section "FMSDK207 unknown host context"
UNKNOWN_OUT=$(dotnet build "$COMPOSITE/AdventureHost" -v:minimal -p:HostContexts=carrier-pigeon 2>&1 || true)
if printf '%s' "$UNKNOWN_OUT" | grep -q "FMSDK207"; then
  pass "FMSDK207 raised for an unknown host context"
else
  fail "FMSDK207 expected for an unknown host context"
fi

HOSTPROJ=$COMPOSITE/AdventureHost/AdventureHost.csproj

# The host manifest's targets use the same fixed indentation as source model
# objects, so a target's block can be cut out the same way, by its name.
target_block() { # file target-name
  awk -v needle="\"name\": \"$2\"" '
    /^    \{/       { buf = ""; inblk = 1 }
    inblk           { buf = buf $0 "\n" }
    /^    \}/       { if (inblk && index(buf, needle)) printf "%s", buf; inblk = 0 }
  ' "$1"
}

assert_target() { # file target-name pattern message
  if target_block "$1" "$2" | grep -q -- "$3"; then pass "$4"; else fail "$4 (target $2 lacks '$3')"; fi
}

section "FMSDK209 a module listed twice"
# Regression: Modules="Core;Sales;Sales" deployed Sales twice without a word.
backup "$HOSTPROJ"
sed -i 's|<Modules>Core;Sales</Modules>|<Modules>Core;Sales;Sales</Modules>|' "$HOSTPROJ"
DUPMOD_OUT=$(build "$COMPOSITE/AdventureHost" 2>&1 || true)
restore "$HOSTPROJ"
if printf '%s' "$DUPMOD_OUT" | grep -q "FMSDK209"; then pass "FMSDK209 raised for a module listed twice"; else fail "FMSDK209 expected for a module listed twice"; fi

section "effective dialect"
# Regression: a target without a Dialect inherited its modules' and got that
# dialect's runner, but the manifest recorded the runner without the dialect.
backup "$HOSTPROJ"
sed -i '/<Dialect>sqlserver<\/Dialect>/d' "$HOSTPROJ"
build "$COMPOSITE/AdventureHost" > /dev/null
assert_target "$HOST" primary '"dialect": "sqlserver"' "an inherited dialect is recorded"
assert_target "$HOST" primary '"runner": "FluentMigrator.Runner.SqlServer"' "with the runner it chose"
assert_target "$HOST" reporting '"dialect": "postgres"' "an explicit dialect still wins for its target"

# Sales now says postgres while Core says sqlserver; primary inherits Core's.
cat > "$EXTRA_TARGETS" <<'XML'
<Project>
  <PropertyGroup Condition="'$(MSBuildProjectName)' == 'Sales'">
    <DatabaseDialect>postgres</DatabaseDialect>
  </PropertyGroup>
</Project>
XML
MIXED_OUT=$(build_extra "$COMPOSITE/AdventureHost" 2>&1 || true)
if printf '%s' "$MIXED_OUT" | grep "FMSDK210" | grep -q "primary"; then
  pass "FMSDK210 raised when the modules of a dialect-less target disagree"
else
  fail "FMSDK210 expected when the modules of a dialect-less target disagree"
fi
if printf '%s' "$MIXED_OUT" | grep "FMSDK210" | grep -q "reporting"; then
  fail "FMSDK210 should not fire for a target with its own dialect"
else
  pass "no FMSDK210 for a target with its own dialect"
fi
restore "$HOSTPROJ"
build "$COMPOSITE/AdventureHost" > /dev/null

section "custom host manifest escape hatch"
HOST_INTERMEDIATE=$COMPOSITE/AdventureHost/obj/Debug/$TFM/AdventureHost.host.json
build "$COMPOSITE/AdventureHost" -p:HostManifestFile=host.custom.json
assert_contains "$HOST" '"name": "Core", "role": "root"' "hand-authored manifest shipped verbatim"
assert_no_file "$HOST_INTERMEDIATE" "the generated intermediate copy is cleaned up"
build "$COMPOSITE/AdventureHost" > /dev/null
assert_not_contains "$HOST" '"name": "Core", "role": "root"' "switching back ships the generated manifest again"
assert_file "$HOST_INTERMEDIATE" "with its intermediate copy"
dotnet clean "$COMPOSITE/AdventureHost" -v:minimal > /dev/null
assert_no_file "$HOST" "dotnet clean removes the host manifest"
build "$COMPOSITE/AdventureHost" > /dev/null

# ------------------------------------------------------------------- flyway

section "flyway convention pack"
build "$FLYWAY"
FLY=$FLYWAY/bin/Debug/$TFM/FlywayStyle.sourcemodel.json
cat "$FLY"
assert_contains "$FLY" '"convention": "flyway"' "convention recorded"
assert_count    "$FLY" '"provider": "sql"' 5 "5 flyway objects classified"
assert_object   "$FLY" "sql/V1__Initial_Schema.sql"      '"version": "1"'          "FlywayPrefixed parses the version"
assert_object   "$FLY" "sql/V1__Initial_Schema.sql"      '"execution": "once"'     "V migrations are once"
assert_object   "$FLY" "sql/U1__Undo_Initial_Schema.sql" '"type": "undo-script"'   "U migrations classified"
assert_object   "$FLY" "sql/R__01_Create_User_View.sql"  '"execution": "onChange"' "R repeatables are onChange, like Schema/*"
assert_object   "$FLY" "sql/callback/beforeMigrate.sql"  '"stage": "callback"'     "callbacks carry their stage"
# The fluentmigrator pack is imported too; selection must filter it back out.
assert_not_contains "$FLY" '"type": "migration"' "non-selected convention's pivots excluded"

# --------------------------------------------------------------------- pack

section "pack"
# Both SDKs ship on the runtime libraries' release train, so neither carries a
# <Version> of its own. Packing with an explicit -p:Version, the way CI does,
# is therefore also the regression test for that: a reintroduced hardcoded
# version would win over this and the assertions below would name the wrong file.
PACK_VERSION=9.9.9-smoketest
dotnet pack src/FluentMigrator.Net.Sdk      -c Release -o "$FEED" -v:minimal -p:Version=$PACK_VERSION
dotnet pack src/FluentMigrator.Net.Sdk.Host -c Release -o "$FEED" -v:minimal -p:Version=$PACK_VERSION
ls -la "$FEED"
assert_file "$FEED/FluentMigrator.Net.Sdk.$PACK_VERSION.nupkg" \
  "FluentMigrator.Net.Sdk packs on the version the build was given"
assert_file "$FEED/FluentMigrator.Net.Sdk.Host.$PACK_VERSION.nupkg" \
  "FluentMigrator.Net.Sdk.Host packs on the version the build was given"

# ------------------------------------------------------------------ summary

printf '\n'
if [ "$FAILURES" -eq 0 ]; then
  echo "ALL SMOKE TESTS PASSED"
else
  echo "$FAILURES SMOKE TEST(S) FAILED"
  exit 1
fi
