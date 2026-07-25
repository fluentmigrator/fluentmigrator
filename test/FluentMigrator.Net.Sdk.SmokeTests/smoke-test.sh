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
FAILURES=0

cleanup() {
  rm -rf "$FEED"
  rm -rf "$ADVENTURE/Schema/Views/legacy" "$ADVENTURE/Schema/Functions/sales.open_orders.sql"
  if [ -f "$COMPOSITE/AdventureHost/AdventureHost.csproj.bak" ]; then
    mv "$COMPOSITE/AdventureHost/AdventureHost.csproj.bak" "$COMPOSITE/AdventureHost/AdventureHost.csproj"
  fi
}
trap cleanup EXIT

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

section "custom host manifest escape hatch"
build "$COMPOSITE/AdventureHost" -p:HostManifestFile=host.custom.json
assert_contains "$HOST" '"name": "Core", "role": "root"' "hand-authored manifest shipped verbatim"
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
