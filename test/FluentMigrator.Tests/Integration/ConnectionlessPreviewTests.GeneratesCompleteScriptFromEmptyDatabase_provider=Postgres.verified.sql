-- Processor: PostgreSQL15_0
-- Generator: Postgres15_0Generator

CREATE TABLE "public"."VersionInfo" ("Version" bigint NOT NULL);
CREATE UNIQUE INDEX "UC_Version" ON "public"."VersionInfo" ("Version" ASC);

ALTER TABLE "public"."VersionInfo" ADD "AppliedOn" timestamp;

ALTER TABLE "public"."VersionInfo" ADD "Description" varchar(1024);
CREATE TABLE "public"."Users" ("Id" integer NOT NULL, "Name" varchar(100) NOT NULL, CONSTRAINT "PK_Users" PRIMARY KEY ("Id"));
INSERT INTO "public"."VersionInfo" ("Version","AppliedOn","Description") VALUES (1,(now() at time zone 'UTC'),'CreateTableMigration');
