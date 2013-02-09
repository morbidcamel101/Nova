CREATE TABLE [Customers].[SystemUsers] (
    [Id]   INT            IDENTITY (1, 1) NOT NULL,
    [Name] NVARCHAR (150) NOT NULL,
    CONSTRAINT [PK_SystemUsers] PRIMARY KEY CLUSTERED ([Id] ASC)
);

