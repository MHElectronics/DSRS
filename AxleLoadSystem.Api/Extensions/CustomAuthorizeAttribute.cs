using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;

namespace AxleLoadSystem.Api.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public class CustomAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        //protected override bool IsAuthorized(HttpActionContext actionContext)
        //{
        //          KeyValuePair<string, string> webAuthenticationValue = new KeyValuePair<string, string>("alcs_1", "authKey");

        //          AuthenticationHeaderValue authenticationHeaderValue = actionContext.Request.Headers.Authorization;

        //	if (authenticationHeaderValue != null)
        //	{
        //		return authenticationHeaderValue.Scheme == webAuthenticationValue.Key
        //			&& authenticationHeaderValue.Parameter == webAuthenticationValue.Value;
        //	}

        //	return false;
        //}
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // skip authorization if action is decorated with [AllowAnonymous] attribute
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
            if (allowAnonymous)
                return;

            //----------
            KeyValuePair<string, string> webAuthenticationValue = new KeyValuePair<string, string>("1", "authKey");
            bool isAuthorized = false;
            
            if (context.HttpContext.Request.Headers.Keys.Contains("Station")
                && context.HttpContext.Request.Headers.Keys.Contains("ApiKey"))
            {
                string station = context.HttpContext.Request.Headers["Station"];
                string apiKey = context.HttpContext.Request.Headers["ApiKey"];

                if(station == webAuthenticationValue.Key
                    && apiKey == webAuthenticationValue.Value)
                {
                    isAuthorized = true;
                }
            }

            // authorization
            if (!isAuthorized)
            {
                // not logged in or role not authorized
                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
            }
        }
    }
}
