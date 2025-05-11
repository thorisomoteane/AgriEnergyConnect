using AgriEnergyConnect.Data;
using AgriEnergyConnect.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AgriEnergyConnect.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Username and password are required.";
                return View();
            }

            // Find the user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

            if (user == null)
            {
                ViewBag.ErrorMessage = "Invalid login attempt.";
                return View();
            }

            // Create claims
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role),
        new Claim("UserId", user.Id.ToString())
    };

            if (user.FarmerId.HasValue)
            {
                claims.Add(new Claim("FarmerId", user.FarmerId.Value.ToString()));
            }

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Set cookie to persist
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(3) // Set expiration time
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (user.Role == "Farmer")
            {
                if (user.FarmerId.HasValue)
                {
                    return RedirectToAction("Index", "Farmer");
                }
                else
                {
                    return RedirectToAction("CreateProfile", "Farmer");
                }
            }
            else
            {
                return RedirectToAction("Index", "Employee");
            }
        }

        // GET: /Account/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(string username, string password, string role)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Username and password are required.";
                return View();
            }

            // Check if user already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (existingUser != null)
            {
                ViewBag.ErrorMessage = "Username already exists.";
                return View();
            }

            // Create new user
            var user = new User
            {
                Username = username,
                Password = password,
                Role = role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }
    }
}