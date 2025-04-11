using System.Data.SqlTypes;

namespace WebApplication1.Model
{
    public class Invite
    {
        public int Id { get; set; }
        public int SenderUserId { get; set; }
        public int ReceiverUserId { get; set; }
        public int GroupId { get; set; }
        public Byte Accepted { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
