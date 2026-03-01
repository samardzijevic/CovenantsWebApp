using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Covenants.BLL.Services;
using Covenants.Common;
using Covenants.DAL.Repositories;
using Covenants.Models;
using Microsoft.AspNet.Identity;
using Constants = Covenants.Common.Constants;

public partial class Covenants_Details : System.Web.UI.Page
{
    private int _id;
    private CovenantService _covenantSvc;
    private FollowUpService _followUpSvc;
    private ScheduleService _scheduleSvc;
    private HistoryService  _historySvc;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!int.TryParse(Request.QueryString["id"], out _id))
        {
            Response.Redirect("List.aspx");
            return;
        }

        _historySvc  = new HistoryService(new CovenantHistoryRepository());
        _covenantSvc = new CovenantService(new CovenantRepository(), _historySvc);
        _followUpSvc = new FollowUpService(new CovenantFollowUpRepository(), _historySvc);
        _scheduleSvc = new ScheduleService(new CovenantScheduleRepository(), _historySvc);

        hfId.Value = _id.ToString();

        if (!IsPostBack)
        {
            if (!string.IsNullOrEmpty(Request.QueryString["msg"]))
            {
                lblMessage.Text    = Request.QueryString["msg"] == "created" ? "Covenant created successfully." : "Covenant updated successfully.";
                lblMessage.Visible = true;
            }

            LoadPage();
        }
    }

    private void LoadPage()
    {
        var covenant = _covenantSvc.GetById(_id);
        if (covenant == null) { Response.Redirect("List.aspx"); return; }

        // Header
        lblTitle.Text = Server.HtmlEncode(covenant.Title);
        if (covenant.IsDeleted)
        {
            lblDeletedBadge.Visible = true;
            lnkEdit.Visible         = false;
            btnDelete.Visible       = false;
            btnRestore.Visible      = true;
        }
        else
        {
            lnkEdit.NavigateUrl = $"Edit.aspx?id={_id}";
        }

        // Info tab
        lblType.Text          = Server.HtmlEncode(covenant.CovenantTypeName);
        lblProcessingDate.Text = covenant.ProcessingDate.ToString("dd MMM yyyy");
        lblStatus.Text        = covenant.Status;
        lblValue.Text         = covenant.Value.HasValue ? covenant.Value.Value.ToString("N2") : "-";
        lblCurrency.Text      = covenant.Currency ?? "-";
        lblCreatedBy.Text     = covenant.CreatedBy ?? "-";
        lblCreatedAt.Text     = covenant.CreatedAt.ToString("dd MMM yyyy HH:mm");
        lblUpdatedAt.Text     = covenant.UpdatedAt.HasValue ? covenant.UpdatedAt.Value.ToString("dd MMM yyyy HH:mm") : "-";
        lblUpdatedBy.Text     = covenant.UpdatedBy ?? "-";
        lblDescription.Text   = Server.HtmlEncode(covenant.Description ?? "-");

        // Schedule tab
        var schedule = _scheduleSvc.GetActive(_id);
        if (schedule != null)
        {
            pnlNoSchedule.Visible     = false;
            pnlScheduleDetails.Visible = true;
            lblSchedType.Text   = schedule.Description;
            lblSchedStart.Text  = schedule.StartDate.ToString("dd MMM yyyy");
            lblSchedEnd.Text    = schedule.EndDate.HasValue ? schedule.EndDate.Value.ToString("dd MMM yyyy") : "No end";
            lblSchedNext.Text   = schedule.NextRunAt.HasValue ? schedule.NextRunAt.Value.ToString("dd MMM yyyy HH:mm") : "-";
            lblSchedLast.Text   = schedule.LastRunAt.HasValue ? schedule.LastRunAt.Value.ToString("dd MMM yyyy HH:mm") : "Never";
            lblSchedActive.Text = schedule.IsActive ? "Yes" : "No";
        }

        // Follow-ups
        var followUps = _followUpSvc.GetByCovenant(_id);
        gvFollowUps.DataSource = followUps;
        gvFollowUps.DataBind();

        // History
        var history = _historySvc == null ? null : new CovenantHistoryRepository().GetByCovenantId(_id);
        gvHistory.DataSource = history;
        gvHistory.DataBind();

        // Change-schedule panel hidden if covenant is deleted
        pnlChangeSchedule.Visible = !covenant.IsDeleted;
    }

    protected void btnDelete_Click(object sender, EventArgs e)
    {
        string userId = User.Identity.GetUserName();
        var result = _covenantSvc.SoftDelete(_id, userId);
        if (result.Success)
        {
            lblMessage.Text    = "Covenant deleted.";
            lblMessage.Visible = true;
        }
        else
        {
            lblError.Text    = result.ErrorMessage;
            lblError.Visible = true;
        }
        LoadPage();
    }

    protected void btnRestore_Click(object sender, EventArgs e)
    {
        string userId = User.Identity.GetUserName();
        var result = _covenantSvc.Restore(_id, userId);
        if (result.Success)
        {
            lblMessage.Text    = "Covenant restored.";
            lblMessage.Visible = true;
        }
        else
        {
            lblError.Text    = result.ErrorMessage;
            lblError.Visible = true;
        }
        LoadPage();
    }

    protected void btnSaveSchedule_Click(object sender, EventArgs e)
    {
        string userId = User.Identity.GetUserName();
        if (string.IsNullOrEmpty(ddlNewScheduleType.SelectedValue))
        {
            new CovenantScheduleRepository().DeactivateAllForCovenant(_id, userId);
            _historySvc.Write(_id, Constants.HistoryActions.ScheduleChanged, userId, notes: "Schedule removed.");
        }
        else
        {
            var schedule = BuildNewSchedule();

            var result = _scheduleSvc.CreateOrReplace(schedule, userId);
            if (!result.Success)
            {
                lblError.Text    = result.ErrorMessage;
                lblError.Visible = true;
                LoadPage();
                return;
            }
        }

        lblMessage.Text    = "Schedule updated.";
        lblMessage.Visible = true;
        LoadPage();
    }

    private CovenantSchedule BuildNewSchedule()
    {
        string type = ddlNewScheduleType.SelectedValue;

        int interval = 1;
        int iv;
        if (int.TryParse(txtNewInterval.Text, out iv) && iv > 0)
            interval = iv;

        return new CovenantSchedule
        {
            CovenantId   = _id,
            ScheduleType = type,
            Interval     = interval,
            StartDate    = string.IsNullOrEmpty(txtNewSchedStart.Text)
                            ? DateTime.Today
                            : DateTime.Parse(txtNewSchedStart.Text),
            EndDate      = string.IsNullOrEmpty(txtNewSchedEnd.Text)
                            ? (DateTime?)null
                            : DateTime.Parse(txtNewSchedEnd.Text),
            DaysOfWeek   = type == "Weekly"  ? CollectNewDaysOfWeek()  : null,
            DayOfMonth   = (type == "Monthly" || type == "Yearly") && !string.IsNullOrEmpty(txtNewDayOfMonth.Text)
                            ? int.Parse(txtNewDayOfMonth.Text)
                            : (int?)null,
            MonthOfYear  = type == "Yearly"
                            ? int.Parse(ddlNewMonth.SelectedValue)
                            : (int?)null
        };
    }

    private string CollectNewDaysOfWeek()
    {
        var days = new List<string>();
        if (chkNewMon.Checked) days.Add("1");
        if (chkNewTue.Checked) days.Add("2");
        if (chkNewWed.Checked) days.Add("3");
        if (chkNewThu.Checked) days.Add("4");
        if (chkNewFri.Checked) days.Add("5");
        if (chkNewSat.Checked) days.Add("6");
        if (chkNewSun.Checked) days.Add("0");
        return days.Count > 0 ? string.Join(",", days.ToArray()) : "1";
    }

    protected void btnAddFollowUp_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtFuTitle.Text) ||
            string.IsNullOrEmpty(txtFuStart.Text)      ||
            string.IsNullOrEmpty(txtFuEnd.Text))
        {
            lblError.Text    = "Title, Start Date, and End Date are required.";
            lblError.Visible = true;
            LoadPage();
            return;
        }

        string userId = User.Identity.GetUserName();
        var fu = new CovenantFollowUp
        {
            CovenantId  = _id,
            Title       = txtFuTitle.Text.Trim(),
            Description = txtFuDescription.Text.Trim(),
            StartDate   = DateTime.Parse(txtFuStart.Text),
            EndDate     = DateTime.Parse(txtFuEnd.Text),
            Notes       = txtFuNotes.Text.Trim()
        };

        var result = _followUpSvc.Create(fu, userId);
        if (!result.Success)
        {
            lblError.Text    = result.ErrorMessage;
            lblError.Visible = true;
        }
        else
        {
            lblMessage.Text    = "Follow-up added.";
            lblMessage.Visible = true;
            txtFuTitle.Text = txtFuDescription.Text = txtFuStart.Text = txtFuEnd.Text = txtFuNotes.Text = "";
        }

        LoadPage();
    }

    protected void gvFollowUps_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int fuId = int.Parse(e.CommandArgument.ToString());
        string userId = User.Identity.GetUserName();

        if (e.CommandName == "FuStart")
        {
            var r = _followUpSvc.Start(fuId, userId);
            lblMessage.Text    = r.Success ? "Follow-up started." : r.ErrorMessage;
            lblMessage.Visible = r.Success;
            lblError.Text      = r.Success ? "" : r.ErrorMessage;
            lblError.Visible   = !r.Success;
        }
        else if (e.CommandName == "FuComplete")
        {
            // Store follow-up ID and show modal via JS
            hfCompleteId.Value = fuId.ToString();
            // The modal is shown by JS; actual completion happens in btnConfirmComplete_Click
        }
        else if (e.CommandName == "FuCancel")
        {
            var r = _followUpSvc.Cancel(fuId, userId);
            lblMessage.Text    = r.Success ? "Follow-up cancelled." : r.ErrorMessage;
            lblMessage.Visible = r.Success;
            lblError.Text      = r.Success ? "" : r.ErrorMessage;
            lblError.Visible   = !r.Success;
        }

        LoadPage();
    }

    protected void btnConfirmComplete_Click(object sender, EventArgs e)
    {
        if (!int.TryParse(hfCompleteId.Value, out int fuId)) return;

        string userId = User.Identity.GetUserName();
        var result = _followUpSvc.Complete(fuId, userId, txtCompletionNotes.Text.Trim());
        if (result.Success)
        {
            lblMessage.Text    = "Follow-up completed.";
            lblMessage.Visible = true;
        }
        else
        {
            lblError.Text    = result.ErrorMessage;
            lblError.Visible = true;
        }
        txtCompletionNotes.Text = "";
        LoadPage();
    }

    protected void gvFollowUps_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow) return;
        var fu = (CovenantFollowUp)e.Row.DataItem;

        // Status badge
        var lblFuStatus = (Label)e.Row.FindControl("lblFuStatus");
        string css;
        if (fu.Status == Constants.FollowUpStatuses.InProgress)
            css = "label label-info";
        else if (fu.Status == Constants.FollowUpStatuses.Completed)
            css = "label label-success";
        else if (fu.Status == Constants.FollowUpStatuses.CompletedLate)
            css = "label label-danger";
        else if (fu.Status == Constants.FollowUpStatuses.Cancelled)
            css = "label label-warning";
        else
            css = "label label-default";
        lblFuStatus.Text     = fu.Status;
        lblFuStatus.CssClass = css;

        // Row color
        if (fu.IsOverdue)
            e.Row.CssClass = "danger";
        else if (fu.Status == Constants.FollowUpStatuses.Completed)
            e.Row.CssClass = "success";

        // Button visibility
        var btnStart    = (LinkButton)e.Row.FindControl("btnFuStart");
        var btnComplete = (LinkButton)e.Row.FindControl("btnFuComplete");
        var btnCancel   = (LinkButton)e.Row.FindControl("btnFuCancel");

        bool closed = fu.Status == Constants.FollowUpStatuses.Completed   ||
                      fu.Status == Constants.FollowUpStatuses.CompletedLate ||
                      fu.Status == Constants.FollowUpStatuses.Cancelled;

        btnStart.Visible    = fu.Status == Constants.FollowUpStatuses.Pending;
        btnComplete.Visible = !closed;
        btnCancel.Visible   = !closed;

        // Set CommandArgument for complete button (used by row command)
        btnComplete.CommandArgument = fu.Id.ToString();
    }
}
