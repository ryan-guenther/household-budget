using HouseholdBudget.DTO;
using HouseholdBudget.Service.Interfaces;

using Microsoft.AspNetCore.Mvc;

namespace HouseholdBudget.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var transactions = await _transactionService.GetAllAsync();
            return Ok(transactions);  // Returns TransactionDTOs
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var transaction = await _transactionService.GetByIdAsync(id);
            if (transaction == null) return NotFound();

            return Ok(transaction);  // Returns TransactionDTO
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TransactionDTO transactionDto)
        {
            await _transactionService.AddAsync(transactionDto);
            return CreatedAtAction(nameof(GetById), new { id = transactionDto.Id }, transactionDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TransactionDTO transactionDto)
        {
            if (id != transactionDto.Id) return BadRequest();

            await _transactionService.UpdateAsync(transactionDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _transactionService.DeleteAsync(id);
            return NoContent();
        }
    }
}
