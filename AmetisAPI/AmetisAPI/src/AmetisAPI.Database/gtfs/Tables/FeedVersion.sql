CREATE TABLE [gtfs].[FeedVersion] (
    [Id] int NOT NULL IDENTITY,
    [Source] tinyint NOT NULL,
    [Status] tinyint NOT NULL,
    [IsActive] bit NOT NULL,
    [SourceUrl] nvarchar(1000) NULL,
    [ContentHash] nvarchar(64) NULL,
    [StartedAtUtc] datetime2 NOT NULL,
    [FinishedAtUtc] datetime2 NULL,
    [RouteCount] int NOT NULL,
    [StopCount] int NOT NULL,
    [TripCount] int NOT NULL,
    [StopTimeCount] int NOT NULL,
    [ShapePointCount] int NOT NULL,
    [RejectedCount] int NOT NULL,
    [ValidFrom] date NULL,
    [ValidTo] date NULL,
    [ChangeSummary] nvarchar(max) NULL,
    [ErrorMessage] nvarchar(max) NULL,
    CONSTRAINT [PK_FeedVersion] PRIMARY KEY ([Id])
);
GO

CREATE INDEX [IX_FeedVersion_Source_IsActive] ON [gtfs].[FeedVersion] ([Source], [IsActive]);
GO
