using AutoMapper;
using CityInfo.API.Entities;
using CityInfo.API.Models.PointOfInterestDtos;

namespace CityInfo.API.Profiles
{
    public class PointOfInterestProfile : Profile
    {
        public PointOfInterestProfile() {
            CreateMap<PointOfInterest, PointOfInterestDto>();
            CreateMap<PointOfInterestForCreateDto, PointOfInterest>();
            CreateMap<PointOfInterestForUpdateDto, PointOfInterest>();
            CreateMap<PointOfInterest, PointOfInterestForUpdateDto>();
        }
    }
}
