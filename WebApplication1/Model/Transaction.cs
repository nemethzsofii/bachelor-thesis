using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Model
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required]
        public TransactionType Type { get; set; }  // Enum for type ('income', 'expense')

        public int? CategoryId { get; set; }  // Nullable because of ON DELETE SET NULL

        [Column(TypeName = "datetime")]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        public string? Description { get; set; }

        // Navigation properties for relationships
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }
    }

    // Enum for transaction type
    public enum TransactionType
    {
        Income,
        Expense
    }
}
