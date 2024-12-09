using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using Microsoft.Extensions.Options;

namespace W2.Mezon;

public class SecretKeyFilterAttribute : ActionFilterAttribute
{
    private readonly Configurations.MezonConfiguration _mezonConfiguration;

    public SecretKeyFilterAttribute(
        IOptions<Configurations.MezonConfiguration> mezonConfigurationOptions
    )
    {
        _mezonConfiguration = mezonConfigurationOptions.Value;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var secretKeyHeaderName = _mezonConfiguration.SecretKeyHeaderName;
        var secretKey = _mezonConfiguration.XSecretKey;
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