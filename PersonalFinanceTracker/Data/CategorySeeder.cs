using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Data
{
    public static class CategorySeeder
    {
        public static async Task SeedDefaultCategoriesAsync(ApplicationDbContext context, string userId)
        {
            if (string.IsNullOrEmpty(userId)) return;

            // Check if user already has any categories configured
            var hasCategories = await context.Categories.AnyAsync(c => c.UserId == userId);
            
            if (!hasCategories)
            {
                var defaults = new List<Category>
                {
                    new Category { UserId = userId, Name = "Salary", Icon = "fa-solid fa-wallet", Type = "Income" },
                    new Category { UserId = userId, Name = "Business Income", Icon = "fa-solid fa-briefcase", Type = "Income" },
                    new Category { UserId = userId, Name = "Food / Groceries", Icon = "fa-solid fa-utensils", Type = "Expense" },
                    new Category { UserId = userId, Name = "Rent / Bills", Icon = "fa-solid fa-house", Type = "Expense" },
                    new Category { UserId = userId, Name = "Transport", Icon = "fa-solid fa-car", Type = "Expense" },
                    new Category { UserId = userId, Name = "Shopping", Icon = "fa-solid fa-shirt", Type = "Expense" },
                    new Category { UserId = userId, Name = "Entertainment", Icon = "fa-solid fa-film", Type = "Expense" },
                    new Category { UserId = userId, Name = "Medical / Health", Icon = "fa-solid fa-heart-pulse", Type = "Expense" }
                };

                context.Categories.AddRange(defaults);
                await context.SaveChangesAsync();
            }
        }
    }
}
