using CitiesManager.Core.Domain.Entities;
using CitiesManager.Infrastructure.DataBaseContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CitiesManager.WebAPI.Controllers.v1
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
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
        public async Task<ActionResult<IEnumerable<City>>> GetCities()
        {
            if (_context.Cities == null)
            {
                return NotFound();
            }
            return await _context.Cities.ToListAsync();
        }

        // GET: api/Cities/5
        [HttpGet("{id}")]
        public async Task<ActionResult<City>> GetCity(Guid id)
        {
            if (_context.Cities == null)
            {
                return NotFound();
            }
            var city = await _context.Cities.FindAsync(id);

            if (city == null)
            {
                return NotFound();
            }

            return city;
        }

        // PUT: api/Cities/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCity(Guid id, [Bind(nameof(City.CityId), nameof(City.CityName))] City city)
        {
            // [Bind(nameof(City.CityId),nameof(City.CityName))]
            // this is to protect from overposting attacks
            // by mentioning this only CityId,CityName will be accept from body
            // no other model prop from City will accept from body.
            // so this avoid unwanted posting from body.
            if (id != city.CityId)
            {
                return BadRequest();
            }
            var existingCity = await _context.Cities.FindAsync(id);
            if (existingCity == null)
            {
                return NotFound();
            }
            existingCity.CityName = city.CityName;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            // while saveing changes if some one parallely make changes in cities in db
            // it will catch this exception
            // so when saving changes if id dont exists then return NotFound();
            // else throw 500 error.
            {
                if (!CityExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Cities
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<City>> PostCity([Bind(nameof(City.CityId), nameof(City.CityName))] City city)
        {
            if (_context.Cities == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Cities'  is null.");
            }
            _context.Cities.Add(city);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCity", new { id = city.CityId }, city);
            // this will create a location header in response, which contain url to GetCity action.
        }

        // DELETE: api/Cities/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCity(Guid id)
        {
            if (_context.Cities == null)
            {
                return NotFound();
            }
            var city = await _context.Cities.FindAsync(id);
            if (city == null)
            {
                return NotFound();
            }

            _context.Cities.Remove(city);
            await _context.SaveChangesAsync();

            return NoContent(); // statuscode 200
        }

        private bool CityExists(Guid id)
        {
            return (_context.Cities?.Any(e => e.CityId == id)).GetValueOrDefault();
        }
    }
}
