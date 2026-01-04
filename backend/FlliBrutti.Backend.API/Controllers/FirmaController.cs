using FlliBrutti.Backend.Application.IContext;
using FlliBrutti.Backend.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FlliBrutti.Backend.Application.IServices;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace FlliBrutti.Backend.API.Controllers
{
    //[Authorize(AuthenticationSchemes = "Bearer")]
    [Route("[controller]")]
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
        public async Task<ActionResult<IEnumerable<Firma>>> GetFirmeOfUser(long idUser)
        {
            _logger.LogInformation($"Getting Firme of User with Id: {idUser}");
            var firme = await _service.GetFirmaByIdUserAsync(idUser);
            if (firme == null || firme.Count() == 0)
            {
                _logger.LogWarning($"No Firme found for User with Id: {idUser}");
                return NotFound($"No Firme found for User with Id: {idUser}");
            }
            else
            {
                _logger.LogInformation($"Firme found for User with Id: {idUser}");
                return Ok(firme);
            }
        }

        [HttpPost]
        [Route("Entry")]
        public async Task<IActionResult> Entry(long idUser)
        {
            _logger.LogInformation($"Creating Firma for User with Id: {idUser}");
            var res = await _service.CreateFirma(idUser);
            if (!res)
            {
                _logger.LogWarning($"Could not create Firma for User with Id: {idUser}, It does not exist");
                return NotFound($"Could not create Firma for user with Id: {idUser}, It does not exist");
            }
            else
            {
                _logger.LogInformation($"Firma created for User with Id: {idUser}");
                return Ok();
            }
        }

        [HttpPost]
        [Route("Exit")]
        public async Task<IActionResult> Exit(long idUser, DateOnly date)
        {
            _logger.LogInformation($"Exiting Firma with Id: {idUser} and Date: {date}");
            var res = await _service.ExitFirma(idUser, date);
            if (!res)
            {
                _logger.LogWarning($"Could not exit Firma with Id: {idUser} and Date: {date}, It does not exist");
                return NotFound($"Could not exit Firma with Id: {idUser} and Date: {date}, It does not exist");
            }
            else
            {
                _logger.LogInformation($"Firma with Id: {idUser} and Date: {date} exited successfully");
                return Ok("Exited Successfully");
            }
        }
    }
}
