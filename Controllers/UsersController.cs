using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Homera.Data;
using Homera.Models;
using Homera.Models.Enums;

namespace Homera.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public UsersController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole<int>> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        private async Task PopulateRolesDropdown(string? selectedRole = null)
        {
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            // Ensure our default roles are present if not found in DB for some reason
            var defaultRoles = new List<string> { UserRole.Administrator, UserRole.Housekeeper, UserRole.Client };
            foreach (var dr in defaultRoles)
            {
                if (!roles.Contains(dr)) roles.Add(dr);
            }

            ViewBag.Roles = new SelectList(roles, selectedRole);
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRoles = new Dictionary<int, string>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = string.Join(", ", roles);
            }

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null) return NotFound();

            if (string.IsNullOrEmpty(user.UserName)) return NotFound();
            var identityUser = await _userManager.FindByNameAsync(user.UserName);
            if (identityUser != null)
            {
                var roles = await _userManager.GetRolesAsync(identityUser);
                ViewBag.UserRole = string.Join(", ", roles);
            }
            else
            {
                ViewBag.UserRole = "No roles";
            }

            return View(user);
        }

        // GET: Users/Create
        public async Task<IActionResult> Create()
        {
            await PopulateRolesDropdown();
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("FirstName,LastName,UserName")] User user,
            string role,
            string password)
        {
            if (ModelState.IsValid)
            {
                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(role))
                    {
                        if (!await _roleManager.RoleExistsAsync(role))
                            await _roleManager.CreateAsync(new IdentityRole<int>(role));

                        await _userManager.AddToRoleAsync(user, role);
                    }
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            await PopulateRolesDropdown(role);
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id.ToString()!);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await PopulateRolesDropdown(currentRoles.FirstOrDefault());
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,FirstName,LastName,UserName")] User user,
            string role)
        {
            if (id != user.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByIdAsync(id.ToString());
                if (existingUser == null) return NotFound();

                // Update properties
                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.UserName = user.UserName;

                var result = await _userManager.UpdateAsync(existingUser);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(role))
                    {
                        var currentRoles = await _userManager.GetRolesAsync(existingUser);
                        if (!currentRoles.Contains(role))
                        {
                            await _userManager.RemoveFromRolesAsync(existingUser, currentRoles);
                            await _userManager.AddToRoleAsync(existingUser, role);
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            await PopulateRolesDropdown(role);
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null) return NotFound();

            if (user.UserName != null)
            {
                var identityUser = await _userManager.FindByNameAsync(user.UserName); 
                if (identityUser != null)
                {
                    var roles = await _userManager.GetRolesAsync(identityUser);
                    ViewBag.UserRole = string.Join(", ", roles);
                }
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
