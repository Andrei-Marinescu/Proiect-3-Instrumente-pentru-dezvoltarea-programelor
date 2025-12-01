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
using Microsoft.EntityFrameworkCore; 
using PCShop.Models;

namespace PCShop.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly PCShopContext _context; 

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

        public IList<Address> ExistingAddresses { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

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
            [Display(Name = "Oras")]
            public string City { get; set; }

            [Required(ErrorMessage = "Strada este obligatorie")]
            [Display(Name = "Strada")]
            public string Street { get; set; }

            [Display(Name = "Bloc")]
            public string ApartmentBlock { get; set; }

            [Display(Name = "Numar Apartament")]
            public int ApartmentNumber { get; set; }
        }

        private async Task LoadAsync(User user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;
            ExistingProfilePicture = user.AvatarImage;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName
            };

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


        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

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
                StatusMessage = "Nu au fost detectate modificari la profil.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddAddressAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("User not found");

            if (string.IsNullOrWhiteSpace(AddressInput.City) || string.IsNullOrWhiteSpace(AddressInput.Street))
            {
                StatusMessage = "Eroare: Orasul și Strada sunt obligatorii.";
                await LoadAsync(user);
                return Page();
            }

            var newAddress = new Address
            {
                City = AddressInput.City,
                Street = AddressInput.Street,
                ApartmentBlock = AddressInput.ApartmentBlock,
                ApartmentNumber = AddressInput.ApartmentNumber
            };

            _context.Addresses.Add(newAddress);
            await _context.SaveChangesAsync();

            var userAdressLink = new UserAdress
            {
                UserId = user.Id,
                AdressId = newAddress.AdressId
            };
            _context.UserAdresses.Add(userAdressLink);
            await _context.SaveChangesAsync();

            StatusMessage = "Adresa noua adaugata!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAddressAsync(int addressId)
        {
            var user = await _userManager.GetUserAsync(User);

            var link = await _context.UserAdresses
                .FirstOrDefaultAsync(ua => ua.UserId == user.Id && ua.AdressId == addressId);

            if (link != null)
            {
                _context.UserAdresses.Remove(link);

                await _context.SaveChangesAsync();
                StatusMessage = "Adresa a fost stearsa.";
            }

            return RedirectToPage();
        }
    }
}