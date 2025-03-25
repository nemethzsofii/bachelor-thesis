using Microsoft.AspNetCore.Http.HttpResults;

namespace WebApplication1.Model
{
    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TypeId { get; set; }
        public string Message { get; set; }
        public string IsSeen { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

