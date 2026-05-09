using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TransactionController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .Include(t => t.Category)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
            return View(transactions);
        }
        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User);
            ViewBag.Categories = await _context.Categories
                .Where(c => c.UserId == userId)
                .ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Transaction transaction)
        {
            transaction.UserId = _userManager.GetUserId(User);
            transaction.Date = DateTime.Now;
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}