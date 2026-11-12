CREATE TABLE [gtfs].[Calendar] (
    [FeedVersionId] int NOT NULL,
    [ServiceId] nvarchar(255) NOT NULL,
    [DaysMask] int NOT NULL,
    [StartDate] date NOT NULL,
    [EndDate] date NOT NULL,
    CONSTRAINT [PK_Calendar] PRIMARY KEY ([FeedVersionId], [ServiceId])
);
GO
