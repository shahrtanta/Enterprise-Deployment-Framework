IF OBJECT_ID(N'dbo.ExampleDeploymentEntity', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ExampleDeploymentEntity
    (
        Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name nvarchar(200) NOT NULL,
        CreatedAtUtc datetime2(0) NOT NULL
            CONSTRAINT DF_ExampleDeploymentEntity_CreatedAtUtc
            DEFAULT SYSUTCDATETIME()
    );
END
