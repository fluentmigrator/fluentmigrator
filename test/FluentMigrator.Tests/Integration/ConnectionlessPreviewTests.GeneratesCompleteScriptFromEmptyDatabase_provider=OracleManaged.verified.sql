-- Processor: OracleManaged
-- Generator: OracleManagedGenerator

CREATE TABLE VersionInfo (Version NUMBER(19,0) NOT NULL);
CREATE UNIQUE INDEX UC_Version ON VersionInfo (Version ASC);

ALTER TABLE VersionInfo ADD AppliedOn TIMESTAMP(4);

ALTER TABLE VersionInfo ADD Description NVARCHAR2(1024);
CREATE TABLE Users (Id NUMBER(10,0) NOT NULL, Name NVARCHAR2(100) NOT NULL, CONSTRAINT PK_Users PRIMARY KEY (Id));
INSERT ALL INTO VersionInfo (Version, AppliedOn, Description) VALUES (1, sys_extract_utc(SYSTIMESTAMP), 'CreateTableMigration') SELECT 1 FROM DUAL;
