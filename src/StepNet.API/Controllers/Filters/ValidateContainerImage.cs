using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StepNet.API.Models.Image;

namespace StepNet.API.Controllers.Filters
{
    public class ValidateContainerImage : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var image = context.ActionArguments["containerImage"] as ContainerImage;

            if (image is not null && !context.ModelState.IsValid)
            {
                context.ModelState.AddModelError("containerImage", "Invalid container image object.");
                context.Result = new BadRequestObjectResult(context.ModelState);
            }
        }
    }
}
