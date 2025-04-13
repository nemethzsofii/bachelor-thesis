using Microsoft.Extensions.Hosting;

namespace WebApplication1.Model
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public IList<GroupMembership> GroupMemberships { get; } = new List<GroupMembership>();
    }
}
