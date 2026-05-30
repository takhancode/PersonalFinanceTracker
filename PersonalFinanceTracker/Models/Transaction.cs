using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonalFinanceTracker.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")] 
        public decimal Amount { get; set; }

        [Required]
        [StringLength(20)] 
        public string Type { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Now; 

        [StringLength(255)]
        public string Description { get; set; }
    }
}