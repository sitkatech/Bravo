using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bravo.Common.Shared
{
    public static class StorageLocations
    {
        private const string PARSED_INPUT_FILE_NAME = "inputs.json";
        private const string PARSED_WELL_INPUT_FILE_NAME = "mapinputs.json";
        private const string PARSED_WELL_PARTICLE_INPUT_FILE_NAME = "mapparticleinputs.json";
        private const string PARSED_ZONE_INPUT_FILE_NAME = "mapzoneinputs.json";
        private const string INPUT_FOLDER_NAME = "inputs";
        private const string OUTPUT_FOLDER_NAME = "outputs";
        private const string GENERATE_INPUT_OUTPUT_FOLDER_NAME = "generateinputoutputs";
        private const string ANALYSIS_OUTPUT_FOLDER_NAME = "analysisoutputs";

        public static string InputDirectoryPathForRun(string fileStorageLocator)
        {
            return $"{fileStorageLocator}/{INPUT_FOLDER_NAME}";
        }
        public static string InputFilePathForRun(string inputFileName, string fileLocator)
        {
            return $"{fileLocator}/{INPUT_FOLDER_NAME}/{inputFileName}";
        }
        public static string ParsedInputFilePathForRun(string fileLocator)
        {
            return $"{fileLocator}/{INPUT_FOLDER_NAME}/{PARSED_INPUT_FILE_NAME}";
        }
        public static string ParsedWellInputFilePathForRun(string fileLocator)
        {
            return $"{fileLocator}/{INPUT_FOLDER_NAME}/{PARSED_WELL_INPUT_FILE_NAME}";
        }
        public static string ParsedWellParticleInputFilePathForRun(string fileLocator)
        {
            return $"{fileLocator}/{INPUT_FOLDER_NAME}/{PARSED_WELL_PARTICLE_INPUT_FILE_NAME}";
        }
        public static string ParsedZoneInputFilePathForRun(string fileLocator)
        {
            return $"{fileLocator}/{INPUT_FOLDER_NAME}/{PARSED_ZONE_INPUT_FILE_NAME}";
        }
        public static string OutputDirectoryPathForRun(string fileStorageLocator)
        {
            return $"{fileStorageLocator}/{OUTPUT_FOLDER_NAME}";
        }
        public static string OutputFilePathForRun(string outputFileName, string fileLocator)
        {
            return $"{fileLocator}/{OUTPUT_FOLDER_NAME}/{outputFileName}";
        }
        public static string GenerateInputOutputFilePath(string outputFileName, string fileLocator)
        {
            return $"{fileLocator}/{GENERATE_INPUT_OUTPUT_FOLDER_NAME}/{outputFileName}";
        }
        public static string GenerateInputOutputFolderPath(string fileLocator)
        {
            return $"{fileLocator}/{GENERATE_INPUT_OUTPUT_FOLDER_NAME}";
        }
        public static string AnalysisOutputFilePath(string outputFileName, string fileLocator)
        {
            return $"{fileLocator}/{ANALYSIS_OUTPUT_FOLDER_NAME}/{outputFileName}";
        }
        public static string AnalysisOutputFolderPath(string fileLocator)
        {
            return $"{fileLocator}/{ANALYSIS_OUTPUT_FOLDER_NAME}";
        }
        public static string ModelOutputFolderPath(string modelName, string fileName)
        {
            return $"{modelName}/{fileName}";
        }
    }
}
