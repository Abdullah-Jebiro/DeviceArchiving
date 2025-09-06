USE [master]
GO
/****** Object:  Database [DeviceArchive]    Script Date: 6/16/2025 10:39:50 AM ******/
CREATE DATABASE [DeviceArchive]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'DeviceArchive', FILENAME = N'C:\Database\DeviceArchive.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'DeviceArchive_log', FILENAME = N'C:\Database\DeviceArchive_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [DeviceArchive].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [DeviceArchive] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [DeviceArchive] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [DeviceArchive] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [DeviceArchive] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [DeviceArchive] SET ARITHABORT OFF 
GO
ALTER DATABASE [DeviceArchive] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [DeviceArchive] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [DeviceArchive] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [DeviceArchive] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [DeviceArchive] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [DeviceArchive] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [DeviceArchive] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [DeviceArchive] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [DeviceArchive] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [DeviceArchive] SET  DISABLE_BROKER 
GO
ALTER DATABASE [DeviceArchive] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [DeviceArchive] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [DeviceArchive] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [DeviceArchive] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [DeviceArchive] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [DeviceArchive] SET READ_COMMITTED_SNAPSHOT ON 
GO
ALTER DATABASE [DeviceArchive] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [DeviceArchive] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [DeviceArchive] SET  MULTI_USER 
GO
ALTER DATABASE [DeviceArchive] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [DeviceArchive] SET DB_CHAINING OFF 
GO
ALTER DATABASE [DeviceArchive] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [DeviceArchive] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [DeviceArchive] SET DELAYED_DURABILITY = DISABLED 
GO
USE [DeviceArchive]
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 6/16/2025 10:39:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Devices]    Script Date: 6/16/2025 10:39:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Devices](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Source] [nvarchar](max) NOT NULL,
	[BrotherName] [nvarchar](max) NOT NULL,
	[LaptopName] [nvarchar](max) NOT NULL,
	[SystemPassword] [nvarchar](max) NOT NULL,
	[WindowsPassword] [nvarchar](max) NOT NULL,
	[HardDrivePassword] [nvarchar](max) NOT NULL,
	[FreezePassword] [nvarchar](max) NOT NULL,
	[Code] [nvarchar](max) NOT NULL,
	[Type] [nvarchar](max) NOT NULL,
	[SerialNumber] [nvarchar](450) NOT NULL,
	[Comment] [nvarchar](max) NULL,
	[ContactNumber] [nvarchar](max) NULL,
	[Card] [nvarchar](max) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UserId] [int] NOT NULL,
 CONSTRAINT [PK_Devices] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UQ_Serial] UNIQUE NONCLUSTERED 
(
	[SerialNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Operations]    Script Date: 6/16/2025 10:39:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Operations](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DeviceId] [int] NOT NULL,
	[OperationName] [nvarchar](max) NOT NULL,
	[OldValue] [nvarchar](max) NULL,
	[NewValue] [nvarchar](max) NULL,
	[Comment] [nvarchar](max) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UserId] [int] NOT NULL,
 CONSTRAINT [PK_Operations] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OperationsTypes]    Script Date: 6/16/2025 10:39:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OperationsTypes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_OperationsTypes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 6/16/2025 10:39:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](450) NOT NULL,
	[Password] [nvarchar](max) NOT NULL,
	[Picture] [varbinary](max) NOT NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Devices_SerialNumber]    Script Date: 6/16/2025 10:39:50 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Devices_SerialNumber] ON [dbo].[Devices]
(
	[SerialNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Devices_UserId]    Script Date: 6/16/2025 10:39:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_Devices_UserId] ON [dbo].[Devices]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Operations_DeviceId]    Script Date: 6/16/2025 10:39:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_Operations_DeviceId] ON [dbo].[Operations]
