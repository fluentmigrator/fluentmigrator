-- Processor: SqlServer2016
-- Generator: SqlServer2016Generator

CREATE TABLE [dbo].[VersionInfo] ([Version] BIGINT NOT NULL);
CREATE UNIQUE CLUSTERED INDEX [UC_Version] ON [dbo].[VersionInfo] ([Version] ASC);

ALTER TABLE [dbo].[VersionInfo] ADD [AppliedOn] DATETIME;

ALTER TABLE [dbo].[VersionInfo] ADD [Description] NVARCHAR(1024);
CREATE TABLE [dbo].[Users] ([Id] INT NOT NULL, [Name] NVARCHAR(100) NOT NULL, CONSTRAINT [PK_Users] PRIMARY KEY ([Id]));
INSERT INTO [dbo].[VersionInfo] ([Version], [AppliedOn], [Description]) VALUES (1, GETUTCDATE(), N'CreateTableMigration');
