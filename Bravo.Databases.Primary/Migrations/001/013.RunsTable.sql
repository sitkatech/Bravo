/****** Object:  Table [dbo].[Runs]    Script Date: 4/8/2021 10:25:44 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Runs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[FileStorageLocator] [nvarchar](50) NOT NULL,
	[ImageId] [int] NULL,
	[ModelId] [int] NOT NULL,
	[ScenarioId] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[InputFileName] [varchar](256) NULL,
	[ProcessingStartDate] [datetime] NULL,
	[ProcessingEndDate] [datetime] NULL,
	[ShouldCreateMaps] [bit] NULL,
	[Output] [varchar](max) NULL,
	[RestartCount] [int] NOT NULL,
	[InputVolumeUnit] [int] NOT NULL,
	[OutputVolumeUnit] [int] NOT NULL,
	[IsDifferential] [bit] NOT NULL,
	[Description] [varchar](max) NULL,
 CONSTRAINT [PK_dbo.Runs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[Runs] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Runs] ADD  DEFAULT ((0)) FOR [ShouldCreateMaps]
GO
ALTER TABLE [dbo].[Runs] ADD  DEFAULT ((0)) FOR [RestartCount]
GO
ALTER TABLE [dbo].[Runs] ADD  CONSTRAINT [DF__Runs__IsDifferential]  DEFAULT ((1)) FOR [IsDifferential]
GO
ALTER TABLE [dbo].[Runs]  WITH CHECK ADD FOREIGN KEY([ImageId])
REFERENCES [dbo].[Images] ([Id])
GO
ALTER TABLE [dbo].[Runs]  WITH CHECK ADD FOREIGN KEY([ModelId])
REFERENCES [dbo].[Models] ([Id])
GO
ALTER TABLE [dbo].[Runs]  WITH CHECK ADD FOREIGN KEY([ScenarioId])
REFERENCES [dbo].[Scenarios] ([Id])
GO
ALTER TABLE [dbo].[Runs]  WITH CHECK ADD  CONSTRAINT [FK_Runs_InputVolumeUnit] FOREIGN KEY([InputVolumeUnit])
REFERENCES [dbo].[VolumeUnits] ([Id])
GO
ALTER TABLE [dbo].[Runs] CHECK CONSTRAINT [FK_Runs_InputVolumeUnit]
GO
ALTER TABLE [dbo].[Runs]  WITH CHECK ADD  CONSTRAINT [FK_Runs_OutputVolumeUnit] FOREIGN KEY([OutputVolumeUnit])
REFERENCES [dbo].[VolumeUnits] ([Id])
GO
ALTER TABLE [dbo].[Runs] CHECK CONSTRAINT [FK_Runs_OutputVolumeUnit]
GO
