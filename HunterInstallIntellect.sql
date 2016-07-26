USE [intellect]
GO


/****** Object:  Table [dbo].[PROTOCOL1C]    Script Date: 16.06.2015 15:14:48 ******/
/*
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PROTOCOL1C]') AND type in (N'U'))
DROP TABLE [dbo].[PROTOCOL1C]
GO
*/
/****** Object:  Table [dbo].[PROTOCOL1C]    Script Date: 16.06.2015 15:14:48 ******/
/*
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
*/
/*********** CHANGE VS-KRE *****************/
/*********** CHANGE VS-KRE *****************/
/*********** CHANGE VS-KRE *****************/
/*********** CHANGE VS-KRE *****************/
/*********** CHANGE VS-KRE *****************/
/*********** CHANGE VS-KRE *****************/
/*
CREATE TABLE [dbo].[PROTOCOL1C](
	[owner] [nvarchar](50) NOT NULL DEFAULT ('VS-KRE'),
	[ID] [nvarchar](50) NOT NULL,
	[TruckID] [nvarchar](50) NULL,
	[Date] [datetime] NOT NULL DEFAULT (getdate()),
	[Entry] [int] NULL,
	[Culture] [nvarchar](50) NULL,
	[WHO] [nvarchar](50) NULL,
	[pk] [uniqueidentifier] NOT NULL DEFAULT (newid())
) ON [PRIMARY]

GO
*/
/****** Object:  Table [dbo].[SATMSync]    Script Date: 16.06.2015 15:41:51 ******/
/*
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SATMSync]') AND type in (N'U'))
DROP TABLE [dbo].[SATMSync]
GO
*/
/****** Object:  Table [dbo].[SATMSync]    Script Date: 16.06.2015 15:41:51 ******/
/*
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SATMSync](
	[LastProtocol] [datetime] NOT NULL,
	[LastProtocol1C] [datetime] NOT NULL,
) ON [PRIMARY]

GO

*/

EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPaneCount' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ViewProtocol'

GO

EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPane1' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ViewProtocol'

GO

/****** Object:  View [dbo].[ViewProtocol]    Script Date: 16.06.2015 15:10:15 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ViewProtocol]') AND type in (N'V'))
DROP VIEW [dbo].[ViewProtocol]
GO

/****** Object:  View [dbo].[ViewProtocol]    Script Date: 16.06.2015 15:10:15 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[ViewProtocol]
AS
SELECT        objtype, objid, action, date, owner, pk
FROM            dbo.PROTOCOL
WHERE        (date >
                             (SELECT        TOP (1) LastProtocol
                               FROM            dbo.SATMSync) - 2) AND (objtype = 'GRAY')

GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "PROTOCOL"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 240
               Right = 247
            End
            DisplayFlags = 280
            TopColumn = 4
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ViewProtocol'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ViewProtocol'
GO


EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPaneCount' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ViewProtocol1C'

GO

EXEC sys.sp_dropextendedproperty @name=N'MS_DiagramPane1' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ViewProtocol1C'

GO

/****** Object:  View [dbo].[ViewProtocol1C]    Script Date: 16.06.2015 15:12:45 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ViewProtocol1C]') AND type in (N'V'))
DROP VIEW [dbo].[ViewProtocol1C]
GO

/****** Object:  View [dbo].[ViewProtocol1C]    Script Date: 16.06.2015 15:12:45 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[ViewProtocol1C]
AS
SELECT        owner, ID, TruckID, Date, Entry, Culture, WHO, pk
FROM            dbo.PROTOCOL1C
WHERE        (Date >
                             (SELECT        TOP (1) LastProtocol1C
                               FROM            dbo.SATMSync) - 2)

GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "PROTOCOL1C"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 208
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ViewProtocol1C'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ViewProtocol1C'
GO




/************************** ADD LOGIN *******************************/

/****** Object:  Login [hunter]    Script Date: 07/05/2013 11:10:11 ******/

IF  EXISTS (SELECT * FROM sys.server_principals WHERE name = N'video')
DROP LOGIN [video]
GO

USE [intellect]
GO


/****** Object:  User [video]    Script Date: 16.06.2015 21:22:38 ******/
IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'video')
DROP USER [video]
GO

/* For security reasons the login is created disabled and with a random password. */
/****** Object:  Login [hunter]    Script Date: 07/05/2013 11:10:11 ******/

CREATE LOGIN [video] WITH PASSWORD=N'P@ssw0rd1', DEFAULT_DATABASE=[intellect], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
GO
USE [intellect]
GO
CREATE USER [video] FOR LOGIN [video]
GO
USE [intellect]
GO
EXEC sp_addrolemember N'db_datareader', N'video'
GO
USE [intellect]
GO
EXEC sp_addrolemember N'db_datawriter', N'video'
GO