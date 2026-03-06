using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class AppUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = String.Empty;


        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Address>  Addresses { get; set; } = new List<Address>();
        
    }
}
