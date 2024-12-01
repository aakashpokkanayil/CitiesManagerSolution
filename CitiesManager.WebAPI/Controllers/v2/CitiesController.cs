using CitiesManager.Infrastructure.DataBaseContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CitiesManager.WebAPI.Controllers.v2
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("2.0")]
    public class CitiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Cities
        /// <summary>
        /// This method returns entire city List. 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Produces("application/xml")]
        // [Produces("application/xml")] will make resposnse as application/xml
        // in global filter we have added all resposnse as application/json
        // but here we can override for a action that using local filters.
        // in order to activate xml serialization we have to enable AddXmlSerializerFormatters()
        // in program.cs
        public async Task<ActionResult<IEnumerable<string?>>> GetCities()
        {
            if (_context.Cities == null)
            {
                return NotFound();
            }
            return await _context.Cities
                .OrderBy(x=>x.CityName)
                .Select(x=>x.CityName)
                .ToListAsync();
        }

       
    }
}
