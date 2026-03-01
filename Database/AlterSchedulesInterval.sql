-- ============================================================
-- Alter CovenantSchedules: add Interval and DaysOfWeek
-- Run against DESKTOP-LEA2DKG\CovenantsDB after CreateDatabase.sql
-- Safe to run multiple times (checks column existence first)
-- ============================================================
USE CovenantsDB;
GO

-- Interval: "every N [days / weeks / months / years]", default 1
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.CovenantSchedules') AND name = 'Interval'
)
BEGIN
    ALTER TABLE dbo.CovenantSchedules
        ADD [Interval] INT NOT NULL
        CONSTRAINT DF_CovenantSchedules_Interval DEFAULT 1;
    PRINT 'Column [Interval] added.';
END
ELSE
    PRINT 'Column [Interval] already exists.';
GO

-- DaysOfWeek: comma-separated 0-6 values for multi-day weekly
-- e.g. "1,3,5" = Mon, Wed, Fri
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.CovenantSchedules') AND name = 'DaysOfWeek'
)
BEGIN
    ALTER TABLE dbo.CovenantSchedules
        ADD DaysOfWeek NVARCHAR(20) NULL;
    PRINT 'Column DaysOfWeek added.';
END
ELSE
    PRINT 'Column DaysOfWeek already exists.';
GO

PRINT 'AlterSchedulesInterval completed.';
GO
