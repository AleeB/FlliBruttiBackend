using Microsoft.AspNetCore.Mvc;
using FlliBrutti.Backend.Application.IServices;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;
using Microsoft.AspNetCore.Authorization;

namespace FlliBrutti.Backend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FirmaController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IFirmaService _service;

        public FirmaController(ILogger<FirmaController> logger, IFirmaService service)
        {
            _logger = logger;
            _service = service;
        }


        [HttpGet]
        [Route("Get/{idUser}")]
        public async Task<IActionResult> GetFirmeOfUser(long idUser)
        {
            _logger.LogInformation($"Getting Firme of User with Id: {idUser}");
            var firme = await _service.GetFirmaByIdUserAsync(idUser);

            if (firme == null || !firme.Any())
            {
                _logger.LogWarning($"No Firme found for User with Id: {idUser}");
                return NotFound($"No Firme found for User with Id: {idUser}");
            }

            _logger.LogInformation($"Found {firme.Count()} Firme for User with Id: {idUser}");
            return Ok(firme);
        }

        [HttpPost]
        [Route("Entry")]
        public async Task<IActionResult> Entry(long idUser)
        {
            try
            {
                _logger.LogInformation($"Creating Firma for User with Id: {idUser}");
                var res = await _service.CreateFirma(idUser);

                if (!res.Item1)
                {
                    _logger.LogWarning(res.Item2);
                    return NotFound(res.Item2);
                }

                _logger.LogInformation($"Firma created for User with Id: {idUser}");
                return Ok(new { message = res.Item2 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating Firma for User with Id: {idUser}. Exception: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        [HttpPost]
        [Route("Exit")]
        public async Task<IActionResult> Exit(long idUser)
        {
            try
            {
                _logger.LogInformation($"Exiting Firma for User with Id: {idUser}");
                var res = await _service.ExitFirma(idUser);

                if (!res.Item1)
                {
                    _logger.LogWarning(res.Item2);
                    return NotFound(res.Item2);
                }

                _logger.LogInformation($"User with Id: {idUser} exited successfully");
                return Ok(new { message = res.Item2 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error exiting Firma for User with Id: {idUser}. Exception: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }
    }
}
