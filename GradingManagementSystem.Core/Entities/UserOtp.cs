namespace GradingManagementSystem.Core.Entities
{
    public class UserOtp : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
        public DateTime ExpiryTime { get; set; }
    }
}
