using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Homera.Data;
using Homera.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Homera.Models.Enums;

namespace Homera.Controllers
{
    [Authorize]
    public class LocationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public LocationsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Locations
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            IQueryable<Location> locations = _context.Locations.Include(l => l.Client);
            
            if (!await _userManager.IsInRoleAsync(user, UserRole.Administrator))
            {
                locations = locations.Where(l => l.ClientId == user.Id);
            }

            return View(await locations.ToListAsync());
        }

        // GET: Locations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var location = await _context.Locations
                .Include(l => l.Client)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (location == null) return NotFound();

            if (location.ClientId != user.Id && !await _userManager.IsInRoleAsync(user, UserRole.Administrator))
            {
                return Forbid();
            }

            return View(location);
        }

        // GET: Locations/Create
        [Authorize(Roles = UserRole.Client + "," + UserRole.Administrator)]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Locations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRole.Client + "," + UserRole.Administrator)]
        public async Task<IActionResult> Create([Bind("Id,Name,Street,HouseNumber,Neighbourhood,City,PostalCode,Country,Latitude,Longitude")] Location location)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            location.ClientId = user.Id;

            if (ModelState.IsValid)
            {
                _context.Add(location);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(location);
        }

        // GET: Locations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var location = await _context.Locations.FindAsync(id);
            if (location == null) return NotFound();

            if (location.ClientId != user.Id && !await _userManager.IsInRoleAsync(user, UserRole.Administrator))
            {
                return Forbid();
            }

            return View(location);
        }

        // POST: Locations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Street,HouseNumber,Neighbourhood,City,PostalCode,Country,Latitude,Longitude")] Location location)
        {
            if (id != location.Id) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var existingLocation = await _context.Locations.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
            if (existingLocation == null) return NotFound();

            if (existingLocation.ClientId != user.Id && !await _userManager.IsInRoleAsync(user, UserRole.Administrator))
            {
                return Forbid();
            }

            location.ClientId = existingLocation.ClientId;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(location);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LocationExists(location.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(location);
        }

        // GET: Locations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var location = await _context.Locations
                .Include(l => l.Client)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (location == null) return NotFound();

            if (location.ClientId != user.Id && !await _userManager.IsInRoleAsync(user, UserRole.Administrator))
            {
                return Forbid();
            }

            return View(location);
        }

        // POST: Locations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var location = await _context.Locations.FindAsync(id);
            if (location == null) return NotFound();

            if (location.ClientId != user.Id && !await _userManager.IsInRoleAsync(user, UserRole.Administrator))
            {
                return Forbid();
            }

            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LocationExists(int id)
        {
            return _context.Locations.Any(e => e.Id == id);
        }
    }
}
