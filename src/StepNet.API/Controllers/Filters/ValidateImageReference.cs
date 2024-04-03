using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace Clutch.API.Controllers.Filters
{
    public partial class ValidateImageReference : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var image = System.Net.WebUtility.UrlDecode(context.ActionArguments["imageReference"] as string ?? string.Empty);

            if (!IsValidImageReference(image))
            {
                context.ModelState.AddModelError("imageReference", "Invalid image reference format.");
                context.Result = new BadRequestObjectResult(context.ModelState);
            }
        }

        private static bool IsValidImageReference(string imageReference) => ImageReferencePattern().IsMatch(imageReference);

        // <image-name>/<tag>:<version>
        [GeneratedRegex(@"[A-Za-z0-9]+/[A-Za-z0-9]+:([0-9]+(\.[0-9]+)+)", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex ImageReferencePattern();
    }
}