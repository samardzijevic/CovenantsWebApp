using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Covenants.BLL.Services;
using Covenants.Common;
using Covenants.DAL.Repositories;
using Covenants.Models;
using Microsoft.AspNet.Identity;
using Constants = Covenants.Common.Constants;

// -----------------------------------------------------------------------
// DETAILS.ASPX.CS — CODE-BEHIND FOR THE COVENANT DETAILS PAGE
// -----------------------------------------------------------------------
// This page is more complex than List.aspx because it has FOUR tabs:
//   1. Info       — view covenant fields, soft-delete, restore
//   2. Schedule   — view current schedule, change schedule
//   3. Follow-Ups — view, add, start, complete, cancel follow-ups
//   4. History    — read-only audit trail
//
// URL FORMAT: Details.aspx?id=42
//   The covenant Id is passed in the query string.
//   If id is missing or invalid, we redirect back to List.aspx.
//
// SERVICE WIRING:
//   Each service needs its own repository injected.
//   HistoryService is shared — all services write to the same audit table.
//
// LOADPAGE vs PAGE_LOAD:
//   LoadPage() is a private helper that pulls data from the DB and populates
//   all controls. We call it from Page_Load (initial load) AND from any
//   event handler that changes data, so the page always shows fresh data
//   after a save — no redirect needed.
// -----------------------------------------------------------------------

public partial class Covenants_Details : System.Web.UI.Page
{
    // The covenant Id extracted from the query string, used throughout
    private int _id;

    // Services — one per business domain
    private CovenantService _covenantSvc;
    private FollowUpService _followUpSvc;
    private ScheduleService _scheduleSvc;
    private HistoryService  _historySvc;

    // ---------------------------------------------------------------
    // PAGE INIT
    // ---------------------------------------------------------------
    protected void Page_Load(object sender, EventArgs e)
    {
        // Try to parse ?id=42 from the URL.
        // If missing or not a number, redirect to the list.
        if (!int.TryParse(Request.QueryString["id"], out _id))
        {
            Response.Redirect("List.aspx");
            return;
        }

        // Wire up the service layer — fresh per request
        _historySvc  = new HistoryService(new CovenantHistoryRepository());
        _covenantSvc = new CovenantService(new CovenantRepository(), _historySvc);
        _followUpSvc = new FollowUpService(new CovenantFollowUpRepository(), _historySvc);
        _scheduleSvc = new ScheduleService(new CovenantScheduleRepository(), _historySvc);

        // Store the Id in a hidden field so postback event handlers can read it
        // (they can't access Request.QueryString on postbacks in some scenarios)
        hfId.Value = _id.ToString();

        if (!IsPostBack)
        {
            // Show a success message if redirected here after Create/Edit
            // e.g. Details.aspx?id=5&msg=created
            if (!string.IsNullOrEmpty(Request.QueryString["msg"]))
            {
                lblMessage.Text    = Request.QueryString["msg"] == "created"
                    ? "Covenant created successfully."
                    : "Covenant updated successfully.";
                lblMessage.Visible = true;
            }

            LoadPage();
        }
    }

