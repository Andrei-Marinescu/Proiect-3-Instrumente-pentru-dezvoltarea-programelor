using Microsoft.AspNetCore.Mvc.Rendering;
using PCShop.Models;
using System.ComponentModel.DataAnnotations;

namespace PCShop.ViewModels
{
    public class CheckoutViewModel
    {

        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal TotalAmount { get; set; }
        public List<SelectListItem> SavedAddresses { get; set; } = new List<SelectListItem>();
        [Required(ErrorMessage = "Te rog selectează o adresă de livrare.")]
        public int SelectedAddressId { get; set; }

        [Required(ErrorMessage = "Te rog selectează o metodă de plată.")]
        public string PaymentMethod { get; set; }

    }
}
