using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Ivony.Html.Web;

namespace Jumony.Demo.DomViewer
{
  public class DomExplorerRegistration : AreaRegistration
  {

    public override string AreaName
    {
      get { return "DomViewer"; }
    }

    public override void RegisterArea( AreaRegistrationContext context )
    {
      context.SimpleRouteTable()
      .MapAction( "~/Dom", "DomTree", "Default" )
      .MapAction( "~/Dom/{hash}", "DomTree", "Default" )
      .MapRoute( "~/Dom/{controller}/{action}" );
    }
  }
}
