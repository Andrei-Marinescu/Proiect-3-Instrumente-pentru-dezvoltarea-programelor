using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PCShop.Models;
using PCShop.ViewModels; 
using System.Linq; 
using System.Threading.Tasks;

namespace PCShop.Controllers
{
    [Authorize]
    public class CartsController : Controller
    {
        private readonly PCShopContext _context;
        private readonly UserManager<User> _userManager;

        public CartsController(PCShopContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Extragem cosul si produsele din cos
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            // Extragem adresele utilizatorului
            var userAddresses = await _context.UserAdresses
                .Where(ua => ua.UserId == user.Id)
                .Include(ua => ua.Address)
                .Select(ua => ua.Address) // Extragem obiectul Address din relatia UserAdress
                .ToListAsync();

            // Calculam Totalul Cosului
            decimal total = 0;
            if (cart?.CartItems != null)
            {
                total = cart.CartItems.Sum(i => i.Product.Price * (i.Quantity ?? 1));
            }

            // Construim ViewModel-ul pentru pagina Checkout
            var viewModel = new CartIndexViewModel
            {
                Cart = cart,
                TotalPrice = total,
                SavedAddresses = userAddresses.Select(a => new SelectListItem
                {
                    Value = a.AdressId.ToString(),
                    Text = $"{a.City}, {a.Street}, Bl. {a.ApartmentBlock}, Ap. {a.ApartmentNumber}"
                }).ToList()
            };

            return View(viewModel);
        }

        //Adaugarea in cos
        public async Task<IActionResult> AddToCart(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new Cart { UserId = user.Id };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var cartItem = cart.CartItems?.FirstOrDefault(ci => ci.ProductId == productId);

            if (cartItem != null)
            {
                cartItem.Quantity = (cartItem.Quantity ?? 0) + 1;
            }
            else
            {
                var newItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = productId,
                    Quantity = 1,
                    Price = product.Price
                };
                _context.CartItems.Add(newItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Stergerea din cos
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        //Actualizeaza Cantitatea (+ sau -)
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                if (quantity > 0)
                {
                    cartItem.Quantity = quantity;
                }
                else
                {
                    _context.CartItems.Remove(cartItem);
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}