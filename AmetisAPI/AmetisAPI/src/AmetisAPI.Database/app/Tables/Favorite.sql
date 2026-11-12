CREATE TABLE [app].[Favorite] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NULL,
    [Kind] tinyint NOT NULL,
    [TargetId] nvarchar(255) NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    CONSTRAINT [PK_Favorite] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Favorite_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE UNIQUE INDEX [IX_Favorite_UserId_Kind_TargetId] ON [app].[Favorite] ([UserId], [Kind], [TargetId]) WHERE [UserId] IS NOT NULL AND [TargetId] IS NOT NULL;
GO
