using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgriEnergyConnect.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; }

        // Image path should not be required
        public string? ImagePath { get; set; }

        // Foreign key for Farmer - DO NOT make this Required
        // Will be set by controller
        public int FarmerId { get; set; }

        [ForeignKey("FarmerId")]
        public virtual Farmer? Farmer { get; set; }
    }
}