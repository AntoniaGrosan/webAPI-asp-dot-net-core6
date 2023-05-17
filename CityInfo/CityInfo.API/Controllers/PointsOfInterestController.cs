using AutoMapper;
using CityInfo.API.Entities;
using CityInfo.API.Models.PointOfInterestDtos;
using CityInfo.API.Repositories;
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
        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;

        // constructor injection (best way to inject, use it whenever possible)
        public PointsOfInterestController(ILogger<PointsOfInterestController> logger,
            IMailService localMailService,
            ICityInfoRepository cityInfoRepository,
            IMapper mapper
            )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mailService = localMailService ?? throw new ArgumentNullException(nameof(localMailService));
            _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PointOfInterestDto>>> GetPointsOfInterest(int cityId)
        {
            if (!await _cityInfoRepository.CityExists(cityId))
            {
                _logger.LogInformation($"City with {cityId} was not found when accessing points of interest.");
                return NotFound();
            }

            var poisEntities = await _cityInfoRepository.GetPointsOfInterestForCityAsync(cityId);
            return Ok(_mapper.Map<IEnumerable<PointOfInterestDto>>(poisEntities));

            /*
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
            */
        }

        [HttpGet("{pointofinterestid}", Name = "GetPointOfInterest")]
        public async Task<ActionResult<PointOfInterestDto>> GetPointOfInterest(int cityId, int pointOfInterestId)
        {
            if (!await _cityInfoRepository.CityExists(cityId))
            {
                _logger.LogInformation($"City with {cityId} was not found when accessing points of interest.");
                return NotFound();
            }

            var poiEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            if (poiEntity == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<PointOfInterestDto>(poiEntity));
        }

        [HttpPost]
        public async Task<ActionResult<PointOfInterestDto>> CreatePointOfInterest(int cityId, PointOfInterestForCreateDto pointOfInterest)
        {
            if (!await _cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }

            var poiEntity = _mapper.Map<PointOfInterest>(pointOfInterest);

            await _cityInfoRepository.AddPointOfInterestForCityAsync(cityId, poiEntity);

            await _cityInfoRepository.SaveChangesAsync();

            return CreatedAtRoute("GetPointOfInterest",
                new
                {
                    cityId = cityId,
                    pointOfInterestId = poiEntity.Id
                },
                _mapper.Map<PointOfInterestDto>(poiEntity));
        }

        [HttpPut("{pointofinterestid}")]
        public async Task<ActionResult> UpdatePointOfInterest(int cityId, int pointOfInterestId, PointOfInterestForUpdateDto pointOfInterest)
        {
            if (!await _cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }

            var poiEntityToUpdate = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            if (poiEntityToUpdate == null)
            {
                return NotFound();
            }

            _mapper.Map(pointOfInterest, poiEntityToUpdate); // this will automatically override the dest obj with source obj
            
            await _cityInfoRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{pointofinterestid}")]
        public async Task<ActionResult> UpdatePointOfInterest(int cityId, int pointOfInterestId,
            JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
        {            
            if (! await _cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }

            var poiEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            if (poiEntity == null)
            {
                return NotFound();
            }

            var poiToPatch = _mapper.Map<PointOfInterestForUpdateDto>(poiEntity);

            patchDocument.ApplyTo(poiToPatch, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!TryValidateModel(poiToPatch))
            {
                return BadRequest(ModelState);
            }

            _mapper.Map(poiToPatch, poiEntity);

            await _cityInfoRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{pointofinterestid}")]
        public async Task<ActionResult> DeletePointOfInterest(int cityId, int pointOfInterestId)
        {
            if (!await _cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }

            var poiEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            if (poiEntity == null)
            {
                return NotFound();
            }

            _cityInfoRepository.DeletePointOfInterest(poiEntity);

            await _cityInfoRepository.SaveChangesAsync();

            _mailService.Send("Point of interest deleted", $"Point of intrerest {poiEntity.Name} deleted.");

            return NoContent();
        }
    }
}

