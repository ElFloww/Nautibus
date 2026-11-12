CREATE TABLE [gtfs].[Stop] (
    [FeedVersionId] int NOT NULL,
    [StopId] nvarchar(255) NOT NULL,
    [Code] nvarchar(255) NULL,
    [Name] nvarchar(255) NULL,
    [Latitude] float NOT NULL,
    [Longitude] float NOT NULL,
    [Location] geography NULL,
    [LocationType] int NOT NULL,
    [ParentStationId] nvarchar(255) NULL,
    [WheelchairBoarding] int NOT NULL,
    CONSTRAINT [PK_Stop] PRIMARY KEY ([FeedVersionId], [StopId])
);
GO

CREATE INDEX [IX_Stop_FeedVersionId_ParentStationId] ON [gtfs].[Stop] ([FeedVersionId], [ParentStationId]);
GO

CREATE SPATIAL INDEX [SIX_Stop_Location] ON [gtfs].[Stop] ([Location]);
GO