(
	[DeviceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Operations_UserId]    Script Date: 6/16/2025 10:39:50 AM ******/
CREATE NONCLUSTERED INDEX [IX_Operations_UserId] ON [dbo].[Operations]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Users_UserName]    Script Date: 6/16/2025 10:39:50 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_UserName] ON [dbo].[Users]
(
	[UserName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Devices]  WITH CHECK ADD  CONSTRAINT [FK_Devices_Users_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Devices] CHECK CONSTRAINT [FK_Devices_Users_UserId]
GO
ALTER TABLE [dbo].[Operations]  WITH CHECK ADD  CONSTRAINT [FK_Operations_Devices_DeviceId] FOREIGN KEY([DeviceId])
REFERENCES [dbo].[Devices] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Operations] CHECK CONSTRAINT [FK_Operations_Devices_DeviceId]
GO
ALTER TABLE [dbo].[Operations]  WITH CHECK ADD  CONSTRAINT [FK_Operations_Users_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Operations] CHECK CONSTRAINT [FK_Operations_Users_UserId]
GO
/****** Object:  StoredProcedure [dbo].[sp_addAppUser]    Script Date: 6/16/2025 10:39:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ����� ������ ������ ���� (�� ������ ���� Picture)
CREATE   PROCEDURE [dbo].[sp_addAppUser]
    @UserName NVARCHAR(255),
    @Password NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

IF EXISTS (SELECT 1 FROM Users WHERE UserName = @UserName)
    THROW 50000, 'اسم المستخدم موجود بالفعل', 1;

    INSERT INTO Users (UserName, Password, Picture)
    VALUES (@UserName, @Password, 0x); -- ���� ����� ����� ��������
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_AddDevice]    Script Date: 6/16/2025 10:39:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE  PROCEDURE [dbo].[sp_AddDevice]
    @Source NVARCHAR(100),
    @BrotherName NVARCHAR(100),
    @LaptopName NVARCHAR(100),
    @SystemPassword NVARCHAR(100),
    @WindowsPassword NVARCHAR(100),
    @HardDrivePassword NVARCHAR(100),
    @FreezePassword NVARCHAR(100),
    @Code NVARCHAR(50),
    @Type NVARCHAR(50),
    @SerialNumber NVARCHAR(50),
    @Card NVARCHAR(50),
    @Comment NVARCHAR(500),
    @ContactNumber NVARCHAR(50),
    @CreatedAt DATETIME,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

IF EXISTS (SELECT 1 FROM Devices WHERE SerialNumber = @SerialNumber OR LaptopName = @LaptopName)
    THROW 50000, 'الجهاز موجود بالفعل في النظام', 1;

    INSERT INTO Devices (Source, BrotherName, LaptopName, SystemPassword, WindowsPassword, HardDrivePassword, FreezePassword, Code, Type, SerialNumber, Card, Comment, ContactNumber, IsActive, CreatedAt, UserId)
    VALUES (@Source, @BrotherName, @LaptopName, @SystemPassword, @WindowsPassword, @HardDrivePassword, @FreezePassword, @Code, @Type, @SerialNumber, @Card, @Comment, @ContactNumber, 1, @CreatedAt, @UserId);
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_AddOperation]    Script Date: 6/16/2025 10:39:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_AddOperation]
    @Comment NVARCHAR(500),
    @NewValue NVARCHAR(255),
    @OldValue NVARCHAR(255),
    @OperationName NVARCHAR(100),
    @DeviceId INT,
    @CreatedAt DATETIME,
	@UserId INT
AS
BEGIN
    INSERT INTO Operations (Comment, NewValue, OldValue, OperationName, DeviceId, CreatedAt ,UserId)
    VALUES (@Comment, @NewValue, @OldValue, @OperationName, @DeviceId, @CreatedAt	,@UserId );
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_AddOperationType]    Script Date: 6/16/2025 10:39:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_AddOperationType]
    @Name NVARCHAR(100),
    @Description NVARCHAR(500)
AS
BEGIN
    INSERT INTO OperationsTypes (Name, Description)
    VALUES (@Name, @Description);
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_adduser]    Script Date: 6/16/2025 10:39:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ����� ������ ������ ���� (�� ������ ���� Picture)
CREATE   PROCEDURE [dbo].[sp_adduser]
    @loginName NVARCHAR(255),
    @UserName NVARCHAR(255),
    @Password NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

IF EXISTS (SELECT 1 FROM Users WHERE UserName = @UserName)
    THROW 50000, 'اسم المستخدم موجود بالفعل', 1;

    INSERT INTO Users (UserName, Password, Picture)
    VALUES (@UserName, @Password, 0x); -- ���� ����� ����� ��������
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_AuthenticateUser]    Script Date: 6/16/2025 10:39:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ----------------------------------
-- 3. ������� ���������� (Users)
-- ----------------------------------

-- ����� ������ �� ������ ����� ���� ��������
CREATE   PROCEDURE [dbo].[sp_AuthenticateUser]
    @UserName NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, UserName, Password
    FROM Users
    WHERE UserName = @UserName;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_BackupDatabase]    Script Date: 6/16/2025 10:39:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ----------------------------------
-- 4. ����� ����� ��������� (Backup)
-- ----------------------------------

-- ����� ������ ���� �������� ������ ��������
CREATE   PROCEDURE [dbo].[sp_BackupDatabase]
    @BackupPath NVARCHAR(512)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @BackupFile NVARCHAR(512);
    DECLARE @DatabaseName NVARCHAR(128) = 'DeviceArchive';
    DECLARE @Timestamp NVARCHAR(20) = REPLACE(CONVERT(NVARCHAR, GETDATE(), 120), ':', '-');

    -- ����� ��� ��� ����� ��������� �� ���� ����
    SET @BackupFile = @BackupPath + '\\' + @DatabaseName + '_Backup_' + @Timestamp + '.bak';

    -- ����� ����� ���������
    BACKUP DATABASE @DatabaseName
    TO DISK = @BackupFile
    WITH INIT, -- ������� ��� ����� ��� ��� �������
         NAME = @DatabaseName,
         DESCRIPTION = '���� �������� ������ �������� DeviceArchive';
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_CheckDuplicates]    Script Date: 6/16/2025 10:39:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_CheckDuplicates]
    @SerialNumbers NVARCHAR(MAX),
    @LaptopNames NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    -- تحويل سلسلة الأرقام التسلسلية إلى جدول مؤقت باستخدام STRING_SPLIT
    DECLARE @SerialTable TABLE (Value NVARCHAR(100));
    INSERT INTO @SerialTable (Value)
    SELECT TRIM(value)
    FROM STRING_SPLIT(@SerialNumbers, ',')
    WHERE TRIM(value) <> '';

    -- تحويل أسماء اللابتوبات إلى جدول مؤقت
    DECLARE @LaptopNameTable TABLE (Value NVARCHAR(255));
    INSERT INTO @LaptopNameTable (Value)
    SELECT TRIM(value)
    FROM STRING_SPLIT(@LaptopNames, ',')
    WHERE TRIM(value) <> '';

    -- جلب التكرارات الموجودة في جدول الأجهزة
    SELECT DISTINCT d.SerialNumber, NULL AS LaptopName
    FROM Devices d
    INNER JOIN @SerialTable s ON d.SerialNumber = s.Value

    UNION

    SELECT DISTINCT NULL AS SerialNumber, d.LaptopName
    FROM Devices d
    INNER JOIN @LaptopNameTable l ON d.LaptopName = l.Value;
END;

GO
/****** Object:  StoredProcedure [dbo].[sp_CheckDuplicatesLaptopName]    Script Date: 6/16/2025 10:39:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[sp_CheckDuplicatesLaptopName]
    @LaptopNames NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;



    SELECT DISTINCT LaptopName FROM Devices 
    WHERE LaptopName IN (SELECT value FROM STRING_SPLIT(@LaptopNames, ','))
END

GO
/****** Object:  StoredProcedure [dbo].[sp_CheckDuplicatesSerialNumber]    Script Date: 6/16/2025 10:39:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[sp_CheckDuplicatesSerialNumber]
    @SerialNumbers NVARCHAR(MAX)

AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT SerialNumber FROM Devices 
    WHERE SerialNumber IN (SELECT value FROM STRING_SPLIT(@SerialNumbers, ','))


END
GO
/****** Object:  StoredProcedure [dbo].[sp_DeleteDevice]    Script Date: 6/16/2025 10:39:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_DeleteDevice]
    @Id INT,
    @UpdatedAt DATETIME,
	@UserId INT
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Devices WHERE Id = @Id)
        THROW 50000, 'الجهاز غير موجود في النظام', 1;

    UPDATE Devices
    SET IsActive = 0, UpdatedAt = @UpdatedAt
    WHERE Id = @Id;

    INSERT INTO Operations (DeviceId, OperationName, CreatedAt, UserId)
    VALUES (@Id, N'تم حذف الجهاز', @UpdatedAt, @UserId);
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_DeleteOperationType]    Script Date: 6/16/2025 10:39:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_DeleteOperationType]
    @Id INT
AS
BEGIN
    DELETE FROM OperationsTypes WHERE Id = @Id;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_GetAllDevices]    Script Date: 6/16/2025 10:39:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_GetAllDevices]
AS
BEGIN
    SELECT d.*, u.UserName
    FROM Devices d
    LEFT JOIN Users u ON d.UserId = u.Id;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_GetAllOperations]    Script Date: 6/16/2025 10:39:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_GetAllOperations]
    @DeviceId INT
AS
BEGIN
    SELECT o.*, u.*
    FROM Operations o
    LEFT JOIN Users u ON o.UserId = u.Id
    WHERE o.DeviceId = @DeviceId;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_GetAllOperationTypes]    Script Date: 6/16/2025 10:39:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_GetAllOperationTypes]
    @SearchTerm NVARCHAR(100) = NULL
AS
BEGIN
    IF @SearchTerm IS NOT NULL
        SELECT * FROM OperationsTypes
        WHERE Name LIKE '%' + @SearchTerm + '%' OR Description LIKE '%' + @SearchTerm + '%';
    ELSE
        SELECT * FROM OperationsTypes;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_GetDeviceById_Device]    Script Date: 6/16/2025 10:39:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_GetDeviceById_Device]
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        d.Id,
        d.Source,
        d.BrotherName,
        d.LaptopName,
        d.SystemPassword,
        d.WindowsPassword,
        d.HardDrivePassword,
        d.FreezePassword,
        d.Code,
        d.Type,
        d.SerialNumber,
        d.Card,
        d.Comment,
        d.ContactNumber,
        u.UserName,
		IsActive
    FROM 
        Devices d
    LEFT JOIN 
        Users u ON d.UserId = u.Id
    WHERE 
        d.Id = @Id;
END

GO
/****** Object:  StoredProcedure [dbo].[sp_GetDeviceById_Operations]    Script Date: 6/16/2025 10:39:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_GetDeviceById_Operations]
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        o.OperationName,
        o.OldValue,
        o.NewValue,
        o.Comment,
        o.CreatedAt,
        u.UserName
    FROM 
        Operations o
    INNER JOIN 
        Users u ON o.UserId = u.Id
    WHERE 
        o.DeviceId = @Id
    ORDER BY 
        o.CreatedAt DESC;
END

GO
/****** Object:  StoredProcedure [dbo].[sp_GetDeviceBySerialOrLaptop]    Script Date: 6/16/2025 10:39:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_GetDeviceBySerialOrLaptop]
    @SerialNumber NVARCHAR(50),
    @LaptopName NVARCHAR(100)
AS
BEGIN
    SELECT TOP 1 *
    FROM Devices
    WHERE SerialNumber = @SerialNumber OR LaptopName = @LaptopName;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_GetInactiveDeviceBySerialOrLaptop]    Script Date: 6/16/2025 10:39:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_GetInactiveDeviceBySerialOrLaptop]
    @SerialNumber NVARCHAR(100) = NULL,
    @LaptopName NVARCHAR(100) = NULL
AS
BEGIN
    SELECT 
        d.Id,
        Source,
        BrotherName,
        LaptopName,
        SystemPassword,
        WindowsPassword,
        HardDrivePassword,
        FreezePassword,
        Code,
        Type,
        SerialNumber,
        Card,
        Comment,
        ContactNumber,
		u.UserName,
        IsActive
    FROM Devices d
	    LEFT JOIN 
        Users u ON d.UserId = u.Id
    WHERE IsActive = 0
    AND (
        (@SerialNumber IS NOT NULL AND SerialNumber = @SerialNumber)
        OR (@LaptopName IS NOT NULL AND LaptopName = @LaptopName)
    );
END
GO
/****** Object:  StoredProcedure [dbo].[sp_UpdateDevice]    Script Date: 6/16/2025 10:39:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ����� ������ ���� �����
CREATE   PROCEDURE [dbo].[sp_UpdateDevice]
    @Id INT,
    @Source NVARCHAR(100),
    @BrotherName NVARCHAR(100),
    @LaptopName NVARCHAR(100),
    @SystemPassword NVARCHAR(100),
    @WindowsPassword NVARCHAR(100),
    @HardDrivePassword NVARCHAR(100),
    @FreezePassword NVARCHAR(100),
    @Code NVARCHAR(50),
    @Type NVARCHAR(50),
    @SerialNumber NVARCHAR(50),
    @Card NVARCHAR(50),
    @Comment NVARCHAR(500),
    @ContactNumber NVARCHAR(50),
    @UpdatedAt DATETIME,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM Devices WHERE Id = @Id)
    THROW 50000, 'الجهاز غير موجود في النظام', 1;

IF EXISTS (SELECT 1 FROM Devices WHERE (SerialNumber = @SerialNumber OR LaptopName = @LaptopName) AND IsActive = 0  AND Id != @Id)
THROW 50000, 'الجهاز موجود بالنظام بس محذوف سابقاً', 1;

IF EXISTS (SELECT 1 FROM Devices WHERE (SerialNumber = @SerialNumber OR LaptopName = @LaptopName) AND Id != @Id)
    THROW 50000, 'الجهاز موجود بالفعل في النظام', 1;

    UPDATE Devices
    SET Source = @Source,
        BrotherName = @BrotherName,
        LaptopName = @LaptopName,
        SystemPassword = @SystemPassword,
        WindowsPassword = @WindowsPassword,
        HardDrivePassword = @HardDrivePassword,
        FreezePassword = @FreezePassword,
        Code = @Code,
        Type = @Type,
        SerialNumber = @SerialNumber,
        Card = @Card,
        Comment = @Comment,
        ContactNumber = @ContactNumber,
        UpdatedAt = @UpdatedAt,
        UserId = @UserId,
		IsActive =1
    WHERE Id = @Id;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_UpdateOperationType]    Script Date: 6/16/2025 10:39:50 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_UpdateOperationType]
    @Id INT,
    @Name NVARCHAR(100),
    @Description NVARCHAR(500)
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM OperationsTypes WHERE Id = @Id)
        THROW 50000, 'نوع العملية غير موجود في النظام', 1;

    UPDATE OperationsTypes
    SET Name = @Name, Description = @Description
    WHERE Id = @Id;
END;
GO
USE [master]
GO
ALTER DATABASE [DeviceArchive] SET  READ_WRITE 
GO
