CREATE TABLE [gtfs].[StopTime] (
    [FeedVersionId] int NOT NULL,
    [TripId] nvarchar(255) NOT NULL,
    [StopSequence] int NOT NULL,
    [StopId] nvarchar(255) NULL,
    [ArrivalSeconds] int NULL,
    [DepartureSeconds] int NULL,
    [PickupType] int NOT NULL,
    [DropOffType] int NOT NULL,
    [ShapeDistTraveled] float NULL,
    CONSTRAINT [PK_StopTime] PRIMARY KEY ([FeedVersionId], [TripId], [StopSequence])
);
GO

CREATE INDEX [IX_StopTime_FeedVersionId_StopId_DepartureSeconds] ON [gtfs].[StopTime] ([FeedVersionId], [StopId], [DepartureSeconds]);
GO
