using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PCShop.Models;
using PCShop.ViewModels;

namespace PCShop.Controllers
{
    public class OrdersController : Controller
    {
        private readonly PCShopContext _context;
        private readonly UserManager<User> _userManager;
        public OrdersController(PCShopContext context, UserManager<User> userManager) 
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var pCShopContext = _context.Orders.Include(o => o.Address).Include(o => o.User);
            return View(await pCShopContext.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Address)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["AdressId"] = new SelectList(_context.Addresses, "AdressId", "AdressId");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,OrderDate,TotalAmount,PaymentMethode,UserId,AdressId")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AdressId"] = new SelectList(_context.Addresses, "AdressId", "AdressId", order.AdressId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["AdressId"] = new SelectList(_context.Addresses, "AdressId", "AdressId", order.AdressId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,OrderDate,TotalAmount,PaymentMethode,UserId,AdressId")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AdressId"] = new SelectList(_context.Addresses, "AdressId", "AdressId", order.AdressId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Address)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }

        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == user.Id);

            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Cosul tau este gol.";
                return RedirectToAction("Index", "Carts");
            }

            var userAdresses = await _context.UserAdresses
                .Include(ua => ua.Address)
                .Where(ua => ua.UserId == user.Id)
                .Include(ua => ua.Address)
                .Select(ua => ua.Address)
                .ToListAsync();

            decimal total = 0;

            if (cart?.CartItems != null)
                total = cart.CartItems.Sum(i => i.Product.Price * (i.Quantity ?? 1));

            var viewModel = new CartIndexViewModel
            {
                Cart = cart,
                TotalPrice = total,
                SavedAddresses = userAdresses.Select(a => new SelectListItem
                {
                    Value = a.AdressId.ToString(),
                    Text = $"{a.City}, {a.Street}, Bl. {a.ApartmentBlock}"
                }).ToList()
            };

            return View(viewModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CartIndexViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id); 

            if (cart == null || !cart.CartItems.Any())
                return RedirectToAction("Index", "Products");

            if (ModelState.IsValid)
            {
                var order = new Order
                {
                    UserId = user.Id,
                    AdressId = model.SelectedAddressId,
                    OrderDate = DateTime.UtcNow,
                    PaymentMethode = model.PaymentMethod,
                    TotalAmount = (int)cart.CartItems.Sum(i => i.Product.Price * (i.Quantity ?? 1))
                };

                _context.Add(order);
                await _context.SaveChangesAsync();

                foreach (var item in cart.CartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Product.Price
                    };
                    _context.Add(orderItem);
                }

                _context.CartItems.RemoveRange(cart.CartItems);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(OrderConfirmation), new { id = order.OrderId });
            }

            var userAdresses = await _context.UserAdresses
                .Where(ua => ua.UserId == user.Id)
                .Include(ua => ua.Address)
                .Select(ua => ua.Address)
                .ToListAsync();

            model.SavedAddresses = userAdresses.Select(a => new SelectListItem
            {
                Value = a.AdressId.ToString(),
                Text = $"{a.City}, {a.Street}, Bl. {a.ApartmentBlock}"  
            }).ToList();

            model.CartItems = cart.CartItems.ToList();

            return View(model);
        }

        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Address)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            return View(order);
        }
        
    }
}
