using AutoMapper;
using Bravo.Common.DataContracts.Runs;
using System;
using Bravo.Common.DataContracts.Models;
using System.Collections.Generic;
using Bravo.Accessors.EntityFramework;
using BaseflowTableProcessingConfiguration = Bravo.Common.DataContracts.Models.BaseflowTableProcessingConfiguration;
using Image = Bravo.Common.DataContracts.Models.Image;
using Model = Bravo.Common.DataContracts.Models.Model;
using ModelScenario = Bravo.Common.DataContracts.Models.ModelScenario;
using ModelStressPeriodCustomStartDate = Bravo.Common.DataContracts.Models.ModelStressPeriodCustomStartDate;
using Run = Bravo.Common.DataContracts.Runs.Run;
using RunBucket = Bravo.Common.DataContracts.Runs.RunBucket;
using Scenario = Bravo.Common.DataContracts.Models.Scenario;
using ScenarioFile = Bravo.Common.DataContracts.Models.ScenarioFile;

namespace Bravo.Accessors
{
    internal static class DTOMapper
    {
        static IMapper _mapper;
        private static IConfigurationProvider _config;

        public static IMapper Mapper => _mapper ?? (_mapper = Configuration.CreateMapper());

        public static IConfigurationProvider Configuration
        {
            get
            {
                if (_config == null)
                {
                    var config = new AutoMapper.MapperConfiguration(cfg =>
                    {
                        cfg.CreateMap<EntityFramework.ModelScenario, ModelScenario>().ReverseMap();

                        cfg.CreateMap<EntityFramework.Scenario, Scenario>().ReverseMap();
                        cfg.CreateMap<EntityFramework.Scenario, Scenario>().ForMember(destination => destination.InputControlType,
                            opt => opt.MapFrom(source => Enum.GetName(typeof(InputControlType), source.InputControlType)));

                        cfg.CreateMap<EntityFramework.ScenarioFile, ScenarioFile>().ReverseMap();

                        cfg.CreateMap<EntityFramework.BaseflowTableProcessingConfiguration,
                            BaseflowTableProcessingConfiguration>().ReverseMap();

                        cfg.CreateMap<EntityFramework.ModelStressPeriodCustomStartDate,
                            ModelStressPeriodCustomStartDate>().ReverseMap();

                        cfg.CreateMap<EntityFramework.Model, Model>()
                            .ForMember(dest => dest.Scenarios, opts => opts.MapFrom(src => src.Scenarios))
                            .ForMember(dest => dest.ModelStressPeriodCustomStartDates, opts => opts.MapFrom(src => src.ModelStressPeriodCustomStartDates))
                            .ForMember(dest => dest.BaseflowTableProcessingConfiguration, opts => opts.MapFrom(src => src.BaseflowTableProcessingConfiguration))
                            .ReverseMap();

                        cfg.CreateMap<EntityFramework.Image, Image>().ReverseMap();

                        cfg.CreateMap<EntityFramework.Run, Run>()
                            .ForMember(destination => destination.Status, opt => opt.MapFrom(source => Enum.GetName(typeof(RunStatus), source.Status)))
                            .ForMember(dest => dest.InputVolumeType, opts => opts.MapFrom(source => source.InputVolumeUnit))
                            .ForMember(dest => dest.OutputVolumeType, opts => opts.MapFrom(source => source.OutputVolumeUnit));

                        cfg.CreateMap<Run, EntityFramework.Run>()
                            .ForMember(destination => destination.Status, opt => opt.MapFrom(source => (int)source.Status))
                            .ForMember(dest => dest.InputVolumeUnit, opts => opts.MapFrom(source => source.InputVolumeType))
                            .ForMember(dest => dest.OutputVolumeUnit, opts => opts.MapFrom(source => source.OutputVolumeType))
                            .ForMember(dest => dest.Model, opts => opts.Ignore())
                            .ForMember(dest => dest.Scenario, opts => opts.Ignore());

                        cfg.CreateMap<EntityFramework.RunBucket, RunBucket>()
                            .ForMember(dest => dest.Runs, opts => opts.MapFrom(src => src.RunBucketRuns == null ? src.RunBucketRuns : new List<EntityFramework.RunBucketRun>()));

                        cfg.CreateMap<RunBucket, EntityFramework.RunBucket>()
                            .ForMember(dest => dest.RunBucketRuns, opts => opts.Ignore());
                    });
                    _config = config;
                }
                return _config;
            }
        }
    }
}
