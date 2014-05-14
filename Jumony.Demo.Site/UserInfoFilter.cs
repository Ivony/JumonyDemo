using Ivony.Html;
using Ivony.Html.ExpandedAPI;
using Ivony.Html.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Jumony.Demo.Site
{
  public class UserInfoFilter : IViewFilter, IMasterViewFiler
  {
    public void OnPostProcess( ViewContext context, JumonyView view )
    {

      if ( view.IsPartialView || view.IsContentView )
        return;

      var body = view.Scope.FindFirstOrDefault( "body" );
      if ( body == null )
        return;

      var container = body.AddElement( 0, "div" ).Style( "text-align", "right" ).Style( "padding", "0px 20px" );
      container.InnerText( SiteController.GetUsername() );

    }

    public void OnPostRender( ViewContext context, JumonyView view )
    {
    }

    public void OnPreProcess( ViewContext context, JumonyView view )
    {
    }

    public void OnPreRender( ViewContext context, JumonyView view )
    {
    }
  }

  public class SiteFilterProvider : IViewFilterProvider
  {
    public IViewFilter[] GetFilters( string virtualPath )
    {
      return new[] { new UserInfoFilter() };
    }
  }

}
