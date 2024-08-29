using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Clutch.API.Controllers.Filters
{
    public class ValidateRequest : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.ActionArguments["request"] as ContainerImageRequest;

            if (request is null || !context.ModelState.IsValid)
            {
                context.ModelState.AddModelError("request", "Invalid request object.");
                context.Result = new BadRequestObjectResult(context.ModelState);
                return;
            }

            if (!ValidateRequestData(request))
            {
                context.ModelState.AddModelError("request", "Invalid request object data.");
                context.Result = new BadRequestObjectResult(context.ModelState);
                return;
            }

            base.OnActionExecuting(context);
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
