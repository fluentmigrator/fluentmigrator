-- Processor: Redshift
-- Generator: RedshiftGenerator

CREATE TABLE "public"."VersionInfo" ("Version" bigint NOT NULL);

;
ALTER TABLE "public"."VersionInfo" ADD "AppliedOn" timestamp;
;
ALTER TABLE "public"."VersionInfo" ADD "Description" varchar(1024);
CREATE TABLE "public"."Users" ("Id" integer NOT NULL, "Name" varchar(100) NOT NULL, CONSTRAINT "PK_Users" PRIMARY KEY ("Id"));
INSERT INTO "public"."VersionInfo" ("Version","AppliedOn","Description") VALUES (1,(SYSDATE at time zone 'UTC'),'CreateTableMigration');
