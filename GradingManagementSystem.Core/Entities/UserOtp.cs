namespace GradingManagementSystem.Core.Entities
{
    public class UserOtp : BaseEntity
    {
        public string? Email { get; set; }
        public string? OtpCode { get; set; }
        public DateTime ExpiryTime { get; set; }
    }
}
