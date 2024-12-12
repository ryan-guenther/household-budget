using HouseholdBudget.DTO.Transaction;
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
        public async Task<ActionResult<TransactionListResponseDTO>> GetAll()
        {
            var transactions = await _transactionService.GetAllAsync();
            return Ok(transactions);  // Returns TransactionDTOs
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionDetailResponseDTO>> GetById(int id)
        {
            var transaction = await _transactionService.GetByIdAsync(id);
            if (transaction == null) return NotFound();

            return Ok(transaction);  // Returns TransactionDTO
        }

        [HttpPost]
        public async Task<ActionResult<TransactionDetailResponseDTO>> Create([FromBody] TransactionCreateRequestDTO transactionDto)
        {
            var transactionDetail = await _transactionService.AddAsync(transactionDto);
            return Ok(transactionDetail);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TransactionDetailResponseDTO>> Update(int id, [FromBody] TransactionUpdateRequestDTO transactionDto)
        {
            if (id != transactionDto.Id) return BadRequest();

            TransactionDetailResponseDTO transactionDetail = await _transactionService.UpdateAsync(transactionDto);
            return Ok(transactionDetail);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _transactionService.DeleteAsync(id);
            return NoContent();
        }
    }
}
