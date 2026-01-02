using FlliBrutti.Backend.Application.IContext;
using FlliBrutti.Backend.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace FlliBrutti.Backend.API.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("[controller]")]
    [ApiController]
    public class FirmaController : ControllerBase
    {
        private readonly IFlliBruttiContext _context;

        public FirmaController(IFlliBruttiContext context)
        {
            _context = context;
        }


        [HttpGet]
        [Route("Get/{idUser}")]
        public async Task<ActionResult<IEnumerable<Firma>>> GetFirmeOfUser(long idUser)
        {
            var firme = _context.Firme.Where(f => f.IdUser == idUser).ToList();
            return Ok(firme);
        }

    }
}
