using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PCShop.Models
{
    public class Wishlist
    {
        [Key]
        public int WishlistId { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }

        public int? ProductId { get; set; }
        public Product? Product { get; set; }
    }
}
