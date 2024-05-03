using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Clutch.API.Controllers.Filters;
using Clutch.API.Models.Image;
using Clutch.API.Services.Interfaces;
using System.Reflection;

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

        [HttpGet("GetImage/{request}")]
        //[ValidateRepository]  Validate ContainerImageRequest
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
        public async Task<ActionResult<ContainerImageResponse>> GetContainerImage(ContainerImageRequest request)
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";
            var containerImageResponseData = await _service.GetImageAsync(request, version);

            return !ValidateResponse(containerImageResponseData)
                ? NotFound()
                : Ok(new ContainerImageResponse(
                    containerImageResponseData.Success,
                    _mapper.Map<ContainerImageVersion>(containerImageResponseData.ContainerImageModel),
                    containerImageResponseData.RegistryManifest
                ));
        }

        [HttpPut("SetImage/")]
        [ValidateContainerImage]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
        public async Task<IActionResult> SetContainerImage(ContainerImage containerImage)
        {
            var containerImageModel = _mapper.Map<ContainerImageModel>(containerImage);
            bool success = await _service.SetImageAsync(containerImageModel);

            return success 
                ? Ok()
                : NoContent();
        }

        [HttpDelete("DeleteImage/{Repository}")]
        [ValidateRepository]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
        public async Task<IActionResult> DeleteContainerImageModel(string Repository)
        {
            Repository = System.Net.WebUtility.UrlDecode(Repository);
            var success = await _service.DeleteImageAsync(Repository);

            return success 
                ? Ok()
                : NoContent();
        }

        [HttpGet("GetLatestImages")]
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
                && containerImageResponseData.RegistryManifest.HasValue;
        }
    }
}
