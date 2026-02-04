using System;
using System.Threading.Tasks;
using ModernPaySystem.Domain;
using ModernPaySystem.Domain.Interfaces;
using ModernPaySystem.Application.Contracts;
using ModernPaySystem.Application.Interfaces;

namespace ModernPaySystem.Application.UseCases
{
    public class CreatePaymentUseCase : ICreatePaymentUseCase
    {
        private readonly IPaymentRepository _repository;

        public CreatePaymentUseCase(IPaymentRepository repository)
        {
            _repository = repository;
        }

        public async Task<Payment> CreatePaymentAsync(CreatePaymentRequest request)
        {
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                Amount = request.Amount,
                Currency = request.Currency,
                Recipient = request.Recipient,
                CreatedAt = DateTime.UtcNow
            };

            return await _repository.AddAsync(payment);
        }
    }
}
