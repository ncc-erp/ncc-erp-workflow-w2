using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;

namespace W2.Authentication
{
    public class ExternalAuthentication: ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var secretCode = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build()
                            .GetValue<string>($"App:SecurityCode");
            var header = context.HttpContext.Request.Headers;
            var securityCodeHeader = header["X-Secret-Key"];

            if (string.IsNullOrEmpty(securityCodeHeader))
            {
                securityCodeHeader = header["securityCode"];
            }

            if (secretCode != securityCodeHeader)
            {
                throw new UserFriendlyException("SecretCode does not match!");
            }
        }
    }
}
