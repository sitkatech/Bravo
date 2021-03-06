/****** Object:  Table [dbo].[ScenarioFiles]    Script Date: 4/8/2021 10:21:48 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ScenarioFiles](
	[Id] [int] NOT NULL,
	[ScenarioId] [int] NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[Description] [nvarchar](512) NULL,
	[Required] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.ScenarioFiles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Index [UC_ScenarioId_Name]    Script Date: 4/8/2021 10:21:48 AM ******/
ALTER TABLE [dbo].[ScenarioFiles] ADD  CONSTRAINT [UC_ScenarioId_Name] UNIQUE NONCLUSTERED 
(
	[ScenarioId] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ScenarioFiles]  WITH CHECK ADD FOREIGN KEY([ScenarioId])
REFERENCES [dbo].[Scenarios] ([Id])
GO
