using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Adoptly.Web.Filters;

public class ApiKeyAuthFilter : IAuthorizationFilter
{
	private readonly IConfiguration _configuration;
    private static readonly string _apiKeySectionName = "Authentication:ApiKey";
    private static readonly string _apiKeyHeaderName = "X-Api-Key";

    public ApiKeyAuthFilter(IConfiguration configuration) => _configuration = configuration;

    // Checks that API key for BlobExplorerController is correct. 

    public void OnAuthorization(AuthorizationFilterContext context)
	{
        if (!context.HttpContext.Request.Headers.TryGetValue(_apiKeyHeaderName, out var extractedApiKey))
        {
            context.Result = new UnauthorizedObjectResult("API key missing");
            return;
        }

        string apiKey = _configuration.GetValue<string>(_apiKeySectionName);

        if (!apiKey.Equals(extractedApiKey))
        {
            context.Result = new UnauthorizedObjectResult("Invalid API key");
            return;
        }
    }
}