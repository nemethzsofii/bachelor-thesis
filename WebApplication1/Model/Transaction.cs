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
        public int TypeId { get; set; }

        [Column("category_id")]
        public int? CategoryId { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string? Description { get; set; }
        public int? GroupId { get; set; }

        public Transaction(int userId, decimal amount, int typeId, int? categoryId, DateTime date, string description, int? groupId)
        {
            UserId = userId;
            Amount = amount;
            TypeId = typeId;
            CategoryId = categoryId;
            Date = date;
            Description = description;
            GroupId = groupId;
        }
    }
}
