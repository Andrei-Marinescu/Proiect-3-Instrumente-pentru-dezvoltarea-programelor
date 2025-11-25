using System.ComponentModel.DataAnnotations;

namespace PCShop.Models
{
    public class Provider
    {
        [Key]
        public int ProviderId { get; set; }
        public string Name { get; set; }

        // Provider(1) - Produts(N)
        public ICollection<Product>? Products { get; set; }
    }
}
