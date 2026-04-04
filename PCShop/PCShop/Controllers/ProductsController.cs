using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PCShop.Helpers;
using PCShop.Models;

namespace PCShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly PCShopContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(PCShopContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Products
        public async Task<IActionResult> Index(int? categoryId, string searchString)
        {
            var productsQuery = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Provider)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId);
            }

            var productsList = await productsQuery.ToListAsync();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                productsList = TfIdfSearch.Search(productsList, searchString);
            }

            ViewData["Categories"] = new SelectList(_context.Categories, "CategoryId", "Name", categoryId);
            ViewData["CurrentSearch"] = searchString;

            return View(productsList);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Provider)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // GET: Products/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name");
            ViewData["ProviderId"] = new SelectList(_context.Providers, "ProviderId", "Name");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("ProductId,Name,Description,QuantityAvailable,Price,ProviderId,CategoryId")] Product product, IFormFile? imageFile, IFormFile? pdfFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await imageFile.CopyToAsync(memoryStream);
                        byte[] fileBytes = memoryStream.ToArray();
                        product.ProductImage = fileBytes;
                        product.OriginalImage = fileBytes; // Salvam si originalul înca de la creare
                    }
                }

                if (pdfFile != null && pdfFile.Length > 0)
                {
                    // Extragere si Compresie LZW pentru Create
                    string rawText = PdfExtractor.ExtractText(pdfFile);
                    List<int> compressedData = MultimediaHelper.CompressLZW(rawText);
                    product.ExtractedPdfText = string.Join(",", compressedData);

                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "pdfs");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + pdfFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await pdfFile.CopyToAsync(fileStream);
                    }

                    product.PdfFilePath = "/pdfs/" + uniqueFileName;
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name");
            ViewData["ProviderId"] = new SelectList(_context.Providers, "ProviderId", "Name");
            return View(product);
        }

        // GET: Products/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
            ViewData["ProviderId"] = new SelectList(_context.Providers, "ProviderId", "Name", product.ProviderId);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Name,Description,QuantityAvailable,Price,ProviderId,CategoryId")] Product product, IFormFile? imageFile, IFormFile? pdfFile, string? mainImageAction, List<string> selectedFilters, float blurRadius = 5f, float rotationDegrees = 90f, int scalePercent = 50, int cropWidth = 200, int cropHeight = 200)
        {
            if (id != product.ProductId) return NotFound();

            if (ModelState.IsValid)
            {
                var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == id);
                if (existingProduct == null) return NotFound();

                // --- 1. procesare de imagine
                if (imageFile != null && imageFile.Length > 0)
                {
                    // incarcarea unei imagini complet noua
                    using (var memoryStream = new MemoryStream())
                    {
                        await imageFile.CopyToAsync(memoryStream);
                        byte[] newBytes = memoryStream.ToArray();
                        product.ProductImage = newBytes;
                        product.OriginalImage = newBytes;
                    }
                }
                else
                {
                    // Daca nu s-a incarcat o imagine noua, le pastram pe cele vechi din BD
                    product.ProductImage = existingProduct.ProductImage;
                    product.OriginalImage = existingProduct.OriginalImage ?? existingProduct.ProductImage;
                }

                if (!string.IsNullOrEmpty(mainImageAction))
                {
                    // Reseteaza imaginea la original
                    if (mainImageAction == "restore" && product.OriginalImage != null)
                    {
                        product.ProductImage = product.OriginalImage;
                    }
                    else if (mainImageAction == "modify" && selectedFilters != null && selectedFilters.Any() && product.ProductImage != null)
                    {
                        // Se aplica filtrele   
                        product.ProductImage = MultimediaHelper.ProcessImage(
                            product.ProductImage,
                            selectedFilters,
                            blurRadius,
                            rotationDegrees,
                            scalePercent,
                            cropWidth,
                            cropHeight
                            );
                    }
                }

                // Compresie LZW
                if (pdfFile != null && pdfFile.Length > 0)
                {
                    // Extragere text si compresie
                    string rawText = PdfExtractor.ExtractText(pdfFile);
                    List<int> compressedData = MultimediaHelper.CompressLZW(rawText);
                    product.ExtractedPdfText = string.Join(",", compressedData);

                    // Salvare fisier fizic
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "pdfs");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    if (!string.IsNullOrEmpty(existingProduct.PdfFilePath))
                    {
                        string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingProduct.PdfFilePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + pdfFile.FileName;
                    string newFilePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(newFilePath, FileMode.Create))
                    {
                        await pdfFile.CopyToAsync(fileStream);
                    }

                    product.PdfFilePath = "/pdfs/" + uniqueFileName;
                }
                else
                {
                    // Pastram pdf-ul vechi
                    product.PdfFilePath = existingProduct.PdfFilePath;
                    product.ExtractedPdfText = existingProduct.ExtractedPdfText;
                }

                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
            ViewData["ProviderId"] = new SelectList(_context.Providers, "ProviderId", "Name", product.ProviderId);
            return View(product);
        }

        // GET: Products/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Provider)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                if (!string.IsNullOrEmpty(product.PdfFilePath))
                {
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, product.PdfFilePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                }
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}