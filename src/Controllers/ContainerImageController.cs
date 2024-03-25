using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StepNet.API.Models.Image;

// Automatically generated skeleton
namespace StepNet.API.Controllers
{
    [Route("image")]
    [ApiController]
    public class ContainerImageController : ControllerBase
    {
        private readonly ContainerImageContext _context;

        public ContainerImageController(ContainerImageContext context)
        {
            _context = context;
        }

        // GET: api/ContainerImageModels
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContainerImageModel>>> GetContainerImages()
        {
            return await _context.ContainerImages.ToListAsync();
        }

        // GET: api/ContainerImageModels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ContainerImageModel>> GetContainerImageModel(int id)
        {
            var containerImageModel = await _context.ContainerImages.FindAsync(id);

            if (containerImageModel == null)
            {
                return NotFound();
            }

            return containerImageModel;
        }

        // PUT: api/ContainerImageModels/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutContainerImageModel(int id, ContainerImageModel containerImageModel)
        {
            if (id != containerImageModel.ImageID)
            {
                return BadRequest();
            }

            _context.Entry(containerImageModel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContainerImageModelExists(id))
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

        // POST: api/ContainerImageModels
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ContainerImageModel>> PostContainerImageModel(ContainerImageModel containerImageModel)
        {
            _context.ContainerImages.Add(containerImageModel);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetContainerImageModel", new { id = containerImageModel.ImageID }, containerImageModel);
        }

        // DELETE: api/ContainerImageModels/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContainerImageModel(int id)
        {
            var containerImageModel = await _context.ContainerImages.FindAsync(id);
            if (containerImageModel == null)
            {
                return NotFound();
            }

            _context.ContainerImages.Remove(containerImageModel);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ContainerImageModelExists(int id)
        {
            return _context.ContainerImages.Any(e => e.ImageID == id);
        }
    }
}
