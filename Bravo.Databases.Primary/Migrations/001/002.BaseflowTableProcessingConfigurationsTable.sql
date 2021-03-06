/****** Object:  Table [dbo].[BaseflowTableProcessingConfigurations]    Script Date: 4/8/2021 10:21:48 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BaseflowTableProcessingConfigurations](
	[BaseflowTableProcessingConfigurationID] [int] IDENTITY(1,1) NOT NULL,
	[BaseflowTableIndicatorRegexPattern] [varchar](200) NOT NULL,
	[SegmentColumnNum] [int] NOT NULL,
	[FlowToAquiferColumnNum] [int] NOT NULL,
	[ReachColumnNum] [int] NULL,
 CONSTRAINT [PK_BaseflowProcessingConfiguration_BaseflowProcessingConfigurationID] PRIMARY KEY CLUSTERED 
(
	[BaseflowTableProcessingConfigurationID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[BaseflowTableProcessingConfigurations] ON 

INSERT [dbo].[BaseflowTableProcessingConfigurations] ([BaseflowTableProcessingConfigurationID], [BaseflowTableIndicatorRegexPattern], [SegmentColumnNum], [FlowToAquiferColumnNum], [ReachColumnNum]) VALUES (1, N'^\s+STREAM LISTING\s+PERIOD\s+[0-9]+\s+STEP\s+[0-9]+$', 4, 7, 5)
INSERT [dbo].[BaseflowTableProcessingConfigurations] ([BaseflowTableProcessingConfigurationID], [BaseflowTableIndicatorRegexPattern], [SegmentColumnNum], [FlowToAquiferColumnNum], [ReachColumnNum]) VALUES (2, N'^\s+STREAM LISTING\s+PERIOD\s+[0-9]+\s+STEP\s+[0-9]+$', 2, 5, 3)
INSERT [dbo].[BaseflowTableProcessingConfigurations] ([BaseflowTableProcessingConfigurationID], [BaseflowTableIndicatorRegexPattern], [SegmentColumnNum], [FlowToAquiferColumnNum], [ReachColumnNum]) VALUES (3, N'^\s+SFR \(SFR-\d+\) FLOWS\s+PERIOD\s+[0-9]+\s+STEP\s+[0-9]+$', 1, 7, NULL)
INSERT [dbo].[BaseflowTableProcessingConfigurations] ([BaseflowTableProcessingConfigurationID], [BaseflowTableIndicatorRegexPattern], [SegmentColumnNum], [FlowToAquiferColumnNum], [ReachColumnNum]) VALUES (4, N'^\s+SFR \(STREAMS_SFR\) FLOWS\s+PERIOD\s+[0-9]+\s+STEP\s+[0-9]+$', 2, 8, NULL)
INSERT [dbo].[BaseflowTableProcessingConfigurations] ([BaseflowTableProcessingConfigurationID], [BaseflowTableIndicatorRegexPattern], [SegmentColumnNum], [FlowToAquiferColumnNum], [ReachColumnNum]) VALUES (5, N'^\s+SFR-\d+ PACKAGE - SUMMARY OF FLOWS FOR EACH CONTROL VOLUME\s+PERIOD\s+[0-9]+\s+STEP\s+[0-9]+$', 1, 5, NULL)
SET IDENTITY_INSERT [dbo].[BaseflowTableProcessingConfigurations] OFF
