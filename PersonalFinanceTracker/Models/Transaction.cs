namespace PersonalFinanceTracker.Models
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public string UserId { get; set; }
        public int CategoryId { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public Category? Category { get; set; }
    }
}