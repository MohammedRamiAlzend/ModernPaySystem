using System.Threading.Tasks;
using ModernPaySystem.Domain;
using ModernPaySystem.Application.Contracts;
using ModernPaySystem.Domain.Interfaces;

namespace ModernPaySystem.Application.Interfaces
{
    public interface ICreatePaymentUseCase
    {
        Task<Payment> CreatePaymentAsync(CreatePaymentRequest request);
    }
}
