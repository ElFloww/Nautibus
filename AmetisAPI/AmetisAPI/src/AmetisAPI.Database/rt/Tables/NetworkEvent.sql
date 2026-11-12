CREATE TABLE [rt].[NetworkEvent] (
    [Id] int NOT NULL IDENTITY,
    [Kind] tinyint NOT NULL,
    [ExternalId] nvarchar(400) NULL,
    [Title] nvarchar(max) NULL,
    [Description] nvarchar(max) NULL,
    [Url] nvarchar(1000) NULL,
    [LineIds] nvarchar(max) NULL,
    [StartUtc] datetime2 NOT NULL,
    [EndUtc] datetime2 NULL,
    [ClosedUtc] datetime2 NULL,
    [LastSeenUtc] datetime2 NOT NULL,
    [CreatedUtc] datetime2 NOT NULL,
    [Confidence] tinyint NOT NULL,
    CONSTRAINT [PK_NetworkEvent] PRIMARY KEY ([Id])
);
GO

CREATE INDEX [IX_NetworkEvent_Kind_ClosedUtc] ON [rt].[NetworkEvent] ([Kind], [ClosedUtc]);
GO
