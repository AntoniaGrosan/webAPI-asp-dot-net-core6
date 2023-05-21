using CityInfo.API.DbContexts;
using CityInfo.API.Entities;
using CityInfo.API.Services;
using Microsoft.EntityFrameworkCore;

namespace CityInfo.API.Repositories
{
    public class CityInfoRepository : ICityInfoRepository
    {
        private readonly CityInfoContext _context;

        public CityInfoRepository(CityInfoContext context)
        {
            _context = context ?? throw new ArgumentException(nameof(context));
        }

         public async Task<bool> CityExists(int cityId)
        {
            return await _context.Cities.AnyAsync(c => c.Id == cityId);
        }

        public async Task<IEnumerable<City>> GetCitiesAsync()
        {
            return await _context.Cities.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<(IEnumerable<City>, PaginationMetadata)> GetCitiesAsync(
            string? name, string? searchQuery, int pageNumber, int pageSize)
        {
            var citiesCollection = _context.Cities as IQueryable<City>;

            if (!string.IsNullOrEmpty(name)) 
            {
                name = name.Trim();
                citiesCollection = citiesCollection.Where(c => c.Name == name);
            }

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.Trim();

                citiesCollection = citiesCollection
                    .Where(c => c.Name.Contains(searchQuery) ||
                    (!string.IsNullOrEmpty(c.Description) && c.Description.Contains(searchQuery)));                    
            }

            var totalItemCount = await citiesCollection.CountAsync();

            var paginationMetaData = new PaginationMetadata(totalItemCount, pageSize, pageNumber);

            var collectionToReturn = await citiesCollection.
                OrderBy(c => c.Name).
                Skip(pageSize * (pageNumber - 1)).
                Take(pageSize).
                ToListAsync();

            return (collectionToReturn, paginationMetaData);
        }

        public async Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest = false)
        {
            if (includePointsOfInterest)
            {
                return await _context.Cities
                    .Include(c => c.PointsOfInterest)
                    .Where(c => c.Id == cityId)
                    .FirstOrDefaultAsync();
            }

            return await _context.Cities
                    .Where(c => c.Id == cityId)
                    .FirstOrDefaultAsync();
        }

        public async Task<PointOfInterest?> GetPointOfInterestForCityAsync(int cityId, int pointOfInterestId)
        {
            return await _context.PointsOfInterest
                .Where(poi => poi.CityId == cityId && poi.Id == pointOfInterestId)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCityAsync(int cityId)
        {
            return await _context.PointsOfInterest
                .Where(poi => poi.CityId == cityId)
                .ToListAsync();
        }

        public async Task AddPointOfInterestForCityAsync(int cityId, PointOfInterest pointOfInterest)
        {
            var city = await GetCityAsync(cityId);
            if(city != null)
            {
                city.PointsOfInterest.Add(pointOfInterest);
            }

        }
        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() >= 0;
        }

        public void DeletePointOfInterest(PointOfInterest pointOfInterest)
        {
            _context.PointsOfInterest.Remove(pointOfInterest);
        }

        public async Task<bool> CityNameMatchesCityId(string? name, int cityId)
        {
            return await _context.Cities.AnyAsync(c => c.Id == cityId && c.Name == name);
        }
    }
}
