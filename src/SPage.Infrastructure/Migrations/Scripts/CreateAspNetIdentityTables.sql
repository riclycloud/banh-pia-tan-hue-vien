-- Create ASP.NET Core Identity tables (AspNetRoles, AspNetUsers, etc.) if they do not exist.
-- Run once against your database when you get "Invalid object name 'AspNetRoles'".
-- Compatible with IdentityDbContext<ApplicationUser> and ApplicationUser (IdentityUser + FullName, AvatarUrl, Bio, CreatedAt, IsActive, PreferredLanguage).

IF OBJECT_ID(N'dbo.AspNetRoles', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetRoles] (
        [Id]               NVARCHAR(450) NOT NULL,
        [Name]             NVARCHAR(256) NULL,
        [NormalizedName]   NVARCHAR(256) NULL,
        [ConcurrencyStamp] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE INDEX [RoleNameIndex] ON [dbo].[AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
END
GO

IF OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUsers] (
        [Id]                   NVARCHAR(450) NOT NULL,
        [UserName]             NVARCHAR(256) NULL,
        [NormalizedUserName]   NVARCHAR(256) NULL,
        [Email]                NVARCHAR(256) NULL,
        [NormalizedEmail]      NVARCHAR(256) NULL,
        [EmailConfirmed]       BIT NOT NULL DEFAULT 0,
        [PasswordHash]         NVARCHAR(MAX) NULL,
        [SecurityStamp]        NVARCHAR(MAX) NULL,
        [ConcurrencyStamp]     NVARCHAR(MAX) NULL,
        [PhoneNumber]          NVARCHAR(MAX) NULL,
        [PhoneNumberConfirmed]  BIT NOT NULL DEFAULT 0,
        [TwoFactorEnabled]     BIT NOT NULL DEFAULT 0,
        [LockoutEnd]           DATETIMEOFFSET NULL,
        [LockoutEnabled]       BIT NOT NULL DEFAULT 0,
        [AccessFailedCount]    INT NOT NULL DEFAULT 0,
        [FullName]             NVARCHAR(256) NULL,
        [AvatarUrl]            NVARCHAR(512) NULL,
        [Bio]                  NVARCHAR(MAX) NULL,
        [CreatedAt]            DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
        [IsActive]             BIT NOT NULL DEFAULT 1,
        [PreferredLanguage]    NVARCHAR(10) NOT NULL DEFAULT N'vi',
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
    CREATE INDEX [EmailIndex] ON [dbo].[AspNetUsers] ([NormalizedEmail]);
    CREATE UNIQUE INDEX [UserNameIndex] ON [dbo].[AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
END
GO

IF OBJECT_ID(N'dbo.AspNetUserRoles', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserRoles] (
        [UserId] NVARCHAR(450) NOT NULL,
        [RoleId] NVARCHAR(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [dbo].[AspNetUserRoles] ([RoleId]);
END
GO

IF OBJECT_ID(N'dbo.AspNetUserClaims', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserClaims] (
        [Id]         INT IDENTITY(1,1) NOT NULL,
        [UserId]     NVARCHAR(450) NOT NULL,
        [ClaimType]  NVARCHAR(MAX) NULL,
        [ClaimValue] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims] ([UserId]);
END
GO

IF OBJECT_ID(N'dbo.AspNetUserLogins', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserLogins] (
        [LoginProvider]       NVARCHAR(128) NOT NULL,
        [ProviderKey]         NVARCHAR(128) NOT NULL,
        [ProviderDisplayName] NVARCHAR(MAX) NULL,
        [UserId]              NVARCHAR(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins] ([UserId]);
END
GO

IF OBJECT_ID(N'dbo.AspNetUserTokens', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserTokens] (
        [UserId]        NVARCHAR(450) NOT NULL,
        [LoginProvider] NVARCHAR(128) NOT NULL,
        [Name]          NVARCHAR(128) NOT NULL,
        [Value]         NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'dbo.AspNetRoleClaims', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetRoleClaims] (
        [Id]         INT IDENTITY(1,1) NOT NULL,
        [RoleId]     NVARCHAR(450) NOT NULL,
        [ClaimType]  NVARCHAR(MAX) NULL,
        [ClaimValue] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims] ([RoleId]);
END
GO
