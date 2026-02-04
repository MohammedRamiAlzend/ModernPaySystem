namespace ModernPaySystem.Domain.DTOs
{
    public class CreatePaymentRequest
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string Recipient { get; set; } = string.Empty;
    }
}
