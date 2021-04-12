# Project Summary
The Bravo Groundwater Modeling Engine application provides an interface for running Modflow and Modpath simulations.

Bravo exists to
- trigger Modflow and Modpath runs to start
- store the results of these runs in Azure Blob storage
- allow reading and interpreting of the results of these runs

The basic workflow of Bravo is:
- Create actions/runs
- Generate the Modflow/Modpath inputs for the action/run 
- Run the analysis for the action/run
- Generate outputs for the action/run
- View outputs (i.e. graphs, data) for the action/run

# Tooling

This application uses the following tools:
- Visual Studio 2019
- SQL Server & SSMS
- Azure Blob Storage
- Docker

# Setup

## Configuration

1. Install the SlowCheetah Extension from the Visual Studio Extensions Menu

## Database

(Ensure you've updated your connection strings with a connection to your database)
1. Run DbUp for your local database
    a. In Visual Studio, set Primary (under 5.Databases) as the startup project
2. Run DbUp for your unit test database
    a. Change the connection string to use the initial catalog "Bravo.Primary.Tests"
    b. After you run it, undo the change (back to "Bravo.Primary")

## Blob Storage

1. Run Azure Storage Emulator to simulate blobs and queues locally
    - You can manipulate these directly with Azure Storage Explorer

# Running the Application Locally

## Test Projects

Make sure you've setup your test database (see previous section).

CAUTION: Avoid stopping tests in the middle of execution. The tests will not gracefully free up resources.
If you get in this scenario, restart visual studio to manually free things back up.

Accessor, Engine, and Manager tests have all been included to help with ease of understanding as well as aiding in testing new features.

It is highly recommended to develop against tests as much as possible since the models can take quite a long time to run.
You can even point tests to your local Model folder to test against without running the time-consuming local process.

## Running a Model

During the steps above, the database should've added a test model for called 'Modflow 6 Structured'. The model folder that contains all the information Bravo needs to complete a run can be found in the "Documentation" folder, in a subfolder called "Model". For this step you'll also  need an application that can make web requests without a browser, such as Postman.

1)Copy the 'Documentation/Model' folder onto your C drive, or to a place that you want to access it from (you could even leave it where it is if you'd like)
2)Ensure that the "ModflowDataFolder" key in your AppSettingsConfig is pointing to wherever this model file was stored. This is going to tell the application where to pull files from for the run.
3)Ensure that you've started Azure Storage Emulator and that it is currently running (if you've not done this before, make sure that you run the 'init' command prior to the 'start' command).
4)Open up AzureStorageExplorer and dropdown the option for 'Local & Attached' at the top right. Then dropdown '(Emular - Default Ports)(Key)'. This is where your queues are going to appear as well as the results of your runs and even logs. If you've not run anything that uses this previously, these will all be empty.
5)With '(Emulator - Default Ports)(Key)' selected, look in the bottom right in the 'Properties' tab. This is going to hold a number of pieces of information. We want the AccountName and AccountKey. In your AppSettingsConfig, update 'AzureStorageAccountName' with the AccountName and 'AzureStorageAccountKey' with the account key.
6)In Visual Studio, right click the Solution in the Solution explorer and choose 'Set Startup Projects'. We want to set both 'APIFunctions' and 'Orchestrator' to start.
7)Hit 'Start' and you should see two windows pop-up, one for the APIFunctions and one for the Orchestrator. The APIFunctions window will show you all the endpoints you have to access, while the Orchestrator will show any output as the queues do their work.
8)Using your web request tool, set the url to perform a post request to your 'StartRun' endpoint. The easiest way to give the input is as a raw JSON object. Here's a sample: 

{
    "ModelID" : 1,
    "ScenarioID" : 1,
    "Name" : "APITest",
    "IsDifferential" : false,
    "CreateMaps": true,
    "PivotedRunWellInputs" : [
        {
            "Name" : "TestWell1",
            "Lat" : 40.881242,
            "Lng" : -99.672285,
            "AverageValue" : 0,
            "ManuallyAdded" : true
        }
    ]
}

This will start a run using our model, for the 'Add a Well' scenario. "IsDifferential" will control whether it compares against results that have been established as what you'd expect to see given no inputs or inputs of 0 at certain locations (similar to what we're doing above, so right now we're creating a baseline). "CreateMaps" will tell Bravo if we want to generate a visual representation of our results in the form of heat maps for each stress period. Each scenario is going to have its own required inputs, and for the 'Add a Well' scenario we require a "PivotedRunWellInputs" array that contains the wells and the average value per stress period pumped. There is an option to add InputVolumeTypeID and OutputVolumeTypeID, but if it's excluded they will default to whatever the application has set as the Default Volume Type for that scenario (see Functions.cs in APIFunctions for this).

9)Once the run is triggered, you should get a response that states that the run has been queued and provides a RunID. If you switch to watching the 'Orchestrator' window, you'll be able to see as operations complete and any output that happens along the way.
10)To see the run's status as it's going, you can query the 'RunResultStatus' endpoint which will return what step in the process the run is at.
11)Once the process is complete, you can query the "GetAvailableRunResults" endpoint with your runID and it will show  all the available results for the run. Another way to see all the results is to go into AzureStorageExplorer, dropdown the "Blob Containers" option and select model-data. You'll then see a list of GUIDs, these are all associated with the FileStorageLocator value in the 'Runs' table of your database. Here you can see all the inputs and outputs for the run
12)To get a json result of a particular file, you can query the "RetrieveResult" endpoint, passing a RunID and the name of the file to be retrieved.

As development continues this documentation will be expanded, but this should be all it takes to get the application up and running.
