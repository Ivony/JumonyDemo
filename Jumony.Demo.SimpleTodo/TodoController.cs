using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Ivony.Html;
using Ivony.Html.Web;
using Ivony.Data;
using Ivony.Web;
using System.Threading.Tasks;

namespace Jumony.Demo.SimpleTodo
{

  public class TodoController : Controller
  {



    private Ivony.Data.SqlClient.SqlDbExecutor dbUtility = SqlServer.FromConfiguration( "Database" );

    public async Task<ActionResult> Index()
    {
      return View( "index", await dbUtility.T( "SELECT ID, Title, Completed FROM Tasks" ).ExecuteEntitiesAsync<TodoTask>() );
    }


    public async Task<ActionResult> Add( string title )
    {

      await dbUtility.T( "INSERT Tasks ( Title, Completed ) VALUES ( {...} )", title, false ).ExecuteNonQueryAsync();

      return RedirectToAction( "Index" );
    }

    public async Task<ActionResult> Complete( int taskId )
    {
      await dbUtility.T( "UPDATE Tasks SET Completed = 1 WHERE ID = {0}", taskId ).ExecuteNonQueryAsync();

      return RedirectToAction( "Index" );
    }

    public async Task<ActionResult> Revert( int taskId )
    {
      await dbUtility.T( "UPDATE Tasks SET Completed = 0 WHERE ID = {0}", taskId ).ExecuteNonQueryAsync();

      return RedirectToAction( "Index" );
    }

    public async Task<ActionResult> Remove( int taskId )
    {
      await dbUtility.T( "DELETE Tasks WHERE ID = {0}", taskId ).ExecuteNonQueryAsync();

      return RedirectToAction( "Index" );
    }

    [HttpGet]
    public async Task<ActionResult> Modify( int taskId )
    {

      return View( "modify", await dbUtility.T( "SELECT ID, Title, Completed FROM Tasks WHERE ID = {0}", taskId ).ExecuteEntityAsync<TodoTask>() );

    }

    [HttpPost]
    public async Task<ActionResult> Modify( int taskId, string title )
    {

      if ( !ViewData.ModelState.IsValid )
        return View( "Index" );


      await dbUtility.T( "UPDATE Tasks SET Title = {1} WHERE ID = {0}", taskId, title ).ExecuteNonQueryAsync();

      return RedirectToAction( "Index" );
    }

  }


  public class TodoCachePolicyProvider : ControllerCachePolicyProvider
  {
    public CachePolicy Index( ControllerContext context, IDictionary<string, object> args )
    {

      var token = CacheToken.FromCookies( context.HttpContext ) + CacheToken.CreateToken( "Index" );

      return null;
      return new StandardCachePolicy( context.HttpContext, token, this, TimeSpan.FromMinutes( 1 ), true );

    }
  }
}