CREATE TABLE [gtfs].[Route] (
    [FeedVersionId] int NOT NULL,
    [RouteId] nvarchar(255) NOT NULL,
    [ShortName] nvarchar(255) NULL,
    [LongName] nvarchar(255) NULL,
    [RouteType] int NOT NULL,
    [Color] nvarchar(20) NULL,
    [TextColor] nvarchar(20) NULL,
    [SortOrder] int NULL,
    CONSTRAINT [PK_Route] PRIMARY KEY ([FeedVersionId], [RouteId])
);
GO
