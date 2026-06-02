using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Data;

namespace PersonalFinanceTracker.Services
{
    public class NotificationItem
    {
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string Type { get; set; } = "";
        public string Icon { get; set; } = "";
    }

    public class MessageItem
    {
        public string Sender { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Body { get; set; } = "";
        public string TimeReceived { get; set; } = "";
    }

    public class FinanceAlertService
    {
        private readonly ApplicationDbContext _context;

        public FinanceAlertService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<NotificationItem>> GetNotificationsAsync(string userId)
        {
            var list = new List<NotificationItem>();
            if (string.IsNullOrEmpty(userId)) return list;

            var budgets = await _context.Budgets
                .Where(b => b.UserId == userId)
                .Include(b => b.Category)
                .ToListAsync();

            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .ToListAsync();

            foreach (var budget in budgets)
            {
                int year = 0;
                int month = 0;
                var parts = budget.MonthYear.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[0], out year) && int.TryParse(parts[1], out month))
                {
                    var spent = transactions
                        .Where(t => t.CategoryId == budget.CategoryId && t.Date.Year == year && t.Date.Month == month && t.Type == "Expense")
                        .Sum(t => t.Amount);

                    if (spent > budget.LimitAmount)
                    {
                        list.Add(new NotificationItem
                        {
                            Title = "Budget Exceeded",
                            Message = $"Exceeded limit for {budget.Category?.Name ?? "General"} (Spent Rs. {spent:N0} of Rs. {budget.LimitAmount:N0})",
                            Type = "danger",
                            Icon = "fa-solid fa-triangle-exclamation"
                        });
                    }
                    else if (spent >= budget.LimitAmount * 0.8m)
                    {
                        list.Add(new NotificationItem
                        {
                            Title = "Budget Warning",
                            Message = $"Used {((double)spent / (double)budget.LimitAmount) * 100:0}% for {budget.Category?.Name ?? "General"} (Rs. {spent:N0} of Rs. {budget.LimitAmount:N0})",
                            Type = "warning",
                            Icon = "fa-solid fa-bell"
                        });
                    }
                }
            }

            if (list.Count == 0)
            {
                list.Add(new NotificationItem
                {
                    Title = "All Clear",
                    Message = "Your budgets are healthy and within limits.",
                    Type = "success",
                    Icon = "fa-solid fa-circle-check"
                });
            }

            return list;
        }

        public async Task<List<MessageItem>> GetMessagesAsync(string userId)
        {
            var list = new List<MessageItem>();
            if (string.IsNullOrEmpty(userId)) return list;

            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .ToListAsync();

            var budgets = await _context.Budgets
                .Where(b => b.UserId == userId)
                .CountAsync();

            var totalIncome = transactions.Where(t => t.Type == "Income").Sum(t => t.Amount);
            var totalExpense = transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);
            var balance = totalIncome - totalExpense;

            if (balance < 0)
            {
                list.Add(new MessageItem
                {
                    Sender = "Smart Finance AI",
                    Subject = "Negative Balance Warning",
                    Body = "Your total expenses exceed your total income. Try reducing non-essential category expenditures to stabilize your balance.",
                    TimeReceived = "Just Now"
                });
            }
            else if (balance > 0 && totalExpense > totalIncome * 0.7m)
            {
                list.Add(new MessageItem
                {
                    Sender = "Smart Finance AI",
                    Subject = "High Expense Ratio Alert",
                    Body = "You spent over 70% of your earnings. Consider setting tighter budgets to increase your monthly savings rate.",
                    TimeReceived = "Just Now"
                });
            }

            if (budgets == 0)
            {
                list.Add(new MessageItem
                {
                    Sender = "Wallet Setup Coach",
                    Subject = "Create Your First Budget Limit",
                    Body = "You haven't set any monthly budgets yet. Go to the Budgets page to create one and start tracking your expenses effectively.",
                    TimeReceived = "1 hour ago"
                });
            }

            list.Add(new MessageItem
            {
                Sender = "Wallet Advisor",
                Subject = "Golden Rule of Savings",
                Body = "Aim to save at least 20% of your monthly income. Check your Reports periodically to monitor progress towards your goals.",
                TimeReceived = "2 hours ago"
            });

            return list;
        }
    }
}
