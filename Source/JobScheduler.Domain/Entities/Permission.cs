namespace JobScheduler.Domain.Entities
{
    public class Permission
    {
        public Guid Id { get; private set; }
        public string Title { get; set; } = string.Empty;
        public string ApiPath { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = "GET";
        public string? Description { get; set; }
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

        public Permission()
        {
            Id = Guid.NewGuid();
        }
    }
}