/****** Object:  Database [DB_RentACar]    Script Date: 05.05.2025 17:35:29 ******/
CREATE DATABASE [DB_RentACar]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'DB_RentACar_Data', FILENAME = N'c:\dzsqls\DB_RentACar.mdf' , SIZE = 30720KB , MAXSIZE = 30720KB , FILEGROWTH = 22528KB )
 LOG ON 
( NAME = N'DB_RentACar_Logs', FILENAME = N'c:\dzsqls\DB_RentACar.ldf' , SIZE = 8192KB , MAXSIZE = 30720KB , FILEGROWTH = 22528KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [DB_RentACar] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [DB_RentACar].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [DB_RentACar] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [DB_RentACar] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [DB_RentACar] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [DB_RentACar] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [DB_RentACar] SET ARITHABORT OFF 
GO
ALTER DATABASE [DB_RentACar] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [DB_RentACar] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [DB_RentACar] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [DB_RentACar] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [DB_RentACar] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [DB_RentACar] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [DB_RentACar] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [DB_RentACar] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [DB_RentACar] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [DB_RentACar] SET  ENABLE_BROKER 
GO
ALTER DATABASE [DB_RentACar] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [DB_RentACar] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [DB_RentACar] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [DB_RentACar] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [DB_RentACar] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [DB_RentACar] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [DB_RentACar] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [DB_RentACar] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [DB_RentACar] SET  MULTI_USER 
GO
ALTER DATABASE [DB_RentACar] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [DB_RentACar] SET DB_CHAINING OFF 
GO
ALTER DATABASE [DB_RentACar] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [DB_RentACar] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [DB_RentACar] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [DB_RentACar] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [DB_RentACar] SET QUERY_STORE = ON
GO
ALTER DATABASE [DB_RentACar] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
/****** Object:  User [ABTOP_SQLLogin_1]    Script Date: 05.05.2025 17:35:32 ******/
CREATE USER [ABTOP_SQLLogin_1] FOR LOGIN [ABTOP_SQLLogin_1] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [ABTOP_SQLLogin_1]
GO
/****** Object:  Schema [ABTOP_SQLLogin_1]    Script Date: 05.05.2025 17:35:32 ******/
CREATE SCHEMA [ABTOP_SQLLogin_1]
GO
/****** Object:  Table [dbo].[Bookings]    Script Date: 05.05.2025 17:35:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Bookings](
	[BookingID] [int] IDENTITY(1,1) NOT NULL,
	[VehicleID] [int] NOT NULL,
	[UserID] [int] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[ReturnDate] [datetime] NULL,
	[TotalCost] [decimal](10, 2) NOT NULL,
	[StatusID] [int] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[InsuranceID] [int] NULL,
	[ActualCost] [decimal](10, 2) NULL,
 CONSTRAINT [PK__Bookings__73951ACDE1C0F6C7] PRIMARY KEY CLUSTERED 
(
	[BookingID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BookingStatuses]    Script Date: 05.05.2025 17:35:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BookingStatuses](
	[BookingStatusID] [int] IDENTITY(1,1) NOT NULL,
	[BookingStatus] [nvarchar](20) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[BookingStatusID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Insurances]    Script Date: 05.05.2025 17:35:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Insurances](
	[InsuranceID] [int] IDENTITY(1,1) NOT NULL,
	[InsuranceName] [nvarchar](50) NOT NULL,
	[InsuranceDetails] [nvarchar](300) NOT NULL,
	[InsurancePrice] [decimal](10, 2) NOT NULL,
 CONSTRAINT [PK__Insuranc__74231BC4CA5C2E4F] PRIMARY KEY CLUSTERED 
(
	[InsuranceID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PaymentMethods]    Script Date: 05.05.2025 17:35:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PaymentMethods](
	[PaymentMethodID] [int] IDENTITY(1,1) NOT NULL,
	[PaymentMethod] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[PaymentMethodID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Payments]    Script Date: 05.05.2025 17:35:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Payments](
	[PaymentID] [int] IDENTITY(1,1) NOT NULL,
	[BookingID] [int] NOT NULL,
	[Amount] [decimal](10, 2) NOT NULL,
	[PaymentMethodID] [int] NULL,
	[PaymentStatusID] [int] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
 CONSTRAINT [PK__Payments__9B556A586E33B639] PRIMARY KEY CLUSTERED 
(
	[PaymentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PaymentStatuses]    Script Date: 05.05.2025 17:35:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PaymentStatuses](
	[PaymentStatusID] [int] IDENTITY(1,1) NOT NULL,
	[PaymentStatus] [nvarchar](20) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[PaymentStatusID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Reviews]    Script Date: 05.05.2025 17:35:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Reviews](
	[ReviewID] [int] IDENTITY(1,1) NOT NULL,
	[VehicleID] [int] NOT NULL,
	[UserID] [int] NOT NULL,
	[Rating] [int] NOT NULL,
	[Comment] [nvarchar](500) NULL,
	[CreatedAt] [datetime] NOT NULL,
 CONSTRAINT [PK__Reviews__74BC79AE99D08A32] PRIMARY KEY CLUSTERED 
(
	[ReviewID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Roles]    Script Date: 05.05.2025 17:35:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Roles](
	[RoleID] [int] IDENTITY(1,1) NOT NULL,
	[Role] [nvarchar](20) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[RoleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 05.05.2025 17:35:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[UserID] [int] IDENTITY(1,1) NOT NULL,
	[FirstName] [nvarchar](50) NOT NULL,
	[LastName] [nvarchar](50) NOT NULL,
	[Email] [nvarchar](100) NOT NULL,
	[PasswordHash] [nvarchar](255) NOT NULL,
	[RoleID] [int] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
 CONSTRAINT [PK__Users__1788CCAC48F90411] PRIMARY KEY CLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[VehicleCategories]    Script Date: 05.05.2025 17:35:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VehicleCategories](
	[VehicleCategoryID] [int] IDENTITY(1,1) NOT NULL,
	[VehicleCategory] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[VehicleCategoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[VehicleImages]    Script Date: 05.05.2025 17:35:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VehicleImages](
	[ImageID] [int] IDENTITY(1,1) NOT NULL,
	[VehicleID] [int] NULL,
	[ImagePath] [nvarchar](500) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ImageID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Vehicles]    Script Date: 05.05.2025 17:35:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Vehicles](
	[VehicleID] [int] IDENTITY(1,1) NOT NULL,
	[Make] [nvarchar](50) NOT NULL,
	[Model] [nvarchar](50) NOT NULL,
	[Year] [int] NOT NULL,
	[LicensePlate] [nvarchar](20) NOT NULL,
	[VehicleCategoryID] [int] NOT NULL,
	[DailyRate] [decimal](10, 2) NOT NULL,
	[Available] [bit] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[AvgRating] [decimal](3, 2) NULL,
 CONSTRAINT [PK__Vehicles__476B54B216C8883C] PRIMARY KEY CLUSTERED 
(
	[VehicleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[Bookings] ON 
GO
INSERT [dbo].[Bookings] ([BookingID], [VehicleID], [UserID], [StartDate], [EndDate], [ReturnDate], [TotalCost], [StatusID], [CreatedAt], [InsuranceID], [ActualCost]) VALUES (2, 1, 2, CAST(N'2025-05-10T00:00:00.000' AS DateTime), CAST(N'2025-05-17T00:00:00.000' AS DateTime), NULL, CAST(350.00 AS Decimal(10, 2)), 3, CAST(N'2025-03-09T18:31:44.953' AS DateTime), 1, NULL)
GO
INSERT [dbo].[Bookings] ([BookingID], [VehicleID], [UserID], [StartDate], [EndDate], [ReturnDate], [TotalCost], [StatusID], [CreatedAt], [InsuranceID], [ActualCost]) VALUES (3, 2, 2, CAST(N'2025-05-03T00:00:00.000' AS DateTime), CAST(N'2025-05-15T00:00:00.000' AS DateTime), CAST(N'2025-05-03T00:00:00.000' AS DateTime), CAST(300.00 AS Decimal(10, 2)), 3, CAST(N'2025-03-09T18:31:44.953' AS DateTime), 2, CAST(300.00 AS Decimal(10, 2)))
GO
INSERT [dbo].[Bookings] ([BookingID], [VehicleID], [UserID], [StartDate], [EndDate], [ReturnDate], [TotalCost], [StatusID], [CreatedAt], [InsuranceID], [ActualCost]) VALUES (4, 1, 2, CAST(N'2025-05-05T00:00:00.000' AS DateTime), CAST(N'2025-05-31T00:00:00.000' AS DateTime), NULL, CAST(1380.00 AS Decimal(10, 2)), 4, CAST(N'2025-05-04T16:12:38.767' AS DateTime), 2, NULL)
GO
INSERT [dbo].[Bookings] ([BookingID], [VehicleID], [UserID], [StartDate], [EndDate], [ReturnDate], [TotalCost], [StatusID], [CreatedAt], [InsuranceID], [ActualCost]) VALUES (5, 4, 25, CAST(N'2025-05-06T00:00:00.000' AS DateTime), CAST(N'2025-05-17T00:00:00.000' AS DateTime), NULL, CAST(3630.00 AS Decimal(10, 2)), 2, CAST(N'2025-05-05T14:59:00.573' AS DateTime), 2, NULL)
GO
SET IDENTITY_INSERT [dbo].[Bookings] OFF
GO
SET IDENTITY_INSERT [dbo].[BookingStatuses] ON 
GO
INSERT [dbo].[BookingStatuses] ([BookingStatusID], [BookingStatus]) VALUES (1, N'Pending')
GO
INSERT [dbo].[BookingStatuses] ([BookingStatusID], [BookingStatus]) VALUES (2, N'Confirmed')
GO
INSERT [dbo].[BookingStatuses] ([BookingStatusID], [BookingStatus]) VALUES (3, N'Completed')
GO
INSERT [dbo].[BookingStatuses] ([BookingStatusID], [BookingStatus]) VALUES (4, N'Cancelled')
GO
INSERT [dbo].[BookingStatuses] ([BookingStatusID], [BookingStatus]) VALUES (5, N'Pending Cancellation')
GO
INSERT [dbo].[BookingStatuses] ([BookingStatusID], [BookingStatus]) VALUES (6, N'Pending Completion')
GO
SET IDENTITY_INSERT [dbo].[BookingStatuses] OFF
GO
SET IDENTITY_INSERT [dbo].[Insurances] ON 
GO
INSERT [dbo].[Insurances] ([InsuranceID], [InsuranceName], [InsuranceDetails], [InsurancePrice]) VALUES (1, N'Базовая страховка', N'Покрывает ннебольшие повреждения', CAST(15.00 AS Decimal(10, 2)))
GO
INSERT [dbo].[Insurances] ([InsuranceID], [InsuranceName], [InsuranceDetails], [InsurancePrice]) VALUES (2, N'Полная страховка', N'Покрывает все повреждения', CAST(30.00 AS Decimal(10, 2)))
GO
SET IDENTITY_INSERT [dbo].[Insurances] OFF
GO
SET IDENTITY_INSERT [dbo].[PaymentMethods] ON 
GO
INSERT [dbo].[PaymentMethods] ([PaymentMethodID], [PaymentMethod]) VALUES (1, N'Credit Card')
GO
INSERT [dbo].[PaymentMethods] ([PaymentMethodID], [PaymentMethod]) VALUES (2, N'Cash')
GO
INSERT [dbo].[PaymentMethods] ([PaymentMethodID], [PaymentMethod]) VALUES (3, N'Bank Transfer')
GO
SET IDENTITY_INSERT [dbo].[PaymentMethods] OFF
GO
SET IDENTITY_INSERT [dbo].[Payments] ON 
GO
INSERT [dbo].[Payments] ([PaymentID], [BookingID], [Amount], [PaymentMethodID], [PaymentStatusID], [CreatedAt]) VALUES (4, 2, CAST(350.00 AS Decimal(10, 2)), 1, 2, CAST(N'2025-03-09T18:41:54.797' AS DateTime))
GO
INSERT [dbo].[Payments] ([PaymentID], [BookingID], [Amount], [PaymentMethodID], [PaymentStatusID], [CreatedAt]) VALUES (5, 3, CAST(300.00 AS Decimal(10, 2)), 1, 2, CAST(N'2025-03-09T18:41:54.797' AS DateTime))
GO
INSERT [dbo].[Payments] ([PaymentID], [BookingID], [Amount], [PaymentMethodID], [PaymentStatusID], [CreatedAt]) VALUES (6, 4, CAST(1380.00 AS Decimal(10, 2)), 1, 2, CAST(N'2025-05-04T16:12:38.880' AS DateTime))
GO
INSERT [dbo].[Payments] ([PaymentID], [BookingID], [Amount], [PaymentMethodID], [PaymentStatusID], [CreatedAt]) VALUES (7, 5, CAST(3630.00 AS Decimal(10, 2)), 2, 2, CAST(N'2025-05-05T14:59:01.113' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[Payments] OFF
GO
SET IDENTITY_INSERT [dbo].[PaymentStatuses] ON 
GO
INSERT [dbo].[PaymentStatuses] ([PaymentStatusID], [PaymentStatus]) VALUES (1, N'Pending')
GO
INSERT [dbo].[PaymentStatuses] ([PaymentStatusID], [PaymentStatus]) VALUES (2, N'Completed')
GO
INSERT [dbo].[PaymentStatuses] ([PaymentStatusID], [PaymentStatus]) VALUES (3, N'Cancelled')
GO
INSERT [dbo].[PaymentStatuses] ([PaymentStatusID], [PaymentStatus]) VALUES (4, N'Failed')
GO
SET IDENTITY_INSERT [dbo].[PaymentStatuses] OFF
GO
SET IDENTITY_INSERT [dbo].[Reviews] ON 
GO
INSERT [dbo].[Reviews] ([ReviewID], [VehicleID], [UserID], [Rating], [Comment], [CreatedAt]) VALUES (6, 1, 2, 5, N'Супер!', CAST(N'2025-04-01T14:20:28.333' AS DateTime))
GO
INSERT [dbo].[Reviews] ([ReviewID], [VehicleID], [UserID], [Rating], [Comment], [CreatedAt]) VALUES (7, 2, 2, 4, N'Процесс прошел гладко, но у автомобиля были небольшие проблемы.', CAST(N'2025-04-01T20:02:32.567' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[Reviews] OFF
GO
SET IDENTITY_INSERT [dbo].[Roles] ON 
GO
INSERT [dbo].[Roles] ([RoleID], [Role]) VALUES (1, N'User')
GO
INSERT [dbo].[Roles] ([RoleID], [Role]) VALUES (2, N'Admin')
GO
SET IDENTITY_INSERT [dbo].[Roles] OFF
GO
SET IDENTITY_INSERT [dbo].[Users] ON 
GO
INSERT [dbo].[Users] ([UserID], [FirstName], [LastName], [Email], [PasswordHash], [RoleID], [CreatedAt]) VALUES (1, N'admin', N'admin', N'admin', N'7C222FB2927D828AF22F592134E8932480637C0D', 2, CAST(N'2025-03-09T18:07:21.997' AS DateTime))
GO
INSERT [dbo].[Users] ([UserID], [FirstName], [LastName], [Email], [PasswordHash], [RoleID], [CreatedAt]) VALUES (2, N'Slavoj', N'Žižek', N'user@email.com', N'7C4A8D09CA3762AF61E59520943DC26494F8941B', 1, CAST(N'2025-04-27T22:02:57.827' AS DateTime))
GO
INSERT [dbo].[Users] ([UserID], [FirstName], [LastName], [Email], [PasswordHash], [RoleID], [CreatedAt]) VALUES (4, N'Test', N'User', N'test1@email.com', N'70CCD9007338D6D81DD3B6271621B9CF9A97EA00', 1, CAST(N'2025-03-25T20:18:32.657' AS DateTime))
GO
INSERT [dbo].[Users] ([UserID], [FirstName], [LastName], [Email], [PasswordHash], [RoleID], [CreatedAt]) VALUES (5, N'Test', N'User', N'test2@email.com', N'9237CB0FB91EB2A245845F9F3EF42DEFA2E494B6', 1, CAST(N'2025-03-25T20:19:09.660' AS DateTime))
GO
INSERT [dbo].[Users] ([UserID], [FirstName], [LastName], [Email], [PasswordHash], [RoleID], [CreatedAt]) VALUES (10, N'admin', N'admin', N'admin2', N'D033E22AE348AEB5660FC2140AEC35850C4DA997', 2, CAST(N'2025-03-31T20:09:41.630' AS DateTime))
GO
INSERT [dbo].[Users] ([UserID], [FirstName], [LastName], [Email], [PasswordHash], [RoleID], [CreatedAt]) VALUES (23, N'qwr', N'qwrqw', N'12', N'1A0FFDBA8FE313AADF89BDC50A8C1385147148BE', 1, CAST(N'2025-05-03T17:57:17.660' AS DateTime))
GO
INSERT [dbo].[Users] ([UserID], [FirstName], [LastName], [Email], [PasswordHash], [RoleID], [CreatedAt]) VALUES (24, N'asd', N'asd', N'asd@asd.asd', N'7C222FB2927D828AF22F592134E8932480637C0D', 1, CAST(N'2025-05-05T10:29:23.503' AS DateTime))
GO
INSERT [dbo].[Users] ([UserID], [FirstName], [LastName], [Email], [PasswordHash], [RoleID], [CreatedAt]) VALUES (25, N'Любитель', N'ОдинС', N'1conelove@mail.ru', N'B9EBCC2F57698F133D0116B05EDF0C94D6AF02E5', 1, CAST(N'2025-05-05T14:57:24.287' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[Users] OFF
GO
SET IDENTITY_INSERT [dbo].[VehicleCategories] ON 
GO
INSERT [dbo].[VehicleCategories] ([VehicleCategoryID], [VehicleCategory]) VALUES (1, N'Седан')
GO
INSERT [dbo].[VehicleCategories] ([VehicleCategoryID], [VehicleCategory]) VALUES (2, N'SUV')
GO
INSERT [dbo].[VehicleCategories] ([VehicleCategoryID], [VehicleCategory]) VALUES (3, N'Пикап')
GO
INSERT [dbo].[VehicleCategories] ([VehicleCategoryID], [VehicleCategory]) VALUES (4, N'Спорткар')
GO
INSERT [dbo].[VehicleCategories] ([VehicleCategoryID], [VehicleCategory]) VALUES (5, N'Минивен')
GO
INSERT [dbo].[VehicleCategories] ([VehicleCategoryID], [VehicleCategory]) VALUES (6, N'Фургон')
GO
INSERT [dbo].[VehicleCategories] ([VehicleCategoryID], [VehicleCategory]) VALUES (7, N'Электромобиль')
GO
INSERT [dbo].[VehicleCategories] ([VehicleCategoryID], [VehicleCategory]) VALUES (8, N'Грузовик')
GO
INSERT [dbo].[VehicleCategories] ([VehicleCategoryID], [VehicleCategory]) VALUES (9, N'Кабриолет')
GO
INSERT [dbo].[VehicleCategories] ([VehicleCategoryID], [VehicleCategory]) VALUES (10, N'Лимузин')
GO
SET IDENTITY_INSERT [dbo].[VehicleCategories] OFF
GO
SET IDENTITY_INSERT [dbo].[VehicleImages] ON 
GO
INSERT [dbo].[VehicleImages] ([ImageID], [VehicleID], [ImagePath]) VALUES (2, 1, N'https://i.ibb.co/KjntMH14/d2afce47ec84.png')
GO
INSERT [dbo].[VehicleImages] ([ImageID], [VehicleID], [ImagePath]) VALUES (3, 1, N'https://i.ibb.co/N2NM6R2Q/9aae5a843260.png')
GO
INSERT [dbo].[VehicleImages] ([ImageID], [VehicleID], [ImagePath]) VALUES (4, 2, N'https://i.ibb.co/ynj7vmqy/7d419f2b4eeb.png')
GO
INSERT [dbo].[VehicleImages] ([ImageID], [VehicleID], [ImagePath]) VALUES (5, 1, N'https://i.ibb.co/kVJgwhVM/8e6caec30459.png')
GO
INSERT [dbo].[VehicleImages] ([ImageID], [VehicleID], [ImagePath]) VALUES (6, 4, N'https://i.ibb.co/5xCVR5V0/0c96a5c8a0ac.png')
GO
INSERT [dbo].[VehicleImages] ([ImageID], [VehicleID], [ImagePath]) VALUES (7, 4, N'https://i.ibb.co/mC7xPWTc/147b31ef89f9.png')
GO
INSERT [dbo].[VehicleImages] ([ImageID], [VehicleID], [ImagePath]) VALUES (8, 2, N'https://i.ibb.co/0pwjJJFd/7e37aa4202b8.png')
GO
INSERT [dbo].[VehicleImages] ([ImageID], [VehicleID], [ImagePath]) VALUES (9, 5, N'https://i.ibb.co/kVbqfdRD/e0ff98a8050a.png')
GO
INSERT [dbo].[VehicleImages] ([ImageID], [VehicleID], [ImagePath]) VALUES (10, 6, N'https://i.ibb.co/XxTNZZFP/e9d90f416a0a.png')
GO
INSERT [dbo].[VehicleImages] ([ImageID], [VehicleID], [ImagePath]) VALUES (11, 7, N'https://i.ibb.co/YBLG6fPL/4c099c73bf3b.jpg')
GO
SET IDENTITY_INSERT [dbo].[VehicleImages] OFF
GO
SET IDENTITY_INSERT [dbo].[Vehicles] ON 
GO
INSERT [dbo].[Vehicles] ([VehicleID], [Make], [Model], [Year], [LicensePlate], [VehicleCategoryID], [DailyRate], [Available], [CreatedAt], [AvgRating]) VALUES (1, N'Toyota', N'Camry', 2022, N'ABC123', 1, CAST(50.00 AS Decimal(10, 2)), 1, CAST(N'2025-03-08T17:58:04.680' AS DateTime), CAST(5.00 AS Decimal(3, 2)))
GO
INSERT [dbo].[Vehicles] ([VehicleID], [Make], [Model], [Year], [LicensePlate], [VehicleCategoryID], [DailyRate], [Available], [CreatedAt], [AvgRating]) VALUES (2, N'Honda', N'CR-V Hybrid', 2025, N'XYZ789', 2, CAST(60.00 AS Decimal(10, 2)), 1, CAST(N'2025-03-08T17:58:04.680' AS DateTime), CAST(4.00 AS Decimal(3, 2)))
GO
INSERT [dbo].[Vehicles] ([VehicleID], [Make], [Model], [Year], [LicensePlate], [VehicleCategoryID], [DailyRate], [Available], [CreatedAt], [AvgRating]) VALUES (4, N'Polaris', N'Slingshot', 2021, N'999VHS', 4, CAST(300.00 AS Decimal(10, 2)), 1, CAST(N'2025-05-05T11:00:21.147' AS DateTime), NULL)
GO
INSERT [dbo].[Vehicles] ([VehicleID], [Make], [Model], [Year], [LicensePlate], [VehicleCategoryID], [DailyRate], [Available], [CreatedAt], [AvgRating]) VALUES (5, N'Ford', N'F-150', 2018, N'F150aa', 3, CAST(70.00 AS Decimal(10, 2)), 1, CAST(N'2025-05-05T16:00:51.917' AS DateTime), NULL)
GO
INSERT [dbo].[Vehicles] ([VehicleID], [Make], [Model], [Year], [LicensePlate], [VehicleCategoryID], [DailyRate], [Available], [CreatedAt], [AvgRating]) VALUES (6, N'Tesla', N'Model Y', 2024, N'96588EL', 7, CAST(55.00 AS Decimal(10, 2)), 1, CAST(N'2025-05-05T16:06:58.420' AS DateTime), NULL)
GO
INSERT [dbo].[Vehicles] ([VehicleID], [Make], [Model], [Year], [LicensePlate], [VehicleCategoryID], [DailyRate], [Available], [CreatedAt], [AvgRating]) VALUES (7, N'Hummer', N'H2 Limo', 2009, N'H209hh', 10, CAST(299.00 AS Decimal(10, 2)), 1, CAST(N'2025-05-05T16:24:14.267' AS DateTime), NULL)
GO
SET IDENTITY_INSERT [dbo].[Vehicles] OFF
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__Users__A9D105345A688893]    Script Date: 05.05.2025 17:35:35 ******/
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [UQ__Users__A9D105345A688893] UNIQUE NONCLUSTERED 
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__Vehicles__026BC15CD8052416]    Script Date: 05.05.2025 17:35:35 ******/
ALTER TABLE [dbo].[Vehicles] ADD  CONSTRAINT [UQ__Vehicles__026BC15CD8052416] UNIQUE NONCLUSTERED 
(
	[LicensePlate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Bookings] ADD  CONSTRAINT [DF__Bookings__Create__4CA06362]  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Payments] ADD  CONSTRAINT [DF__Payments__Create__4F7CD00D]  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Reviews] ADD  CONSTRAINT [DF__Reviews__Created__5070F446]  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF__Users__CreatedAt__3B75D760]  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Vehicles] ADD  CONSTRAINT [DF__Vehicles__Availa__440B1D61]  DEFAULT ((1)) FOR [Available]
