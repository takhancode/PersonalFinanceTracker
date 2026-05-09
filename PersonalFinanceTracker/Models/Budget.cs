namespace PersonalFinanceTracker.Models
{
    public class Budget
    {
        public int BudgetId { get; set; }
        public string UserId { get; set; }
        public int CategoryId { get; set; }
        public string MonthYear { get; set; }
        public decimal LimitAmount { get; set; }
        public Category? Category { get; set; }
    }
}
