-- Processor: Hana
-- Generator: HanaGenerator

CREATE COLUMN TABLE "VersionInfo" ("Version" BIGINT NOT NULL);
CREATE UNIQUE INDEX "UC_Version" ON "VersionInfo" ("Version" ASC);

ALTER TABLE "VersionInfo" ADD ("AppliedOn" DATETIME NULL);

ALTER TABLE "VersionInfo" ADD ("Description" NVARCHAR(1024) NULL);
CREATE COLUMN TABLE "Users" ("Id" INTEGER NOT NULL, "Name" NVARCHAR(100) NOT NULL, PRIMARY KEY ("Id"));
INSERT INTO "VersionInfo" ("Version", "AppliedOn", "Description") VALUES (1, CURRENT_UTCTIMESTAMP, N'CreateTableMigration');