GO
ALTER TABLE [dbo].[Vehicles] ADD  CONSTRAINT [DF__Vehicles__Create__44FF419A]  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Vehicles] ADD  CONSTRAINT [DF__Vehicles__AvgRat__01142BA1]  DEFAULT ((0)) FOR [AvgRating]
GO
ALTER TABLE [dbo].[Bookings]  WITH CHECK ADD  CONSTRAINT [FK__Bookings__CarID__49C3F6B7] FOREIGN KEY([VehicleID])
REFERENCES [dbo].[Vehicles] ([VehicleID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Bookings] CHECK CONSTRAINT [FK__Bookings__CarID__49C3F6B7]
GO
ALTER TABLE [dbo].[Bookings]  WITH CHECK ADD  CONSTRAINT [FK__Bookings__Insura__68487DD7] FOREIGN KEY([InsuranceID])
REFERENCES [dbo].[Insurances] ([InsuranceID])
GO
ALTER TABLE [dbo].[Bookings] CHECK CONSTRAINT [FK__Bookings__Insura__68487DD7]
GO
ALTER TABLE [dbo].[Bookings]  WITH CHECK ADD  CONSTRAINT [FK__Bookings__Renter__4AB81AF0] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Bookings] CHECK CONSTRAINT [FK__Bookings__Renter__4AB81AF0]
GO
ALTER TABLE [dbo].[Bookings]  WITH CHECK ADD  CONSTRAINT [FK__Bookings__Status__4BAC3F29] FOREIGN KEY([StatusID])
REFERENCES [dbo].[BookingStatuses] ([BookingStatusID])
GO
ALTER TABLE [dbo].[Bookings] CHECK CONSTRAINT [FK__Bookings__Status__4BAC3F29]
GO
ALTER TABLE [dbo].[Payments]  WITH CHECK ADD  CONSTRAINT [FK__Payments__Bookin__534D60F1] FOREIGN KEY([BookingID])
REFERENCES [dbo].[Bookings] ([BookingID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Payments] CHECK CONSTRAINT [FK__Payments__Bookin__534D60F1]
GO
ALTER TABLE [dbo].[Payments]  WITH CHECK ADD  CONSTRAINT [FK__Payments__Paymen__59FA5E80] FOREIGN KEY([PaymentMethodID])
REFERENCES [dbo].[PaymentMethods] ([PaymentMethodID])
GO
ALTER TABLE [dbo].[Payments] CHECK CONSTRAINT [FK__Payments__Paymen__59FA5E80]
GO
ALTER TABLE [dbo].[Payments]  WITH CHECK ADD  CONSTRAINT [FK__Payments__Paymen__5AEE82B9] FOREIGN KEY([PaymentStatusID])
REFERENCES [dbo].[PaymentStatuses] ([PaymentStatusID])
GO
ALTER TABLE [dbo].[Payments] CHECK CONSTRAINT [FK__Payments__Paymen__5AEE82B9]
GO
ALTER TABLE [dbo].[Reviews]  WITH CHECK ADD  CONSTRAINT [FK__Reviews__UserID__5BE2A6F2] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[Reviews] CHECK CONSTRAINT [FK__Reviews__UserID__5BE2A6F2]
GO
ALTER TABLE [dbo].[Reviews]  WITH CHECK ADD  CONSTRAINT [FK_Reviews_Vehicles] FOREIGN KEY([VehicleID])
REFERENCES [dbo].[Vehicles] ([VehicleID])
GO
ALTER TABLE [dbo].[Reviews] CHECK CONSTRAINT [FK_Reviews_Vehicles]
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD  CONSTRAINT [FK_Users_Roles] FOREIGN KEY([RoleID])
REFERENCES [dbo].[Roles] ([RoleID])
GO
ALTER TABLE [dbo].[Users] CHECK CONSTRAINT [FK_Users_Roles]
GO
ALTER TABLE [dbo].[VehicleImages]  WITH CHECK ADD  CONSTRAINT [FK__VehicleIm__Vehic__5EBF139D] FOREIGN KEY([VehicleID])
REFERENCES [dbo].[Vehicles] ([VehicleID])
GO
ALTER TABLE [dbo].[VehicleImages] CHECK CONSTRAINT [FK__VehicleIm__Vehic__5EBF139D]
GO
ALTER TABLE [dbo].[Vehicles]  WITH CHECK ADD  CONSTRAINT [FK__Vehicles__Catego__4316F928] FOREIGN KEY([VehicleCategoryID])
REFERENCES [dbo].[VehicleCategories] ([VehicleCategoryID])
GO
ALTER TABLE [dbo].[Vehicles] CHECK CONSTRAINT [FK__Vehicles__Catego__4316F928]
GO
ALTER TABLE [dbo].[Bookings]  WITH CHECK ADD  CONSTRAINT [CK_Bookings_Dates] CHECK  (([EndDate]>[StartDate]))
GO
ALTER TABLE [dbo].[Bookings] CHECK CONSTRAINT [CK_Bookings_Dates]
GO
ALTER TABLE [dbo].[Reviews]  WITH CHECK ADD  CONSTRAINT [CK__Reviews__Rating__619B8048] CHECK  (([Rating]>=(1) AND [Rating]<=(5)))
GO
ALTER TABLE [dbo].[Reviews] CHECK CONSTRAINT [CK__Reviews__Rating__619B8048]
GO
ALTER TABLE [dbo].[Vehicles]  WITH CHECK ADD  CONSTRAINT [CK__Vehicles__Year__4222D4EF] CHECK  (([Year]>=(1990) AND [Year]<=datepart(year,getdate())))
GO
ALTER TABLE [dbo].[Vehicles] CHECK CONSTRAINT [CK__Vehicles__Year__4222D4EF]
GO
/****** Object:  Trigger [dbo].[trg_UpdateVehicleRating]    Script Date: 05.05.2025 17:35:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TRIGGER [dbo].[trg_UpdateVehicleRating]
ON [dbo].[Reviews]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    UPDATE Vehicles
    SET AvgRating = (SELECT AVG(CAST(Rating AS DECIMAL(3,2))) FROM Reviews WHERE VehicleID = inserted.VehicleID)
    FROM inserted
    WHERE Vehicles.VehicleID = inserted.VehicleID;
END;
GO
ALTER TABLE [dbo].[Reviews] ENABLE TRIGGER [trg_UpdateVehicleRating]
GO
ALTER DATABASE [DB_RentACar] SET  READ_WRITE 
GO
