﻿using AutoMapper;
using Bravo.Common.DataContracts.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bravo.Common.AspIdentity
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
                        cfg.CreateMap<User, ApplicationUser>();
                        cfg.CreateMap<ApplicationUser, User>();

                        cfg.CreateMap<Role, ApplicationRole>();
                        cfg.CreateMap<ApplicationRole, Role>();
                    });
                    _config = config;
                }
                return _config;
            }
        }
    }
}
