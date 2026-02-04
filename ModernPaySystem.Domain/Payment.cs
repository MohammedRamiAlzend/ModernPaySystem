using System;

namespace ModernPaySystem.Domain
{
    public class Payment
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string Recipient { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
