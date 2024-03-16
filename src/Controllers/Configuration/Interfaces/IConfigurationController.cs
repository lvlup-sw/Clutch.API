using Microsoft.AspNetCore.Mvc;
using static StepNet.API.Models.Images.ContainerImageModel;

namespace StepNet.API.Controllers.Configuration.Interfaces
{
    public interface IConfigurationController
    {
        // GET /Get
        Task<ActionResult<DbImage>> GetImage(string uniqueIdentifier);

        // POST /Set
        Task<ActionResult> SetImage(DbImage dbImage);

        // DELETE /Delete
        Task<ActionResult> DeleteImage(string uniqueIdentifier);
    }
}
