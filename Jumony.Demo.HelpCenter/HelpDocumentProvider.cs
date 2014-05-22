using Ivony.Html;
using Ivony.Html.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;

namespace Jumony.Demo.HelpCenter
{
  public class HelpDocumentProvider
  {

    public static IHtmlDocument LoadDocument( string path, out CacheDependency dependency )
    {
      return HtmlServices.LoadDocument( GetDocumentPath( path ), out dependency );
    }


    public static void SaveDocument( string path, string content )
    {
      File.WriteAllText( HostingEnvironment.MapPath( GetDocumentPath( path ) ), content );
    }


    public static VirtualPathProvider VirtualPathProvider
    {
      get { return HostingEnvironment.VirtualPathProvider; }
    }


    public static string GetDocumentPath( string path )
    {
      if ( path.EndsWith( "/" ) )
      {

        var _path = VirtualPathUtility.RemoveTrailingSlash( path ) + ".html";
        if ( VirtualPathProvider.FileExists( _path ) )
          return _path;

        _path = path + "_.html";
        if ( VirtualPathProvider.FileExists( _path ) )
          return _path;
      }

      return path;
    }

    public static string LoadContent( string path )
    {
      return HtmlServices.LoadContent( path ).Content;
    }
  }
}
