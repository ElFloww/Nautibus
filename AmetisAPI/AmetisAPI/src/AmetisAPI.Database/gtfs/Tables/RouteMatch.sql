CREATE TABLE [gtfs].[RouteMatch] (
    [RealtimeRouteId] nvarchar(255) NOT NULL,
    [NetworkRouteId] nvarchar(255) NULL,
    [Confidence] tinyint NOT NULL,
    CONSTRAINT [PK_RouteMatch] PRIMARY KEY ([RealtimeRouteId])
);
GO
