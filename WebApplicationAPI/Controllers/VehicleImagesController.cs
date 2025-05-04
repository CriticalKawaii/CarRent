using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationAPI.EF;

namespace WebApplicationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleImagesController : ControllerBase
    {
        private readonly Entities _context;

        public VehicleImagesController(Entities context)
        {
            _context = context;
        }

        // GET: api/VehicleImages
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehicleImage>>> GetVehicleImages()
        {
            return await _context.VehicleImages.ToListAsync();
        }

        // GET: api/VehicleImages/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VehicleImage>> GetVehicleImage(int id)
        {
            var vehicleImage = await _context.VehicleImages.FindAsync(id);

            if (vehicleImage == null)
            {
                return NotFound();
            }

            return vehicleImage;
        }

        // PUT: api/VehicleImages/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVehicleImage(int id, VehicleImage vehicleImage)
        {
            if (id != vehicleImage.ImageId)
            {
                return BadRequest();
            }

            _context.Entry(vehicleImage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehicleImageExists(id))
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

        // POST: api/VehicleImages
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<VehicleImage>> PostVehicleImage(VehicleImage vehicleImage)
        {
            _context.VehicleImages.Add(vehicleImage);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVehicleImage", new { id = vehicleImage.ImageId }, vehicleImage);
        }

        // DELETE: api/VehicleImages/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicleImage(int id)
        {
            var vehicleImage = await _context.VehicleImages.FindAsync(id);
            if (vehicleImage == null)
            {
                return NotFound();
            }

            _context.VehicleImages.Remove(vehicleImage);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VehicleImageExists(int id)
        {
            return _context.VehicleImages.Any(e => e.ImageId == id);
        }
    }
}
