CREATE TABLE [dbo].[Users]
(
    [Id] INT NOT NULL IDENTITY(1,1),
    [Name] NVARCHAR(100) NOT NULL,
    [Email] NVARCHAR(255) NOT NULL,
    [CreatedDate] DATETIME2 NOT NULL,
    [IsActive] BIT NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
