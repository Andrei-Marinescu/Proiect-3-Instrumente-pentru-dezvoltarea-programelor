using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace PCShop.Models
{
    public class Order
    {
        [Key] public int OrderId { get; set; }
        public DateTime OrderDate {  get; set; }
        public int? TotalAmount { get; set; }
        public string PaymentMethode { get; set; }
        // User(1) : Orders(N)
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        // Order(1) : Adress(1)
        public int AdressId { get; set; }
        [ForeignKey("AdressId")]
        public Address Address { get; set; }

        // Order(1) : OrderItem(N) | Order(N) : Product(M)
        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}
