using System.ComponentModel.DataAnnotations;

namespace AgriEnergyConnect.Models
{
    public class Farmer
    {
        public Farmer()
        {
            // Initialize Products collection to avoid null reference errors
            Products = new List<Product>();
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Area is required")]
        public string Area { get; set; }

        [Required(ErrorMessage = "Contact number is required")]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }

        // Navigation property for products - make it non-required
        public virtual List<Product> Products { get; set; }
    }
}