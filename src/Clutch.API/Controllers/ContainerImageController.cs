using AutoMapper;
using Microsoft.AspNetCore.Mvc;

// TODO:
// - Batch requests for images
// - Authorization with JWT
namespace Clutch.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ContainerImageController(IContainerImageService service, IMapper mapper) : ControllerBase
    {
        private readonly IContainerImageService _service = service;
        private readonly IMapper _mapper = mapper;

        [HttpGet("GetImage/")]
        [ValidateRequest]
        [HandleExceptions]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
        public async Task<ActionResult<ContainerImageResponse>> GetContainerImage([FromQuery] ContainerImageRequest request)
        {
            var containerImageResponseData = await _service.GetImageAsync(request, GetAssemblyVersion());

            return ValidateResponse(containerImageResponseData)
                ? Ok(new ContainerImageResponse(
                    containerImageResponseData.Success,
                    _mapper.Map<ContainerImage>(containerImageResponseData.ContainerImageModel),
                    _mapper.Map<RegistryManifest>(containerImageResponseData.RegistryManifestModel)
                ))
                : NotFound();
        }

        [HttpPut("SetImage/")]
        [ValidateRequest]
        [HandleExceptions]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
        public async Task<IActionResult> SetContainerImage(ContainerImageRequest request)
        {
            bool success = await _service.SetImageAsync(request, GetAssemblyVersion());

            return success
                ? Ok()
                : NoContent();
        }

        [HttpDelete("DeleteImage/")]
        [ValidateRequest]
        [HandleExceptions]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
        public async Task<IActionResult> DeleteContainerImage(ContainerImageRequest request)
        {
            bool success = await _service.DeleteImageAsync(request, GetAssemblyVersion());

            return success
                ? Ok()
                : NoContent();
        }

        [HttpGet("GetLatestImages/")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
        public async Task<ActionResult<IEnumerable<ContainerImage>>> GetLatestContainerImages()
        {
            var containerImageModels = await _service.GetLatestImagesAsync();

            return Ok(_mapper.Map<List<ContainerImage>>(containerImageModels));
        }

        private static bool ValidateResponse(ContainerImageResponseData containerImageResponseData)
        {
            return containerImageResponseData is not null
                && containerImageResponseData.Success
                && containerImageResponseData.ContainerImageModel.HasValue
                && containerImageResponseData.RegistryManifestModel.HasValue;
        }

        private static string GetAssemblyVersion() => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";
    }
}
