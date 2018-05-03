# Current state

## Application of migrations

- Find all migrations
- Remove all migrations when
  - already applied
  - migration version is greater than target version
- Apply migrations
  - Apply maintenance migrations
- Apply profiles
  - Calls `Runner.ApplyMigrationUp` for every migration (wrapped in an `IMigrationInfo`)
  - Apply migration if either:
    - Not attributed (which is true for all profile migrations)
    - Not applied
  - Update version info only if migration info is applied (always false for profile migrations)

## Tags

- `TagsAttribute` on migrations are always evaluated
  - `Behavior` property controls evaluation
    - `RequireAll` is a boolean AND
    - `RequireAny` is a boolean OR
    - `RequireAll` is evaluated first
    - When `RequireAll` attributes exist and all `RequireAll` flagged `TagsAttribute`s are in the specified list of tags, then the migration gets used
    - When `RequireAny` attribute exists and at least one `RequireAny` flagged `TagsAttribute` is in the specified list of tags, then the migration gets used
    - A match for `RequireAll` skips the check for ´RequireAny`

## Migration Traits

- Specified by `MigrationTraitAttribute`
- User-defined Key/Value pairs
- Not used yet

## Migrations

- Loading when ...
  - ... no tags are specified on the migration
  - ... the tags are matching
- Executing
  - Using the non-attributed migrations from the `MaintenanceLoader`

## MaintenanceLoader

- Requires tags?
  - Yes: Only use migrations matching the given tags
  - No: Only use migrations without tags
- Group by stages
  - `BeforeAll`
  - `BeforeEach`
  - `AfterEach`
  - `BeforeProfiles`
  - `AfterAll`
- Returns non-attributed migrations for a given stage

## Structure of the `VersionInfo` table

- Migrations:
  - `VersionSchemaMigration`
    - Create schema
  - `VersionMigration`
    - Create table
    - Column `Version` int64 not null
  - `VersionUniqueMigration`
    - Create unique clustered index on column `Version`
    - Add column `AppliedOn` datetime nullable
  - `VersionDescriptionMigration`
    - Add column `Description` varchar(1000) nullable

# Requirements

TBD
