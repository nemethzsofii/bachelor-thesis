using System.Reflection.Metadata;

namespace WebApplication1.Model
{
    public class GroupMembership
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public Group Group { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; } = 2;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public GroupMembership(int groupId, int userId, int roleId)
        {
            GroupId = groupId;
            UserId = userId;
            RoleId = roleId;
            JoinedAt = DateTime.UtcNow;
        }
    }
}
