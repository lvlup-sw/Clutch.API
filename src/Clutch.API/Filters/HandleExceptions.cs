namespace Clutch.API.Filters
{
    public class HandleExceptions : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            try
            {
                // Continue to the next filter or endpoint handler and await the result
                return await next(context);
            }
            catch (TimeoutException)
            {
                // Handle TimeoutException
                return Results.Problem(
                    detail: "Gateway timeout error.",
                    statusCode: 504,
                    title: "ContainerImageService Error");
            }
            catch (Exception)
            {
                // Handle other exceptions
                return Results.Problem(
                    detail: "Internal server error.",
                    statusCode: 500,
                    title: "ContainerImageService Error");
            }
        }
    }
}
