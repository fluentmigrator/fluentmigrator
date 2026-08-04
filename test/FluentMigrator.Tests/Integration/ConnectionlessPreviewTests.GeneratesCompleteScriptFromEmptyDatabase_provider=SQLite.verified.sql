-- Processor: SQLite
-- Generator: SQLiteGenerator

CREATE TABLE "VersionInfo" ("Version" INTEGER NOT NULL);
CREATE UNIQUE INDEX "UC_Version" ON "VersionInfo" ("Version" ASC);

ALTER TABLE "VersionInfo" ADD COLUMN "AppliedOn" DATETIME;

ALTER TABLE "VersionInfo" ADD COLUMN "Description" TEXT;
CREATE TABLE "Users" ("Id" INTEGER NOT NULL, "Name" TEXT NOT NULL, CONSTRAINT "PK_Users" PRIMARY KEY ("Id"));
INSERT INTO "VersionInfo" ("Version", "AppliedOn", "Description") VALUES (1, CURRENT_TIMESTAMP, 'CreateTableMigration');
