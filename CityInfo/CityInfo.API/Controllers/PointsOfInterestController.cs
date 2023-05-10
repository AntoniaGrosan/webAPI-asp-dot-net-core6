using CityInfo.API.Models.PointOfInterestDtos;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace CityInfo.API.Controllers
{
    [Route("api/cities/{cityId}/pointsofinterest")]
    [ApiController]
    public class PointsOfInterestController : ControllerBase
    {

        private readonly ILogger<PointsOfInterestController> _logger;
        private readonly IMailService _mailService;
        private readonly CitiesDataStore _citiesDataStore;

        // constructor injection (best way to inject, use it whenever possible)
        public PointsOfInterestController(ILogger<PointsOfInterestController> logger, 
            IMailService localMailService, 
            CitiesDataStore citiesDataStore)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mailService = localMailService ?? throw new ArgumentNullException(nameof(localMailService));
            _citiesDataStore= citiesDataStore ?? throw new ArgumentNullException(nameof(citiesDataStore));
        }

        [HttpGet]
        public ActionResult<IEnumerable<PointOfInterestDto>> GetPointsOfInterest(int cityId)
        {
            try
            {
                var city = _citiesDataStore.Cities.FirstOrDefault(city => city.Id == cityId);
                if (city == null)
                {
                    _logger.LogInformation($"City with {cityId} was not found when accessing points of interest.");
                    return NotFound();
                }

                var pois = city.PointsOfInterest;
                return Ok(pois);
            }
            catch (Exception e)
            {
                _logger.LogCritical($"An exception was caught while getting pooints of interest for city with id {cityId}", e);
                return StatusCode(500, "A problem happened hwile handling your request."); //careful with this message, since it is seen by the consumer
            }                       
        }

        [HttpGet("{pointofinterestid}", Name = "GetPointOfInterest")]
        public ActionResult<PointOfInterestDto> GetPointOfInterest(int cityId, int pointOfInterestId)
        {
            var city = _citiesDataStore.Cities.FirstOrDefault(city => city.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }

            var poiToReturn = city.PointsOfInterest.FirstOrDefault(poi => poi.Id == pointOfInterestId);
            if (poiToReturn == null)
            {
                return NotFound();
            }

            return Ok(poiToReturn);
        }

        [HttpPost]
        public ActionResult<PointOfInterestDto> CreatePointOfInterest(int cityId, PointOfInterestForCreateDto pointOfInterest)
        {
            var city = _citiesDataStore.Cities.FirstOrDefault(city => city.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }

            var maxPOIid = _citiesDataStore.Cities.SelectMany(c => c.PointsOfInterest).Max(p => p.Id);

            var poi = new PointOfInterestDto
            {
                Id = ++maxPOIid,
                Name = pointOfInterest.Name,
                Description = pointOfInterest.Description
            };

            city.PointsOfInterest.Add(poi);

            return CreatedAtRoute("GetPointOfInterest",
                new
                {
                    cityId = cityId,
                    pointOfInterestId = poi.Id
                },
                poi);
        }

        [HttpPut("{pointofinterestid}")]
        public ActionResult UpdatePointOfInterest(int cityId, int pointOfInterestId, PointOfInterestForUpdateDto pointOfInterest)
        {
            var city = _citiesDataStore.Cities.FirstOrDefault(city => city.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }

            var poiToUpdate = city.PointsOfInterest.FirstOrDefault(poi => poi.Id == pointOfInterestId);
            if (poiToUpdate == null)
            {
                return NotFound();
            }

            poiToUpdate.Name = pointOfInterest.Name;
            poiToUpdate.Description = pointOfInterest.Description;

            return NoContent();
        }

        [HttpPatch("{pointofinterestid}")]
        public ActionResult UpdatePointOfInterest(int cityId, int pointOfInterestId,
            JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
        {
            var city = _citiesDataStore.Cities.FirstOrDefault(city => city.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }

            var poiFromStore = city.PointsOfInterest.FirstOrDefault(poi => poi.Id == pointOfInterestId);
            if (poiFromStore == null)
            {
                return NotFound();
            }

            var poiToPatch = new PointOfInterestForUpdateDto()
            {
                Name = poiFromStore.Name,
                Description = poiFromStore.Description
            };

            patchDocument.ApplyTo(poiToPatch, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!TryValidateModel(poiToPatch))
            {
                return BadRequest(ModelState);
            }

            poiFromStore.Name = poiToPatch.Name;
            poiFromStore.Description = poiToPatch.Description;

            return NoContent();
        }

        [HttpDelete("{pointofinterestid}")]
        public ActionResult DeletePointOfInterest(int cityId, int pointOfInterestId)
        {
            var city = _citiesDataStore.Cities.FirstOrDefault(city => city.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }

            var poiFromStore = city.PointsOfInterest.FirstOrDefault(poi => poi.Id == pointOfInterestId);
            if (poiFromStore == null)
            {
                return NotFound();
            }

            city.PointsOfInterest.Remove(poiFromStore);
            _mailService.Send("Point of interest deleted", $"Point of intrerest {poiFromStore.Name} deleted.");

            return NoContent();
        }
    }
}
