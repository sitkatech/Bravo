/****** Object:  Table [dbo].[RunGeographies]    Script Date: 4/8/2021 10:21:48 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RunGeographies](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RunId] [int] NOT NULL,
	[StressPeriod] [int] NOT NULL,
	[Color] [nchar](7) NOT NULL,
	[Geography] [geography] NULL,
 CONSTRAINT [PK_dbo.RunGeographies] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[RunGeographies]  WITH CHECK ADD  CONSTRAINT [FK_RunGeographies_Run] FOREIGN KEY([RunId])
REFERENCES [dbo].[Runs] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RunGeographies] CHECK CONSTRAINT [FK_RunGeographies_Run]
GO
