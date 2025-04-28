using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace WebApplication1.Model
{
    public class Transaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int Amount { get; set; }
        public int TypeId { get; set; }
        public int? CategoryId { get; set; }
        public DateTime Date { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"));
        public string? Description { get; set; }
        public int? GroupId { get; set; }

        public Transaction(int userId, int amount, int typeId, int? categoryId, DateTime date, string description, int? groupId)
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
