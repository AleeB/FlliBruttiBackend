namespace FlliBrutti.Backend.Core.Models
{
    public class RefreshToken
    {
        public long Id { get; set; }
        public string Token { get; set; } = null!;
        public long UserId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRevoked { get; set; }
        public string? RevokedByIp { get; set; }
        public DateTime? RevokedAt { get; set; }

        public virtual User User { get; set; } = null!;

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}
