-- Processor: Firebird
-- Generator: FirebirdGenerator

CREATE TABLE VersionInfo (Version BIGINT NOT NULL);
CREATE UNIQUE ASC INDEX UC_Version ON VersionInfo (Version);

ALTER TABLE VersionInfo ADD AppliedOn TIMESTAMP;

ALTER TABLE VersionInfo ADD Description VARCHAR(1024);
CREATE TABLE Users (Id INTEGER NOT NULL, Name VARCHAR(100) NOT NULL, CONSTRAINT PK_Users PRIMARY KEY (Id));
INSERT INTO VersionInfo (Version, AppliedOn, Description) VALUES (1, CURRENT_TIMESTAMP, 'CreateTableMigration');
