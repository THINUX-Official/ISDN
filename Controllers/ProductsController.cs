using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ISDN.Constants;
using ISDN.Repositories;
using ISDN.Models;

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

        // Public Product Catalogue - Anyone can view
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetActiveProductsAsync();
            return View(products);
        }

        // Admin & RDC Staff Product Management Dashboard
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.RdcStaff)] // ✅ Added RDC_STAFF
        public async Task<IActionResult> Manage()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }

        // GET: Products/Create
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.RdcStaff)] // ✅ Added RDC_STAFF
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.RdcStaff)] // ✅ Added RDC_STAFF
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("", "Only image files (.jpg, .jpeg, .png, .gif, .webp) are allowed.");
                        return View(product);
                    }

                    // Create directory if it doesn't exist
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
                    Directory.CreateDirectory(uploadsFolder);

                    // Generate unique filename
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    // Store relative path in database
                    product.ProductImageUrl = "/images/products/" + uniqueFileName;
                }

                // Set timestamps
                product.CreatedAt = DateTime.UtcNow;
                product.IsActive = true;

                await _productRepository.CreateAsync(product);

                TempData["SuccessMessage"] = $"Product '{product.ProductName}' created successfully!";
                return RedirectToAction(nameof(Manage));
            }

            return View(product);
        }

        // GET: Products/Edit/5
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.RdcStaff)] // ✅ Added RDC_STAFF
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)        
            {
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToAction(nameof(Manage));
            }
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.RdcStaff)] // ✅ Added RDC_STAFF
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _productRepository.GetByIdAsync(id);
                    if (existingProduct == null)
                    {
                        TempData["ErrorMessage"] = "Product not found.";
                        return RedirectToAction(nameof(Manage));
                    }

                    // Update properties
                    existingProduct.ProductName = product.ProductName;
                    existingProduct.Description = product.Description;
                    existingProduct.Sku = product.Sku;
                    existingProduct.UnitPrice = product.UnitPrice;
                    existingProduct.Category = product.Category;
                    existingProduct.IsActive = product.IsActive;

                    // Handle new image upload
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Validate file type
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                        var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("", "Only image files are allowed.");
                            return View(product);
                        }

                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(existingProduct.ProductImageUrl))
                        {
                            string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath,
                                existingProduct.ProductImageUrl.TrimStart('/').Replace("/", "\\"));

                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Upload new image
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
                        Directory.CreateDirectory(uploadsFolder);
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        existingProduct.ProductImageUrl = "/images/products/" + uniqueFileName;
                    }

                    await _productRepository.UpdateAsync(existingProduct);

                    TempData["SuccessMessage"] = $"Product '{existingProduct.ProductName}' updated successfully!";
                    return RedirectToAction(nameof(Manage));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Unable to save changes: {ex.Message}");
                    return View(product);
                }
            }

            return View(product);
        }

        // GET: Products/Delete/5
        [Authorize(Roles = UserRoles.Admin)] // ⚠️ ADMIN ONLY - Don't let RDC Staff delete
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToAction(nameof(Manage));
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Admin)] // ⚠️ ADMIN ONLY - Don't let RDC Staff delete
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product != null)
                {
                    // Delete image file if exists
                    if (!string.IsNullOrEmpty(product.ProductImageUrl))
                    {
                        string imagePath = Path.Combine(_webHostEnvironment.WebRootPath,
                            product.ProductImageUrl.TrimStart('/').Replace("/", "\\"));

                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }

                    await _productRepository.DeleteAsync(id);
                    TempData["SuccessMessage"] = $"Product '{product.ProductName}' deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Product not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Unable to delete product: {ex.Message}";
            }

            return RedirectToAction(nameof(Manage));
        }

        // GET: Products/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
    }
}
