using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Jumony.Demo.Site
{


  public class AuthenticationFilter : IAuthorizationFilter
  {
    public void OnAuthorization( AuthorizationContext filterContext )
    {
      var principal = SiteController.Authentication( filterContext.HttpContext );

      if ( principal.Claims.First( claim => claim.Type == "id" ).Value.Equals( "", StringComparison.OrdinalIgnoreCase ) )
        filterContext.RouteData.DataTokens.Add( "Admin", true );

    }
  }
}
