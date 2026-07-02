using Majstor_bob.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Majstor_bob.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MajstorBobController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MajstorBobController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("test")]
        public IActionResult TestConnection()
        {
            try
            {
                _context.Database.CanConnect();
                return Ok("Connected via EF Core!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}