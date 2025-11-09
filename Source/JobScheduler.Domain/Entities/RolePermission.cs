namespace JobScheduler.Domain.Entities
{
    public class RolePermission
    {
        public Guid Id { get; private set; }
        public Guid RoleId { get; set; }
        public Guid PermissionId { get; set; }
        public Permission Permission { get; set; } = default!;

        public RolePermission()
        {
            Id = Guid.NewGuid();
        }
    }
}