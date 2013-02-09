CREATE TABLE [Customers].[CustomerTracking]
(
	[Id] BIGINT NOT NULL IDENTITY(1,1) PRIMARY KEY, 
    [OperationType] NCHAR(1) NOT NULL, 
    [Date] DATETIME NOT NULL, 
    [CustomerId] BIGINT NOT NULL, 
    [SystemUserId] INT NULL, 
    CONSTRAINT [FK_Tracking_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [Customers].[Customers] ([Id])
)

GO

CREATE NONCLUSTERED INDEX [IX_CustomerTracking_CustomerId] ON [Customers].[CustomerTracking] ([CustomerId])

GO

CREATE INDEX [IX_CustomerTracking_Date] ON [Customers].[CustomerTracking] ([Date])

GO

CREATE INDEX [IX_CustomerTracking_SystemUser] ON [Customers].[CustomerTracking] ([SystemUserId])
