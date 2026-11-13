-- Processor: SqlServer2000
-- Generator: SqlServer2000Generator

CREATE TABLE [VersionInfo] ([Version] BIGINT NOT NULL);
CREATE UNIQUE CLUSTERED INDEX [UC_Version] ON [VersionInfo] ([Version] ASC);

ALTER TABLE [VersionInfo] ADD [AppliedOn] DATETIME;

ALTER TABLE [VersionInfo] ADD [Description] NVARCHAR(1024);
CREATE TABLE [Users] ([Id] INT NOT NULL, [Name] NVARCHAR(100) NOT NULL, CONSTRAINT [PK_Users] PRIMARY KEY ([Id]));
INSERT INTO [VersionInfo] ([Version], [AppliedOn], [Description]) VALUES (1, GETUTCDATE(), N'CreateTableMigration');
