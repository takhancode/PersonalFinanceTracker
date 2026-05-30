using System.ComponentModel.DataAnnotations;

namespace PersonalFinanceTracker.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(50)] 
        public string Icon { get; set; }

        [Required]
        [StringLength(20)]
        public string Type { get; set; }
    }
}