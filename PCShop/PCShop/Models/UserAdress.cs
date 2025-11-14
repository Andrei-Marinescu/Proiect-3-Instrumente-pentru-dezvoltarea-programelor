using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PCShop.Models
{
    public class UserAdress
    {
        [Key]
        public int IdUserAdress { get; set; }
        public int UserId {  get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        public int AdressId { get; set; }
        [ForeignKey("AdressId")]
        public Address Address { get; set; }
    }
}
