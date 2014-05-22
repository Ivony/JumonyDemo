using Ivony.Html.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Jumony.Demo.HelpCenter
{
  public class HelpManagerController : Controller
  {

    [HttpGet]
    public ActionResult Edit( string path )
    {

      return View( "edit", (object) HelpDocumentProvider.LoadContent( path ) );
    }



    [HttpPost]
    public ActionResult Edit( string path, string content )
    {
      HelpDocumentProvider.SaveDocument( path, content );
      return RedirectToAction( "Entry", "Help", new { path } );
    }

  }
}