    // ---------------------------------------------------------------
    // LOAD PAGE — populates all tabs from the database
    // ---------------------------------------------------------------
    private void LoadPage()
    {
        var covenant = _covenantSvc.GetById(_id);
        if (covenant == null) { Response.Redirect("List.aspx"); return; }

        // --- HEADER AREA ---
        // HtmlEncode prevents XSS: turns < > " ' into safe HTML entities
        lblTitle.Text = Server.HtmlEncode(covenant.Title);
        if (covenant.IsDeleted)
        {
            // Show a red "DELETED" badge; hide Edit/Delete, show Restore
            lblDeletedBadge.Visible = true;
            lnkEdit.Visible         = false;
            btnDelete.Visible       = false;
            btnRestore.Visible      = true;
        }
        else
        {
            lnkEdit.NavigateUrl = $"Edit.aspx?id={_id}";
        }

        // --- TAB 1: INFO ---
        lblType.Text          = Server.HtmlEncode(covenant.CovenantTypeName);
        lblProcessingDate.Text = covenant.ProcessingDate.ToString("dd MMM yyyy");
        lblStatus.Text        = covenant.Status;
        // N2 format: two decimal places with thousands separator (e.g. 1,234.56)
        lblValue.Text         = covenant.Value.HasValue ? covenant.Value.Value.ToString("N2") : "-";
        lblCurrency.Text      = covenant.Currency ?? "-";
        lblCreatedBy.Text     = covenant.CreatedBy ?? "-";
        lblCreatedAt.Text     = covenant.CreatedAt.ToString("dd MMM yyyy HH:mm");
        // Nullable DateTimes: use HasValue to avoid calling ToString() on null
        lblUpdatedAt.Text     = covenant.UpdatedAt.HasValue ? covenant.UpdatedAt.Value.ToString("dd MMM yyyy HH:mm") : "-";
        lblUpdatedBy.Text     = covenant.UpdatedBy ?? "-";
        lblDescription.Text   = Server.HtmlEncode(covenant.Description ?? "-");

        // --- TAB 2: SCHEDULE ---
        var schedule = _scheduleSvc.GetActive(_id);
        if (schedule != null)
        {
            // Show the schedule details panel, hide the "no schedule" message
            pnlNoSchedule.Visible      = false;
            pnlScheduleDetails.Visible = true;

            // Description is a computed property on the model:
            // e.g. "Every 2 weeks on Mon, Wed" or "Every 3 months on the 15th"
            lblSchedType.Text   = schedule.Description;
            lblSchedStart.Text  = schedule.StartDate.ToString("dd MMM yyyy");
            lblSchedEnd.Text    = schedule.EndDate.HasValue ? schedule.EndDate.Value.ToString("dd MMM yyyy") : "No end";
            lblSchedNext.Text   = schedule.NextRunAt.HasValue ? schedule.NextRunAt.Value.ToString("dd MMM yyyy HH:mm") : "-";
            lblSchedLast.Text   = schedule.LastRunAt.HasValue ? schedule.LastRunAt.Value.ToString("dd MMM yyyy HH:mm") : "Never";
            lblSchedActive.Text = schedule.IsActive ? "Yes" : "No";
        }

        // --- TAB 3: FOLLOW-UPS ---
        var followUps = _followUpSvc.GetByCovenant(_id);
        gvFollowUps.DataSource = followUps;
        gvFollowUps.DataBind();  // RowDataBound fires for each row during DataBind()

        // --- TAB 4: HISTORY ---
        // Note: we query the history repository directly here rather than through
        // a service because there's no business logic — just a read.
        var history = new CovenantHistoryRepository().GetByCovenantId(_id);
        gvHistory.DataSource = history;
        gvHistory.DataBind();

        // Don't show "Change Schedule" if the covenant is deleted
        pnlChangeSchedule.Visible = !covenant.IsDeleted;
    }

    // ---------------------------------------------------------------
    // INFO TAB — SOFT DELETE AND RESTORE
    // ---------------------------------------------------------------
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
        // Reload the page so the DELETED badge appears and buttons update
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

