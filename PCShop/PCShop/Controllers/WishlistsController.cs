using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PCShop.Models;

namespace PCShop.Controllers
{
    [Authorize]
    public class WishlistsController : Controller
    {
        private readonly PCShopContext _context;
        private readonly UserManager<User> _userManager;

        public WishlistsController(PCShopContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var myWishlist = await _context.Wishlists
                .Include(w => w.Product)
                    .ThenInclude(p => p.Category) 
                .Include(w => w.Product)
                    .ThenInclude(p => p.Provider) 
                .Where(w => w.UserId == user.Id)
                .ToListAsync();

            return View(myWishlist);
        }

        // GET: Wishlists/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var wishlist = await _context.Wishlists
                .Include(w => w.Product)
                .Include(w => w.User)
                .FirstOrDefaultAsync(m => m.WishlistId == id);

            if (wishlist == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (wishlist.UserId != user.Id) return Forbid();

            return View(wishlist);
        }

        // Adaugare / Stergere produs din wishlist
        public async Task<IActionResult> ToggleWishlist(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var existingItem = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == user.Id && w.ProductId == productId);

            if (existingItem != null)
            {
                _context.Wishlists.Remove(existingItem);
            }
            else
            {
                var newItem = new Wishlist
                {
                    UserId = user.Id,
                    ProductId = productId
                };
                _context.Wishlists.Add(newItem);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Wishlists/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var wishlist = await _context.Wishlists
                .Include(w => w.Product)
                .Include(w => w.User)
                .FirstOrDefaultAsync(m => m.WishlistId == id);

            if (wishlist == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (wishlist.UserId != user.Id) return Forbid();

            return View(wishlist);
        }

        // POST: Wishlists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var wishlist = await _context.Wishlists.FindAsync(id);

            var user = await _userManager.GetUserAsync(User);
            if (wishlist != null && wishlist.UserId == user.Id)
            {
                _context.Wishlists.Remove(wishlist);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool WishlistExists(int id)
        {
            return _context.Wishlists.Any(e => e.WishlistId == id);
        }
        public async Task<IActionResult> Remove(int id)
        {
            var wishlistItem = await _context.Wishlists.FindAsync(id);

            if (wishlistItem != null)
            {

                var user = await _userManager.GetUserAsync(User);

                if (user != null && wishlistItem.UserId == user.Id)
                {
                    _context.Wishlists.Remove(wishlistItem);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}