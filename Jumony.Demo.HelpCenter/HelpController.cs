﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using System.Web.Mvc;
using Ivony.Web;
using Ivony.Fluent;
using Ivony.Html.Web;
using Ivony.Html;
using Ivony.Html.ExpandedAPI;
using System.IO;


namespace Jumony.Demo.HelpCenter
{
  /// <summary>
  /// HelpController 的摘要说明
  /// </summary>
  public class HelpController : Controller
  {

    private const string helpEntriesVirtualPath = "~/HelpEntries/";




    public ActionResult Entry( string path = "." )
    {
      if ( path == "." || path == "/" )
        return RedirectToAction( "Entry", new { path = helpEntriesVirtualPath } );


      ViewBag.Parents = GetParents( path ).Select( GetTopic ).NotNull().Reverse().ToArray();
      ViewBag.Current = GetTopic( path );
      ViewBag.Childs = GetChilds( path ).Select( GetTopic ).NotNull().ToArray();

      return View( HelpDocumentProvider.GetDocumentPath( path ), "frame" );
    }

    private const string navigationCacheKey = "Navigation";

    public ActionResult Navigation( string path )
    {

      return PartialView( "Navigation" );
    }


    private static readonly string _topicCachePrefix = "Help_Topic_";

    private Topic GetTopic( string path )
    {

      var cacheKey = _topicCachePrefix + path;

      var topic = HttpRuntime.Cache.Get( cacheKey ) as Topic;

      if ( topic == null )
      {
        CacheDependency dependency;
        IHtmlDocument document;

        document = HelpDocumentProvider.LoadDocument( path, out dependency );

        if ( document == null )
          return null;


        var title = document.FindFirst( "title" ).InnerText();
        var summaryElement = document.FindFirstOrDefault( "p" );
        var summary = summaryElement.IfNull( null, e => e.InnerText() );

        topic = new Topic
        {
          VirtualPath = path,
          Title = title,
          Summary = summary
        };

        if ( dependency != null )
          HttpRuntime.Cache.Insert( cacheKey, topic, dependency );
      }

      return topic;
    }

    
    private IEnumerable<string> GetParents( string path )
    {
      while ( !path.EqualsIgnoreCase( helpEntriesVirtualPath ) )
      {
        path = VirtualPathHelper.GetParentDirectory( path );
        yield return path;
      }
    }




    public static VirtualPathProvider VirtualPathProvider
    {
      get { return HostingEnvironment.VirtualPathProvider; }
    }

    public IEnumerable<string> GetChilds( string path )
    {

      var directory = path;

      if ( !path.EndsWith( "/" ) )
        directory = path.Remove( path.Length - VirtualPathUtility.GetExtension( path ).Length ) + "/";


      if ( VirtualPathProvider.DirectoryExists( directory ) )
      {
        return VirtualPathProvider.GetDirectory( directory ).Files
          .Cast<VirtualFile>().Select( file => VirtualPathUtility.ToAppRelative( file.VirtualPath ) )
          .Where( p => p.EndsWith( ".html" ) && !p.EndsWith( "_.html" ) ).ToArray();
      }

      else
        return Enumerable.Empty<string>();
    }
  }

  public class Topic
  {
    public string VirtualPath { get; set; }

    public string Title { get; set; }

    public string Summary { get; set; }
  }

}