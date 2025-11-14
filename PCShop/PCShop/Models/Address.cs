using System.ComponentModel.DataAnnotations;

namespace PCShop.Models
{
    public class Address
    {
        [Key]
        public int AdressId { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string ApartmentBlock { get; set; }
        public int ApartmentNumber {  get; set; }
        // Adress(1) : UserAdress(N)
        public ICollection<UserAdress> UserAdresses { get; set; }
    }
}
