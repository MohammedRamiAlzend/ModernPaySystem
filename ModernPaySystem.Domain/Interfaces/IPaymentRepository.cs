using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModernPaySystem.Domain.Interfaces
{
    using ModernPaySystem.Domain;

    public interface IPaymentRepository
    {
        Task<Payment> AddAsync(Payment payment);
        Task<IEnumerable<Payment>> GetAllAsync();
        Task<Payment?> GetByIdAsync(Guid id);
    }
}
