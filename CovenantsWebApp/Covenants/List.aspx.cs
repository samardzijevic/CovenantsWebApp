using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Covenants.BLL.Services;
using Covenants.DAL.Repositories;
using Covenants.Models;
using Microsoft.AspNet.Identity;
using Constants = Covenants.Common.Constants;

// -----------------------------------------------------------------------
// LIST.ASPX.CS — CODE-BEHIND FOR THE COVENANTS LIST PAGE
// -----------------------------------------------------------------------
// CODE-BEHIND EXPLAINED:
//   In Web Forms, every .aspx page has a paired .aspx.cs "code-behind" file.
//   The .aspx file contains the HTML/server controls (what you see).
//   The .aspx.cs file contains the C# event handlers (what happens when
//   the user clicks buttons, selects dropdowns, etc.)
//
// PAGE LIFECYCLE (simplified, most important events):
//   1. Page_Load       — runs on EVERY request (initial + postback)
//   2. Control events  — e.g. btnAddSave_Click, gvCovenants_RowCommand
//   3. Render          — ASP.NET converts server controls to HTML
//
// POSTBACK:
//   When the user clicks a button, the form submits back to the same URL.
//   IsPostBack is true for all submits AFTER the first page load.
//   We only initialize drop-down lists and bind the grid on non-postback
//   to avoid wiping out filter selections the user has made.
//
// BOOTSTRAP MODALS:
//   Add/Edit/Delete use Bootstrap modal dialogs instead of separate pages.
//   The modal HTML is in the .aspx file (hidden by default).
//   After a postback, ASP.NET re-renders the page and the modal closes.
//   To re-open the correct modal (e.g. when validation fails), we use:
//     ScriptManager.RegisterStartupScript(this, GetType(), "key", "js code", true)
//   This injects a <script> block at the bottom of the page that runs
//   after the page loads in the browser.
// -----------------------------------------------------------------------

public partial class Covenants_List : System.Web.UI.Page
{
    // Services are created fresh each request — not stored as static fields.
    // This is safe because each HTTP request gets its own Page instance.
    private CovenantService        _covenantSvc;
    private CovenantTypeRepository _typeRepo;

    // ---------------------------------------------------------------
    // PAGE INIT — runs on every request (GET and POST)
    // ---------------------------------------------------------------
    protected void Page_Load(object sender, EventArgs e)
    {
        // Wire up dependencies manually (no IoC container)
        var histSvc  = new HistoryService(new CovenantHistoryRepository());
        _covenantSvc = new CovenantService(new CovenantRepository(), histSvc);
        _typeRepo    = new CovenantTypeRepository();

        if (!IsPostBack)
        {
            // First load only: populate dropdowns and show the grid.
            // On postbacks (button clicks) we call these at the end of each
            // handler explicitly so they reflect any changes made.
            BindTypeDropDowns();
            BindGrid();
        }
    }

    // ---------------------------------------------------------------
    // HELPERS
    // ---------------------------------------------------------------

    /// <summary>
    /// Populates ALL three type dropdowns:
    ///   ddlFilterType — the "filter by type" dropdown in the search bar
    ///   ddlAddType    — the type picker in the Add modal
    ///   ddlEditType   — the type picker in the Edit modal
    ///
    /// We must call this on every postback that shows a modal because
    /// dropdown items are NOT preserved across postbacks in Web Forms —
    /// they must be re-populated from the database each time.
    /// </summary>
    private void BindTypeDropDowns()
    {
        var types = _typeRepo.GetAllActive();

        // Filter dropdown gets a blank "All Types" option at the top
        ddlFilterType.Items.Clear();
        ddlFilterType.Items.Add(new ListItem("-- All Types --", ""));
        foreach (var t in types)
            ddlFilterType.Items.Add(new ListItem(t.Name, t.Id.ToString()));

        // Add/Edit modals don't need a blank option (a type is required)
        ddlAddType.Items.Clear();
        foreach (var t in types)
            ddlAddType.Items.Add(new ListItem(t.Name, t.Id.ToString()));

        ddlEditType.Items.Clear();
        foreach (var t in types)
            ddlEditType.Items.Add(new ListItem(t.Name, t.Id.ToString()));
    }

