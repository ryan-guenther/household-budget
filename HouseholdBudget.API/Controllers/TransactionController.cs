using HouseholdBudget.DTO.Account;
using HouseholdBudget.DTO.Transaction;
using HouseholdBudget.Service;
using HouseholdBudget.Service.Interfaces;

using Microsoft.AspNetCore.Authorization;
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

        [HttpGet]
        [Route("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<TransactionListResponseDTO>>> AdminGetAll()
        {
            var accountList = await _transactionService.AdminGetAllAsync();
            return Ok(accountList);  // Returns AccountDTOs
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionDetailResponseDTO>> GetById(int id)
        {
            var transaction = await _transactionService.GetByIdAsync(id);
            if (transaction == null) return NotFound();

            return Ok(transaction);  // Returns TransactionDTO
        }

        [HttpGet("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<TransactionDetailResponseDTO>>> AdminGetById(int id)
        {
            var account = await _transactionService.AdminGetByIdAsync(id);
            if (account == null) return NotFound();

            return Ok(account);  // Returns AccountDTO
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
