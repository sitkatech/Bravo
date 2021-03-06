/****** Object:  Table [dbo].[Models]    Script Date: 4/8/2021 10:25:44 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Models](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[ImageId] [int] NOT NULL,
	[StartDateTime] [datetime] NOT NULL,
	[NamFileName] [varchar](50) NULL,
	[RunFileName] [varchar](50) NULL,
	[ModflowExeName] [varchar](50) NULL,
	[AllowablePercentDiscrepancy] [float] NULL,
	[MapSettings] [varchar](1024) NULL,
	[MapModelArea] [varchar](max) NULL,
	[MapRunFileName] [varchar](50) NULL,
	[IsDoubleSizeHeatMapOutput] [bit] NOT NULL,
	[InputZoneData] [varchar](max) NULL,
	[NumberOfStressPeriods] [int] NOT NULL,
	[CanalData] [varchar](max) NULL,
	[ZoneBudgetExeName] [varchar](50) NULL,
	[ModpathExeName] [varchar](50) NULL,
	[SimulationFileName] [varchar](50) NULL,
	[BuddyGroup] [nvarchar](128) NULL,
	[MapDrawdownFileName] [varchar](50) NULL,
	[ListFileName] [varchar](50) NULL,
	[OutputZoneData] [varchar](max) NULL,
	[BaseflowTableProcessingConfigurationID] [int] NULL,
 CONSTRAINT [PK_dbo.Models] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[Models] ADD  DEFAULT ((600)) FOR [NumberOfStressPeriods]
GO
ALTER TABLE [dbo].[Models]  WITH CHECK ADD FOREIGN KEY([ImageId])
REFERENCES [dbo].[Images] ([Id])
GO
ALTER TABLE [dbo].[Models]  WITH CHECK ADD  CONSTRAINT [FK_Models_BaseflowTableProcessingConfigurations_BaseflowTableProcessingConfigurationID] FOREIGN KEY([BaseflowTableProcessingConfigurationID])
REFERENCES [dbo].[BaseflowTableProcessingConfigurations] ([BaseflowTableProcessingConfigurationID])
GO
ALTER TABLE [dbo].[Models] CHECK CONSTRAINT [FK_Models_BaseflowTableProcessingConfigurations_BaseflowTableProcessingConfigurationID]
GO

INSERT [dbo].[Models] ([Id], [Name], [ImageId], [StartDateTime], [NamFileName], [RunFileName], [ModflowExeName], [AllowablePercentDiscrepancy], [MapSettings], [MapModelArea], [MapRunFileName], [IsDoubleSizeHeatMapOutput], [InputZoneData], [ZoneBudgetExeName], [NumberOfStressPeriods], [CanalData], [ModPathExeName], [SimulationFileName], [BuddyGroup], [MapDrawdownFileName], [ListFileName], [OutputZoneData], [BaseflowTableProcessingConfigurationID]) VALUES (1, N'ModFlow 6 Structured', 1, CAST(N'2020-05-27T00:00:00.000' AS DateTime), N'mfsim.nam', N'test1tr.lst', N'mf6.exe', NULL, N'{zoom:11,center:{lat:30.44,lng:-99.6},mapTypeId:"terrain"}', N'[{lat:30.359409,lng:-99.687445},{lat:30.359367,lng:-99.670672},{lat:30.359322,lng:-99.653898},{lat:30.359276,lng:-99.637125},{lat:30.359227,lng:-99.620351},{lat:30.359177,lng:-99.603578},{lat:30.359124,lng:-99.586804},{lat:30.359068,lng:-99.570031},{lat:30.359011,lng:-99.553258},{lat:30.358952,lng:-99.536484},{lat:30.358890,lng:-99.519711},{lat:30.358826,lng:-99.502938},{lat:30.358760,lng:-99.486164},{lat:30.358691,lng:-99.469391},{lat:30.358621,lng:-99.452618},{lat:30.373109,lng:-99.452535},{lat:30.387597,lng:-99.452452},{lat:30.402084,lng:-99.452369},{lat:30.416572,lng:-99.452286},{lat:30.431060,lng:-99.452203},{lat:30.445547,lng:-99.452119},{lat:30.460035,lng:-99.452036},{lat:30.474522,lng:-99.451953},{lat:30.489010,lng:-99.451870},{lat:30.503497,lng:-99.451787},{lat:30.503567,lng:-99.468586},{lat:30.503636,lng:-99.485384},{lat:30.503702,lng:-99.502183},{lat:30.503766,lng:-99.518982},{lat:30.503828,lng:-99.535781},{lat:30.503888,lng:-99.552580},{lat:30.503945,lng:-99.569378},{lat:30.504000,lng:-99.586177},{lat:30.504053,lng:-99.602976},{lat:30.504104,lng:-99.619775},{lat:30.504153,lng:-99.636574},{lat:30.504199,lng:-99.653373},{lat:30.504244,lng:-99.670172},{lat:30.504286,lng:-99.686971},{lat:30.504326,lng:-99.703770},{lat:30.489838,lng:-99.703815},{lat:30.475351,lng:-99.703860},{lat:30.460863,lng:-99.703905},{lat:30.446376,lng:-99.703950},{lat:30.431888,lng:-99.703995},{lat:30.417400,lng:-99.704039},{lat:30.402913,lng:-99.704084},{lat:30.388425,lng:-99.704129},{lat:30.373937,lng:-99.704174},{lat:30.359448,lng:-99.704219},{lat:30.359409,lng:-99.687445}]', N'test1tr.hds', 0, NULL, N'zbud6.exe', 2, NULL, NULL, NULL, NULL, NULL, N'test1tr.lst', NULL, 5)

GO