    // ---------------------------------------------------------------
    // SCHEDULE TAB — SAVE SCHEDULE CHANGE
    // ---------------------------------------------------------------
    protected void btnSaveSchedule_Click(object sender, EventArgs e)
    {
        string userId = User.Identity.GetUserName();

        if (string.IsNullOrEmpty(ddlNewScheduleType.SelectedValue))
        {
            // User selected "-- None --" → remove the schedule entirely.
            // DeactivateAllForCovenant sets IsActive=0 on any existing schedule rows.
            new CovenantScheduleRepository().DeactivateAllForCovenant(_id, userId);
            _historySvc.Write(_id, Constants.HistoryActions.ScheduleChanged, userId,
                notes: "Schedule removed.");
        }
        else
        {
            // Build the CovenantSchedule object from the form fields
            var schedule = BuildNewSchedule();

            // CreateOrReplace deactivates the old schedule, inserts the new one,
            // calculates NextRunAt, and writes a history entry — all in one call.
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

    /// <summary>
    /// Reads the "Change Schedule" form controls and builds a CovenantSchedule model.
    /// Extracted into its own method to keep btnSaveSchedule_Click readable.
    /// </summary>
    private CovenantSchedule BuildNewSchedule()
    {
        string type = ddlNewScheduleType.SelectedValue;

        // Parse the interval ("every N" field), defaulting to 1 if empty or invalid
        int interval = 1;
        int iv;
        if (int.TryParse(txtNewInterval.Text, out iv) && iv > 0)
            interval = iv;

        return new CovenantSchedule
        {
            CovenantId   = _id,
            ScheduleType = type,
            Interval     = interval,

            // If the Start field is blank, default to today
            StartDate    = string.IsNullOrEmpty(txtNewSchedStart.Text)
                            ? DateTime.Today
                            : DateTime.Parse(txtNewSchedStart.Text),

            // EndDate is optional — null means "run forever"
            EndDate      = string.IsNullOrEmpty(txtNewSchedEnd.Text)
                            ? (DateTime?)null
                            : DateTime.Parse(txtNewSchedEnd.Text),

            // DaysOfWeek only relevant for Weekly — null for all other types
            DaysOfWeek   = type == "Weekly"  ? CollectNewDaysOfWeek()  : null,

            // DayOfMonth for Monthly and Yearly
            DayOfMonth   = (type == "Monthly" || type == "Yearly") && !string.IsNullOrEmpty(txtNewDayOfMonth.Text)
                            ? int.Parse(txtNewDayOfMonth.Text)
                            : (int?)null,

            // MonthOfYear only for Yearly
            MonthOfYear  = type == "Yearly"
                            ? int.Parse(ddlNewMonth.SelectedValue)
                            : (int?)null
        };
    }

    /// <summary>
    /// Reads the seven weekday checkboxes and returns a comma-separated
    /// day number string suitable for storage in CovenantSchedules.DaysOfWeek.
    /// 0=Sunday, 1=Monday, ..., 6=Saturday (matching .NET's DayOfWeek enum).
    /// </summary>
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
        // Default to Monday if nothing was checked
        return days.Count > 0 ? string.Join(",", days.ToArray()) : "1";
    }

    // ---------------------------------------------------------------
    // FOLLOW-UP TAB — ADD MANUAL FOLLOW-UP
    // ---------------------------------------------------------------
    protected void btnAddFollowUp_Click(object sender, EventArgs e)
    {
        // Simple required-field validation
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
            // Status is set to Pending inside FollowUpService.Create()
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
            // Clear the form so it's ready for a new entry
            txtFuTitle.Text = txtFuDescription.Text = txtFuStart.Text = txtFuEnd.Text = txtFuNotes.Text = "";
        }

        LoadPage();
    }

    // ---------------------------------------------------------------
    // FOLLOW-UP TAB — ROW COMMANDS (Start / Complete / Cancel)
    // ---------------------------------------------------------------
    // Same pattern as the Covenants grid: one handler dispatches to
    // the correct service method based on CommandName.
    // ---------------------------------------------------------------
    protected void gvFollowUps_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int fuId   = int.Parse(e.CommandArgument.ToString());
        string userId = User.Identity.GetUserName();

