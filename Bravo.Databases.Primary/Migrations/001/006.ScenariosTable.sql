/****** Object:  Table [dbo].[Scenarios]    Script Date: 4/8/2021 10:21:48 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Scenarios](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[InputControlType] [int] NOT NULL,
	[ShouldSwitchSign] [bit] NOT NULL,
	[InputImageId] [int] NULL,
 CONSTRAINT [PK_dbo.Scenarios] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[Scenarios] ([Id], [Name], [InputControlType], [ShouldSwitchSign], [InputImageId]) VALUES (1, N'Add a Well', 2, 0, NULL)
INSERT [dbo].[Scenarios] ([Id], [Name], [InputControlType], [ShouldSwitchSign], [InputImageId]) VALUES (2, N'Remove a Well', 2, 0, NULL)
INSERT [dbo].[Scenarios] ([Id], [Name], [InputControlType], [ShouldSwitchSign], [InputImageId]) VALUES (3, N'Move a Well', 1, 0, NULL)
INSERT [dbo].[Scenarios] ([Id], [Name], [InputControlType], [ShouldSwitchSign], [InputImageId]) VALUES (4, N'Canal Recharge', 1, 0, NULL)
INSERT [dbo].[Scenarios] ([Id], [Name], [InputControlType], [ShouldSwitchSign], [InputImageId]) VALUES (5, N'Adjust Irrigation', 3, 0, NULL)
INSERT [dbo].[Scenarios] ([Id], [Name], [InputControlType], [ShouldSwitchSign], [InputImageId]) VALUES (6, N'Retire Additional Wells', 2, 1, NULL)
INSERT [dbo].[Scenarios] ([Id], [Name], [InputControlType], [ShouldSwitchSign], [InputImageId]) VALUES (7, N'Specify Pumping', 1, 1, NULL)
INSERT [dbo].[Scenarios] ([Id], [Name], [InputControlType], [ShouldSwitchSign], [InputImageId]) VALUES (8, N'ASR Wells', 1, 0, NULL)
INSERT [dbo].[Scenarios] ([Id], [Name], [InputControlType], [ShouldSwitchSign], [InputImageId]) VALUES (9, N'Adjust Pumping', 3, 0, NULL)
INSERT [dbo].[Scenarios] ([Id], [Name], [InputControlType], [ShouldSwitchSign], [InputImageId]) VALUES (10, N'Particle Trace', 4, 0, NULL)
ALTER TABLE [dbo].[Scenarios] ADD  DEFAULT ((0)) FOR [InputControlType]
GO
ALTER TABLE [dbo].[Scenarios] ADD  DEFAULT ((0)) FOR [ShouldSwitchSign]
GO
ALTER TABLE [dbo].[Scenarios]  WITH CHECK ADD FOREIGN KEY([InputImageId])
REFERENCES [dbo].[Images] ([Id])
GO
