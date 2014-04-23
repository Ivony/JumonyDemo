using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Jumony.Demo.Site
{
  public class SiteController : Controller
  {

    public ActionResult Home()
    {
      return RedirectToAction( "Entry", "Help", new { area = "Help" } );
    }


    public ActionResult Login()
    {
      throw new NotImplementedException(); 
    }


  }
}
