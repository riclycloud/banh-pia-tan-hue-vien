-- Add HomePageId to SiteSettings if missing (fix for migration not applied).
-- Run once against your database, e.g. in SSMS or: sqlcmd -S your_server -d banhpia_thv -i AddHomePageIdColumn.sql

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.SiteSettings') AND name = N'HomePageId'
)
BEGIN
    ALTER TABLE dbo.SiteSettings ADD HomePageId int NULL;
END
GO
