using Microsoft.AspNetCore.Mvc.Rendering;
using PCShop.Models;
using System.ComponentModel.DataAnnotations;

namespace PCShop.ViewModels
{
    public class CartIndexViewModel
    {
        public Cart? Cart { get; set; }
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal TotalPrice { get; set; }

        public List<SelectListItem>? SavedAddresses { get; set; }

        [Required(ErrorMessage = "Te rog selecteaza o adresa.")]
        public int SelectedAddressId { get; set; }

        [Required(ErrorMessage = "Selecteaza metoda de plata.")]
        public string PaymentMethod { get; set; } = "Ramburs"; 
    }
}