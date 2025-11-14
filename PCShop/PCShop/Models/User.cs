using System.ComponentModel.DataAnnotations;

namespace PCShop.Models
{
    public class User
    {
        [Key] public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ContactNumber { get; set; } = string.Empty;

        public byte[] AvatarImage { get; set; }
        
        // User(1) : UserAdress(N) | User(N) : Adress(M)
        public ICollection<UserAdress> UserAdresses { get; set; }

        // User(1) : Wishlist(N)
        public ICollection<Wishlist> Wishlists { get; set; }

        // User(1) : Order(N)
        public ICollection<Order> Orders { get; set; }
    }
}