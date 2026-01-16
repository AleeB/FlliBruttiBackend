using FlliBrutti.Backend.Application.IServices;
using FlliBrutti.Backend.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlliBrutti.Backend.API.Controllers
{
    [Authorize(Roles = "1")]
    [Route("api/[controller]")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IPersonService _service;

        public PersonController(ILogger<PersonController> logger, IPersonService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get(long id)
        {
            try
            {
                _logger.LogInformation($"PersonController Get called with Id: {id}");
                var res = await _service.GetPersonById(id);

                if (res == null)
                {
                    _logger.LogWarning($"Person with Id: {id} not found.");
                    return NotFound(new { error = $"Person with Id: {id} not found." });
                }

                _logger.LogInformation($"Person with Id: {id} retrieved successfully");
                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving Person with Id: {id}. Exception: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        [HttpPatch]
        public async Task<IActionResult> Update(long id, [FromBody] Person person)
        {
            try
            {
                _logger.LogInformation($"PersonController Update called with Id: {id}");

                if (person == null || person.IdPerson != id)
                {
                    _logger.LogWarning($"Invalid Person data provided for Id: {id}, null or mismatched Id.");
                    return BadRequest(new { error = "Invalid Person data.", details = "Person is null or Id mismatch" });
                }

                var res = await _service.UpdatePerson(person);

                if (!res)
                {
                    _logger.LogWarning($"Failed to update Person with Id: {id}, Person not found.");
                    return NotFound(new { error = $"Person with Id: {id} not found." });
                }

                _logger.LogInformation($"Person with Id: {id} updated successfully");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating Person with Id: {id}. Exception: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }
    }
}
