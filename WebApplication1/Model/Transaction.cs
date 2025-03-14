using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace WebApplication1.Model
{
    public class Transaction
    {
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }
        public decimal Amount { get; set; }

        [Column("type")]
        public string Type { get; set; }

        [Column("category_id")]
        public int? CategoryId { get; set; }  // Ensure this matches MySQL schema
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string? Description { get; set; }
    }
}
