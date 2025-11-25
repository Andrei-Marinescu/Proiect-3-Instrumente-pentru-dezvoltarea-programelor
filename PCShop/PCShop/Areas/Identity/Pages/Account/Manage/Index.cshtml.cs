using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore; // NECESAR pentru .Include
using PCShop.Models;

namespace PCShop.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly PCShopContext _context; // 1. Injectam Contextul Bazei de date

        public IndexModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            PCShopContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public byte[] ExistingProfilePicture { get; set; }

        // Lista de adrese pentru afisare
        public IList<Address> ExistingAddresses { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        // Model separat pentru Adresa (ca sa nu se incurce validarea cu profilul)
        [BindProperty]
        public AddressInputModel AddressInput { get; set; }

        public class InputModel
        {
            [Display(Name = "Număr de telefon")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Prenume")]
            public string FirstName { get; set; }

            [Display(Name = "Nume de familie")]
            public string LastName { get; set; }

            [Display(Name = "Poză de profil")]
            public IFormFile ProfilePicture { get; set; }
        }

        public class AddressInputModel
        {
            [Required(ErrorMessage = "Orașul este obligatoriu")]
            [Display(Name = "Oraș")]
            public string City { get; set; }

            [Required(ErrorMessage = "Strada este obligatorie")]
            [Display(Name = "Stradă")]
            public string Street { get; set; }

            [Display(Name = "Bloc")]
            public string ApartmentBlock { get; set; }

            [Display(Name = "Număr Apartament")]
            public int ApartmentNumber { get; set; }
        }

        private async Task LoadAsync(User user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;
            ExistingProfilePicture = user.AvatarImage;

            // Incarcam datele profilului
            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName
            };

            // Incarcam lista de adrese
            // Navigam prin tabela de legatura UserAdress -> Address
            ExistingAddresses = await _context.UserAdresses
                .Where(ua => ua.UserId == user.Id)
                .Select(ua => ua.Address)
                .ToListAsync();
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            await LoadAsync(user);
            return Page();
        }

        // HANDLER 1: Actualizare Profil (Default)
        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            // Validam DOAR Input-ul de profil, ignoram erorile de la AddressInput daca sunt goale
            // Dar aici ModelState va valida tot ce e [BindProperty].
            // Putem verifica manual sau ignora erorile specifice adresei.

            bool hasChanges = false;

            if (Input.PhoneNumber != user.PhoneNumber)
            {
                user.PhoneNumber = Input.PhoneNumber;
                hasChanges = true;
            }
            if (Input.FirstName != user.FirstName)
            {
                user.FirstName = Input.FirstName;
                hasChanges = true;
            }
            if (Input.LastName != user.LastName)
            {
                user.LastName = Input.LastName;
                hasChanges = true;
            }
            if (Input.ProfilePicture != null)
            {
                using (var dataStream = new MemoryStream())
                {
                    await Input.ProfilePicture.CopyToAsync(dataStream);
                    user.AvatarImage = dataStream.ToArray();
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    StatusMessage = "Eroare la salvare.";
                    foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
                    await LoadAsync(user);
                    return Page();
                }
                await _signInManager.RefreshSignInAsync(user);
                StatusMessage = "Profil actualizat cu succes!";
            }
            else
            {
                StatusMessage = "Nu au fost detectate modificări la profil.";
            }

            return RedirectToPage();
        }

        // HANDLER 2: Adaugare Adresa
        // Aceasta metoda se apeleaza cand formularul are asp-page-handler="AddAddress"
        public async Task<IActionResult> OnPostAddAddressAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("User not found");

            // Validare simpla manuala pentru adresa
            if (string.IsNullOrWhiteSpace(AddressInput.City) || string.IsNullOrWhiteSpace(AddressInput.Street))
            {
                StatusMessage = "Eroare: Orașul și Strada sunt obligatorii.";
                await LoadAsync(user);
                return Page();
            }

            // 1. Cream obiectul Address
            var newAddress = new Address
            {
                City = AddressInput.City,
                Street = AddressInput.Street,
                ApartmentBlock = AddressInput.ApartmentBlock,
                ApartmentNumber = AddressInput.ApartmentNumber
            };

            // 2. Il salvam in DB pentru a primi un ID
            _context.Addresses.Add(newAddress);
            await _context.SaveChangesAsync();

            // 3. Facem legatura in tabela UserAdress
            var userAdressLink = new UserAdress
            {
                UserId = user.Id,
                AdressId = newAddress.AdressId
            };
            _context.UserAdresses.Add(userAdressLink);
            await _context.SaveChangesAsync();

            StatusMessage = "Adresă nouă adăugată!";
            return RedirectToPage();
        }

        // HANDLER 3: Stergere Adresa (Optional, dar util)
        public async Task<IActionResult> OnPostDeleteAddressAsync(int addressId)
        {
            var user = await _userManager.GetUserAsync(User);

            // Cautam legatura
            var link = await _context.UserAdresses
                .FirstOrDefaultAsync(ua => ua.UserId == user.Id && ua.AdressId == addressId);

            if (link != null)
            {
                // Stergem legatura
                _context.UserAdresses.Remove(link);

                // Optional: Stergem si adresa fizica daca nu mai e folosita de nimeni
                // Dar pentru siguranta stergem doar legatura momentan

                await _context.SaveChangesAsync();
                StatusMessage = "Adresa a fost ștearsă.";
            }

            return RedirectToPage();
        }
    }
}