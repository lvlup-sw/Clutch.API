using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace Clutch.API.Controllers.Filters
{
    public partial class ValidateRepository : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var image = System.Net.WebUtility.UrlDecode(context.ActionArguments["Repository"] as string ?? string.Empty);

            if (!IsValidRepository(image))
            {
                context.ModelState.AddModelError("Repository", "Invalid image reference format.");
                context.Result = new BadRequestObjectResult(context.ModelState);
            }
        }

        private static bool IsValidRepository(string Repository) => RepositoryPattern().IsMatch(Repository);

        // <image-name>/<tag>:<version>
        [GeneratedRegex(@"[A-Za-z0-9]+/[A-Za-z0-9]+:([0-9]+(\.[0-9]+)+)", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex RepositoryPattern();
    }
}