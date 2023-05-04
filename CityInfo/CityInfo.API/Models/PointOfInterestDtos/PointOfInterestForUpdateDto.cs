using System.ComponentModel.DataAnnotations;

namespace CityInfo.API.Models.PointOfInterestDtos
{
    public class PointOfInterestForUpdateDto
    {
        [Required(ErrorMessage = "Please provide a name.")]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Description { get; set; }
    }
}
