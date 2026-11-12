CREATE TABLE [gtfs].[Trip] (
    [FeedVersionId] int NOT NULL,
    [TripId] nvarchar(255) NOT NULL,
    [RouteId] nvarchar(255) NULL,
    [ServiceId] nvarchar(255) NULL,
    [Headsign] nvarchar(255) NULL,
    [Destination] nvarchar(255) NULL,
    [DirectionId] int NULL,
    [ShapeId] nvarchar(255) NULL,
    CONSTRAINT [PK_Trip] PRIMARY KEY ([FeedVersionId], [TripId])
);
GO

CREATE INDEX [IX_Trip_FeedVersionId_RouteId] ON [gtfs].[Trip] ([FeedVersionId], [RouteId]);
GO
