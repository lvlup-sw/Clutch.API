using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Clutch.API.Controllers.Filters
{
    public class HandleExceptions : ActionFilterAttribute
    {
        public Task OnExceptionAsync(ExceptionContext context)
        {
            if (context.Exception is TimeoutException)
            {
                context.ModelState.AddModelError("ContainerImageService", "Gateway timeout error.");
                context.Result = new ObjectResult(context.ModelState)
                {
                    StatusCode = 504
                };
            }
            else
            {
                context.ModelState.AddModelError("ContainerImageService", "Internal server error.");
                context.Result = new ObjectResult(context.ModelState)
                {
                    StatusCode = 500
                };
            }

            context.ExceptionHandled = true;
            return Task.CompletedTask;
        }
    }
}