        if (e.CommandName == "FuStart")
        {
            // Pending → InProgress (sets StartedAt and StartedBy in the DB)
            var r = _followUpSvc.Start(fuId, userId);
            lblMessage.Text    = r.Success ? "Follow-up started."  : r.ErrorMessage;
            lblMessage.Visible = r.Success;
            lblError.Text      = r.Success ? ""                    : r.ErrorMessage;
            lblError.Visible   = !r.Success;
        }
        else if (e.CommandName == "FuComplete")
        {
            // We don't complete directly here — we open a modal so the user
            // can type completion notes. The actual save is in btnConfirmComplete_Click.
            // Store the follow-up Id in a hidden field that the modal's confirm
            // button will read.
            hfCompleteId.Value = fuId.ToString();
            // The modal is shown by JavaScript triggered from the .aspx button's OnClientClick
        }
        else if (e.CommandName == "FuCancel")
        {
            // Any non-closed status → Cancelled
            var r = _followUpSvc.Cancel(fuId, userId);
            lblMessage.Text    = r.Success ? "Follow-up cancelled." : r.ErrorMessage;
            lblMessage.Visible = r.Success;
            lblError.Text      = r.Success ? ""                     : r.ErrorMessage;
            lblError.Visible   = !r.Success;
        }

        LoadPage();
    }

    // ---------------------------------------------------------------
    // FOLLOW-UP COMPLETION MODAL — CONFIRM
    // ---------------------------------------------------------------
    protected void btnConfirmComplete_Click(object sender, EventArgs e)
    {
        if (!int.TryParse(hfCompleteId.Value, out int fuId)) return;

        string userId = User.Identity.GetUserName();

        // FollowUpService.Complete() checks the EndDate and sets either
        // "Completed" or "CompletedLate" automatically based on current time
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

    // ---------------------------------------------------------------
    // FOLLOW-UP GRID — ROW DATA-BOUND
    // ---------------------------------------------------------------
    // Customizes each row's appearance based on follow-up status and overdue flag.
    // ---------------------------------------------------------------
    protected void gvFollowUps_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow) return;
        var fu = (CovenantFollowUp)e.Row.DataItem;

        // --- STATUS BADGE ---
        var lblFuStatus = (Label)e.Row.FindControl("lblFuStatus");
        string css;
        if      (fu.Status == Constants.FollowUpStatuses.InProgress)    css = "label label-info";    // blue
        else if (fu.Status == Constants.FollowUpStatuses.Completed)     css = "label label-success"; // green
        else if (fu.Status == Constants.FollowUpStatuses.CompletedLate) css = "label label-danger";  // red
        else if (fu.Status == Constants.FollowUpStatuses.Cancelled)     css = "label label-warning"; // yellow
        else                                                             css = "label label-default"; // grey (Pending)
        lblFuStatus.Text     = fu.Status;
        lblFuStatus.CssClass = css;

        // --- ROW COLOUR ---
        // IsOverdue is a computed column in SQL:
        //   true  if CompletedAt > EndDate, OR CompletedAt IS NULL AND GETUTCDATE() > EndDate
        // Bootstrap class "danger" = red background, "success" = green background
        if (fu.IsOverdue)
            e.Row.CssClass = "danger";
        else if (fu.Status == Constants.FollowUpStatuses.Completed)
            e.Row.CssClass = "success";

        // --- ACTION BUTTON VISIBILITY ---
        var btnStart    = (LinkButton)e.Row.FindControl("btnFuStart");
        var btnComplete = (LinkButton)e.Row.FindControl("btnFuComplete");
        var btnCancel   = (LinkButton)e.Row.FindControl("btnFuCancel");

        // "Closed" = no further state transitions possible
        bool closed = fu.Status == Constants.FollowUpStatuses.Completed     ||
                      fu.Status == Constants.FollowUpStatuses.CompletedLate ||
                      fu.Status == Constants.FollowUpStatuses.Cancelled;

        // Start only shown for Pending follow-ups
        btnStart.Visible    = fu.Status == Constants.FollowUpStatuses.Pending;
        // Complete and Cancel shown for any non-closed follow-up
        btnComplete.Visible = !closed;
        btnCancel.Visible   = !closed;

        // Set CommandArgument so RowCommand knows which follow-up was actioned
        btnComplete.CommandArgument = fu.Id.ToString();
    }
}
