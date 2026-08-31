-- Processor: Snowflake
-- Generator: SnowflakeGenerator

CREATE TABLE PUBLIC.VersionInfo (Version NUMBER NOT NULL);


ALTER TABLE PUBLIC.VersionInfo ADD COLUMN AppliedOn TIMESTAMP_NTZ;

ALTER TABLE PUBLIC.VersionInfo ADD COLUMN Description VARCHAR(1024);
CREATE TABLE PUBLIC.Users (Id NUMBER NOT NULL, Name VARCHAR(100) NOT NULL, CONSTRAINT PK_Users PRIMARY KEY (Id));
INSERT INTO PUBLIC.VersionInfo (Version, AppliedOn, Description) VALUES (1, SYSDATE(), 'CreateTableMigration');
