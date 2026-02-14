IF OBJECT_ID('dbo.Directory_Actual', 'U') IS NULL
   Begin
   CREATE TABLE Directory_Actual
   (
       Id             BIGINT Identity(1,1) Not Null Constraint PK_Directory_Actual_id Primary Key,
       Code           NVARCHAR(50) Not Null,
       Name           NVARCHAR(100) Not Null,
       CurrentVersion NVARCHAR(50) Not Null,
       JsonData       NVARCHAR(MAX) Not Null,
       LastUpdate     DATETIME Not Null
   )

    CREATE INDEX IX_Actual_Code_Version ON Directory_Actual(Code, CurrentVersion);
   END
   
IF OBJECT_ID('dbo.Directory_History', 'U') IS NULL 
   BEGIN
   CREATE TABLE Directory_History
   (
       Id BIGINT Identity(1,1) Not Null Constraint PK_Directory_History_id Primary Key,
       Code NVARCHAR(50) Not Null,
       Name NVARCHAR(100) Not Null,
       Version NVARCHAR(50) Not Null,
       JsonData NVARCHAR(MAX) Not Null,
       ArchivedAt DATETIME Not Null
   )
    CREATE INDEX IX_History_Code ON Directory_History(Code);
   END