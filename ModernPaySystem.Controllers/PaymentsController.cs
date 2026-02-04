using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.Domain;
using ModernPaySystem.Domain.DTOs;
using ModernPaySystem.Domain.Interfaces;

namespace ModernPaySystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentRepository _repository;
        public PaymentsController(IPaymentRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePaymentRequest request)
        {
            if (request == null || request.Amount <= 0)
            {
                return BadRequest("Invalid payment request");
            }

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                Amount = request.Amount,
                Currency = request.Currency,
                Recipient = request.Recipient,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _repository.AddAsync(payment);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            return NotFound();
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return StatusCode(501, "Not implemented");
        }
    }
}
