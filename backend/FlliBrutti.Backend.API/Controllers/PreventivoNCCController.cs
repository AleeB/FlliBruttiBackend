using FlliBrutti.Backend.Application.IServices;
using FlliBrutti.Backend.Application.Models;
using FlliBrutti.Backend.Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlliBrutti.Backend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PreventivoNCCController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IPreventivoNCCService _service;
        public PreventivoNCCController(ILogger<PreventivoNCCController> logger, IPreventivoNCCService service)
        {
            _logger = logger;
            _service = service;
        }

        [Authorize(Roles = "1")]
        [HttpGet]
        [Route("ToExamine")]
        public async Task<IActionResult> GetPreventiviToExamine([FromBody] bool isTodo)
        {
            try
            {
                _logger.LogInformation($"Getting Preventivi with isTodo: {isTodo}");
                var preventivi = await _service.GetPreventiviToExamineAsync(isTodo);

                if (preventivi == null || !preventivi.Any())
                {
                    _logger.LogInformation($"No Preventivi found with isTodo: {isTodo}");
                    return Ok(new List<PreventivoNCCResponseDTO>());
                }

                _logger.LogInformation($"Found {preventivi.Count()} Preventivi with isTodo: {isTodo}");
                return Ok(preventivi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Preventivi to examine");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddPreventivo([FromBody] PreventivoNCCDTO preventivo)
        {
            try
            {
                if (preventivo == null)
                {
                    _logger.LogWarning("AddPreventivo called with null preventivo");
                    return BadRequest(new { error = "Preventivo data is required" });
                }

                _logger.LogInformation($"Adding new Preventivo for UserNonAutenticato with Id: {preventivo.IdUserNonAutenticato}");
                var res = await _service.AddPreventivoNCCAsync(preventivo);

                if (res.Item1)
                {
                    _logger.LogInformation("Preventivo added successfully");
                    return Ok(new { message = res.Item2 });
                }

                _logger.LogWarning($"Failed to add Preventivo: {res.Item2}");
                return BadRequest(new { error = res.Item2 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding Preventivo");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        [Authorize(Roles = "1")]
        [HttpGet]
        public async Task<IActionResult> GetPreventivoById(long id)
        {
            try
            {
                _logger.LogInformation($"Getting Preventivo with Id: {id}");
                var preventivo = await _service.GetPreventivoByIdAsync(id);

                if (preventivo == null)
                {
                    _logger.LogWarning($"Preventivo with Id: {id} not found");
                    return NotFound(new { error = $"Preventivo with Id: {id} not found" });
                }

                _logger.LogInformation($"Preventivo with Id: {id} retrieved successfully");
                return Ok(preventivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving Preventivo with Id: {id}");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        [Authorize(Roles = "1")]
        [HttpPatch]
        [Route("AddPreventivoCalculated/{idPreventivo}")]
        public async Task<IActionResult> AddPreventivoCalculated([FromBody] PreventivoUpdateNCCDTO preventivo, long idPreventivo)
        {
            try
            {
                if (preventivo == null)
                {
                    _logger.LogWarning($"AddPreventivoCalculated called with null preventivo for Id: {idPreventivo}");
                    return BadRequest(new { error = "Preventivo data is required" });
                }

                _logger.LogInformation($"Starting update of Preventivo with Id: {idPreventivo}");

                var res = await _service.GetPreventivoByIdAsync(idPreventivo);
                if (res == null)
                {
                    _logger.LogWarning($"Preventivo with Id: {idPreventivo} not found");
                    return NotFound(new { error = "Preventivo not found" });
                }

                var updateRes = await _service.UpdatePreventivoNCCAsync(idPreventivo, preventivo);

                if (!updateRes)
                {
                    _logger.LogWarning($"Failed to update Preventivo with Id: {idPreventivo}");
                    return BadRequest(new { error = "Failed to update Preventivo" });
                }

                _logger.LogInformation($"Successfully updated Preventivo with Id: {idPreventivo}");
                return Ok(new { message = "Preventivo updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating Preventivo with Id: {idPreventivo}");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message, stackTrace = ex.StackTrace });
            }
        }

    }
}