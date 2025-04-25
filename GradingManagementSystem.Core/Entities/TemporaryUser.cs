namespace GradingManagementSystem.Core.Entities
{
    public class TemporaryUser : BaseEntity
    {
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string PasswordHash { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; } = DateTime.Now;
            public string? ProfilePicture { get; set; } = null;
            public string? Specialty { get; set; } = null;
            public bool InTeam { get; set; } = false;
            public int? TeamId { get; set; } = null;
    }
}
