using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PCShop.Models;

namespace PCShop.Controllers
{
    public class UserAdressesController : Controller
    {
        private readonly PCShopContext _context;

        public UserAdressesController(PCShopContext context)
        {
            _context = context;
        }

        // GET: UserAdresses
        public async Task<IActionResult> Index()
        {
            var pCShopContext = _context.UserAdresses.Include(u => u.Address).Include(u => u.User);
            return View(await pCShopContext.ToListAsync());
        }

        // GET: UserAdresses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userAdress = await _context.UserAdresses
                .Include(u => u.Address)
                .Include(u => u.User)
                .FirstOrDefaultAsync(m => m.IdUserAdress == id);
            if (userAdress == null)
            {
                return NotFound();
            }

            return View(userAdress);
        }

        // GET: UserAdresses/Create
        public IActionResult Create()
        {
            ViewData["AdressId"] = new SelectList(_context.Addresses, "AdressId", "AdressId");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: UserAdresses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdUserAdress,UserId,AdressId")] UserAdress userAdress)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userAdress);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AdressId"] = new SelectList(_context.Addresses, "AdressId", "AdressId", userAdress.AdressId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", userAdress.UserId);
            return View(userAdress);
        }

        // GET: UserAdresses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userAdress = await _context.UserAdresses.FindAsync(id);
            if (userAdress == null)
            {
                return NotFound();
            }
            ViewData["AdressId"] = new SelectList(_context.Addresses, "AdressId", "AdressId", userAdress.AdressId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", userAdress.UserId);
            return View(userAdress);
        }

        // POST: UserAdresses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdUserAdress,UserId,AdressId")] UserAdress userAdress)
        {
            if (id != userAdress.IdUserAdress)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userAdress);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserAdressExists(userAdress.IdUserAdress))
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
            ViewData["AdressId"] = new SelectList(_context.Addresses, "AdressId", "AdressId", userAdress.AdressId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", userAdress.UserId);
            return View(userAdress);
        }

        // GET: UserAdresses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userAdress = await _context.UserAdresses
                .Include(u => u.Address)
                .Include(u => u.User)
                .FirstOrDefaultAsync(m => m.IdUserAdress == id);
            if (userAdress == null)
            {
                return NotFound();
            }

            return View(userAdress);
        }

        // POST: UserAdresses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userAdress = await _context.UserAdresses.FindAsync(id);
            if (userAdress != null)
            {
                _context.UserAdresses.Remove(userAdress);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserAdressExists(int id)
        {
            return _context.UserAdresses.Any(e => e.IdUserAdress == id);
        }
    }
}
