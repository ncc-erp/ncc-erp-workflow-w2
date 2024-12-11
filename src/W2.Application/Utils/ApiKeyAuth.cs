using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Microsoft.Extensions.Options;

namespace W2.Utils;

public class ApiKeyAuth : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var serviceProvider = context.HttpContext.RequestServices;

        var apiConfiguration = serviceProvider.GetRequiredService<IOptions<Configurations.ApiConfiguration>>().Value;

        var secretKeyHeaderName = apiConfiguration.SecretKeyHeaderName;
        var secretKey = apiConfiguration.XSecretKey;
        if (!context.HttpContext.Request.Headers.TryGetValue(secretKeyHeaderName, out var extractedSecretKey))
        {
            context.Result = new UnauthorizedObjectResult(new { Message = "Invalid or Missing API Key!" });
            return;
        }

        if (!secretKey.Equals(extractedSecretKey.FirstOrDefault()))
        {
            context.Result = new UnauthorizedObjectResult(new { Message = "Secret Key Not Match!" });
            return;
        }

        base.OnActionExecuting(context);
    }
}