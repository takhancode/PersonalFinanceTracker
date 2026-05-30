using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<HomeController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public class BudgetProgressViewModel
        {
            public string CategoryName { get; set; } = "";
            public string Icon { get; set; } = "";
            public decimal Limit { get; set; }
            public decimal Spent { get; set; }
            public double Percentage => Limit > 0 ? (double)(Spent / Limit) * 100 : 0;
            public bool Exceeded => Spent > Limit;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User) ?? "";

            // Auto-seed default categories for new accounts
            await CategorySeeder.SeedDefaultCategoriesAsync(_context, userId);

            // Fetch user transactions
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .Include(t => t.Category)
                .ToListAsync();

            // 1. Compute summary stats
            var totalIncome = transactions.Where(t => t.Type == "Income").Sum(t => t.Amount);
            var totalExpense = transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);
            var balance = totalIncome - totalExpense;

            ViewBag.TotalIncome = totalIncome;
            ViewBag.TotalExpense = totalExpense;
            ViewBag.Balance = balance;

            // 2. Expense By Category (Doughnut Chart Data)
            var categoryBreakdown = transactions
                .Where(t => t.Type == "Expense" && t.Category != null)
                .GroupBy(t => t.Category!.Name)
                .Select(g => new { Category = g.Key, Amount = g.Sum(t => t.Amount) })
                .ToList();

            ViewBag.DoughnutLabels = categoryBreakdown.Select(c => c.Category).ToArray();
            ViewBag.DoughnutData = categoryBreakdown.Select(c => c.Amount).ToArray();

            // 3. Last 6 Months Income vs Expense Trend (Line/Spline Chart Data)
            var last6Months = Enumerable.Range(0, 6)
                .Select(i => DateTime.Today.AddMonths(-i))
                .Select(d => new { Year = d.Year, Month = d.Month, Label = d.ToString("MMM yyyy") })
                .Reverse()
                .ToList();

            var trendLabels = new List<string>();
            var trendIncome = new List<decimal>();
            var trendExpense = new List<decimal>();

            foreach (var m in last6Months)
            {
                trendLabels.Add(m.Label);
                var inc = transactions.Where(t => t.Type == "Income" && t.Date.Year == m.Year && t.Date.Month == m.Month).Sum(t => t.Amount);
                var exp = transactions.Where(t => t.Type == "Expense" && t.Date.Year == m.Year && t.Date.Month == m.Month).Sum(t => t.Amount);
                trendIncome.Add(inc);
                trendExpense.Add(exp);
            }

            ViewBag.TrendLabels = trendLabels.ToArray();
            ViewBag.TrendIncome = trendIncome.ToArray();
            ViewBag.TrendExpense = trendExpense.ToArray();

            // 4. Budgets Statuses
            var budgets = await _context.Budgets
                .Where(b => b.UserId == userId)
                .Include(b => b.Category)
                .ToListAsync();

            var budgetStats = new List<BudgetProgressViewModel>();
            foreach (var b in budgets)
            {
                int year = 0, month = 0;
                var parts = b.MonthYear.Split('-');
                if (parts.Length == 2)
                {
                    int.TryParse(parts[0], out year);
                    int.TryParse(parts[1], out month);
                }

                var spent = transactions
                    .Where(t => t.CategoryId == b.CategoryId && t.Date.Year == year && t.Date.Month == month && t.Type == "Expense")
                    .Sum(t => t.Amount);

                budgetStats.Add(new BudgetProgressViewModel
                {
                    CategoryName = b.Category?.Name ?? "General",
                    Icon = b.Category?.Icon ?? "fa-solid fa-tag",
                    Limit = b.LimitAmount,
                    Spent = spent
                });
            }
            ViewBag.Budgets = budgetStats;

            // 5. Recent Transactions (Top 5)
            var recentTransactions = transactions
                .OrderByDescending(t => t.Date)
                .Take(5)
                .ToList();

            return View(recentTransactions);
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
