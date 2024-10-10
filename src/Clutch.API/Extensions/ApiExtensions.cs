using AutoMapper;
using Clutch.API.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Clutch.API.Extensions
{
    // This 'extensions' class is really our controller
    // I wanted to follow the minimal api spec 1:1 but it
    // made the Program.cs file feel a bit bloated
    internal static class ApiExtensions
    {
        public static void AddApiEndpoints(this WebApplication app)
        {
            #region ContainerImage GET/SET/DEL
            var containerImageGroup = app.MapGroup("/ContainerImage")
                .WithTags("ContainerImage")
                .AddEndpointFilter<ValidateRequest>()
                .AddEndpointFilter<HandleExceptions>();

            // Get ContainerImage
            containerImageGroup.MapGet("GetContainerImage", async ([AsParameters] ContainerImageRequest request, IContainerImageService _service, IMapper _mapper) =>
            {
                var containerImageResponseData = await _service.GetImageAsync(request, AssemblyVersion);

                return ValidResponse(containerImageResponseData) switch
                {
                    true => Results.Ok(
                        ConvertImageData(containerImageResponseData, _mapper)
                    ),
                    false => HandleFailureCases(containerImageResponseData, _mapper)
                };
            })
            .Produces<ContainerImageResponse>()
            .RequirePerUserRateLimit();

            // Set ContainerImage (upsert)
            containerImageGroup.MapPut("SetContainerImage", async ([FromBody] ContainerImageRequest request, IContainerImageService _service) =>
            {
                var success = await _service.SetImageAsync(request, AssemblyVersion);

                return success
                    ? Results.Ok()
                    : Results.NoContent();
            })
            .Accepts<ContainerImageRequest>("application/json")
            .RequirePerUserRateLimit();

            // Delete ContainerImage
            containerImageGroup.MapDelete("DeleteContainerImage", async ([FromBody] ContainerImageRequest request, IContainerImageService _service) =>
            {
                var success = await _service.DeleteImageAsync(request, AssemblyVersion);

                return success
                    ? Results.Ok()
                    : Results.NoContent();
            })
            .Accepts<ContainerImageRequest>("application/json")
            .RequirePerUserRateLimit();
        }

        #endregion
        #region Utility Functions

        private static bool ValidResponse(ContainerImageResponseData containerImageResponseData)
        {
            return containerImageResponseData is not null
                && containerImageResponseData.Success
                && containerImageResponseData.ContainerImageModel.HasValue
                && containerImageResponseData.RegistryManifestModel.HasValue;
        }

        private static IResult HandleFailureCases(ContainerImageResponseData containerImageResponseData, IMapper _mapper)
        {
            if (containerImageResponseData.ContainerImageModel.HasValue)
            {
                string status = containerImageResponseData.ContainerImageModel.Status is not StatusEnum.Available
                    ? $"The container image manifest is not available at this time: {containerImageResponseData.ContainerImageModel.Status}."
                    : $"Unknown Error: The container image manifest is available but was not retrieved.";

                var resp = new
                {
                    status,
                    success = false,
                    image = _mapper.Map<ContainerImage>(containerImageResponseData.ContainerImageModel)
                };

                return Results.Accepted(value: resp);
            }
            else
            {
                return Results.NotFound();
            }
        }

        private static ContainerImageResponse ConvertImageData(ContainerImageResponseData response, IMapper _mapper)
        {
            return new ContainerImageResponse(
                response.Success,
                _mapper.Map<ContainerImage>(response.ContainerImageModel),
                _mapper.Map<RegistryManifest>(response.RegistryManifestModel)
            );
        }

        // Version from csproj (package)
        private static readonly string AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.1";
        #endregion
    }
}