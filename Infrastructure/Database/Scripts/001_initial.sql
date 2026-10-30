SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.SchemaVersions', N'U') IS NULL
    CREATE TABLE dbo.SchemaVersions(Version int NOT NULL PRIMARY KEY, AppliedAt datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET());
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaVersions WHERE Version=1)
BEGIN
    CREATE TABLE dbo.Accounts(
        UserId uniqueidentifier NOT NULL PRIMARY KEY,
        FirstName nvarchar(80) NOT NULL,
        PaternalLastName nvarchar(80) NOT NULL,
        MaternalLastName nvarchar(80) NULL,
        Email nvarchar(254) NOT NULL,
        NormalizedEmail nvarchar(254) NOT NULL UNIQUE,
        PasswordHash nvarchar(512) NOT NULL,
        IsOwner bit NOT NULL DEFAULT 1,
        IsEnabled bit NOT NULL DEFAULT 1,
        DisplayName nvarchar(100) NOT NULL,
        Description nvarchar(1000) NULL,
        PhoneNumber nvarchar(30) NULL,
        CreatedAt datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET());
    CREATE UNIQUE INDEX UX_Accounts_SingleOwner ON dbo.Accounts(IsOwner) WHERE IsOwner=1;
    CREATE TABLE dbo.Businesses(
        BusinessId uniqueidentifier NOT NULL PRIMARY KEY,
        OwnerUserId uniqueidentifier NOT NULL UNIQUE REFERENCES dbo.Accounts(UserId),
        TradeName nvarchar(120) NOT NULL,
        BusinessType nvarchar(40) NOT NULL CHECK(BusinessType=N'Cafeteria'),
        LegalName nvarchar(200) NULL, Description nvarchar(1000) NULL,
        TaxIdentificationNumber nvarchar(13) NULL, PhoneNumber nvarchar(30) NULL, Email nvarchar(254) NULL,
        CreatedAt datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET());
    CREATE TABLE dbo.Branches(
        BranchId uniqueidentifier NOT NULL PRIMARY KEY,
        BusinessId uniqueidentifier NOT NULL REFERENCES dbo.Businesses(BusinessId),
        Name nvarchar(120) NOT NULL,
        CreatedAt datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET());
    CREATE TABLE dbo.Employees(
        EmployeeId uniqueidentifier NOT NULL PRIMARY KEY,
        OwnerUserId uniqueidentifier NOT NULL REFERENCES dbo.Accounts(UserId),
        FirstName nvarchar(80) NOT NULL, PaternalLastName nvarchar(80) NOT NULL, MaternalLastName nvarchar(80) NULL,
        Description nvarchar(1000) NULL, PhoneNumber nvarchar(30) NULL, Email nvarchar(254) NULL,
        CreatedAt datetimeoffset NOT NULL DEFAULT SYSDATETIMEOFFSET());
    INSERT dbo.SchemaVersions(Version) VALUES(1);
END;
COMMIT;
