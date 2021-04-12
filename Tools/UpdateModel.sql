/*
Assumptions
1) All docker images are on the same server
2) At least one image has been setup already in the database
3) Cannot change image name or model name
*/


/*----- DO NOT CHANGE -----*/
DECLARE @imageName NVARCHAR(256);
DECLARE @modelName NVARCHAR(256);
DECLARE @startDateTime DATETIME;
DECLARE @modflowExeName VARCHAR(50);
DECLARE @namFileName VARCHAR(50);
DECLARE @runFileName VARCHAR(50);
DECLARE @mapRunFileName VARCHAR(50);
DECLARE @mapDrawdownFileName VARCHAR(50);
DECLARE @mapSettings VARCHAR(1024);
DECLARE @mapModelArea VARCHAR(MAX);
DECLARE @mapInputZone VARCHAR(MAX);
DECLARE @mapOutputZone VARCHAR(MAX);
DECLARE @zoneBudgetExeName VARCHAR(50);
DECLARE @isDoubleSizeHeatMapOutput BIT;
DECLARE @allowablePercentDiscrepancy FLOAT;
DECLARE @scenarios as dbo.ScenariosList;
DECLARE @numberOfStressPeriods int;
DECLARE @canalData VARCHAR(MAX);
DECLARE @modPathExeName VARCHAR(50);
DECLARE @simulationFileName VARCHAR(50);
DECLARE @listFileName VARCHAR(50);
DECLARE @baseflowTableProcessingConfigurationID int
/*-------------------------*/


/*----- Set These Values -----*/
SET @modelName = 'Test';
/*----- End Values to Set -----*/


/*----- DO NOT CHANGE -----*/
exec dbo.RetrieveModel @modelName, @imageName OUT, @startDateTime OUT, @modflowExeName OUT, @namFileName OUT, @runFileName OUT, @mapRunFileName OUT, @mapDrawdownFileName OUT, @mapSettings OUT, @mapModelArea OUT, @zoneBudgetExeName OUT, @isDoubleSizeHeatMapOutput OUT, @allowablePercentDiscrepancy OUT, @mapInputZone OUT, @mapOutputZone OUT, @numberOfStressPeriods out, @canalData out, @modPathExeName out, @simulationFileName out, 
@listFileName out, @baseflowTableProcessingConfigurationID out;
/*-------------------------*/


/*
Set Only The Values That Need Changes and Always Set @scenarios
Any variables from above can be set except imageName and modelName
Descriptions for these values can be found in InsertModel.sql
*/
SET @mapRunFileName = 'Test.hds';
SET @mapDrawdownFileName = 'Test_DRAWDOWN.hds';
insert @scenarios(id) values(4);
/*----- End Values to Set -----*/


/*----- DO NOT CHANGE -----*/
exec dbo.UpsertModel @imageName, @modelName, @startDateTime, @modflowExeName, @namFileName, @runFileName, @mapRunFileName, @mapDrawdownFileName, @mapSettings, @mapModelArea, @zoneBudgetExeName, @isDoubleSizeHeatMapOutput, @allowablePercentDiscrepancy, @scenarios, @mapInputZone, @mapOutputZone, @numberOfStressPeriods ,@canalData, @modPathExeName, @simulationFileName, @listFileName, @baseflowTableProcessingConfigurationID;
/*-------------------------*/