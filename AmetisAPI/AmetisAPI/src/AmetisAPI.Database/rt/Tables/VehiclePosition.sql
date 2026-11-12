CREATE TABLE [rt].[VehiclePosition] (
    [Id] bigint NOT NULL IDENTITY,
    [VehicleId] nvarchar(255) NULL,
    [TripId] nvarchar(255) NULL,
    [RouteId] nvarchar(255) NULL,
    [Latitude] float NOT NULL,
    [Longitude] float NOT NULL,
    [Bearing] float NULL,
    [Speed] float NULL,
    [PositionTimestampUtc] datetime2 NOT NULL,
    CONSTRAINT [PK_VehiclePosition] PRIMARY KEY ([Id])
);
GO

CREATE INDEX [IX_VehiclePosition_PositionTimestampUtc] ON [rt].[VehiclePosition] ([PositionTimestampUtc]);
GO

CREATE UNIQUE INDEX [IX_VehiclePosition_VehicleId_PositionTimestampUtc] ON [rt].[VehiclePosition] ([VehicleId], [PositionTimestampUtc]) WHERE [VehicleId] IS NOT NULL;
GO
