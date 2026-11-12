CREATE TABLE [app].[PushSubscription] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NULL,
    [Endpoint] nvarchar(2000) NULL,
    [EndpointHash] nvarchar(64) NULL,
    [P256dh] nvarchar(200) NULL,
    [Auth] nvarchar(100) NULL,
    [AllLines] bit NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    CONSTRAINT [PK_PushSubscription] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PushSubscription_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE UNIQUE INDEX [IX_PushSubscription_EndpointHash] ON [app].[PushSubscription] ([EndpointHash]) WHERE [EndpointHash] IS NOT NULL;
GO

CREATE INDEX [IX_PushSubscription_UserId] ON [app].[PushSubscription] ([UserId]);
GO