    /// <summary>
    /// Loads all covenants, applies filters, and binds the GridView.
    /// Called on every page load and after any data-changing operation.
    /// </summary>
    private void BindGrid()
    {
        bool showDeleted = chkShowDeleted.Checked;

        // GetAll() includes soft-deleted rows — we filter them here in C#
        // rather than adding a parameter to the service, keeping the service simple.
        var all = _covenantSvc.GetAll().ToList();

        // LINQ filtering: each Where() creates a new filtered list
        if (!showDeleted)
            all = all.Where(c => !c.IsDeleted).ToList();

        // Only filter if a value is actually selected (empty string = show all)
        if (!string.IsNullOrEmpty(ddlFilterStatus.SelectedValue))
            all = all.Where(c => c.Status == ddlFilterStatus.SelectedValue).ToList();

        if (!string.IsNullOrEmpty(ddlFilterType.SelectedValue) &&
            int.TryParse(ddlFilterType.SelectedValue, out int typeId))
            all = all.Where(c => c.CovenantTypeId == typeId).ToList();

        // DataSource = the data, DataBind() = render it
        gvCovenants.DataSource = all;
        gvCovenants.DataBind();
    }

    // ---------------------------------------------------------------
    // FILTER BUTTONS
    // ---------------------------------------------------------------
    protected void btnFilter_Click(object sender, EventArgs e)
    {
        // Re-bind with whatever the current filter values are
        BindGrid();
    }

    protected void btnClear_Click(object sender, EventArgs e)
    {
        // Reset all filter controls to their defaults, then re-bind
        ddlFilterStatus.SelectedIndex = 0;
        ddlFilterType.SelectedIndex   = 0;
        chkShowDeleted.Checked        = false;
        BindGrid();
    }

    // ---------------------------------------------------------------
    // GRID: ROW DATA-BOUND
    // ---------------------------------------------------------------
    // RowDataBound fires for EACH row as the grid renders — this is where
    // we customize individual rows based on their data (colours, button
    // visibility, badge CSS classes).
    //
    // e.Row.FindControl("controlId") finds a control inside the row's
    // template — it returns object, so we cast to the correct type.
    // ---------------------------------------------------------------
    protected void gvCovenants_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        // Skip header/footer rows — only process data rows
        if (e.Row.RowType != DataControlRowType.DataRow) return;

        // DataItem is the object bound to this row (our Covenant model)
        var covenant = (Covenant)e.Row.DataItem;

        // --- STATUS BADGE ---
        // We find the label, set its text and Bootstrap CSS class based on status.
        // Bootstrap label classes: label-primary (blue), label-warning (yellow),
        // label-success (green), label-default (grey)
        var lblStatus = (Label)e.Row.FindControl("lblStatus");
        string css;
        if      (covenant.Status == Constants.CovenantStatuses.Active)    css = "label label-primary";
        else if (covenant.Status == Constants.CovenantStatuses.Pending)   css = "label label-warning";
        else if (covenant.Status == Constants.CovenantStatuses.Completed) css = "label label-success";
        else                                                               css = "label label-default";
        lblStatus.Text     = covenant.Status;
        lblStatus.CssClass = css;

        // --- DETAILS LINK ---
        // Build the URL with the covenant's Id as a query string parameter.
        // Server.HtmlEncode is not needed here as it's a URL, not displayed text.
        var lnkDetails = (HyperLink)e.Row.FindControl("lnkDetails");
        lnkDetails.NavigateUrl = string.Format("Details.aspx?id={0}", covenant.Id);

        // --- ACTION BUTTONS ---
        var btnEdit    = (LinkButton)e.Row.FindControl("btnEdit");
        var btnDelete  = (LinkButton)e.Row.FindControl("btnDelete");
        var btnRestore = (LinkButton)e.Row.FindControl("btnRestore");

