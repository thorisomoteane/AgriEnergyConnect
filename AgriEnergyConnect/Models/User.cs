using System.ComponentModel.DataAnnotations;

namespace AgriEnergyConnect.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; } // "Farmer" or "Employee"

        // If the user is a farmer, this will be the farmer's ID
        public int? FarmerId { get; set; }
    }
}