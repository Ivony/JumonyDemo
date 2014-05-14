<%@ Application Language="C#" %>

<script RunAt="server">

  void Application_Start( object sender, EventArgs e )
  {

    AreaRegistration.RegisterAllAreas();

    MvcEnvironment.SimpleRouteTable
      .MapAction( "~/", "Site", "Home" )
      .MapRoute( "~/{action}", new { controller = "Site" } )
      .MapDefaultRoute();

    SimpleRouteTable.DebugMode = true;


    MvcEnvironment.JumonyViewEngine.AreaMasterLocationFormats
      = MvcEnvironment.JumonyViewEngine.AreaPartialViewLocationFormats
      = MvcEnvironment.JumonyViewEngine.AreaViewLocationFormats
      = new[] { "~/Areas/{2}/Views/{0}.htm", "~/Areas/{2}/Views/{0}.html", };

    MvcEnvironment.JumonyViewEngine.MasterLocationFormats
      = MvcEnvironment.JumonyViewEngine.PartialViewLocationFormats
      = MvcEnvironment.JumonyViewEngine.ViewLocationFormats
      = new[] { "~/Views/{0}.htm", "~/Views/{0}.html", };

    WebServiceLocator.RegisterService( new Jumony.Demo.Site.SiteFilterProvider() );

  }

  void Application_End( object sender, EventArgs e )
  {
    //  在应用程序关闭时运行的代码

  }

  void Application_Error( object sender, EventArgs e )
  {
    // 在出现未处理的错误时运行的代码

  }

  void Session_Start( object sender, EventArgs e )
  {
    // 在新会话启动时运行的代码

  }

  void Session_End( object sender, EventArgs e )
  {
    // 在会话结束时运行的代码。 
    // 注意: 只有在 Web.config 文件中的 sessionstate 模式设置为
    // InProc 时，才会引发 Session_End 事件。如果会话模式设置为 StateServer
    // 或 SQLServer，则不引发该事件。

  }
       
</script>
