CREATE TABLE [gtfs].[ImportRejection] (
    [Id] bigint NOT NULL IDENTITY,
    [FeedVersionId] int NOT NULL,
    [FileName] nvarchar(100) NULL,
    [LineNumber] int NOT NULL,
    [Reason] nvarchar(1000) NULL,
    CONSTRAINT [PK_ImportRejection] PRIMARY KEY ([Id])
);
GO

CREATE INDEX [IX_ImportRejection_FeedVersionId] ON [gtfs].[ImportRejection] ([FeedVersionId]);
GO
