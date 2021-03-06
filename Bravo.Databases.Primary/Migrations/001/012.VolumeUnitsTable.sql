/****** Object:  Table [dbo].[VolumeUnits]    Script Date: 4/8/2021 10:21:48 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VolumeUnits](
	[Id] [int] NOT NULL,
	[VolumeType] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[VolumeUnits] ([Id], [VolumeType]) VALUES (1, N'Acre Feet')
INSERT [dbo].[VolumeUnits] ([Id], [VolumeType]) VALUES (2, N'Cubic Feet')
INSERT [dbo].[VolumeUnits] ([Id], [VolumeType]) VALUES (4, N'Cubic Meter')
INSERT [dbo].[VolumeUnits] ([Id], [VolumeType]) VALUES (3, N'Cubic Yard')
INSERT [dbo].[VolumeUnits] ([Id], [VolumeType]) VALUES (0, N'Unknown')
INSERT [dbo].[VolumeUnits] ([Id], [VolumeType]) VALUES (5, N'US Gallon')
INSERT [dbo].[VolumeUnits] ([Id], [VolumeType]) VALUES (6, N'US Gallons in Millions')
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__VolumeUn__87C85F0A1034FD02]    Script Date: 4/8/2021 10:21:48 AM ******/
ALTER TABLE [dbo].[VolumeUnits] ADD UNIQUE NONCLUSTERED 
(
	[VolumeType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
