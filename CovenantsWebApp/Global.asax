<%@ Application Language="C#" %>
<%@ Import Namespace="CovenantsWebApp" %>
<%@ Import Namespace="System.Web.Optimization" %>
<%@ Import Namespace="System.Web.Routing" %>
<%@ Import Namespace="Covenants.Scheduler" %>

<%-- -----------------------------------------------------------------------
     GLOBAL.ASAX — APPLICATION LIFECYCLE HOOKS
     -----------------------------------------------------------------------
     Global.asax is the "heartbeat" of an ASP.NET application. ASP.NET calls
     its methods automatically at key moments in the application's life.

     The two methods we care about:

     Application_Start
       Called ONCE when the web application first starts (e.g. first request
       after IIS starts, or after an app pool recycle). This is where we:
         1. Register URL routes (RouteConfig)
         2. Register CSS/JS bundles (BundleConfig)
         3. Start the background scheduler engines (AppScheduler.Start)

     Application_End
       Called ONCE when the application is shutting down (IIS stopping,
       app pool recycling, bin folder changed, etc.). We stop the background
       timers here so they don't fire after the app is gone.

     WHY STOP THE TIMERS?
       System.Threading.Timer runs on ThreadPool threads. If we don't dispose
       them during shutdown, they might fire after the app context is torn
       down, causing ObjectDisposedException or database errors.

     OTHER EVENTS YOU COULD HANDLE (not used here):
       Application_BeginRequest  — fires at the start of every HTTP request
       Application_Error         — fires when an unhandled exception occurs
       Session_Start             — fires when a new user session begins
     ----------------------------------------------------------------------- --%>

<script runat="server">

    void Application_Start(object sender, EventArgs e)
    {
        // 1. Register URL routes (e.g. ~/Account/Login → Account/Login.aspx)
        RouteConfig.RegisterRoutes(RouteTable.Routes);

        // 2. Bundle and minify CSS + JavaScript files for better performance
        BundleConfig.RegisterBundles(BundleTable.Bundles);

        // 3. Start the background engines:
        //      - SchedulerEngine:    fires every 60s, creates auto follow-ups
        //      - NotificationEngine: fires every 60min, creates deadline alerts
        AppScheduler.Start();
    }

    void Application_End(object sender, EventArgs e)
    {
        // Cleanly stop both background timers before the process exits.
        // Without this, timer callbacks could fire on threads that have no
        // valid HttpContext or database connection, causing cryptic errors.
        AppScheduler.Stop();
    }

</script>
