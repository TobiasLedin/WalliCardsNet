using Microsoft.AspNetCore.Mvc;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Models;
using WalliCardsNet.ClassLibrary.Card;
using WalliCardsNet.ClassLibrary.Customer;

namespace WalliCardsNet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardTemplateController : ControllerBase
    {
        private readonly ICardTemplate _cardTemplateRepo;
        private readonly IBusiness _businessRepo;
        public CardTemplateController(ICardTemplate cardTemplateRepo, IBusiness businessRepo)
        {
            _cardTemplateRepo = cardTemplateRepo;
            _businessRepo = businessRepo;
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

        [HttpGet("{id:guid}")]
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

        [HttpGet("{token}")]
        public async Task<IActionResult> GetByTokenAsync(string token)
        {
            var business = await _businessRepo.GetByTokenAsync(token);
            var result = await _cardTemplateRepo.GetByBusinessIdAsync(business.Id);
            if (result != null)
            {
                var cardResponseDTO = new CardResponseDTO
                {
                    DesignJson = result.DesignJson
                };
                return Ok(cardResponseDTO);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> AddAsync(CardRequestDTO cardRequestDTO)
        {
            try
            {
                var business = await _businessRepo.GetByTokenAsync(cardRequestDTO.BusinessToken);
                var cardTemplate = new CardTemplate
                {
                    Business = business,
                    DesignJson = cardRequestDTO.DesignJson
                };
                await _cardTemplateRepo.AddAsync(cardTemplate);
                return Created($"api/CardTemplate/{cardTemplate.Id}", cardTemplate);
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
