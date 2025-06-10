namespace GradingManagementSystem.Core.Entities
{
    public class TemporaryUser : BaseEntity
    {
            public string? FullName { get; set; } = null;
            public string? Email { get; set; } = null;
            public string? PasswordHash { get; set; }
            public DateTime CreatedAt { get; set; } = DateTime.Now.AddHours(1);
            public string? ProfilePicture { get; set; } = null;
            public string? Specialty { get; set; } = null;
            public bool InTeam { get; set; } = false;
            public int? TeamId { get; set; } = null;
    }
}
