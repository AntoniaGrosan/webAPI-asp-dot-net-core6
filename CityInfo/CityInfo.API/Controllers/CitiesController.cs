using AutoMapper;
using CityInfo.API.Migrations;
using CityInfo.API.Models;
using CityInfo.API.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [ApiController]
    // Can also add "api/[controller] BUT if we refactor it is not ok because the url will change
    [Route("api/cities")]
    public class CitiesController : ControllerBase
    {
        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;

        public CitiesController(ICityInfoRepository cityInfoRepository, IMapper mapper)
        {
            _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }


        // ActionResult<T> is a generic class that is used to represent the result of an action method in a controller
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CityWithoutPoisDto>>> GetCities()
        {
            var cityEntities = await _cityInfoRepository.GetCitiesAsync();

            return Ok(_mapper.Map<IEnumerable<CityWithoutPoisDto>>(cityEntities));

            #region Previous versions
            // 2
            //var results = new List<CityWithoutPoisDto>();
            //foreach (var cityEntity in cityEntities)
            //{
            //    results.Add(new CityWithoutPoisDto
            //    {
            //        Id = cityEntity.Id,
            //        Name = cityEntity.Name,
            //        Description = cityEntity.Description,
            //    });
            //}
            //return Ok(results);

            // 1
            //return Ok(_citiesDataStore.Cities);

            // 0
            /*
            // Specifying 'object' here allows us to pass a list of anonymous objects to the JsonResult constructor,
            // which can then serialize the data into a JSON response:
            return new JsonResult(new List<object>
            {
                new { id = 1, Name = "New York City"},
                new { id = 2, Name = "Chicago"}
            });
            */
            #endregion
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCity(int id, bool includePointsOfInterest = false)
        {
            var cityEntity = await _cityInfoRepository.GetCityAsync(id, includePointsOfInterest);

            if (cityEntity == null)
            {
                return NotFound();
            }

            if (includePointsOfInterest)
            {
                return Ok(_mapper.Map<CityDto>(cityEntity));
            }

            return Ok(_mapper.Map<CityWithoutPoisDto>(cityEntity));

            //var cityToReturn = _citiesDataStore.Cities.FirstOrDefault(city => city.Id == id);

            //if (cityToReturn == null)
            //{
            //    return NotFound();
            //}

            //return Ok(cityToReturn);
        }
    }
}
