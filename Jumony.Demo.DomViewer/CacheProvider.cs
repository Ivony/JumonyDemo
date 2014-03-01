﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Ivony.Html.Web;
using Ivony.Web;



namespace Jumony.Demo.DomViewer
{
  class CacheProvider : IMvcCachePolicyProvider
  {
    public CachePolicy CreateCachePolicy( ControllerContext context, ActionDescriptor action, IDictionary<string, object> parameters )
    {
      var cacheToken = CacheToken.FromVirtualPath( context.HttpContext ) + CacheToken.FromQueryString( context.HttpContext );
      return new MyCachePolicy( context.HttpContext, cacheToken, this );
    }

    public CachePolicy CreateCachePolicy( HttpContextBase context )
    {
      throw new NotSupportedException();
    }


    private class MyCachePolicy : StandardCachePolicy
    {
      public MyCachePolicy( HttpContextBase context, CacheToken cacheToken, ICachePolicyProvider provider ) : base( context, cacheToken, provider, TimeSpan.FromDays( 1 ), true ) { }

      public override void ApplyClientCachePolicy()
      {
        base.ApplyClientCachePolicy();

        HttpContext.GetClientCachePolicy().SetCacheability( HttpCacheability.Private );
      }

    }

  }
}