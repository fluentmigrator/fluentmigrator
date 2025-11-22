CREATE TABLE [dbo].[Orders]
(
    [Id] INT NOT NULL IDENTITY(1,1),
    [UserId] INT NOT NULL,
    [OrderNumber] NVARCHAR(50) NOT NULL,
    [TotalAmount] DECIMAL(18, 2) NOT NULL,
    [OrderDate] DATETIME2 NOT NULL,
    [Status] NVARCHAR(20) NOT NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Orders_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id])
);
