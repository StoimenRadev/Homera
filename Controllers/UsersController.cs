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
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        private void PopulateRolesDropdown()
        {
            var roles = new List<string>
            {
                UserRole.Administrator,
                UserRole.Housekeeper,
                UserRole.Client
            };
            ViewBag.Roles = new SelectList(roles);
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null) return NotFound();

            var identityUser = await _userManager.FindByNameAsync(user.Username);
            if (identityUser != null)
            {
                var roles = await _userManager.GetRolesAsync(identityUser);
                ViewBag.UserRole = string.Join(", ", roles);
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            PopulateRolesDropdown();
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,FirstName,LastName,Username,Password")] User user,
            string role)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(role))
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                        await _roleManager.CreateAsync(new IdentityRole(role));

                    var identityUser = await _userManager.FindByNameAsync(user.Username);
                    if (identityUser != null)
                        await _userManager.AddToRoleAsync(identityUser, role);
                }

                return RedirectToAction(nameof(Index));
            }
            PopulateRolesDropdown();
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            PopulateRolesDropdown();
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,FirstName,LastName,Username,Password")] User user,
            string role)
        {
            if (id != user.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    if (!string.IsNullOrEmpty(role))
                    {
                        if (!await _roleManager.RoleExistsAsync(role))
                            await _roleManager.CreateAsync(new IdentityRole(role));

                        var identityUser = await _userManager.FindByNameAsync(user.Username);
                        if (identityUser != null)
                        {
                            var currentRoles = await _userManager.GetRolesAsync(identityUser);
                            await _userManager.RemoveFromRolesAsync(identityUser, currentRoles);
                            await _userManager.AddToRoleAsync(identityUser, role);
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateRolesDropdown();
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null) return NotFound();

            var identityUser = await _userManager.FindByNameAsync(user.Username);
            if (identityUser != null)
            {
                var roles = await _userManager.GetRolesAsync(identityUser);
                ViewBag.UserRole = string.Join(", ", roles);
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
