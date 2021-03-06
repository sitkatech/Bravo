/****** Object:  Table [dbo].[RunBucketRuns]    Script Date: 4/8/2021 10:25:44 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RunBucketRuns](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RunBucketId] [int] NOT NULL,
	[RunId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.RunBucketRuns] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[RunBucketRuns]  WITH CHECK ADD  CONSTRAINT [FK_RunBuckets_RunBucketRuns] FOREIGN KEY([RunBucketId])
REFERENCES [dbo].[RunBuckets] ([Id])
GO
ALTER TABLE [dbo].[RunBucketRuns] CHECK CONSTRAINT [FK_RunBuckets_RunBucketRuns]
GO
ALTER TABLE [dbo].[RunBucketRuns]  WITH CHECK ADD  CONSTRAINT [FK_Runs_RunBucketRuns] FOREIGN KEY([RunId])
REFERENCES [dbo].[Runs] ([Id])
GO
ALTER TABLE [dbo].[RunBucketRuns] CHECK CONSTRAINT [FK_Runs_RunBucketRuns]
GO
