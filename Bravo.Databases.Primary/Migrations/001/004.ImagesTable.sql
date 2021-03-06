/****** Object:  Table [dbo].[Images]    Script Date: 4/8/2021 10:25:44 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Images](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[Server] [nvarchar](256) NOT NULL,
	[IsLinux] [bit] NOT NULL,
	[CpuCoreCount] [int] NULL,
	[Memory] [decimal](4, 1) NULL,
 CONSTRAINT [PK_dbo.Images] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Images] ADD  DEFAULT ((0)) FOR [IsLinux]

INSERT INTO [dbo].[Images] (Id, [Name], [Server])
values (1, 'TestImage', 'tcp://localhost:2375')
GO
