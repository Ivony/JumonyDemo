﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Ivony.Fluent;
using System.Web.Security;
using Ivony.Data;
using System.Security.Claims;

namespace Jumony.Demo.Site
{
  public class SiteController : Controller
  {


    private static readonly string ClientID = ConfigurationManager.AppSettings["MicrosoftAccount_ClientID"] ?? "000000004811b96d";
    private static readonly string ClientKey = ConfigurationManager.AppSettings["MicrosoftAccount_ClientKey"] ?? "c94hB5rc6BouSrKzc5XerYDbULONs5QQ";


    public ActionResult Home()
    {
      return RedirectToAction( "Entry", "Help", new { area = "Help" } );
    }


    private const string CookieToken = "usertoken";


    [HttpGet]
    public async Task<ActionResult> Login( string sourceUrl = null, string code = null )
    {
      sourceUrl = sourceUrl ?? Url.Action( "Home" );
      var currentUrl = Url.Action( "Login", null, new { source = sourceUrl }, "http" );

      if ( code == null )
      {
        var url = string.Format( "https://login.live.com/oauth20_authorize.srf?client_id={0}&redirect_uri={1}&scope=wl.signin&response_type=code", ClientID, HttpUtility.UrlEncode( currentUrl, Encoding.UTF8 ) );

        return Redirect( url );
      }

      else
      {
        var client = new HttpClient();
        var requestData = new Dictionary<string, string>();

        requestData.Add( "client_id", ClientID );
        requestData.Add( "client_secret", ClientKey );
        requestData.Add( "code", code );
        requestData.Add( "grant_type", "authorization_code" );
        requestData.Add( "redirect_uri", currentUrl );

        var message = await client.PostAsync( "https://login.live.com/oauth20_token.srf", new FormUrlEncodedContent( requestData ) );
        if ( message.StatusCode != HttpStatusCode.OK )
          return LoginFailed( sourceUrl, await message.Content.ReadAsStringAsync() );


        {
          var content = await message.Content.ReadAsStringAsync();
          var data = new JavaScriptSerializer().DeserializeObject( content ).CastTo<IDictionary<string, object>>();
          var accessToken = data["access_token"].CastTo<string>();

          message = await client.GetAsync( "https://apis.live.net/v5.0/me?access_token=" + accessToken );
          if ( message.StatusCode != HttpStatusCode.OK )
            return LoginFailed( sourceUrl, await message.Content.ReadAsStringAsync() );
        }

        {
          var content = await message.Content.ReadAsStringAsync();
          var data = new JavaScriptSerializer().DeserializeObject( content ).CastTo<IDictionary<string, object>>();

          var userid = data["id"].CastTo<string>();
          var name = data["name"].CastTo<string>();


          var ticket = new FormsAuthenticationTicket( 1, userid, DateTime.UtcNow, DateTime.UtcNow.AddHours( 2 ), true, content );
          Response.SetCookie( new HttpCookie( CookieToken, FormsAuthentication.Encrypt( ticket ) ) );
        }

        return Redirect( sourceUrl );
      }
    }

    private static readonly Ivony.Data.SqlClient.SqlDbExecutor db = SqlServer.FromConfiguration( "Database" );



    private ActionResult LoginFailed( string sourceUrl, string message )
    {
      ViewData["error"] = message;
      ViewData["source"] = sourceUrl;
      return View( "LoginFailed" );
    }


    public static ClaimsPrincipal Authentication( HttpContextBase context )
    {
      var cookie = context.Request.Cookies[CookieToken];
      if ( cookie == null )
        return null;


      var ticket = FormsAuthentication.Decrypt( cookie.Value );
      if ( ticket == null || ticket.Expired )
        return null;

      var userid = ticket.Name;
      var content = ticket.UserData;

      var principal = GetClaimsUser( content );

      context.User = principal;

      return principal;
    }

    private static ClaimsPrincipal GetClaimsUser( string content )
    {
      var serializer = new JavaScriptSerializer();

      var userdata = serializer.Deserialize<Dictionary<string, string>>( content );

      var claims = userdata.Select( pair => new Claim( pair.Key, pair.Value ?? "" ) ).ToArray();

      var identity = new ClaimsIdentity( claims, "MicrosoftAccout", "name", null );

      var principal = new ClaimsPrincipal( identity );
      return principal;
    }
  }
}
