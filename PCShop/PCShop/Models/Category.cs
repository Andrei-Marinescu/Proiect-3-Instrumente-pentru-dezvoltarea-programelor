using System.ComponentModel.DataAnnotations;

namespace PCShop.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        public string Name { get; set; }
        
        // Category(1) : Products(N)
        public ICollection<Product> Products { get; set; }
    }
}
