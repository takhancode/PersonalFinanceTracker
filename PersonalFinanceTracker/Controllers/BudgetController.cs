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
    public class BudgetController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public BudgetController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ViewModel inside the controller file for simple, clean encapsulation
        public class BudgetViewModel
        {
            public Budget Budget { get; set; } = null!;
            public decimal SpentAmount { get; set; }
            public decimal LimitAmount => Budget.LimitAmount;
            public double ProgressPercentage => LimitAmount > 0 ? (double)(SpentAmount / LimitAmount) * 100 : 0;
            public bool IsOverBudget => SpentAmount > LimitAmount;
        }

        // GET: Budget
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var budgets = await _context.Budgets
                .Where(b => b.UserId == userId)
                .Include(b => b.Category)
                .ToListAsync();

            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId && t.Type == "Expense")
                .ToListAsync();

            var viewModels = new List<BudgetViewModel>();

            foreach (var budget in budgets)
            {
                int year = 0, month = 0;
                var parts = budget.MonthYear.Split('-');
                if (parts.Length == 2)
                {
                    int.TryParse(parts[0], out year);
                    int.TryParse(parts[1], out month);
                }

                var spent = transactions
                    .Where(t => t.CategoryId == budget.CategoryId && t.Date.Year == year && t.Date.Month == month)
                    .Sum(t => t.Amount);

                viewModels.Add(new BudgetViewModel
                {
                    Budget = budget,
                    SpentAmount = spent
                });
            }

            return View(viewModels);
        }

        // GET: Budget/Create
        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User);
            ViewBag.Categories = await _context.Categories
                .Where(c => c.UserId == userId && c.Type == "Expense")
                .ToListAsync();
            return View();
        }

        // POST: Budget/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Budget budget)
        {
            var userId = _userManager.GetUserId(User) ?? "";
            budget.UserId = userId;

            ModelState.Remove("UserId");
            ModelState.Remove("Category");

            // Prevent duplicate budgets for same Category and MonthYear
            var existingBudget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.UserId == userId && b.CategoryId == budget.CategoryId && b.MonthYear == budget.MonthYear);

            if (existingBudget != null)
            {
                ModelState.AddModelError("", "A budget limit already exists for this category in the selected month.");
            }

            if (ModelState.IsValid)
            {
                _context.Budgets.Add(budget);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _context.Categories
                .Where(c => c.UserId == userId && c.Type == "Expense")
                .ToListAsync();
            return View(budget);
        }

        // GET: Budget/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (budget.UserId != userId) return Forbid();

            ViewBag.Categories = await _context.Categories
                .Where(c => c.UserId == userId && c.Type == "Expense")
                .ToListAsync();
            return View(budget);
        }

        // POST: Budget/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Budget budget)
        {
            if (id != budget.BudgetId) return NotFound();

            var userId = _userManager.GetUserId(User) ?? "";
            budget.UserId = userId;

            ModelState.Remove("UserId");
            ModelState.Remove("Category");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(budget);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BudgetExists(budget.BudgetId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _context.Categories
                .Where(c => c.UserId == userId && c.Type == "Expense")
                .ToListAsync();
            return View(budget);
        }

        // GET: Budget/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var budget = await _context.Budgets
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.BudgetId == id);
            if (budget == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (budget.UserId != userId) return Forbid();

            return View(budget);
        }

        // POST: Budget/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (budget.UserId != userId) return Forbid();

            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BudgetExists(int id)
        {
            return _context.Budgets.Any(e => e.BudgetId == id);
        }
    }
}
