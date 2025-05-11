using System.ComponentModel.DataAnnotations;

namespace AgriEnergyConnect.Models.ViewModels
{
    public class CreateProductViewModel
    {
        [Required(ErrorMessage = "Product name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; }

        // Convert to Product model
        public Product ToProduct()
        {
            return new Product
            {
                Name = this.Name,
                Description = this.Description,
                Category = this.Category
            };
        }
    }
}