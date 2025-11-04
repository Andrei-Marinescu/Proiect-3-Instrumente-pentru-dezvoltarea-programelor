using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PCShop.Models
{
    public class CartItem
    {
        [Key] public int CartItemId { get; set; }
        public int? Quantity { get; set; }
        public decimal? Price { get; set; }

        public int CartId {  get; set; }
        [ForeignKey("CartId")]
        public Cart Cart { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}