using FlliBrutti.Backend.Application.IServices;
using FlliBrutti.Backend.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlliBrutti.Backend.API.Controllers
{
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
        [Route("Get/{id}")]
        public async Task<IActionResult> Get(long id)
        {
            _logger.LogInformation($"PersonController Get called with Id: {id}");
            var res = await _service.GetPersonById(id);
            if (res == null)
            {
                _logger.LogWarning($"Person with Id: {id} not found.");
                return NotFound($"Person with Id: {id} not found.");
            }
            return Ok(res);
        }

        [HttpPatch]
        [Route("Update/{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] Person person)
        {
            _logger.LogInformation($"PersonController Update called with Id: {id}");
            if(person == null || person.IdPerson != id)
            {
                _logger.LogWarning($"Invalid Person data provided for Id: {id}, null or mismatched Id.");
                return BadRequest("Invalid Person data.");
            }
            var res = await _service.UpdatePerson(person);
            if (!res)
            {
                _logger.LogWarning($"Failed to update Person with Id: {id}, Person not found.");
                return NotFound($"Person with Id: {id} not found.");
            }
            return NoContent();
        }
    }
}
