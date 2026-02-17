IF OBJECT_ID('dbo.Directory_Version', 'U') IS NULL
Begin
   CREATE TABLE dbo.Directory_Version
   (
       Id             BIGINT Identity(1,1) Not Null Constraint PK_Directory_Version_id Primary Key,
       Code           NVARCHAR(50) Not Null,
       CurrentVersion NVARCHAR(50) Not Null,
       LastUpdate     DATETIME Not Null
   )

    CREATE UNIQUE INDEX IX_Actual_Code_Version ON Directory_Version(Code);
END