using Microsoft.AspNetCore.Mvc;
using StepNet.API.Controllers.Configuration.Interfaces;
using static StepNet.API.Models.Images.ContainerImageModel;

namespace StepNet.Controllers.Images
{
    public class ContainerImagesController : IConfigurationController
    {
        private readonly ContainerImageContext _context;

        public ContainerImagesController(ContainerImageContext context)
        {
            _context = context;
        }

        // GET /Get
        [HttpGet]
        public async Task<ActionResult<DbImage>> GetImage(string uniqueIdentifier)
        {
            // ... Find Image by uniqueIdentifier
        }

        // POST /Set
        [HttpPost]
        public async Task<ActionResult> SetImage(DbImage dbImage)
        {
            // ... Add or update Image in the database
        }

        // DELETE /Delete
        [HttpDelete("{uniqueIdentifier}")]
        public async Task<ActionResult> DeleteImage(string uniqueIdentifier)
        {
            // ... Delete Image from the database 
        }
    }
}
