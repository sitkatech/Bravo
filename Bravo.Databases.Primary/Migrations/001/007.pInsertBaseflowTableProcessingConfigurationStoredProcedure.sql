/****** Object:  StoredProcedure [dbo].[pInsertBaseflowTableProcessingConfiguration]    Script Date: 4/8/2021 10:25:44 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[pInsertBaseflowTableProcessingConfiguration]
	@baseflowTableIndicatorRegexPattern varchar(200),
	@segmentColumnNum int,
	@flowToAquiferColumnNum int,
	@reachColumnNum int
AS
BEGIN

	SET NOCOUNT ON;
	
	insert into dbo.BaseflowTableProcessingConfigurations(BaseflowTableIndicatorRegexPattern, SegmentColumnNum, FlowToAquiferColumnNum, ReachColumnNum)
	values (@baseflowTableIndicatorRegexPattern, @segmentColumnNum, @flowToAquiferColumnNum, @reachColumnNum)

END
GO
