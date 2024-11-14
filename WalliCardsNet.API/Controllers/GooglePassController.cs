using Microsoft.AspNetCore.Mvc;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Controllers
{
    [Route("api/googlepass")]
    [ApiController]
    public class GooglePassController : ControllerBase
    {
        private readonly IGooglePassRepo _googlePassRepository;
        public GooglePassController(IGooglePassRepo googlePassRepository)
        {
            _googlePassRepository = googlePassRepository;
        }

        [HttpGet("get-all-by-class/{classId}")]
        public async Task<IActionResult> GetAllByClassIdAsync(string classId)
        {
            var result = await _googlePassRepository.GetAllByClassIdAsync(classId);
            
            return Ok(result); 
        }

        [HttpGet("{objectId}")]
        public async Task<IActionResult> GetByIdAsync(string objectId)
        {
            var result = await _googlePassRepository.GetByIdAsync(objectId);
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
        public async Task<IActionResult> AddAsync(GooglePass pass)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _googlePassRepository.AddAsync(pass);
                return CreatedAtAction(nameof(GetByIdAsync), new { id = pass.ObjectId }, pass);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync(GooglePass pass)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _googlePassRepository.UpdateAsync(pass);
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
                await _googlePassRepository.RemoveAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
