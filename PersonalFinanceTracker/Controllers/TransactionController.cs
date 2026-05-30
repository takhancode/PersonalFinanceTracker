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

        // GET: Transaction
        public async Task<IActionResult> Index(string? search)
        {
            var userId = _userManager.GetUserId(User);
            var query = _context.Transactions
                .Where(t => t.UserId == userId)
                .Include(t => t.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                var searchTerm = search.Trim().ToLower();
                query = query.Where(t => 
                    (t.Description != null && t.Description.ToLower().Contains(searchTerm)) ||
                    (t.Category != null && t.Category.Name.ToLower().Contains(searchTerm)) ||
                    t.Type.ToLower().Contains(searchTerm) ||
                    t.Amount.ToString().Contains(searchTerm)
                );
                ViewBag.SearchTerm = search;
            }

            var transactions = await query.OrderByDescending(t => t.Date).ToListAsync();
            return View(transactions);
        }

        // GET: Transaction/Create
        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User) ?? "";

            // Auto-seed default categories if empty
            await CategorySeeder.SeedDefaultCategoriesAsync(_context, userId);

            ViewBag.Categories = await _context.Categories
                .Where(c => c.UserId == userId)
                .ToListAsync();
            return View();
        }

        // POST: Transaction/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Transaction transaction)
        {
            var userId = _userManager.GetUserId(User) ?? "";
            transaction.UserId = userId;

            // Remove non-user input validations
            ModelState.Remove("UserId");
            ModelState.Remove("Category");

            if (ModelState.IsValid)
            {
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _context.Categories
                .Where(c => c.UserId == userId)
                .ToListAsync();
            return View(transaction);
        }

        // GET: Transaction/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return NotFound();

            var userId = _userManager.GetUserId(User) ?? "";
            if (transaction.UserId != userId) return Forbid();

            // Auto-seed default categories if empty
            await CategorySeeder.SeedDefaultCategoriesAsync(_context, userId);

            ViewBag.Categories = await _context.Categories
                .Where(c => c.UserId == userId)
                .ToListAsync();
            return View(transaction);
        }

        // POST: Transaction/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Transaction transaction)
        {
            if (id != transaction.TransactionId) return NotFound();

            var userId = _userManager.GetUserId(User) ?? "";
            transaction.UserId = userId;

            ModelState.Remove("UserId");
            ModelState.Remove("Category");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transaction.TransactionId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _context.Categories
                .Where(c => c.UserId == userId)
                .ToListAsync();
            return View(transaction);
        }

        // GET: Transaction/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(m => m.TransactionId == id);
            if (transaction == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (transaction.UserId != userId) return Forbid();

            return View(transaction);
        }

        // POST: Transaction/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (transaction.UserId != userId) return Forbid();

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.TransactionId == id);
        }
    }
}