using AgriEnergyConnect.Data;
using AgriEnergyConnect.Models;
using AgriEnergyConnect.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AgriEnergyConnect.Controllers
{
    [Authorize(Roles = "Farmer")]
    public class FarmerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public FarmerController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Farmer
        public async Task<IActionResult> Index()
        {
            // Get current farmer's ID
            var farmerId = GetCurrentFarmerId();
            if (farmerId == 0)
            {
                return RedirectToAction("CreateProfile");
            }

            // Get farmer's products
            var products = await _context.Products
                .Where(p => p.FarmerId == farmerId)
                .ToListAsync();

            return View(products);
        }

        // GET: Farmer/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var farmerId = GetCurrentFarmerId();
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.FarmerId == farmerId);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Farmer/Create
        public IActionResult Create()
        {
            return View(new CreateProductViewModel());
        }

        // POST: Farmer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductViewModel viewModel, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                // Log validation errors
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Count > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in {state.Key}: {state.Value.Errors[0].ErrorMessage}");
                        ViewBag.ErrorMessage = $"Validation error in {state.Key}: {state.Value.Errors[0].ErrorMessage}";
                    }
                }
                return View(viewModel);
            }

            try
            {
                var farmerId = GetCurrentFarmerId();
                if (farmerId == 0)
                {
                    ViewBag.ErrorMessage = "Unable to determine your farmer profile. Please create a profile first.";
                    return RedirectToAction("CreateProfile");
                }

                // Convert view model to entity
                var product = viewModel.ToProduct();
                product.FarmerId = farmerId;

                System.Diagnostics.Debug.WriteLine($"Creating product with FarmerId: {product.FarmerId}");

                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    try
                    {
                        // Create a unique filename
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

                        // Ensure directory exists
                        var uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Save the file
                        var filePath = Path.Combine(uploadsFolder, fileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        // Save the path to the database
                        product.ImagePath = "/uploads/" + fileName;
                        System.Diagnostics.Debug.WriteLine($"Image saved at: {product.ImagePath}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error saving image: {ex.Message}");
                        ViewBag.ErrorMessage = $"Error saving image: {ex.Message}";
                        return View(viewModel);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No image file uploaded");
                    product.ImagePath = null; // Ensure it's null if no image
                }

                System.Diagnostics.Debug.WriteLine($"Adding product: {product.Name}, FarmerId: {product.FarmerId}");

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine("Product added successfully");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating product: {ex.Message}");
                ViewBag.ErrorMessage = $"Error: {ex.Message}";
                return View(viewModel);
            }
        }

        // GET: Farmer/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var farmerId = GetCurrentFarmerId();
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.FarmerId == farmerId);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Farmer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            var farmerId = GetCurrentFarmerId();
            product.FarmerId = farmerId;

            if (!ModelState.IsValid)
            {
                // Log validation errors
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Count > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in {state.Key}: {state.Value.Errors[0].ErrorMessage}");
                        ViewBag.ErrorMessage = $"Validation error in {state.Key}: {state.Value.Errors[0].ErrorMessage}";
                    }
                }
                return View(product);
            }

            try
            {
                // Get existing product to check ownership and get existing image path
                var existingProduct = await _context.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id && p.FarmerId == farmerId);

                if (existingProduct == null)
                {
                    return NotFound();
                }

                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    try
                    {
                        // Create a unique filename
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

                        // Ensure directory exists
                        var uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Save the file
                        var filePath = Path.Combine(uploadsFolder, fileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(existingProduct.ImagePath))
                        {
                            var oldFilePath = Path.Combine(_hostEnvironment.WebRootPath, existingProduct.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Save the path to the database
                        product.ImagePath = "/uploads/" + fileName;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error saving image: {ex.Message}");
                        ViewBag.ErrorMessage = $"Error saving image: {ex.Message}";
                        return View(product);
                    }
                }
                else
                {
                    // Keep the existing image if no new one is uploaded
                    product.ImagePath = existingProduct.ImagePath;
                }

                _context.Update(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ProductExists(product.Id))
                {
                    return NotFound();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Concurrency error: {ex.Message}");
                    ViewBag.ErrorMessage = "The record was modified by another user. Please try again.";
                    return View(product);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error editing product: {ex.Message}");
                ViewBag.ErrorMessage = $"Error: {ex.Message}";
                return View(product);
            }
        }

        // GET: Farmer/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var farmerId = GetCurrentFarmerId();
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.FarmerId == farmerId);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Farmer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var farmerId = GetCurrentFarmerId();
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.FarmerId == farmerId);

            if (product == null)
            {
                return NotFound();
            }

            try
            {
                // Delete image file if exists
                if (!string.IsNullOrEmpty(product.ImagePath))
                {
                    var filePath = Path.Combine(_hostEnvironment.WebRootPath, product.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting product: {ex.Message}");
                ViewBag.ErrorMessage = $"Error: {ex.Message}";
                return View(product);
            }
        }

        // GET: Farmer/CreateProfile
        public IActionResult CreateProfile()
        {
            return View();
        }

        // POST: Farmer/CreateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProfile(Farmer farmer)
        {
            // Make sure we initialize the Products collection to avoid null reference errors
            if (farmer.Products == null)
            {
                farmer.Products = new List<Product>();
            }

            // Check for model validation errors, but exclude the Products property
            ModelState.Remove("Products"); // Remove Products from model validation

            if (!ModelState.IsValid)
            {
                // If there are still other validation errors, return to the view
                return View(farmer);
            }

            try
            {
                // Add farmer to database
                _context.Add(farmer);
                await _context.SaveChangesAsync();

                // Get the current user ID from claims
                var userId = User.FindFirstValue("UserId");

                if (string.IsNullOrEmpty(userId))
                {
                    // If userId is null, there's an issue with the claims
                    ViewBag.ErrorMessage = "User ID not found in claims. Please log out and log in again.";
                    return View(farmer);
                }

                if (!int.TryParse(userId, out int id))
                {
                    // If userId can't be parsed to an integer
                    ViewBag.ErrorMessage = "Invalid User ID format.";
                    return View(farmer);
                }

                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    // If user not found in database
                    ViewBag.ErrorMessage = "User not found in database.";
                    return View(farmer);
                }

                // Update the user with the farmer id
                user.FarmerId = farmer.Id;
                _context.Update(user);
                await _context.SaveChangesAsync();

                // Redirect to the Index action
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Error creating farmer profile: {ex.Message}");

                // Add error message to view
                ViewBag.ErrorMessage = $"An error occurred: {ex.Message}";
                return View(farmer);
            }
        }

        // GET: Farmer/TestForm
        [HttpGet]
        public IActionResult TestForm()
        {
            return View();
        }

        // POST: Farmer/TestForm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TestForm(string testName)
        {
            ViewBag.SubmissionResult = $"Form submitted successfully with value: {testName}";
            return View();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        private int GetCurrentFarmerId()
        {
            try
            {
                // Try to get FarmerId from claims
                var farmerIdClaim = User.FindFirstValue("FarmerId");
                System.Diagnostics.Debug.WriteLine($"FarmerId claim: {farmerIdClaim}");

                if (!string.IsNullOrEmpty(farmerIdClaim) && int.TryParse(farmerIdClaim, out int farmerId))
                {
                    System.Diagnostics.Debug.WriteLine($"Found FarmerId in claims: {farmerId}");
                    return farmerId;
                }

                // If no FarmerId in claims, try to find by User ID
                var userId = User.FindFirstValue("UserId");
                System.Diagnostics.Debug.WriteLine($"UserId claim: {userId}");

                if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int id))
                {
                    var user = _context.Users.FirstOrDefault(u => u.Id == id);
                    if (user != null && user.FarmerId.HasValue)
                    {
                        System.Diagnostics.Debug.WriteLine($"Found FarmerId from user: {user.FarmerId.Value}");
                        return user.FarmerId.Value;
                    }
                }

                // As a last resort, try to find by username
                var username = User.Identity.Name;
                System.Diagnostics.Debug.WriteLine($"Username: {username}");

                if (!string.IsNullOrEmpty(username))
                {
                    var user = _context.Users.FirstOrDefault(u => u.Username == username);
                    if (user != null && user.FarmerId.HasValue)
                    {
                        System.Diagnostics.Debug.WriteLine($"Found FarmerId from username: {user.FarmerId.Value}");
                        return user.FarmerId.Value;
                    }
                }

                System.Diagnostics.Debug.WriteLine("Could not determine FarmerId");
                return 0; // Return 0 if no farmer ID is found
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetCurrentFarmerId: {ex.Message}");
                return 0;
            }
        }
    }
}