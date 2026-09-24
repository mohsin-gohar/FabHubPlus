IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [AvatarUrl] nvarchar(300) NULL,
    [DarkMode] bit NOT NULL,
    [FontSize] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Categories] (
    [CategoryId] int NOT NULL IDENTITY,
    [Name] nvarchar(60) NOT NULL,
    [Description] nvarchar(500) NULL,
    [IconUrl] nvarchar(300) NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([CategoryId])
);
GO

CREATE TABLE [ChatFaqs] (
    [FaqId] int NOT NULL IDENTITY,
    [Question] nvarchar(300) NOT NULL,
    [Answer] nvarchar(2000) NOT NULL,
    [Keywords] nvarchar(500) NULL,
    CONSTRAINT [PK_ChatFaqs] PRIMARY KEY ([FaqId])
);
GO

CREATE TABLE [Events] (
    [EventId] int NOT NULL IDENTITY,
    [Title] nvarchar(200) NOT NULL,
    [City] nvarchar(100) NOT NULL,
    [Latitude] float NOT NULL,
    [Longitude] float NOT NULL,
    [EventDate] datetime2 NOT NULL,
    [TicketUrl] nvarchar(500) NULL,
    [Type] nvarchar(60) NULL,
    [Story] nvarchar(4000) NULL,
    CONSTRAINT [PK_Events] PRIMARY KEY ([EventId])
);
GO

CREATE TABLE [Tags] (
    [TagId] int NOT NULL IDENTITY,
    [Name] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Tags] PRIMARY KEY ([TagId])
);
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Bookmarks] (
    [BookmarkId] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ItemType] nvarchar(450) NOT NULL,
    [ItemId] int NOT NULL,
    [Note] nvarchar(1000) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Bookmarks] PRIMARY KEY ([BookmarkId]),
    CONSTRAINT [FK_Bookmarks_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ChatbotQueries] (
    [QueryId] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NULL,
    [Message] nvarchar(1000) NOT NULL,
    [Response] nvarchar(4000) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ChatbotQueries] PRIMARY KEY ([QueryId]),
    CONSTRAINT [FK_ChatbotQueries_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL
);
GO

CREATE TABLE [FanSubmissions] (
    [SubmissionId] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Body] nvarchar(max) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_FanSubmissions] PRIMARY KEY ([SubmissionId]),
    CONSTRAINT [FK_FanSubmissions_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Feedbacks] (
    [FeedbackId] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NULL,
    [Type] nvarchar(max) NOT NULL,
    [Message] nvarchar(2000) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Feedbacks] PRIMARY KEY ([FeedbackId]),
    CONSTRAINT [FK_Feedbacks_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL
);
GO

