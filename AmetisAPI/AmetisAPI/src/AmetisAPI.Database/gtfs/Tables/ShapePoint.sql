CREATE TABLE [gtfs].[ShapePoint] (
    [FeedVersionId] int NOT NULL,
    [ShapeId] nvarchar(255) NOT NULL,
    [Sequence] int NOT NULL,
    [Latitude] float NOT NULL,
    [Longitude] float NOT NULL,
    CONSTRAINT [PK_ShapePoint] PRIMARY KEY ([FeedVersionId], [ShapeId], [Sequence])
);
GO
