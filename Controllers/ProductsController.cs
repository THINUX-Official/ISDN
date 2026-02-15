using Microsoft.AspNetCore.Mvc;
using ISDN.Repositories;
using ISDN.Models;
using Microsoft.AspNetCore.Authorization;

namespace ISDN.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(IProductRepository productRepository, IWebHostEnvironment webHostEnvironment)
        {
            _productRepository = productRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        // Public Catalogue
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetActiveProductsAsync();
            return View(products);
        }

        // Admin Management
        // [Authorize(Roles = "Admin")] // Uncomment when Auth is fully ready
        public async Task<IActionResult> Manage()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
                    Directory.CreateDirectory(uploadsFolder); // Ensure directory exists
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }
                    product.ImageRelativePath = "/images/products/" + uniqueFileName;
                }

                await _productRepository.CreateAsync(product);
                TempData["SuccessMessage"] = "Product created successfully!";
                return RedirectToAction(nameof(Manage));
            }
            return View(product);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile)
        {
            if (id != product.ProductId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _productRepository.GetByIdAsync(id);
                    if (existingProduct == null) return NotFound();

                    // Update properties
                    existingProduct.ProductName = product.ProductName;
                    existingProduct.Description = product.Description;
                    existingProduct.UnitPrice = product.UnitPrice;
                    existingProduct.StockQuantity = product.StockQuantity;
                    existingProduct.Category = product.Category;
                    existingProduct.IsActive = product.IsActive;

                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Upload new image
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
                        Directory.CreateDirectory(uploadsFolder);
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(existingProduct.ImageRelativePath))
                        {
                            string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, existingProduct.ImageRelativePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        existingProduct.ImageRelativePath = "/images/products/" + uniqueFileName;
                    }

                    await _productRepository.UpdateAsync(existingProduct);
                    TempData["SuccessMessage"] = "Product updated successfully!";
                }
                catch (Exception ex)
                {
                    // Log error
                    ModelState.AddModelError("", "Unable to save changes. Try again." + ex.Message);
                    return View(product);
                }
                return RedirectToAction(nameof(Manage));
            }
            return View(product);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product != null)
            {
                if (!string.IsNullOrEmpty(product.ImageRelativePath))
                {
                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageRelativePath.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }
                await _productRepository.DeleteAsync(id);
                TempData["SuccessMessage"] = "Product deleted successfully!";
            }
            return RedirectToAction(nameof(Manage));
        }
    }
}