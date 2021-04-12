/*Assumptions
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
DECLARE @baseflowTableProcessingConfigurationID INT;
/*-------------------------*/


/*----- Set These Values -----*/
/*This is the name of the Docker image.  Is should be all lower case.*/
SET @imageName = 'dockerimagename';

/*This is the name of the model as it will show up in the UI.*/
SET @modelName = 'My New Model';

/*The start date for the model.*/
SET @startDateTime = '2017-11-21';

/*The name of the modflow program for the model.*/
SET @modflowExeName = 'usgs_1.exe';

/*The name of the name file that will be passed to modflow.*/
SET @namFileName = 'test.nam';

/*The name of the output file modflow will generate for the run.*/
SET @runFileName = 'output.dat';

/*The name of the output heatmap binary file.  This can be null if @locationMapFileName is null.*/
SET @mapRunFileName = 'test.hds';

/*The name of the output heatmap binary file.  This can be null. */
SET @mapDrawdownFileName = 'test_DRAWDOWN.hds';

/*These are the map settings to be used by google maps*/
SET @mapSettings = '{zoom:8,center:{lat:40.8876131,lng:-100.0892906},mapTypeId:"terrain"}';

/*a set of points that makeup the border to be displayed on google maps*/
SET @mapModelArea = '[{lat:41.0213531047554,lng:-100.367575873715},{lat:41.0213375972734,lng:-100.372360865754}]';

/*the name of the zone budget executeable.  This can be null if we do not want to generate the zone budget data.*/
SET @zoneBudgetExeName = 'zonbud.exe';

/*Does the heat map output file use double sized value (0=Single, 1=Double)*/
SET @isDoubleSizeHeatMapOutput = 0;

/*The the maximum varience allowed in the percent discrepancy.  This can be set to null (percent discrepancy will not be verified).*/
SET @allowablePercentDiscrepancy = 1.0;

/*Add one value for each scenario that this model supports. 1=Add a Well, 2=Remove a Well, 3=Move a Well, 4=Canal Recharge, 5=Adjust Zone, 6=Retire Additional Wells, 7=Specify Pumping, 8 = ASR, 9 = Adjust Pumping, 10 = particle track*/
insert @scenarios(id) values(1),(4);

/*array of zone name, zone number, and bounds defined a a set of points to draw the zone polygon. Sample at https://jsoneditoronline.org/?id=6efc0290cfe1ed97af040d8592a457da*/
set @mapInputZone = '[{"ZoneNumber":"1","Name":"Zone A","Bounds":[{"Lat":40.9577,"Lng":-100.3192},{"Lat":40.9536,"Lng":-100.2725},{"Lat":40.9121,"Lng":-100.2711},{"Lat":40.9245,"Lng":-100.3192}]},{"ZoneNumber":"2","Name":"Zone B","Bounds":[{"Lat":40.8934,"Lng":-100.0066},{"Lat":40.8851,"Lng":-99.916},{"Lat":40.8477,"Lng":-99.9435}]},{"ZoneNumber":"3","Name":"Zone C","Bounds":[{"Lat":40.8072,"Lng":-99.9154},{"Lat":40.7718,"Lng":-99.9662},{"Lat":40.7801,"Lng":-99.8358}]}]';

/*array of zone name, zone number, and bounds defined a a set of points to draw the zone polygon. This will be used to generate Zones for any output Maps that include Zones Sample at https://jsoneditoronline.org/?id=6efc0290cfe1ed97af040d8592a457da*/
set @mapOutputZone = '[{"ZoneNumber":"1","Name":"Zone A","Bounds":[{"Lat":40.9577,"Lng":-100.3192},{"Lat":40.9536,"Lng":-100.2725},{"Lat":40.9121,"Lng":-100.2711},{"Lat":40.9245,"Lng":-100.3192}]},{"ZoneNumber":"2","Name":"Zone B","Bounds":[{"Lat":40.8934,"Lng":-100.0066},{"Lat":40.8851,"Lng":-99.916},{"Lat":40.8477,"Lng":-99.9435}]},{"ZoneNumber":"3","Name":"Zone C","Bounds":[{"Lat":40.8072,"Lng":-99.9154},{"Lat":40.7718,"Lng":-99.9662},{"Lat":40.7801,"Lng":-99.8358}]}]';

/*Total count of stress periods for the model*/
set @numberOfStressPeriods = 600;

/*Canal Names*/
set @canalData = 'canal 1,canal 2,canal 3'

/*Modpath exe*/
set @modPathExeName = 'mp.exe'

/*Modpath simFile name*/
set @simulationFileName = 'mp.mpsim'

/*Modflow 6 List File name*/
set @listFileName = 'test.lst'

/*Let's Bravo know how to get the values to calculate Baseflow and Impacts to Baseflow.
If left null, Baseflow will not be calculated. Run the following in a separate window
to see the values currently stored in the database:

		select *
		from dbo.BaseflowTableProcessingConfigurations

BaseflowTableIndicatorRegexPattern should match the line that exists BEFORE the dashed lines
that indicate the table headers for the table we are grabbing values from. This website can 
be helpful in checking if your indicator matches any of the patterns https://regex101.com/.
Also ensure that the column numbers match where the values will be located. For anything before
Modflow6, Segment and Reach column num should be defined. Anything Modflow6 and later will have the 
ReachColumnNum as null.
If you need to add a new pattern, consult the 'InsertBaseflowTableProcessingConfiguration.sql' script.
If the ID sent in is not null and does not match an ID present in the BaseflowTableProcessingConfigurations table, the insert will fail.
*/
set @baseflowTableProcessingConfigurationID = null

/*----- End Values to Set -----*/


/*----- DO NOT CHANGE -----*/
exec dbo.UpsertModel @imageName, @modelName, @startDateTime, @modflowExeName, @namFileName, @runFileName, @mapRunFileName, @mapDrawdownFileName, @mapSettings, @mapModelArea, @zoneBudgetExeName, @isDoubleSizeHeatMapOutput, @allowablePercentDiscrepancy, @scenarios, @mapInputZone, @mapOutputZone, @numberOfStressPeriods, @canalData, @modPathExeName, @simulationFileName, @listFileName, @baseflowTableProcessingConfigurationID;
/*-------------------------*/