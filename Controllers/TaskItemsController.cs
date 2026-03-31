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
    public class TaskItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public TaskItemsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
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

            if (!await _userManager.IsInRoleAsync(user, UserRole.Administrator))
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

            if (taskItem.ClientId != user.Id && !await _userManager.IsInRoleAsync(user, UserRole.Administrator))
            {
                // Note: Housekeeper should also be allowed to see it if assigned, but we'll focus on Client for now.
                // Or just allow all for now but filter Index.
                // Better to be strict.
                return Forbid();
            }

            return View(taskItem);
        }

        // GET: TaskItems/Create
        [Authorize(Roles = UserRole.Client)]
        public async Task<IActionResult> Create(int? locationId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            ViewData["LocationId"] = new SelectList(_context.Locations.Where(l => l.ClientId == user.Id), "Id", "DisplayName", locationId);
            return View();
        }

        // POST: TaskItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRole.Client)]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Budget,Deadline,Category,LocationId")] TaskItem taskItem)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            taskItem.ClientId = user.Id;
            taskItem.Status = TaskItemStatus.Pending;

            if (ModelState.IsValid)
            {
                _context.Add(taskItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["LocationId"] = new SelectList(_context.Locations.Where(l => l.ClientId == user.Id), "Id", "DisplayName", taskItem.LocationId);
            return View(taskItem);
        }

        // GET: TaskItems/Edit/5
        [Authorize(Roles = UserRole.Client)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var taskItem = await _context.Tasks.FindAsync(id);
            if (taskItem == null) return NotFound();

            if (taskItem.ClientId != user.Id) return Forbid();
            if (taskItem.Status != TaskItemStatus.Pending)
            {
                TempData["Error"] = "Only pending tasks can be edited.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["LocationId"] = new SelectList(_context.Locations.Where(l => l.ClientId == user.Id), "Id", "DisplayName", taskItem.LocationId);
            return View(taskItem);
        }

        // POST: TaskItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRole.Client)]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Budget,Deadline,Category,LocationId")] TaskItem taskItem)
        {
            if (id != taskItem.Id) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var existingTask = await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
            if (existingTask == null) return NotFound();

            if (existingTask.ClientId != user.Id) return Forbid();
            if (existingTask.Status != TaskItemStatus.Pending)
            {
                return BadRequest("Only pending tasks can be edited.");
            }

            taskItem.ClientId = user.Id;
            taskItem.Status = TaskItemStatus.Pending;

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
            ViewData["LocationId"] = new SelectList(_context.Locations.Where(l => l.ClientId == user.Id), "Id", "DisplayName", taskItem.LocationId);
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
        [Authorize(Roles = UserRole.Client)]
        public async Task<IActionResult> Complete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            if (task.ClientId != user.Id) return Forbid();

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

        private bool TaskItemExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
    }
}