        if (covenant.IsDeleted)
        {
            // Soft-deleted rows: grey out the entire row and hide Edit/Delete.
            // Only Restore is available for deleted records.
            e.Row.CssClass     = "text-muted";
            btnEdit.Visible    = false;
            btnDelete.Visible  = false;
            btnRestore.Visible = true;
        }
        else
        {
            // DEADLINE HIGHLIGHT: if the processing date is within the notification
            // threshold (7 days) and the covenant is not completed, turn the row yellow.
            // Bootstrap class "warning" applies a yellow background.
            int daysLeft = (int)(covenant.ProcessingDate.Date - DateTime.UtcNow.Date).TotalDays;
            if (daysLeft >= 0 && daysLeft <= Constants.NotificationDaysThreshold &&
                covenant.Status != Constants.CovenantStatuses.Completed)
                e.Row.CssClass = "warning";
        }
    }

    // ---------------------------------------------------------------
    // GRID: ROW COMMANDS
    // ---------------------------------------------------------------
    // CommandName is set in the .aspx template (e.g. CommandName="LoadEdit").
    // CommandArgument carries the row's covenant Id.
    //
    // WHY NOT USE OnClick events on each button?
    //   GridView rows are repeated — there are multiple Edit buttons.
    //   RowCommand gives us ONE handler that receives the Id for whichever
    //   row was clicked, avoiding the need to wire up N individual events.
    // ---------------------------------------------------------------
    protected void gvCovenants_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int id = int.Parse(e.CommandArgument.ToString());

        if (e.CommandName == "LoadEdit")
        {
            // Populate edit fields from DB and open the Edit modal via JS
            LoadEditModal(id);
        }
        else if (e.CommandName == "AskDelete")
        {
            // Show the delete confirmation modal (doesn't delete yet)
            LoadDeleteModal(id);
        }
        else if (e.CommandName == "Restore")
        {
            // Restore inline (no modal needed for restore — it's reversible)
            string userId = User.Identity.GetUserName();
            var result = _covenantSvc.Restore(id, userId);
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
            BindGrid();
        }
    }

    // ---------------------------------------------------------------
    // EDIT MODAL SETUP
    // ---------------------------------------------------------------
    private void LoadEditModal(int id)
    {
        var covenant = _covenantSvc.GetById(id);
        if (covenant == null) { BindGrid(); return; }

        // Store the Id in a hidden field — the Edit Save button reads it
        // from there to know which covenant to update.
        hfEditId.Value = id.ToString();

        // Must re-populate dropdowns on every postback (Web Forms wipes them)
        BindTypeDropDowns();

        // Fill the edit form fields with current values
        txtEditTitle.Text          = covenant.Title;
        txtEditDescription.Text    = covenant.Description ?? "";
        // Format as yyyy-MM-dd to match the HTML date input format
        txtEditProcessingDate.Text = covenant.ProcessingDate.ToString("yyyy-MM-dd");
        // F2 = two decimal places; guard against null Value with HasValue check
        txtEditValue.Text          = covenant.Value.HasValue ? covenant.Value.Value.ToString("F2") : "";
        txtEditCurrency.Text       = covenant.Currency ?? "";

        // Pre-select the correct items in the dropdowns.
        // FindByValue searches the ListItem collection by the Value attribute.
        var typeItem = ddlEditType.Items.FindByValue(covenant.CovenantTypeId.ToString());
        if (typeItem != null) typeItem.Selected = true;

        var statusItem = ddlEditStatus.Items.FindByValue(covenant.Status);
        if (statusItem != null) statusItem.Selected = true;

        BindGrid();

        // After the postback, re-open the Edit modal via JavaScript.
        // RegisterStartupScript injects: <script>showModal('#modalEdit');</script>
        // into the page so the modal pops open as if the user had clicked the button.
        ScriptManager.RegisterStartupScript(this, GetType(), "showEditModal",
            "showModal('#modalEdit');", true);
    }

    // ---------------------------------------------------------------
    // DELETE CONFIRMATION MODAL SETUP
    // ---------------------------------------------------------------
    private void LoadDeleteModal(int id)
    {
        var covenant = _covenantSvc.GetById(id);
        if (covenant == null) { BindGrid(); return; }

        hfDeleteId.Value   = id.ToString();
        // HtmlEncode prevents XSS: if the title contained "</div><script>", it would
        // break out of the label. Encoding turns < > into &lt; &gt;, making it safe.
        lblDeleteName.Text = Server.HtmlEncode(covenant.Title);

        BindGrid();
        ScriptManager.RegisterStartupScript(this, GetType(), "showDeleteModal",
            "showModal('#modalDelete');", true);
    }

    // ---------------------------------------------------------------
    // ADD MODAL — SAVE
    // ---------------------------------------------------------------
    protected void btnAddSave_Click(object sender, EventArgs e)
    {
        // SERVER-SIDE VALIDATION
        // Even if the browser validates (HTML5 required attribute), we always
        // validate on the server too — the browser can be bypassed with dev tools.
        if (string.IsNullOrWhiteSpace(txtAddTitle.Text))
        {
            ShowAddError("Title is required.");
            return;
        }

        if (string.IsNullOrEmpty(txtAddProcessingDate.Text))
        {
            ShowAddError("Processing Date is required.");
            return;
        }

        if (string.IsNullOrEmpty(ddlAddType.SelectedValue))
        {
            ShowAddError("Type is required.");
            return;
        }

        // Value is optional — only parse if something was typed
        decimal? value = null;
        if (!string.IsNullOrWhiteSpace(txtAddValue.Text))
        {
            if (!decimal.TryParse(txtAddValue.Text, out decimal parsed))
            {
                ShowAddError("Value must be a valid number.");
                return;
            }
            value = parsed;
        }

        // Get the currently logged-in username from ASP.NET Identity
        string userId = User.Identity.GetUserName();

        var covenant = new Covenant
        {
            CovenantTypeId = int.Parse(ddlAddType.SelectedValue),
            Title          = txtAddTitle.Text.Trim(),
            Description    = txtAddDescription.Text.Trim(),
            // DateTime.Parse converts "2025-01-31" → DateTime object
            ProcessingDate = DateTime.Parse(txtAddProcessingDate.Text),
            Value          = value,
            Currency       = txtAddCurrency.Text.Trim(),
            Status         = ddlAddStatus.SelectedValue
        };

        var result = _covenantSvc.Create(covenant, userId);
        if (!result.Success)
        {
            ShowAddError(result.ErrorMessage);
            return;
        }

        // Clear the Add form so it's blank next time the modal opens
        txtAddTitle.Text           = "";
        txtAddDescription.Text     = "";
        txtAddProcessingDate.Text  = "";
        txtAddValue.Text           = "";
        txtAddCurrency.Text        = "";
        ddlAddStatus.SelectedIndex = 0;

        // Show a green success banner above the grid
        lblMessage.Text    = "Covenant created successfully.";
        lblMessage.Visible = true;
        BindTypeDropDowns();
        BindGrid();
    }

    // Helper: show an error in the Add modal and re-open it
    private void ShowAddError(string message)
    {
        lblAddError.Text    = message;
        lblAddError.Visible = true;
        BindTypeDropDowns();
        BindGrid();
        ScriptManager.RegisterStartupScript(this, GetType(), "showAddModal",
            "showModal('#modalAdd');", true);
    }

    // ---------------------------------------------------------------
    // EDIT MODAL — SAVE
    // ---------------------------------------------------------------
    protected void btnEditSave_Click(object sender, EventArgs e)
    {
        // Read the covenant Id from the hidden field that LoadEditModal set
        if (!int.TryParse(hfEditId.Value, out int id))
        {
            BindGrid();
            return;
        }

        // Validate — same rules as Add
        if (string.IsNullOrWhiteSpace(txtEditTitle.Text))
        {
            ShowEditError("Title is required.");
            return;
        }

        if (string.IsNullOrEmpty(txtEditProcessingDate.Text))
        {
            ShowEditError("Processing Date is required.");
            return;
        }

        decimal? value = null;
        if (!string.IsNullOrWhiteSpace(txtEditValue.Text))
        {
            if (!decimal.TryParse(txtEditValue.Text, out decimal parsed))
            {
                ShowEditError("Value must be a valid number.");
                return;
            }
            value = parsed;
        }

        string userId = User.Identity.GetUserName();

        // Load the EXISTING record from DB — CovenantService.Update() will
        // compare old vs new values to write field-level history rows.
        var covenant = _covenantSvc.GetById(id);
        if (covenant == null)
        {
            lblError.Text    = "Covenant not found.";
            lblError.Visible = true;
            BindGrid();
            return;
        }

        // Overwrite only the editable fields — Id, IsDeleted, CreatedAt etc. stay unchanged
        covenant.CovenantTypeId = int.Parse(ddlEditType.SelectedValue);
        covenant.Title          = txtEditTitle.Text.Trim();
        covenant.Description    = txtEditDescription.Text.Trim();
        covenant.ProcessingDate = DateTime.Parse(txtEditProcessingDate.Text);
        covenant.Value          = value;
        covenant.Currency       = txtEditCurrency.Text.Trim();
        covenant.Status         = ddlEditStatus.SelectedValue;

        var result = _covenantSvc.Update(covenant, userId);
        if (!result.Success)
        {
            ShowEditError(result.ErrorMessage);
            return;
        }

        lblMessage.Text    = "Covenant updated successfully.";
        lblMessage.Visible = true;
        BindTypeDropDowns();
        BindGrid();
    }

    // Helper: show an error in the Edit modal and re-open it
    private void ShowEditError(string message)
    {
        lblEditError.Text    = message;
        lblEditError.Visible = true;
        BindTypeDropDowns();
        BindGrid();
        ScriptManager.RegisterStartupScript(this, GetType(), "showEditModal",
            "showModal('#modalEdit');", true);
    }

    // ---------------------------------------------------------------
    // DELETE MODAL — CONFIRM
    // ---------------------------------------------------------------
    protected void btnDeleteConfirm_Click(object sender, EventArgs e)
    {
        if (!int.TryParse(hfDeleteId.Value, out int id))
        {
            BindGrid();
            return;
        }

        string userId = User.Identity.GetUserName();

        // SOFT DELETE — the row is NOT physically removed from the database.
        // CovenantService.SoftDelete() sets IsDeleted=1, records who deleted it.
        // The covenant can be restored later from this same list.
        var result = _covenantSvc.SoftDelete(id, userId);
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

        // Clear the hidden field so a subsequent accidental submit doesn't re-delete
        hfDeleteId.Value = "";
        BindGrid();
    }
}
