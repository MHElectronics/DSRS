using BOL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Services;
using System.Web.Http;

namespace AxleLoadSystem.Api.Extensions
{
    public class CustomAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // skip authorization if action is decorated with [AllowAnonymous] attribute
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
            if (allowAnonymous)
                return;

            //----------
            //KeyValuePair<string, string> webAuthenticationValue = new KeyValuePair<string, string>("1", "authKey");
            bool isAuthorized = false;

            if (context.HttpContext.Request.Headers.Keys.Contains("Station")
                && context.HttpContext.Request.Headers.Keys.Contains("ApiKey"))
            {
                string strStationId = context.HttpContext.Request.Headers["Station"];
                string apiKey = context.HttpContext.Request.Headers["ApiKey"];
                int stationId;
                if (int.TryParse(strStationId, out stationId))
                {
                    IStationService service = context.HttpContext.RequestServices.GetService<IStationService>();
                    Station station = service.GetById(new() { StationId = stationId }).GetAwaiter().GetResult();
                    if (station != null && station.AuthKey == apiKey)
                    {
                        isAuthorized = true;
                    }
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
