namespace WebApplication1.Model
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<GroupMembership> GroupMembers { get; set; }
    }
}
