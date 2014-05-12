using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Jumony.Demo.Site
{
  public class SiteController : Controller
  {


    private static readonly string ClientID = "000000004811b96d";
    private static readonly string ClientKey = "c94hB5rc6BouSrKzc5XerYDbULONs5QQ"


    public ActionResult Home()
    {
      return RedirectToAction( "Entry", "Help", new { area = "Help" } );
    }


    [HttpGet]
    public async Task<ActionResult> Login( string code = null )
    {
      var currentUrl = HttpUtility.UrlEncode( Request.Url.AbsoluteUri, Encoding.UTF8 );

      if ( code == null )
      {
        var url = string.Format( "https://login.live.com/oauth20_authorize.srf?client_id={0}&redirect_uri={1}&scope=wl.signin&response_type=code", ClientID, currentUrl );

        return Redirect( url );
      }

      else
      {
        var client = new HttpClient();
        var data = new Dictionary<string, string>();

        data.Add( "client_id", ClientID );
        data.Add( "client_secret", ClientKey );
        data.Add( "code", code );
        data.Add( "grant_type", "authorization_code" );
        data.Add( "redirect_uri", currentUrl );

        var message = await client.PostAsync( "https://login.live.com/oauth20_token.srf", new FormUrlEncodedContent( data ) );
        
      }


    }


  }
}
