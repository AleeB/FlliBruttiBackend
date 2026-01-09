using FlliBrutti.Backend.Application.IContext;
using FlliBrutti.Backend.Application.IServices;
using FlliBrutti.Backend.Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlliBrutti.Backend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PreventivoController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IPreventivoNCCService _service;
        public PreventivoController(ILogger<PreventivoController> logger, IPreventivoNCCService service)
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
    }
}