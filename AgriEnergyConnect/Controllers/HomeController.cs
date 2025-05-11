using AgriEnergyConnect.Data;
using AgriEnergyConnect.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace AgriEnergyConnect.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(string category, string area)
        {
            // Start with all products
            var productsQuery = _context.Products
                .Include(p => p.Farmer)
                .AsQueryable();

            // Apply category filter if provided
            if (!string.IsNullOrEmpty(category))
            {
                productsQuery = productsQuery.Where(p => p.Category == category);
                ViewData["CurrentCategory"] = category;
            }

            // Apply area filter if provided
            if (!string.IsNullOrEmpty(area))
            {
                productsQuery = productsQuery.Where(p => p.Farmer.Area == area);
                ViewData["CurrentArea"] = area;
            }

            // Get distinct categories for dropdown
            ViewData["Categories"] = await _context.Products
                .Select(p => p.Category)
                .Distinct()
                .ToListAsync();

            // Get distinct areas for dropdown
            ViewData["Areas"] = await _context.Farmers
                .Select(f => f.Area)
                .Distinct()
                .ToListAsync();

            var products = await productsQuery.ToListAsync();
            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}