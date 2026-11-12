CREATE TABLE [gtfs].[StopRoute] (
    [FeedVersionId] int NOT NULL,
    [StopId] nvarchar(255) NOT NULL,
    [RouteId] nvarchar(255) NOT NULL,
    CONSTRAINT [PK_StopRoute] PRIMARY KEY ([FeedVersionId], [StopId], [RouteId])
);
GO
