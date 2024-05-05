using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Clutch.API.Models.Image;

namespace Clutch.API.Controllers.Filters
{
    public class ValidateRequest : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.ActionArguments["request"] as ContainerImageRequest;

            if (request is not null && !context.ModelState.IsValid)
            {
                context.ModelState.AddModelError("request", "Invalid request object.");
                context.Result = new BadRequestObjectResult(context.ModelState);
            }
        }
    }
}
