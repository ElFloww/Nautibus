CREATE TABLE [gtfs].[CalendarDate] (
    [FeedVersionId] int NOT NULL,
    [ServiceId] nvarchar(255) NOT NULL,
    [Date] date NOT NULL,
    [ExceptionType] int NOT NULL,
    CONSTRAINT [PK_CalendarDate] PRIMARY KEY ([FeedVersionId], [ServiceId], [Date])
);
GO
