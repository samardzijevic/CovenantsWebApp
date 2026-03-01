using System;
using System.Web.UI.WebControls;
using Covenants.BLL.Services;
using Covenants.DAL.Repositories;
using Covenants.Models;
using Microsoft.AspNet.Identity;

public partial class Notifications_List : System.Web.UI.Page
{
    private NotificationService _svc;

    protected void Page_Load(object sender, EventArgs e)
    {
        _svc = new NotificationService(new NotificationRepository());
        if (!IsPostBack) BindGrid();
    }

    private void BindGrid()
    {
        string userId = User.Identity.GetUserId();
        gvNotifications.DataSource = _svc.GetForUser(userId);
        gvNotifications.DataBind();
    }

    protected void btnMarkAllRead_Click(object sender, EventArgs e)
    {
        _svc.MarkAllRead(User.Identity.GetUserId());
        lblMessage.Text    = "All notifications marked as read.";
        lblMessage.Visible = true;
        BindGrid();
    }

    protected void gvNotifications_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int id = int.Parse(e.CommandArgument.ToString());

        if (e.CommandName == "MarkRead")
            _svc.MarkRead(id);
        else if (e.CommandName == "Dismiss")
            _svc.Dismiss(id);

        BindGrid();
    }

    protected void gvNotifications_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow) return;
        var n = (Notification)e.Row.DataItem;

        // Unread indicator
        var lblUnread = (Label)e.Row.FindControl("lblUnread");
        if (lblUnread != null) lblUnread.Visible = !n.IsRead;

        // Covenant link
        var lnkCovenant = (HyperLink)e.Row.FindControl("lnkCovenant");
        if (lnkCovenant != null)
        {
            lnkCovenant.Text        = n.CovenantTitle ?? "-";
            lnkCovenant.NavigateUrl = $"~/Covenants/Details.aspx?id={n.CovenantId}";
        }

        // Mark-read button hidden if already read
        var btnMarkRead = (LinkButton)e.Row.FindControl("btnMarkRead");
        if (btnMarkRead != null) btnMarkRead.Visible = !n.IsRead;

        // Bold row for unread
        if (!n.IsRead) e.Row.Font.Bold = true;
    }
}
