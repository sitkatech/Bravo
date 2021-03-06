/****** Object:  Table [dbo].[ModelScenarios]    Script Date: 4/8/2021 10:25:44 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ModelScenarios](
	[ModelId] [int] NOT NULL,
	[ScenarioId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.ModelScenarios] PRIMARY KEY CLUSTERED 
(
	[ModelId] ASC,
	[ScenarioId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ModelScenarios]  WITH CHECK ADD  CONSTRAINT [FK_ModelScenarios_Models] FOREIGN KEY([ModelId])
REFERENCES [dbo].[Models] ([Id])
GO
ALTER TABLE [dbo].[ModelScenarios] CHECK CONSTRAINT [FK_ModelScenarios_Models]
GO
ALTER TABLE [dbo].[ModelScenarios]  WITH CHECK ADD  CONSTRAINT [FK_ModelScenarios_Scenarios] FOREIGN KEY([ScenarioId])
REFERENCES [dbo].[Scenarios] ([Id])
GO
ALTER TABLE [dbo].[ModelScenarios] CHECK CONSTRAINT [FK_ModelScenarios_Scenarios]
GO

INSERT INTO [dbo].[ModelScenarios] (ModelID, ScenarioID) values (1,1)
GO
