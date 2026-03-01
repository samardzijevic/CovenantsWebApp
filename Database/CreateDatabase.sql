-- ============================================================
-- CovenantsDB - Full Database Schema
-- Target: DESKTOP-LEA2DKG\CovenantsDB
-- Run this script once to create all tables and indexes
-- ============================================================

USE CovenantsDB;
GO

-- ============================================================
-- 1. COVENANT TYPES
-- ============================================================
IF OBJECT_ID('dbo.CovenantTypes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CovenantTypes (
        Id          INT IDENTITY(1,1)   NOT NULL,
        Name        NVARCHAR(100)       NOT NULL,
        Description NVARCHAR(500)       NULL,
        IsActive    BIT                 NOT NULL CONSTRAINT DF_CovenantTypes_IsActive DEFAULT 1,
        CreatedAt   DATETIME2           NOT NULL CONSTRAINT DF_CovenantTypes_CreatedAt DEFAULT GETUTCDATE(),
        CreatedBy   NVARCHAR(128)       NULL,
        CONSTRAINT PK_CovenantTypes PRIMARY KEY CLUSTERED (Id ASC)
    );
END
GO

-- ============================================================
-- 2. COVENANTS  (soft delete via IsDeleted)
-- ============================================================
IF OBJECT_ID('dbo.Covenants', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Covenants (
        Id              INT IDENTITY(1,1)   NOT NULL,
        CovenantTypeId  INT                 NOT NULL,
        Title           NVARCHAR(200)       NOT NULL,
        Description     NVARCHAR(MAX)       NULL,
        ProcessingDate  DATETIME2           NOT NULL,
        Value           DECIMAL(18,2)       NULL,
        Currency        NVARCHAR(10)        NULL,
        -- Active | Pending | Completed
        Status          NVARCHAR(50)        NOT NULL CONSTRAINT DF_Covenants_Status DEFAULT 'Active',
        IsDeleted       BIT                 NOT NULL CONSTRAINT DF_Covenants_IsDeleted DEFAULT 0,
        DeletedAt       DATETIME2           NULL,
        DeletedBy       NVARCHAR(128)       NULL,
        CreatedAt       DATETIME2           NOT NULL CONSTRAINT DF_Covenants_CreatedAt DEFAULT GETUTCDATE(),
        CreatedBy       NVARCHAR(128)       NULL,
        UpdatedAt       DATETIME2           NULL,
        UpdatedBy       NVARCHAR(128)       NULL,
        CONSTRAINT PK_Covenants PRIMARY KEY CLUSTERED (Id ASC),
        CONSTRAINT FK_Covenants_CovenantTypes
            FOREIGN KEY (CovenantTypeId) REFERENCES dbo.CovenantTypes(Id)
    );

    CREATE INDEX IX_Covenants_IsDeleted
        ON dbo.Covenants(IsDeleted);

    CREATE INDEX IX_Covenants_ProcessingDate
        ON dbo.Covenants(ProcessingDate);

    CREATE INDEX IX_Covenants_Status
        ON dbo.Covenants(Status);
END
GO

-- ============================================================
-- 3. COVENANT SCHEDULES
-- ============================================================
IF OBJECT_ID('dbo.CovenantSchedules', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CovenantSchedules (
        Id           INT IDENTITY(1,1)   NOT NULL,
        CovenantId   INT                 NOT NULL,
        -- Once | Daily | Weekly | Monthly | Yearly
        ScheduleType NVARCHAR(20)        NOT NULL,
        StartDate    DATETIME2           NOT NULL,
        EndDate      DATETIME2           NULL,
        -- Weekly: 0=Sun, 1=Mon ... 6=Sat
        DayOfWeek    INT                 NULL,
        -- Monthly: 1–31
        DayOfMonth   INT                 NULL,
        -- Yearly: 1–12
        MonthOfYear  INT                 NULL,
        IsActive     BIT                 NOT NULL CONSTRAINT DF_CovenantSchedules_IsActive DEFAULT 1,
        LastRunAt    DATETIME2           NULL,
        NextRunAt    DATETIME2           NULL,
        CreatedAt    DATETIME2           NOT NULL CONSTRAINT DF_CovenantSchedules_CreatedAt DEFAULT GETUTCDATE(),
        CreatedBy    NVARCHAR(128)       NULL,
        UpdatedAt    DATETIME2           NULL,
        UpdatedBy    NVARCHAR(128)       NULL,
        CONSTRAINT PK_CovenantSchedules PRIMARY KEY CLUSTERED (Id ASC),
        CONSTRAINT FK_CovenantSchedules_Covenants
            FOREIGN KEY (CovenantId) REFERENCES dbo.Covenants(Id)
    );

    CREATE INDEX IX_CovenantSchedules_CovenantId
        ON dbo.CovenantSchedules(CovenantId);

    -- Filtered index for the scheduler engine hot path
    CREATE INDEX IX_CovenantSchedules_NextRunAt
        ON dbo.CovenantSchedules(NextRunAt)
        WHERE IsActive = 1;
END
GO

-- ============================================================
-- 4. COVENANT FOLLOW-UPS
-- ============================================================
IF OBJECT_ID('dbo.CovenantFollowUps', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CovenantFollowUps (
        Id               INT IDENTITY(1,1)   NOT NULL,
        CovenantId       INT                 NOT NULL,
        ScheduleId       INT                 NULL,
        Title            NVARCHAR(200)       NOT NULL,
        Description      NVARCHAR(MAX)       NULL,
        StartDate        DATETIME2           NOT NULL,
        EndDate          DATETIME2           NOT NULL,
        -- Pending | InProgress | Completed | CompletedLate | Cancelled
        Status           NVARCHAR(50)        NOT NULL CONSTRAINT DF_FollowUps_Status DEFAULT 'Pending',
        Notes            NVARCHAR(MAX)       NULL,
        CompletionNotes  NVARCHAR(MAX)       NULL,
        StartedAt        DATETIME2           NULL,
        StartedBy        NVARCHAR(128)       NULL,
        CompletedAt      DATETIME2           NULL,
        CompletedBy      NVARCHAR(128)       NULL,
        IsAutoGenerated  BIT                 NOT NULL CONSTRAINT DF_FollowUps_IsAutoGenerated DEFAULT 0,
        -- Computed: 1 if completed late OR still open and past EndDate
        IsOverdue        AS (CAST(
                            CASE
                                WHEN CompletedAt IS NOT NULL AND CompletedAt > EndDate THEN 1
                                WHEN CompletedAt IS NULL AND GETUTCDATE() > EndDate THEN 1
                                ELSE 0
                            END AS BIT)),
        CreatedAt        DATETIME2           NOT NULL CONSTRAINT DF_FollowUps_CreatedAt DEFAULT GETUTCDATE(),
        CreatedBy        NVARCHAR(128)       NULL,
        UpdatedAt        DATETIME2           NULL,
        UpdatedBy        NVARCHAR(128)       NULL,
        CONSTRAINT PK_CovenantFollowUps PRIMARY KEY CLUSTERED (Id ASC),
        CONSTRAINT FK_FollowUps_Covenants
            FOREIGN KEY (CovenantId) REFERENCES dbo.Covenants(Id),
        CONSTRAINT FK_FollowUps_Schedules
            FOREIGN KEY (ScheduleId) REFERENCES dbo.CovenantSchedules(Id)
    );

    CREATE INDEX IX_FollowUps_CovenantId
        ON dbo.CovenantFollowUps(CovenantId);

    CREATE INDEX IX_FollowUps_EndDate
        ON dbo.CovenantFollowUps(EndDate);

    CREATE INDEX IX_FollowUps_Status
        ON dbo.CovenantFollowUps(Status);
END
GO

-- ============================================================
-- 5. COVENANT HISTORY  (audit trail — never deleted)
-- ============================================================
IF OBJECT_ID('dbo.CovenantHistory', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CovenantHistory (
        Id         INT IDENTITY(1,1)   NOT NULL,
        CovenantId INT                 NOT NULL,
        -- Created | Updated | Deleted | Restored | FollowUpAdded | ScheduleChanged
        Action     NVARCHAR(50)        NOT NULL,
        FieldName  NVARCHAR(100)       NULL,
        OldValue   NVARCHAR(MAX)       NULL,
        NewValue   NVARCHAR(MAX)       NULL,
        ChangedAt  DATETIME2           NOT NULL CONSTRAINT DF_CovenantHistory_ChangedAt DEFAULT GETUTCDATE(),
        ChangedBy  NVARCHAR(128)       NULL,
        Notes      NVARCHAR(MAX)       NULL,
        CONSTRAINT PK_CovenantHistory PRIMARY KEY CLUSTERED (Id ASC),
        CONSTRAINT FK_CovenantHistory_Covenants
            FOREIGN KEY (CovenantId) REFERENCES dbo.Covenants(Id)
    );

    CREATE INDEX IX_CovenantHistory_CovenantId
        ON dbo.CovenantHistory(CovenantId);
END
GO

-- ============================================================
-- 6. NOTIFICATIONS
-- ============================================================
IF OBJECT_ID('dbo.Notifications', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Notifications (
        Id          INT IDENTITY(1,1)   NOT NULL,
        CovenantId  INT                 NOT NULL,
        UserId      NVARCHAR(128)       NULL,
        -- ProcessingDateApproaching | FollowUpDue | ScheduleTriggered
        Type        NVARCHAR(50)        NOT NULL,
        Message     NVARCHAR(500)       NOT NULL,
        IsRead      BIT                 NOT NULL CONSTRAINT DF_Notifications_IsRead DEFAULT 0,
        ReadAt      DATETIME2           NULL,
        CreatedAt   DATETIME2           NOT NULL CONSTRAINT DF_Notifications_CreatedAt DEFAULT GETUTCDATE(),
        DismissedAt DATETIME2           NULL,
        CONSTRAINT PK_Notifications PRIMARY KEY CLUSTERED (Id ASC),
        CONSTRAINT FK_Notifications_Covenants
            FOREIGN KEY (CovenantId) REFERENCES dbo.Covenants(Id)
    );

    CREATE INDEX IX_Notifications_UserId_IsRead
        ON dbo.Notifications(UserId, IsRead);

    CREATE INDEX IX_Notifications_CovenantId
        ON dbo.Notifications(CovenantId);
END
GO

-- ============================================================
-- SEED: default covenant types
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM dbo.CovenantTypes)
BEGIN
    INSERT INTO dbo.CovenantTypes (Name, Description, IsActive, CreatedBy)
    VALUES
        ('Financial', 'Financial covenants related to monetary obligations', 1, 'SYSTEM'),
        ('Operational', 'Covenants governing operational compliance', 1, 'SYSTEM'),
        ('Reporting', 'Covenants requiring periodic reporting', 1, 'SYSTEM'),
        ('Maintenance', 'Asset maintenance and upkeep covenants', 1, 'SYSTEM');
END
GO

PRINT 'CovenantsDB schema created successfully.';
GO
