using System;
using System.Web.UI.WebControls;
using Covenants.BLL.Services;
using Covenants.DAL.Repositories;
using Covenants.Models;

public partial class _Default : System.Web.UI.Page
{
    protected int NotificationThreshold => Covenants.Common.Constants.NotificationDaysThreshold;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack) LoadDashboard();
    }

    private void LoadDashboard()
    {
        var histSvc  = new HistoryService(new CovenantHistoryRepository());
        var svc      = new CovenantService(new CovenantRepository(), histSvc);

        var active    = svc.GetActive();
        var completed = svc.GetCompleted();

        gvActive.DataSource    = active;
        gvActive.DataBind();
        lblActiveCount.Text    = System.Linq.Enumerable.Count(active).ToString();

        gvCompleted.DataSource = completed;
        gvCompleted.DataBind();
        lblCompletedCount.Text = System.Linq.Enumerable.Count(completed).ToString();
    }

    protected void gvActive_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow) return;
        var c = (Covenant)e.Row.DataItem;

        var lnk = (HyperLink)e.Row.FindControl("lnkActive");
        lnk.Text        = System.Web.HttpUtility.HtmlEncode(c.Title);
        lnk.NavigateUrl = $"~/Covenants/Details.aspx?id={c.Id}";

        var lblStatus = (Label)e.Row.FindControl("lblActiveStatus");
        lblStatus.Text     = c.Status;
        lblStatus.CssClass = c.Status == Covenants.Common.Constants.CovenantStatuses.Active ? "label label-primary" : "label label-warning";

        // Highlight approaching processing date
        int daysLeft = (int)(c.ProcessingDate - DateTime.UtcNow).TotalDays;
        if (daysLeft >= 0 && daysLeft <= Covenants.Common.Constants.NotificationDaysThreshold)
            e.Row.CssClass = "warning";
    }

    protected void gvCompleted_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow) return;
        var c = (Covenant)e.Row.DataItem;

        var lnk = (HyperLink)e.Row.FindControl("lnkCompleted");
        lnk.Text        = System.Web.HttpUtility.HtmlEncode(c.Title);
        lnk.NavigateUrl = $"~/Covenants/Details.aspx?id={c.Id}";
    }
}
