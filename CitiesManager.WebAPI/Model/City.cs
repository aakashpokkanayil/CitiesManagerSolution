using System.ComponentModel.DataAnnotations;

namespace CitiesManager.WebAPI.model
{
    public class City
    {
        [Key]
        public Guid CityId { get; set; }

        public string? CityName { get; set; }
    }
}
