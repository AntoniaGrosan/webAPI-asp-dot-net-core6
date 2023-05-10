using CityInfo.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [ApiController]
    // Can also add "api/[controller] BUT if we refactor it is not ok because the url will change
    [Route("api/cities")]
    public class CitiesController : ControllerBase
    {
        private readonly CitiesDataStore _citiesDataStore;

        public CitiesController(CitiesDataStore citiesDataStore)
        {
            _citiesDataStore = citiesDataStore ?? throw new ArgumentNullException(nameof(citiesDataStore));
        }

        // ActionResult<T> is a generic class that is used to represent the result of an action method in a controller
        [HttpGet]
        public ActionResult<IEnumerable<CityDto>> GetCities()
        {
            return Ok(_citiesDataStore.Cities);

            /*
            // Specifying 'object' here allows us to pass a list of anonymous objects to the JsonResult constructor,
            // which can then serialize the data into a JSON response:
            return new JsonResult(new List<object>
            {
                new { id = 1, Name = "New York City"},
                new { id = 2, Name = "Chicago"}
            });
            */
        }

        [HttpGet("{id}")]
        public ActionResult<CityDto> GetCity(int id)
        {
            var cityToReturn = _citiesDataStore.Cities.FirstOrDefault(city => city.Id == id);

            if (cityToReturn == null)
            {
                return NotFound();
            }

            return Ok(cityToReturn);
        }
    }
}
