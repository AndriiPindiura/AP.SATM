/*********************** CREATING ENTRY CONFIG *****************************/



/****** Object:  Table [dbo].[TruckHunterEntries]    Script Date: 07/05/2013 11:06:18 ******/
/*
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TruckHunterEntries]') AND type in (N'U'))
DROP TABLE [dbo].[TruckHunterEntries]
GO
*/

/****** Object:  Table [dbo].[TruckHunterEntries]    Script Date: 07/05/2013 11:06:20 ******/
/*
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TruckHunterEntries](
	[ID] [int] NULL,
	[Name] [ntext] NULL,
	[InCam] [int] NULL,
	[OutCam] [int] NULL,
	[UpCam] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
*/


/*********************** CREATING RAYS CONFIG *****************************/



/****** Object:  Table [dbo].[TruckHunterRays]    Script Date: 07/05/2013 11:07:47 ******/
/*
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TruckHunterRays]') AND type in (N'U'))
DROP TABLE [dbo].[TruckHunterRays]
GO
*/


/****** Object:  Table [dbo].[TruckHunterRays]    Script Date: 07/05/2013 11:07:48 ******/
/*
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TruckHunterRays](
	[Entry] [int] NOT NULL,
	[Ray] [int] NOT NULL,
	[vector] [int] NOT NULL
) ON [PRIMARY]

GO
*/




/*********************** CREATING 1C TABLE *****************************/


IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__PROTOCOL1C__Date__59C55456]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[PROTOCOL1C] DROP CONSTRAINT [DF__PROTOCOL1C__Date__59C55456]
END

GO


/****** Object:  Table [dbo].[PROTOCOL1C]    Script Date: 07/05/2013 11:04:12 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PROTOCOL1C]') AND type in (N'U'))
DROP TABLE [dbo].[PROTOCOL1C]
GO


/****** Object:  Table [dbo].[PROTOCOL1C]    Script Date: 07/05/2013 11:04:13 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PROTOCOL1C](
	[owner] [nvarchar](50) NOT NULL DEFAULT 'VS-JLE',
	[ID] [nvarchar](50) NOT NULL,
	[TruckID] [nvarchar](50) NULL,
	[[Date] [datetime] NOT NULL DEFAULT (getdate()),
	[Entry] [int] NULL,
	[Culture] [nvarchar](50) NULL,
	[WHO] [nvarchar](50) NULL,
	[pk] [uniqueidentifier] NOT NULL DEFAULT (newid())
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

/**ALTER TABLE [dbo].[PROTOCOL1C] ADD  DEFAULT (getdate()) FOR [Date]
GO**/

/************************** ADD LOGIN *******************************/

/****** Object:  Login [hunter]    Script Date: 07/05/2013 11:10:11 ******/
/*
IF  EXISTS (SELECT * FROM sys.server_principals WHERE name = N'hunter')
DROP LOGIN [hunter]
GO
*/

/* For security reasons the login is created disabled and with a random password. */
/****** Object:  Login [hunter]    Script Date: 07/05/2013 11:10:11 ******/
CREATE LOGIN [video] WITH PASSWORD=N'P@ssw0rd1', DEFAULT_DATABASE=[intellect], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
GO