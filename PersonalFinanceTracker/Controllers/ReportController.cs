using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ReportController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Report
        public async Task<IActionResult> Index(int? categoryId, string? type, DateTime? startDate, DateTime? endDate)
        {
            var userId = _userManager.GetUserId(User) ?? "";

            // Auto-seed default categories if empty
            await CategorySeeder.SeedDefaultCategoriesAsync(_context, userId);

            // Fetch user categories for filter dropdown
            ViewBag.Categories = await _context.Categories
                .Where(c => c.UserId == userId)
                .ToListAsync();

            var query = _context.Transactions
                .Where(t => t.UserId == userId)
                .Include(t => t.Category)
                .AsQueryable();

            // Apply Filters
            if (categoryId.HasValue)
            {
                query = query.Where(t => t.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(t => t.Type == type);
            }

            if (startDate.HasValue)
            {
                query = query.Where(t => t.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                // Set to end of day to include all logs on that day
                var endOfLimit = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(t => t.Date <= endOfLimit);
            }

            var transactions = await query.OrderByDescending(t => t.Date).ToListAsync();

            // Set current values to restore form states
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SelectedType = type;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(transactions);
        }
    }
}
