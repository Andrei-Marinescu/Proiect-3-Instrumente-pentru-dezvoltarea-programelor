using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PCShop.Models
{
    public class Product
    {
        [Key] public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int QuantityAvailable { get; set; }
        public decimal Price { get; set; }

        // Cart - CartItem - Product
        // Product(1) : CartItem(N) | Product(N) : Cart(M)
        public ICollection<CartItem> CartItems { get; set; }


        // Order - OrderItem - Product
        // Product(1) : OrderItem(N) | Product(N) : Order(M)
        public ICollection<OrderItem> OrderItems { get; set; }

        public int ProviderId { get; set; }
        [ForeignKey("ProviderId")]
        public Provider Provider { get; set; }

        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
        
    }
}
