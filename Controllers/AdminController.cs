using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Homera.Data;
using Homera.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Homera.Models;
using Microsoft.AspNetCore.Identity;

namespace Homera.Controllers
{
    [Authorize(Roles = UserRole.Administrator)]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalLocations = await _context.Locations.CountAsync();
            var totalTasks = await _context.Tasks.CountAsync();
            var pendingReviews = await _context.Tasks.Where(t => t.Status == TaskItemStatus.InReview).CountAsync();

            var usersByRole = new Dictionary<string, int>();
            string[] roles = { UserRole.Administrator, UserRole.Housekeeper, UserRole.Client };
            foreach (var role in roles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role);
                usersByRole[role] = usersInRole.Count;
            }

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalLocations = totalLocations;
            ViewBag.TotalTasks = totalTasks;
            ViewBag.PendingReviews = pendingReviews;
            ViewBag.UsersByRole = usersByRole;

            return View();
        }
    }
}
