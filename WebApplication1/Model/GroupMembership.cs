namespace WebApplication1.Model
{
    public class GroupMembership
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public Role Role { get; set; }
    }
}
