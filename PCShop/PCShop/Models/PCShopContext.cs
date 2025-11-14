using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace PCShop.Models
{
    public class PCShopContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public PCShopContext(DbContextOptions<PCShopContext> options) : base(options) { }

        public DbSet<User>? Users { get; set; }
        public DbSet<Address>? Addresses { get; set; }
        public DbSet<UserAdress>? UserAdresses { get; set; }
        public DbSet<Cart>? Carts { get; set; }
        public DbSet<CartItem>? CartItems { get; set; }
        public DbSet<Product>? Products { get; set; }
        public DbSet<Provider>? Providers { get; set; }
        public DbSet<Category>? Categories { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }    
    }
}
