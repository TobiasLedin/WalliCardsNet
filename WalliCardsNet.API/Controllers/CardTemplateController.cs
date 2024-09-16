using Microsoft.AspNetCore.Mvc;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Data.Models;

namespace WalliCardsNet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardTemplateController : ControllerBase
    {
        private readonly ICardTemplate _cardTemplateRepo;
        public CardTemplateController(ICardTemplate cardTemplateRepo)
        {
            _cardTemplateRepo = cardTemplateRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await _cardTemplateRepo.GetAllAsync();
            if (result != null && result.Any())
            {
                return Ok(result);
            }
            else
            {
                return Ok(new List<CardTemplate>());
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var result = await _cardTemplateRepo.GetByIdAsync(id);
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> AddAsync(CardTemplate cardTemplate)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _cardTemplateRepo.AddAsync(cardTemplate);
                return CreatedAtAction(nameof(GetByIdAsync), new { id = cardTemplate.Id }, cardTemplate);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync(CardTemplate cardTemplate)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _cardTemplateRepo.UpdateAsync(cardTemplate);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveAsync(int id)
        {
            try
            {
                await _cardTemplateRepo.RemoveAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
