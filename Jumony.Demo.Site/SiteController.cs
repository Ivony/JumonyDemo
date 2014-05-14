using System;
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


    private static readonly string useridCacheToken = "userid";


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


        var data = await LoadJsonData( message );
        var accessToken = data["access_token"].CastTo<string>();

        message = await client.GetAsync( "https://apis.live.net/v5.0/me?access_token=" + accessToken );
        if ( message.StatusCode != HttpStatusCode.OK )
          return LoginFailed( sourceUrl, await message.Content.ReadAsStringAsync() );


        data = await LoadJsonData( message );
        var userid = data["id"].CastTo<string>();
        var name = data["name"].CastTo<string>();
        EnsureUserdata( userid, name );

        HttpContext.Response.SetCookie( new HttpCookie( useridCacheToken, userid ) );

        return Redirect( sourceUrl );
      }
    }

    private static readonly SqlDbUtility db = SqlDbUtility.Create( "Database" );


    public static string GetUsername( string userid = null )
    {
      if ( userid == null )
      {
        var cookie = System.Web.HttpContext.Current.Request.Cookies[useridCacheToken];
        if ( cookie == null )
          return null;

        userid = cookie.Value;
      }


      return db.T( "SELECT name FROM Users WHERE id = {0}", userid ).ExecuteScalar<string>();
    }


    private void EnsureUserdata( string userId, string name )
    {
      if ( userId == null )
        throw new ArgumentNullException( "userId" );

      if ( string.IsNullOrWhiteSpace( name ) || name.Length > 50 )
        name = userId;

      using ( var transaction = db.BeginTransaction() )
      {
        var dataItem = transaction.T( "SELECT id, name FROM Users WHERE id = {0}", userId ).ExecuteDynamicObject();
        if ( dataItem == null )
        {
          transaction.T( "INSERT INTO Users ( id, name ) VALUES ( {...} )", userId, name ).ExecuteNonQuery();
          transaction.Commit();

          return;
        }

        if ( name == dataItem.name )
          return;

        transaction.T( "UPDATE Users SET name = {1} WHERE id = {0}", userId, name ).ExecuteNonQuery();
        transaction.Commit();
      }
    }


    private ActionResult LoginFailed( string sourceUrl, string message )
    {
      ViewData["error"] = message;
      ViewData["source"] = sourceUrl;
      return View( "LoginFailed" );
    }


    private static async Task<IDictionary<string, object>> LoadJsonData( HttpResponseMessage message )
    {
      var data = new JavaScriptSerializer().DeserializeObject( await message.Content.ReadAsStringAsync() ).CastTo<IDictionary<string, object>>();
      return data;
    }
  }
}
