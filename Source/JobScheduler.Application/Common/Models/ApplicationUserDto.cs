namespace JobScheduler.Application.Common.Models
{
    public class ApplicationUserDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public bool IsActive { get; set; }
        public string? Email { get; set; }
    }
}