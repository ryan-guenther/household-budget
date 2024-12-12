using HouseholdBudget.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using HouseholdBudget.DTO.Account;

namespace HouseholdBudget.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccountListResponseDTO>>> GetAll()
        {
            var accountList = await _accountService.GetAllAsync();
            return Ok(accountList);  // Returns AccountDTOs
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<AccountDetailResponseDTO>>> GetById(int id)
        {
            var account = await _accountService.GetByIdAsync(id);
            if (account == null) return NotFound();

            return Ok(account);  // Returns AccountDTO
        }

        [HttpPost]
        public async Task<ActionResult<AccountDetailResponseDTO>> Create([FromBody] AccountCreateRequestDTO accountDto)
        {
            var accountDetail = await _accountService.AddAsync(accountDto);
            return Ok(accountDetail);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AccountDetailResponseDTO>> Update(int id, [FromBody] AccountUpdateRequestDTO accountDto)
        {
            if (id != accountDto.Id) return BadRequest();

            var accountDetail = await _accountService.UpdateAsync(accountDto);
            return Ok(accountDetail);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _accountService.DeleteAsync(id);
            return NoContent();
        }
    }
}
