namespace CityInfo.API.Models
{
    /// <summary>
    /// A Dto for a city without its points of interest
    /// </summary>
    public class CityWithoutPoisDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