CREATE TABLE [ViewLogs] (
    [LogId] int NOT NULL IDENTITY,
    [ItemType] nvarchar(50) NOT NULL,
    [ItemId] int NOT NULL,
    [UserId] nvarchar(450) NULL,
    [ViewedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ViewLogs] PRIMARY KEY ([LogId]),
    CONSTRAINT [FK_ViewLogs_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL
);
GO

CREATE TABLE [Articles] (
    [ArticleId] int NOT NULL IDENTITY,
    [CategoryId] int NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Body] nvarchar(max) NOT NULL,
    [AuthorId] nvarchar(450) NULL,
    [PublishedAt] datetime2 NOT NULL,
    [IsTimeline] bit NOT NULL,
    CONSTRAINT [PK_Articles] PRIMARY KEY ([ArticleId]),
    CONSTRAINT [FK_Articles_AspNetUsers_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Articles_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([CategoryId]) ON DELETE NO ACTION
);
GO

CREATE TABLE [CharacterProfiles] (
    [CharacterId] int NOT NULL IDENTITY,
    [CategoryId] int NOT NULL,
    [Name] nvarchar(120) NOT NULL,
    [Fandom] nvarchar(120) NULL,
    [Bio] nvarchar(2000) NULL,
    [ImageUrl] nvarchar(300) NULL,
    CONSTRAINT [PK_CharacterProfiles] PRIMARY KEY ([CharacterId]),
    CONSTRAINT [FK_CharacterProfiles_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([CategoryId]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Contents] (
    [ContentId] int NOT NULL IDENTITY,
    [CategoryId] int NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Type] nvarchar(max) NOT NULL,
    [Genre] nvarchar(100) NULL,
    [Description] nvarchar(2000) NULL,
    [ReleaseDate] datetime2 NULL,
    [PopularityScore] int NOT NULL,
    [ViewCount] int NOT NULL,
    [ThumbnailUrl] nvarchar(300) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Contents] PRIMARY KEY ([ContentId]),
    CONSTRAINT [FK_Contents_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([CategoryId]) ON DELETE NO ACTION
);
GO

CREATE TABLE [MerchandiseItems] (
    [ItemId] int NOT NULL IDENTITY,
    [CategoryId] int NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [ImageUrl] nvarchar(300) NULL,
    [Tag] nvarchar(max) NOT NULL,
    [IsUpcoming] bit NOT NULL,
    [ReleaseDate] datetime2 NULL,
    [ViewCount] int NOT NULL,
    CONSTRAINT [PK_MerchandiseItems] PRIMARY KEY ([ItemId]),
    CONSTRAINT [FK_MerchandiseItems_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([CategoryId]) ON DELETE NO ACTION
);
GO

CREATE TABLE [UserCategories] (
    [UserId] nvarchar(450) NOT NULL,
    [CategoryId] int NOT NULL,
    CONSTRAINT [PK_UserCategories] PRIMARY KEY ([UserId], [CategoryId]),
    CONSTRAINT [FK_UserCategories_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserCategories_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([CategoryId]) ON DELETE CASCADE
);
GO

CREATE TABLE [ContentTags] (
    [ContentId] int NOT NULL,
    [TagId] int NOT NULL,
    CONSTRAINT [PK_ContentTags] PRIMARY KEY ([ContentId], [TagId]),
    CONSTRAINT [FK_ContentTags_Contents_ContentId] FOREIGN KEY ([ContentId]) REFERENCES [Contents] ([ContentId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ContentTags_Tags_TagId] FOREIGN KEY ([TagId]) REFERENCES [Tags] ([TagId]) ON DELETE CASCADE
);
GO

CREATE TABLE [MediaItems] (
    [MediaId] int NOT NULL IDENTITY,
    [ContentId] int NOT NULL,
    [MediaType] nvarchar(max) NOT NULL,
    [EmbedUrl] nvarchar(500) NOT NULL,
    [Tag] nvarchar(100) NULL,
    CONSTRAINT [PK_MediaItems] PRIMARY KEY ([MediaId]),
    CONSTRAINT [FK_MediaItems_Contents_ContentId] FOREIGN KEY ([ContentId]) REFERENCES [Contents] ([ContentId]) ON DELETE CASCADE
);
GO

CREATE TABLE [Ratings] (
    [RatingId] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ContentId] int NOT NULL,
    [Stars] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Ratings] PRIMARY KEY ([RatingId]),
    CONSTRAINT [FK_Ratings_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Ratings_Contents_ContentId] FOREIGN KEY ([ContentId]) REFERENCES [Contents] ([ContentId]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Articles_AuthorId] ON [Articles] ([AuthorId]);
GO

CREATE INDEX [IX_Articles_CategoryId] ON [Articles] ([CategoryId]);
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

CREATE UNIQUE INDEX [IX_Bookmarks_UserId_ItemType_ItemId] ON [Bookmarks] ([UserId], [ItemType], [ItemId]);
GO

CREATE UNIQUE INDEX [IX_Categories_Name] ON [Categories] ([Name]);
GO

CREATE INDEX [IX_CharacterProfiles_CategoryId] ON [CharacterProfiles] ([CategoryId]);
GO

CREATE INDEX [IX_ChatbotQueries_UserId] ON [ChatbotQueries] ([UserId]);
GO

CREATE INDEX [IX_Contents_CategoryId] ON [Contents] ([CategoryId]);
GO

CREATE INDEX [IX_Contents_ReleaseDate] ON [Contents] ([ReleaseDate]);
GO

CREATE INDEX [IX_Contents_Title] ON [Contents] ([Title]);
GO

CREATE INDEX [IX_ContentTags_TagId] ON [ContentTags] ([TagId]);
GO

CREATE INDEX [IX_Events_EventDate] ON [Events] ([EventDate]);
GO

CREATE INDEX [IX_FanSubmissions_UserId] ON [FanSubmissions] ([UserId]);
GO

CREATE INDEX [IX_Feedbacks_UserId] ON [Feedbacks] ([UserId]);
GO

CREATE INDEX [IX_MediaItems_ContentId] ON [MediaItems] ([ContentId]);
GO

CREATE INDEX [IX_MerchandiseItems_CategoryId] ON [MerchandiseItems] ([CategoryId]);
GO

CREATE INDEX [IX_MerchandiseItems_IsUpcoming] ON [MerchandiseItems] ([IsUpcoming]);
GO

CREATE INDEX [IX_Ratings_ContentId] ON [Ratings] ([ContentId]);
GO

CREATE UNIQUE INDEX [IX_Ratings_UserId_ContentId] ON [Ratings] ([UserId], [ContentId]);
GO

CREATE INDEX [IX_UserCategories_CategoryId] ON [UserCategories] ([CategoryId]);
GO

CREATE INDEX [IX_ViewLogs_UserId] ON [ViewLogs] ([UserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260924095153_InitialCreate', N'8.0.11');
GO

COMMIT;
GO

