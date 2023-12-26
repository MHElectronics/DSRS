using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace AxleLoadSystem.Api.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public class CustomAuthorizeAttribute : AuthorizeAttribute
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="actionContext"></param>
		/// <returns></returns>
		protected override bool IsAuthorized(HttpActionContext actionContext)
		{
			KeyValuePair<string, string> webAuthenticationValue = new KeyValuePair<string, string>("MRT6", "jkWp709VGzI4UWwvLN7ZDN$XJaWOd69C3oht");

            AuthenticationHeaderValue authenticationHeaderValue = actionContext.Request.Headers.Authorization;

			if (authenticationHeaderValue != null)
			{
				return authenticationHeaderValue.Scheme == webAuthenticationValue.Key
					&& authenticationHeaderValue.Parameter == webAuthenticationValue.Value;
			}

			return false;
		}
	}
}
