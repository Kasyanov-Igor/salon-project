using AutoMapper;
using Domain.Domains;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class MapperConfig : Profile
    {
        public ILoggerFactory loggerFactory;

        public MapperConfig()
        {
            this.loggerFactory = new LoggerFactory();
            CreateMap<DTOClient, Client>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Client"));
        }

        public IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MapperConfig>(), loggerFactory);
            return config.CreateMapper();
        }
    }
}
