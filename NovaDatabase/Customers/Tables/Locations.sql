CREATE TABLE [Customers].[Locations] (
    [Id]      INT            IDENTITY(1,1) NOT NULL,
	[City]    NVARCHAR (150) NOT NULL,
    [State]   NVARCHAR (100) NOT NULL,
    [Country] NVARCHAR (50) NOT NULL,
    
    CONSTRAINT [PK_Locations] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO

CREATE INDEX [IX_Locations_Country] ON [Customers].[Locations] ([Country])

GO

CREATE INDEX [IX_Locations_State] ON [Customers].[Locations] ([State])

GO

CREATE INDEX [IX_Locations_City] ON [Customers].[Locations] ([City])

GO

CREATE INDEX [IX_Locations_CSC] ON [Customers].[Locations] ([Country],[State],[City])
