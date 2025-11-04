using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PCShop.Models
{
    public class Cart
    {
        [Key] public int CartId { get; set; }

        // User(1) : Cart(1)
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
        // Cart(1) : CartItems(N) | Cart(N) : Product(M)
        public ICollection<CartItem> CartItems { get; set; }
    }
}