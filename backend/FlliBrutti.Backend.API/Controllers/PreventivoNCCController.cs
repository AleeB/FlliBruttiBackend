using FlliBrutti.Backend.Application.IContext;
using FlliBrutti.Backend.Application.IServices;
using FlliBrutti.Backend.Application.Models;
using FlliBrutti.Backend.Core.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [HttpGet]
        [Route("GetPreventiviToExamine/{isTodo}")]
        public async Task<IActionResult> GetPreventiviToExamine(bool isTodo)
        {
            try
            {
                var preventivi = await _service.GetPreventiviToExamineAsync(isTodo);
                return Ok(preventivi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Preventivi to examine");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Route("AddPreventivo")]
        public async Task<IActionResult> AddPreventivo([FromBody] PreventivoNCCDTO preventivo)
        {
            try
            {
                var res = await _service.AddPreventivoNCCAsync(preventivo);
                if (res.Item1)
                {
                    return Ok(res.Item2);
                }
                else
                {
                    return BadRequest(res.Item2);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding Preventivo");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        [Route("GetPreventivoById/{id}")]
        public async Task<IActionResult> GetPreventivoById(long id)
        {
            try
            {
                var preventivo = await _service.GetPreventivoByIdAsync(id);
                if (preventivo != null)
                {
                    return Ok(preventivo);
                }
                else
                {
                    return NotFound("Preventivo not found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Preventivo by ID");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPatch]
        [Route("AddPreventivoCalculated/{idPreventivo}")]
        public async Task<IActionResult> AddPreventivoCalculated([FromBody] PreventivoUpdateNCCDTO preventivo, long idPreventivo)
        {
            try
            {
                _logger.LogInformation("Starting update of Preventivo with ID: {IdPreventivo}", idPreventivo);
                var res = await _service.GetPreventivoByIdAsync(idPreventivo);
                if (res == null)
                {
                    _logger.LogWarning("Preventivo with ID: {IdPreventivo} not found", idPreventivo);
                    return NotFound("Preventivo was not Found");
                }
                var updateRes = await _service.UpdatePreventivoNCCAsync(idPreventivo, preventivo);
                _logger.LogInformation("Successfully updated Preventivo with ID: {IdPreventivo}", idPreventivo);
                return Ok("Preventivo updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.StackTrace);
            }

        }

    }
}