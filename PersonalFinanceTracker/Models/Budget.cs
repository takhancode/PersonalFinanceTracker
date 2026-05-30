using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalFinanceTracker.Models
{
    public class Budget
    {
        [Key]
        public int BudgetId { get; set; } 

        [Required]
        public string UserId { get; set; } 

        [Required]
        public int CategoryId { get; set; } 

        [Required]
        [StringLength(7)] 
        public string MonthYear { get; set; } 

        [Required]
        [Column(TypeName = "decimal(18,2)")] 
        public decimal LimitAmount { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }
    }
}