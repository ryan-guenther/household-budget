using HouseholdBudget.Service.Interfaces;
using HouseholdBudget.DTO;
using Microsoft.AspNetCore.Mvc;
using HouseholdBudget.Service;

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
        public async Task<IActionResult> GetAll()
        {
            var accounts = await _accountService.GetAllAsync();
            return Ok(accounts);  // Returns AccountDTOs
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var account = await _accountService.GetByIdAsync(id);
            if (account == null) return NotFound();

            return Ok(account);  // Returns AccountDTO
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AccountDTO accountDto)
        {
            await _accountService.AddAsync(accountDto);
            return CreatedAtAction(nameof(GetById), new { id = accountDto.Id }, accountDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AccountDTO accountDto)
        {
            if (id != accountDto.Id) return BadRequest();

            await _accountService.UpdateAsync(accountDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _accountService.DeleteAsync(id);
            return NoContent();
        }
    }
}
