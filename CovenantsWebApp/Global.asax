<%@ Application Language="C#" %>
<%@ Import Namespace="CovenantsWebApp" %>
<%@ Import Namespace="System.Web.Optimization" %>
<%@ Import Namespace="System.Web.Routing" %>
<%@ Import Namespace="Covenants.Scheduler" %>

<script runat="server">

    void Application_Start(object sender, EventArgs e)
    {
        RouteConfig.RegisterRoutes(RouteTable.Routes);
        BundleConfig.RegisterBundles(BundleTable.Bundles);
        AppScheduler.Start();
    }

    void Application_End(object sender, EventArgs e)
    {
        AppScheduler.Stop();
    }

</script>
