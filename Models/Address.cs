using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Address
    {

        public int AddressId { get; set; }

        public string UserId { get; set; } = string.Empty;
        [Required]
        public string Country { get; set; }= string.Empty;
        [Required]
        public string City { get; set; }= string.Empty;
        [Required]
        public string Street { get; set; }= string.Empty;
        [Required]
        public string Zip { get; set; }= string.Empty;

        public bool IsDefault { get; set; } = false;

        public AppUser User { get; set; } = null!;


    }
}
