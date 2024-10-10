namespace Clutch.API.Filters
{
    public class ValidateRequest : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            // Access the request object.
            var httpContext = context.HttpContext;

            // Bypass for GET with query params
            if (httpContext.Request.Method == "GET" && httpContext.Request.QueryString.HasValue)
                return await next(context);

            // Get the request object, prioritizing UserRecordRequest
            var containerImageRequest = context.Arguments.OfType<ContainerImageRequest>().FirstOrDefault();
            if (containerImageRequest is null)
            {   // Continue to the next filter or endpoint handler
                return await next(context);
            }

            // Validate request and content type
            if (containerImageRequest is null || !httpContext.Request.HasJsonContentType())
                return Results.BadRequest(new { error = "Invalid request object or content type." });

            return await ValidateImageRequest(context, next, containerImageRequest);
        }

        private static async ValueTask<object?> ValidateImageRequest(EndpointFilterInvocationContext context, EndpointFilterDelegate next, ContainerImageRequest request)
        {
            // Type-specific validation
            if (!ValidateRequestData(request))
                return Results.BadRequest(new { error = "Invalid ContainerImageRequest data." });

            return await next(context);
        }

        private static bool ValidateRequestData(ContainerImageRequest request)
        {
            return
                !string.IsNullOrEmpty(request.Repository) &&
                !string.IsNullOrEmpty(request.Tag) &&
                request.Repository.Contains('/') &&
                Enum.IsDefined(typeof(RegistryType), request.RegistryType) &&
                request.RegistryType != RegistryType.Invalid;
        }
    }
}
