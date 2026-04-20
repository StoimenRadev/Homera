using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Homera.Data;
using Homera.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Homera.Models.Enums;

namespace Homera.Controllers
{
    [Authorize]
    public class TaskItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _hostEnvironment;

        public TaskItemsController(ApplicationDbContext context, UserManager<User> userManager, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hostEnvironment = hostEnvironment;
        }

        // GET: TaskItems
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            IQueryable<TaskItem> tasks = _context.Tasks
                .Include(t => t.Client)
                .Include(t => t.Housekeeper)
                .Include(t => t.Location);

            if (await _userManager.IsInRoleAsync(user, UserRole.Housekeeper))
            {
                tasks = tasks.Where(t => t.HousekeeperId == user.Id);
            }
            else if (!await _userManager.IsInRoleAsync(user, UserRole.Administrator))
            {
                tasks = tasks.Where(t => t.ClientId == user.Id);
            }
 
            return View(await tasks.ToListAsync());
        }

        // GET: TaskItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var taskItem = await _context.Tasks
                .Include(t => t.Client)
                .Include(t => t.Housekeeper)
                .Include(t => t.Location)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (taskItem == null) return NotFound();

            if (taskItem.ClientId != user.Id && 
                taskItem.HousekeeperId != user.Id && 
                !await _userManager.IsInRoleAsync(user, UserRole.Administrator))
            {
                return Forbid();
            }

            return View(taskItem);
        }

        // GET: TaskItems/Create
        [Authorize(Roles = UserRole.Client + "," + UserRole.Administrator)]
        public async Task<IActionResult> Create(int? locationId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            IQueryable<Location> locations = _context.Locations;
            if (!await _userManager.IsInRoleAsync(user, UserRole.Administrator))
            {
                locations = locations.Where(l => l.ClientId == user.Id);
            }

            ViewData["LocationId"] = new SelectList(locations, "Id", "DisplayName", locationId);
            await PopulateHousekeepersDropdown();
            return View();
        }

        // POST: TaskItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRole.Client + "," + UserRole.Administrator)]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Budget,Deadline,Category,LocationId,HousekeeperId,ReviewDate,ImagePath")] TaskItem taskItem)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (await _userManager.IsInRoleAsync(user, UserRole.Administrator))
            {
                var location = await _context.Locations.FindAsync(taskItem.LocationId);
                if (location != null) taskItem.ClientId = location.ClientId;
            }
            else
            {
                taskItem.ClientId = user.Id;
            }
            taskItem.Status = taskItem.HousekeeperId.HasValue ? TaskItemStatus.Assigned : TaskItemStatus.Pending;
 
            if (ModelState.IsValid)
            {
                _context.Add(taskItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            IQueryable<Location> locations = _context.Locations;
            if (!await _userManager.IsInRoleAsync(user, UserRole.Administrator))
            {
                locations = locations.Where(l => l.ClientId == user.Id);
            }
            ViewData["LocationId"] = new SelectList(locations, "Id", "DisplayName", taskItem.LocationId);
            await PopulateHousekeepersDropdown(taskItem.HousekeeperId);
            return View(taskItem);
        }

        // GET: TaskItems/Edit/5
        [Authorize(Roles = UserRole.Client + "," + UserRole.Administrator)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
 
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
 
            var taskItem = await _context.Tasks.FindAsync(id);
            if (taskItem == null) return NotFound();
 
            bool isAdmin = await _userManager.IsInRoleAsync(user, UserRole.Administrator);

            if (taskItem.ClientId != user.Id && !isAdmin) return Forbid();
            if (taskItem.Status != TaskItemStatus.Pending && !isAdmin)
            {
                TempData["Error"] = "Only pending tasks can be edited by clients.";
                return RedirectToAction(nameof(Index));
            }
 
            IQueryable<Location> locations = _context.Locations;
            if (!isAdmin)
            {
                locations = locations.Where(l => l.ClientId == user.Id);
            }

            ViewData["LocationId"] = new SelectList(locations, "Id", "DisplayName", taskItem.LocationId);
            await PopulateHousekeepersDropdown(taskItem.HousekeeperId);
            return View(taskItem);
        }

        // POST: TaskItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRole.Client + "," + UserRole.Administrator)]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Budget,Deadline,Category,LocationId,HousekeeperId,ReviewDate,ImagePath")] TaskItem taskItem)
        {
            if (id != taskItem.Id) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var existingTask = await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
            if (existingTask == null) return NotFound();

            bool isAdmin = await _userManager.IsInRoleAsync(user, UserRole.Administrator);

            if (existingTask.ClientId != user.Id && !isAdmin) return Forbid();
            if (existingTask.Status != TaskItemStatus.Pending && !isAdmin)
            {
                return BadRequest("Only pending tasks can be edited by clients.");
            }
 
            if (isAdmin)
            {
                var location = await _context.Locations.FindAsync(taskItem.LocationId);
                if (location != null) taskItem.ClientId = location.ClientId;
                else taskItem.ClientId = existingTask.ClientId;
            }
            else
            {
                taskItem.ClientId = user.Id;
            }

            taskItem.Status = taskItem.HousekeeperId.HasValue ? TaskItemStatus.Assigned : TaskItemStatus.Pending;
 
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(taskItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskItemExists(taskItem.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            IQueryable<Location> locations = _context.Locations;
            if (!await _userManager.IsInRoleAsync(user, UserRole.Administrator))
            {
                locations = locations.Where(l => l.ClientId == user.Id);
            }
            ViewData["LocationId"] = new SelectList(locations, "Id", "DisplayName", taskItem.LocationId);
            await PopulateHousekeepersDropdown(taskItem.HousekeeperId);
            return View(taskItem);
        }

        [Authorize(Roles = UserRole.Client + "," + UserRole.Administrator)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var taskItem = await _context.Tasks
                .Include(t => t.Client)
                .Include(t => t.Housekeeper)
                .Include(t => t.Location)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (taskItem == null) return NotFound();

            if (taskItem.ClientId != user.Id && !await _userManager.IsInRoleAsync(user, UserRole.Administrator))
            {
                return Forbid();
            }

            if (taskItem.Status != TaskItemStatus.Pending && !await _userManager.IsInRoleAsync(user, UserRole.Administrator))
            {
                TempData["Error"] = "Only pending tasks can be cancelled.";
                return RedirectToAction(nameof(Index));
            }

            return View(taskItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRole.Housekeeper)]
        public async Task<IActionResult> SubmitWork(int id, IFormFile fileProof)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            if (task.HousekeeperId != user.Id) return Forbid();
            if (task.Status != TaskItemStatus.Assigned && task.Status != TaskItemStatus.InReview)
            {
                TempData["Error"] = "Samo naznacheni ili zadachi v proces na pregled mogat da se izprashtat.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (fileProof != null && fileProof.Length > 0)
            {
                // Delete old file if exists
                if (!string.IsNullOrEmpty(task.ImagePath))
                {
                    string oldFilePath = Path.Combine(_hostEnvironment.WebRootPath, task.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + fileProof.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await fileProof.CopyToAsync(fileStream);
                }

                task.ImagePath = "/uploads/" + uniqueFileName;
                task.Status = TaskItemStatus.InReview;
                task.ReviewDate = DateTime.Now;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Rabotata e izpratena za pregled!";
            }
            else
            {
                TempData["Error"] = "Molya prikazhete snimka kato dokazatelstvo.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRole.Client + "," + UserRole.Administrator)]
        public async Task<IActionResult> Complete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            if (task.ClientId != user.Id && !await _userManager.IsInRoleAsync(user, UserRole.Administrator)) return Forbid();

            if (task.Status != TaskItemStatus.InReview)
            {
                return BadRequest("Task must be in 'InReview' status to be completed.");
            }

            task.Status = TaskItemStatus.Completed;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRole.Client + "," + UserRole.Administrator)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var taskItem = await _context.Tasks.FindAsync(id);
            if (taskItem == null) return NotFound();

            if (taskItem.ClientId != user.Id && !await _userManager.IsInRoleAsync(user, UserRole.Administrator))
            {
                return Forbid();
            }

            if (taskItem.Status != TaskItemStatus.Pending && !await _userManager.IsInRoleAsync(user, UserRole.Administrator))
            {
                return BadRequest("Only pending tasks can be cancelled.");
            }

            _context.Tasks.Remove(taskItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateHousekeepersDropdown(int? selectedId = null)
        {
            var housekeepers = await _userManager.GetUsersInRoleAsync(UserRole.Housekeeper);
            ViewData["HousekeeperId"] = new SelectList(housekeepers.Select(u => new { 
                Id = u.Id, 
                FullName = $"{u.FirstName} {u.LastName}" 
            }), "Id", "FullName", selectedId);
        }

        private bool TaskItemExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
    }
}
