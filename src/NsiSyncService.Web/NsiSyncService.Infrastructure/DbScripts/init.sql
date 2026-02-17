IF OBJECT_ID('dbo.Directory_Actual_Version', 'U') IS NULL
Begin
   CREATE TABLE dbo.Directory_Actual_Version
   (
       Id             BIGINT Identity(1,1) Not Null Constraint PK_Directory_Actual_Version_id Primary Key,
       Code           NVARCHAR(50) Not Null,
       CurrentVersion NVARCHAR(50) Not Null,
       LastUpdate     DATETIME Null
   )

    CREATE UNIQUE INDEX IX_Actual_Code_Version ON Directory_Actual_Version(Code);
END