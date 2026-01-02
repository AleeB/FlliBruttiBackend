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
            var firme = _service.GetFirmaByIdUser(idUser);
            if (firme == null || firme.Count() == 0)
            {
                _logger.LogWarning($"No Firme found for User with Id: {idUser}");
                return NotFound();
            }
            else
            {
                _logger.LogInformation($"Firme found for User with Id: {idUser}");
                return Ok(firme);
            }
        }

        [HttpPost]
        [Route("Entry/{idUser}")]
        public async Task<IActionResult> Entry(long idUser)
        {
            _logger.LogInformation($"Creating Firma for User with Id: {idUser}");
            var res = await _service.CreateFirma(idUser);
            if (!res)
            {
                _logger.LogWarning($"Could not create Firma for User with Id: {idUser}, It does not exist");
                return BadRequest($"Could not create Firma for user with Id: {idUser}, It does not exist");
            }
            else
            {
                _logger.LogInformation($"Firma created for User with Id: {idUser}");
                return Ok();
            }
        }

        [HttpPost]
        [Route("Exit/{idFirma}")]
        public async Task<IActionResult> Exit(long idFirma)
        {
            _logger.LogInformation($"Exiting Firma with Id: {idFirma}");
            var res = await _service.ExitFirma(idFirma);
            if (!res)
            {
                _logger.LogWarning($"Could not exit Firma with Id: {idFirma}, It does not exist");
                return BadRequest($"Could not exit Firma with Id: {idFirma}, It does not exist");
            }
            else
            {
                _logger.LogInformation($"Firma with Id: {idFirma} exited successfully");
                return Ok("Exited Successfully");
            }
        }
    }
}
