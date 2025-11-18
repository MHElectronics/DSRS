//using BOL;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Filters;
//using Services;
//using System.Web.Http;

//namespace DSRSystem.Api.Extensions
//{
//    public class CustomAuthorizeAttribute : Attribute, IAuthorizationFilter
//    {
//        public void OnAuthorization(AuthorizationFilterContext context)
//        {
//            // skip authorization if action is decorated with [AllowAnonymous] attribute
//            var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
//            if (allowAnonymous)
//                return;

//            bool isAuthorized = false;
//            string message = "Unauthorized";

//            if (context.HttpContext.Request.Headers.ContainsKey("StationId") && context.HttpContext.Request.Headers.Keys.Contains("ApiKey"))
//            {
//                string strStationId = context.HttpContext.Request.Headers["StationId"];
//                string apiKey = context.HttpContext.Request.Headers["ApiKey"];
//                int stationId;
//                if (int.TryParse(strStationId, out stationId))
//                {
//                    IStationService service = context.HttpContext.RequestServices.GetService<IStationService>();
//                    Station station = service.GetById(new() { StationId = stationId }).GetAwaiter().GetResult();
//                    if (station != null && station.AuthKey == apiKey)
//                    {
//                        isAuthorized = true;
//                    }
//                    else
//                    {
//                        message = "Station Id and Api Key doesn't match";
//                    }
//                }
//            }

//            // authorization
//            if (!isAuthorized)
//            {
//                // not logged in or role not authorized
//                context.Result = new JsonResult(new { message }) { StatusCode = StatusCodes.Status401Unauthorized };
//            }
//        }
//    }
//}
