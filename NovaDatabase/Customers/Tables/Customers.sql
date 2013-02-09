CREATE TABLE [Customers].[Customers] 
(
	[Id]                BIGINT         IDENTITY(1,1) NOT NULL,
    [Name]              NVARCHAR (500) NOT NULL,
    [Gender]            NCHAR (1)      NOT NULL,
    [HouseNumber]       INT            NULL,
    [AddressLine1]      NVARCHAR (500) NULL,
    [DOB]               DATETIME       NOT NULL,
   
    [LocationId]        INT            NOT NULL,
    [CategoryId]        INT            NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_CategoryCustomer] FOREIGN KEY ([CategoryId]) REFERENCES [Customers].[Categories] ([Id]),
    CONSTRAINT [FK_LocationCustomer] FOREIGN KEY ([LocationId]) REFERENCES [Customers].[Locations] ([Id]),
   
);
GO
CREATE NONCLUSTERED INDEX [IX_FK_LocationCustomer]
    ON [Customers].[Customers]([LocationId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_CategoryCustomer]
    ON [Customers].[Customers]([CategoryId] ASC);


GO

CREATE NONCLUSTERED INDEX [IX_Customers_Name] ON [Customers].[Customers] ([Name])
