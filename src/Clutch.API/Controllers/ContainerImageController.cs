﻿using AutoMapper;
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
        //[ValidateRequest]  Validate ContainerImageRequest
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
        public async Task<ActionResult<ContainerImageResponse>> GetContainerImage(ContainerImageRequest request)
        {
            var containerImageResponseData = await _service.GetImageAsync(request, GetAssemblyVersion());

            return ValidateResponse(containerImageResponseData)
                ? Ok(new ContainerImageResponse(
                    containerImageResponseData.Success,
                    _mapper.Map<ContainerImageVersion>(containerImageResponseData.ContainerImageModel),
                    containerImageResponseData.RegistryManifest
                ))
                : NotFound();
        }

        [HttpPut("SetImage/")]
        [ValidateContainerImage]
        // [ValidateRequest]
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

        [HttpDelete("DeleteImage/{Repository}")]
        [ValidateRepository]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
        public async Task<IActionResult> DeleteContainerImageModel(ContainerImageRequest request)
        {
            bool success = await _service.DeleteImageAsync(request, GetAssemblyVersion());

            return success 
                ? Ok()
                : NoContent();
        }

        [HttpGet("GetLatestImages")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
        public async Task<ActionResult<IEnumerable<ContainerImageVersion>>> GetLatestContainerImages()
        {
            var containerImageModels = await _service.GetLatestImagesAsync();

            return Ok(_mapper.Map<List<ContainerImageVersion>>(containerImageModels));
        }

        private static bool ValidateResponse(ContainerImageResponseData containerImageResponseData)
        {
            return containerImageResponseData is not null
                && containerImageResponseData.Success
                && containerImageResponseData.ContainerImageModel.HasValue
                && containerImageResponseData.RegistryManifest.HasValue;
        }

        private static string GetAssemblyVersion() => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";
    }
}
