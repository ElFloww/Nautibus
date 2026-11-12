CREATE TABLE [gtfs].[StopMatch] (
    [RealtimeStopId] nvarchar(255) NOT NULL,
    [NetworkStopId] nvarchar(255) NULL,
    [DistanceMeters] float NOT NULL,
    [Confidence] tinyint NOT NULL,
    CONSTRAINT [PK_StopMatch] PRIMARY KEY ([RealtimeStopId])
);
GO

CREATE INDEX [IX_StopMatch_NetworkStopId] ON [gtfs].[StopMatch] ([NetworkStopId]);
GO
