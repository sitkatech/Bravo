/****** Object:  StoredProcedure [dbo].[RetrieveModel]    Script Date: 4/8/2021 10:25:44 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RetrieveModel]
	@modelName NVARCHAR(256),
    @imageName NVARCHAR(256) OUTPUT,
	@startDateTime DATETIME OUTPUT,
	@modflowExeName VARCHAR(50) OUTPUT,
	@namFileName VARCHAR(50) OUTPUT,
	@runFileName VARCHAR(50) OUTPUT,
	@mapRunFileName VARCHAR(50) OUTPUT,
	@mapDrawdownFileName VARCHAR(50) OUTPUT,
	@mapSettings VARCHAR(1024) OUTPUT,
	@mapModelArea VARCHAR(MAX) OUTPUT,
	@zoneBudgetExeName VARCHAR(50) OUTPUT,
	@isDoubleSizeHeatMapOutput BIT OUTPUT,
	@allowablePercentDiscrepancy FLOAT OUTPUT,
	@mapInputZone VARCHAR(MAX) OUTPUT,
	@mapOutputZone VARCHAR(MAX) OUTPUT,
	@numberOfStressPeriods int OUTPUT,
	@canalData varchar(max) OUTPUT,
	@modPathExeName VARCHAR(50) OUTPUT,
	@simulationFileName VARCHAR(50) OUTPUT,
	@listFileName VARCHAR(50) OUTPUT,
	@baseflowTableProcessingConfigurationID int OUTPUT
AS
BEGIN

	SET NOCOUNT ON;
SELECT @imageName = i.[Name],
       @startDateTime = m.StartDateTime,
	   @modflowExeName = m.ModflowExeName,
	   @namFileName = m.NamFileName,
	   @runFileName = m.RunFileName,
	   @mapRunFileName = m.MapRunFileName,
	   @mapDrawdownFileName = m.MapDrawdownFileName,
	   @mapSettings = m.MapSettings,
	   @mapModelArea = m.MapModelArea,
	   @zoneBudgetExeName = m.ZoneBudgetExeName,
	   @isDoubleSizeHeatMapOutput = m.IsDoubleSizeHeatMapOutput,
	   @allowablePercentDiscrepancy = m.AllowablePercentDiscrepancy,
	   @mapInputZone = m.InputZoneData,
	   @mapOutputZone = m.OutputZoneData,
	   @numberOfStressPeriods = m.NumberOfStressPeriods,
	   @canalData = m.CanalData,
	   @modPathExeName = m.ModPathExeName,
	   @simulationFileName = m.SimulationFileName,
	   @listFileName = m.ListFileName,
	   @baseflowTableProcessingConfigurationID = m.BaseflowTableProcessingConfigurationID
FROM Images i INNER JOIN 
     Models m ON i.Id = m.ImageId
WHERE @modelName = m.[Name]
END
GO
