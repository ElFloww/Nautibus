CREATE TABLE [rt].[StopPassage] (
    [Id] bigint NOT NULL IDENTITY,
    [TripId] nvarchar(255) NULL,
    [RouteId] nvarchar(255) NULL,
    [StopId] nvarchar(255) NULL,
    [StopOrder] int NOT NULL,
    [ScheduledUtc] datetime2 NOT NULL,
    [ExpectedUtc] datetime2 NOT NULL,
    [DelaySeconds] int NOT NULL,
    [VehicleId] nvarchar(255) NULL,
    [UpdatedUtc] datetime2 NOT NULL,
    CONSTRAINT [PK_StopPassage] PRIMARY KEY ([Id])
);
GO

CREATE INDEX [IX_StopPassage_ScheduledUtc] ON [rt].[StopPassage] ([ScheduledUtc]);
GO

CREATE INDEX [IX_StopPassage_StopId_ExpectedUtc] ON [rt].[StopPassage] ([StopId], [ExpectedUtc]);
GO

CREATE UNIQUE INDEX [IX_StopPassage_TripId_StopId_ScheduledUtc] ON [rt].[StopPassage] ([TripId], [StopId], [ScheduledUtc]) WHERE [TripId] IS NOT NULL AND [StopId] IS NOT NULL;
GO
