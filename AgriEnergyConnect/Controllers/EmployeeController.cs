using AgriEnergyConnect.Data;
using AgriEnergyConnect.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgriEnergyConnect.Controllers
{
    [Authorize(Roles = "Employee")]
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Employee
        public async Task<IActionResult> Index()
        {
            var farmers = await _context.Farmers.ToListAsync();
            return View(farmers);
        }

        // GET: Employee/FarmerDetails/5
        public async Task<IActionResult> FarmerDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var farmer = await _context.Farmers
                .Include(f => f.Products)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (farmer == null)
            {
                return NotFound();
            }

            return View(farmer);
        }

        // GET: Employee/AddFarmer
        public IActionResult AddFarmer()
        {
            return View();
        }

        // POST: Employee/AddFarmer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFarmer(Farmer farmer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(farmer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(farmer);
        }

        // GET: Employee/ProductDetails/5
        public async Task<IActionResult> ProductDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Farmer)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
    }
}