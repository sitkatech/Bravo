/****** Object:  StoredProcedure [dbo].[UpsertModel]    Script Date: 4/26/2021 1:15:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[UpsertModel]
	@imageName NVARCHAR(256),
	@modelName NVARCHAR(256),
	@startDateTime DATETIME,
	@modflowExeName VARCHAR(50),
	@namFileName VARCHAR(50),
	@runFileName VARCHAR(50),
	@mapRunFileName VARCHAR(50),
	@mapDrawdownFileName VARCHAR(50) OUTPUT,
	@mapSettings VARCHAR(1024),
	@mapModelArea VARCHAR(MAX),
	@zoneBudgetExeName VARCHAR(50),
	@isDoubleSizeHeatMapOutput BIT,
	@allowablePercentDiscrepancy FLOAT,
	@scenarios dbo.ScenariosList READONLY,
	@inputZoneData varchar(MAX),
	@outputZoneData varchar(MAX),
	@numberOfStressPeriods int,
	@canalData varchar(max),
	@modPathExeName VARCHAR(50),
	@simulationFileName VARCHAR(50),
	@listFileName VARCHAR(50),
	@baseflowTableProcessingConfigurationID int,
	@customStartDatesForStressPeriods dbo.DateList READONLY
AS
BEGIN

DECLARE @imageId INT;
DECLARE @modelId INT;

	SET NOCOUNT ON;

    SELECT @imageId = Id FROM Images WHERE [Name] = @imageName
   IF @imageId is null
	BEGIN
		declare @ID table (ID int);
		insert Images(Id, [Name], [Server])
		output inserted.Id into @ID
		values ((select (max(id) + 1) from Images),@imageName,(select top 1 [server] from Images));
		SELECT @imageId = ID FROM @ID;
	END

	MERGE Models as Target
		USING (select @modelName as [Name]) as Source
		ON Target.Name = Source.Name
		WHEN MATCHED THEN
			UPDATE SET StartDateTime = @startDateTime,
			           ModflowExeName = @modflowExeName,
					   NamFileName = @namFileName,
					   RunFileName = @runFileName,
					   AllowablePercentDiscrepancy = @allowablePercentDiscrepancy,
					   MapRunFileName = @mapRunFileName,
					   MapDrawdownFileName = @mapDrawdownFileName,
					   IsDoubleSizeHeatMapOutput = @isDoubleSizeHeatMapOutput,
					   MapSettings = @mapSettings,
					   MapModelArea = @mapModelArea,
					   ImageId = @imageId,
					   InputZoneData = @inputZoneData,
					   OutputZoneData = @outputZoneData,
					   ZoneBudgetExeName = @zoneBudgetExeName,
					   NumberOfStressPeriods = @numberOfStressPeriods,
					   CanalData = @canalData,
					   ModpathExeName = @modPathExeName,
					   SimulationFileName = @simulationFileName,
					   ListFileName = @listFileName,
					   BaseflowTableProcessingConfigurationID = @baseflowTableProcessingConfigurationID
		WHEN NOT MATCHED THEN
			INSERT (Id, Name, StartDateTime, ModflowExeName, NamFileName, RunFileName, AllowablePercentDiscrepancy, MapRunFileName, MapDrawdownFileName, IsDoubleSizeHeatMapOutput, MapSettings, MapModelArea, ImageId, InputZoneData, OutputZoneData, ZoneBudgetExeName, NumberOfStressPeriods, CanalData, ModpathExeName, SimulationFileName, ListFileName, BaseflowTableProcessingConfigurationID) 
			VALUES ((select (max(id) + 1) from Models), @modelName, @startDateTime, @modflowExeName, @namFileName, @runFileName, @allowablePercentDiscrepancy, @mapRunFileName, @mapDrawdownFileName, @isDoubleSizeHeatMapOutput, @mapSettings, @mapModelArea, @imageId, @inputZoneData, @outputZoneData, @zoneBudgetExeName, @numberOfStressPeriods, @canalData,  @modPathExeName, @simulationFileName, @listFileName, @baseflowTableProcessingConfigurationID);

	SELECT @modelId = Id FROM Models WHERE [Name] = @modelName;

	MERGE ModelScenarios as Target
		USING @scenarios as Source
		ON Target.ModelId = @ModelId and Target.ScenarioId = Source.id
		WHEN NOT MATCHED THEN
			INSERT (ModelId, ScenarioId) VALUES (@modelId, Source.Id)
		WHEN NOT MATCHED BY SOURCE AND Target.ModelId = @modelId THEN
			DELETE;

	DECLARE @customStartDateCount int;
	SELECT @customStartDateCount = count(*) from @customStartDatesForStressPeriods
	IF (@customStartDateCount = @numberOfStressPeriods)
	BEGIN
		DELETE from dbo.ModelStressPeriodCustomStartDates where ModelID = @modelId
		
		INSERT INTO dbo.ModelStressPeriodCustomStartDates(ModelID, StressPeriod, StressPeriodStartDate)
		select @modelId, ROW_NUMBER() OVER(ORDER BY [Date]) as StressPeriod, [Date]
		from @customStartDatesForStressPeriods
		order by [Date]
	END
END