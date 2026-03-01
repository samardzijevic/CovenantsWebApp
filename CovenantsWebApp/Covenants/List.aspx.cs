using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Covenants.BLL.Services;
using Covenants.DAL.Repositories;
using Covenants.Models;
using Microsoft.AspNet.Identity;
using Constants = Covenants.Common.Constants;

public partial class Covenants_List : System.Web.UI.Page
{
    private CovenantService     _covenantSvc;
    private CovenantTypeRepository _typeRepo;

    // ---------------------------------------------------------------
    // Page init
    // ---------------------------------------------------------------
    protected void Page_Load(object sender, EventArgs e)
    {
        var histSvc  = new HistoryService(new CovenantHistoryRepository());
        _covenantSvc = new CovenantService(new CovenantRepository(), histSvc);
        _typeRepo    = new CovenantTypeRepository();

        if (!IsPostBack)
        {
            BindTypeDropDowns();
            BindGrid();
        }
    }

    // ---------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------
    private void BindTypeDropDowns()
    {
        var types = _typeRepo.GetAllActive();

        ddlFilterType.Items.Clear();
        ddlFilterType.Items.Add(new ListItem("-- All Types --", ""));
        foreach (var t in types)
            ddlFilterType.Items.Add(new ListItem(t.Name, t.Id.ToString()));

        ddlAddType.Items.Clear();
        foreach (var t in types)
            ddlAddType.Items.Add(new ListItem(t.Name, t.Id.ToString()));

        ddlEditType.Items.Clear();
        foreach (var t in types)
            ddlEditType.Items.Add(new ListItem(t.Name, t.Id.ToString()));
    }

    private void BindGrid()
    {
        bool showDeleted = chkShowDeleted.Checked;
        var all = _covenantSvc.GetAll().ToList();

        if (!showDeleted)
            all = all.Where(c => !c.IsDeleted).ToList();

        if (!string.IsNullOrEmpty(ddlFilterStatus.SelectedValue))
            all = all.Where(c => c.Status == ddlFilterStatus.SelectedValue).ToList();

        if (!string.IsNullOrEmpty(ddlFilterType.SelectedValue) &&
            int.TryParse(ddlFilterType.SelectedValue, out int typeId))
            all = all.Where(c => c.CovenantTypeId == typeId).ToList();

        gvCovenants.DataSource = all;
        gvCovenants.DataBind();
    }

    // ---------------------------------------------------------------
    // Filter buttons
    // ---------------------------------------------------------------
    protected void btnFilter_Click(object sender, EventArgs e)
    {
        BindGrid();
    }

    protected void btnClear_Click(object sender, EventArgs e)
    {
        ddlFilterStatus.SelectedIndex = 0;
        ddlFilterType.SelectedIndex   = 0;
        chkShowDeleted.Checked        = false;
        BindGrid();
    }

    // ---------------------------------------------------------------
    // Grid: row data-bound
    // ---------------------------------------------------------------
    protected void gvCovenants_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow) return;
        var covenant = (Covenant)e.Row.DataItem;

        // Status badge
        var lblStatus = (Label)e.Row.FindControl("lblStatus");
        string css;
        if (covenant.Status == Constants.CovenantStatuses.Active)
            css = "label label-primary";
        else if (covenant.Status == Constants.CovenantStatuses.Pending)
            css = "label label-warning";
        else if (covenant.Status == Constants.CovenantStatuses.Completed)
            css = "label label-success";
        else
            css = "label label-default";
        lblStatus.Text     = covenant.Status;
        lblStatus.CssClass = css;

        // Details link
        var lnkDetails = (HyperLink)e.Row.FindControl("lnkDetails");
        lnkDetails.NavigateUrl = string.Format("Details.aspx?id={0}", covenant.Id);

        // Action buttons
        var btnEdit    = (LinkButton)e.Row.FindControl("btnEdit");
        var btnDelete  = (LinkButton)e.Row.FindControl("btnDelete");
        var btnRestore = (LinkButton)e.Row.FindControl("btnRestore");

        if (covenant.IsDeleted)
        {
            e.Row.CssClass    = "text-muted";
            btnEdit.Visible   = false;
            btnDelete.Visible = false;
            btnRestore.Visible = true;
        }
        else
        {
            // Highlight approaching processing date
            int daysLeft = (int)(covenant.ProcessingDate.Date - DateTime.UtcNow.Date).TotalDays;
            if (daysLeft >= 0 && daysLeft <= Constants.NotificationDaysThreshold &&
                covenant.Status != Constants.CovenantStatuses.Completed)
                e.Row.CssClass = "warning";
        }
    }

    // ---------------------------------------------------------------
    // Grid: row commands
    // ---------------------------------------------------------------
    protected void gvCovenants_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int id = int.Parse(e.CommandArgument.ToString());

        if (e.CommandName == "LoadEdit")
        {
            LoadEditModal(id);
        }
        else if (e.CommandName == "AskDelete")
        {
            LoadDeleteModal(id);
        }
        else if (e.CommandName == "Restore")
        {
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
    // Load edit modal
    // ---------------------------------------------------------------
    private void LoadEditModal(int id)
    {
        var covenant = _covenantSvc.GetById(id);
        if (covenant == null) { BindGrid(); return; }

        hfEditId.Value = id.ToString();

        // Re-populate type dropdowns (PostBack wipes them)
        BindTypeDropDowns();

        txtEditTitle.Text          = covenant.Title;
        txtEditDescription.Text    = covenant.Description ?? "";
        txtEditProcessingDate.Text = covenant.ProcessingDate.ToString("yyyy-MM-dd");
        txtEditValue.Text          = covenant.Value.HasValue ? covenant.Value.Value.ToString("F2") : "";
        txtEditCurrency.Text       = covenant.Currency ?? "";

        // Select correct type
        var typeItem = ddlEditType.Items.FindByValue(covenant.CovenantTypeId.ToString());
        if (typeItem != null) typeItem.Selected = true;

        // Select correct status
        var statusItem = ddlEditStatus.Items.FindByValue(covenant.Status);
        if (statusItem != null) statusItem.Selected = true;

        BindGrid();
        ScriptManager.RegisterStartupScript(this, GetType(), "showEditModal",
            "showModal('#modalEdit');", true);
    }

    // ---------------------------------------------------------------
    // Load delete confirmation modal
    // ---------------------------------------------------------------
    private void LoadDeleteModal(int id)
    {
        var covenant = _covenantSvc.GetById(id);
        if (covenant == null) { BindGrid(); return; }

        hfDeleteId.Value       = id.ToString();
        lblDeleteName.Text     = Server.HtmlEncode(covenant.Title);

        BindGrid();
        ScriptManager.RegisterStartupScript(this, GetType(), "showDeleteModal",
            "showModal('#modalDelete');", true);
    }

    // ---------------------------------------------------------------
    // Add modal — Save
    // ---------------------------------------------------------------
    protected void btnAddSave_Click(object sender, EventArgs e)
    {
        // Validate
        if (string.IsNullOrWhiteSpace(txtAddTitle.Text))
        {
            lblAddError.Text    = "Title is required.";
            lblAddError.Visible = true;
            BindTypeDropDowns();
            BindGrid();
            ScriptManager.RegisterStartupScript(this, GetType(), "showAddModal",
                "showModal('#modalAdd');", true);
            return;
        }

        if (string.IsNullOrEmpty(txtAddProcessingDate.Text))
        {
            lblAddError.Text    = "Processing Date is required.";
            lblAddError.Visible = true;
            BindTypeDropDowns();
            BindGrid();
            ScriptManager.RegisterStartupScript(this, GetType(), "showAddModal",
                "showModal('#modalAdd');", true);
            return;
        }

        if (string.IsNullOrEmpty(ddlAddType.SelectedValue))
        {
            lblAddError.Text    = "Type is required.";
            lblAddError.Visible = true;
            BindTypeDropDowns();
            BindGrid();
            ScriptManager.RegisterStartupScript(this, GetType(), "showAddModal",
                "showModal('#modalAdd');", true);
            return;
        }

        decimal? value = null;
        if (!string.IsNullOrWhiteSpace(txtAddValue.Text))
        {
            if (!decimal.TryParse(txtAddValue.Text, out decimal parsed))
            {
                lblAddError.Text    = "Value must be a valid number.";
                lblAddError.Visible = true;
                BindTypeDropDowns();
                BindGrid();
                ScriptManager.RegisterStartupScript(this, GetType(), "showAddModal",
                    "showModal('#modalAdd');", true);
                return;
            }
            value = parsed;
        }

        string userId = User.Identity.GetUserName();
        var covenant = new Covenant
        {
            CovenantTypeId = int.Parse(ddlAddType.SelectedValue),
            Title          = txtAddTitle.Text.Trim(),
            Description    = txtAddDescription.Text.Trim(),
            ProcessingDate = DateTime.Parse(txtAddProcessingDate.Text),
            Value          = value,
            Currency       = txtAddCurrency.Text.Trim(),
            Status         = ddlAddStatus.SelectedValue
        };

        var result = _covenantSvc.Create(covenant, userId);
        if (!result.Success)
        {
            lblAddError.Text    = result.ErrorMessage;
            lblAddError.Visible = true;
            BindTypeDropDowns();
            BindGrid();
            ScriptManager.RegisterStartupScript(this, GetType(), "showAddModal",
                "showModal('#modalAdd');", true);
            return;
        }

        // Clear add form
        txtAddTitle.Text          = "";
        txtAddDescription.Text    = "";
        txtAddProcessingDate.Text = "";
        txtAddValue.Text          = "";
        txtAddCurrency.Text       = "";
        ddlAddStatus.SelectedIndex = 0;

        lblMessage.Text    = "Covenant created successfully.";
        lblMessage.Visible = true;
        BindTypeDropDowns();
        BindGrid();
    }

    // ---------------------------------------------------------------
    // Edit modal — Save
    // ---------------------------------------------------------------
    protected void btnEditSave_Click(object sender, EventArgs e)
    {
        if (!int.TryParse(hfEditId.Value, out int id))
        {
            BindGrid();
            return;
        }

        // Validate
        if (string.IsNullOrWhiteSpace(txtEditTitle.Text))
        {
            lblEditError.Text    = "Title is required.";
            lblEditError.Visible = true;
            BindTypeDropDowns();
            BindGrid();
            ScriptManager.RegisterStartupScript(this, GetType(), "showEditModal",
                "showModal('#modalEdit');", true);
            return;
        }

        if (string.IsNullOrEmpty(txtEditProcessingDate.Text))
        {
            lblEditError.Text    = "Processing Date is required.";
            lblEditError.Visible = true;
            BindTypeDropDowns();
            BindGrid();
            ScriptManager.RegisterStartupScript(this, GetType(), "showEditModal",
                "showModal('#modalEdit');", true);
            return;
        }

        decimal? value = null;
        if (!string.IsNullOrWhiteSpace(txtEditValue.Text))
        {
            if (!decimal.TryParse(txtEditValue.Text, out decimal parsed))
            {
                lblEditError.Text    = "Value must be a valid number.";
                lblEditError.Visible = true;
                BindTypeDropDowns();
                BindGrid();
                ScriptManager.RegisterStartupScript(this, GetType(), "showEditModal",
                    "showModal('#modalEdit');", true);
                return;
            }
            value = parsed;
        }

        string userId = User.Identity.GetUserName();
        var covenant = _covenantSvc.GetById(id);
        if (covenant == null)
        {
            lblError.Text    = "Covenant not found.";
            lblError.Visible = true;
            BindGrid();
            return;
        }

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
            lblEditError.Text    = result.ErrorMessage;
            lblEditError.Visible = true;
            BindTypeDropDowns();
            BindGrid();
            ScriptManager.RegisterStartupScript(this, GetType(), "showEditModal",
                "showModal('#modalEdit');", true);
            return;
        }

        lblMessage.Text    = "Covenant updated successfully.";
        lblMessage.Visible = true;
        BindTypeDropDowns();
        BindGrid();
    }

    // ---------------------------------------------------------------
    // Delete modal — Confirm
    // ---------------------------------------------------------------
    protected void btnDeleteConfirm_Click(object sender, EventArgs e)
    {
        if (!int.TryParse(hfDeleteId.Value, out int id))
        {
            BindGrid();
            return;
        }

        string userId = User.Identity.GetUserName();
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

        hfDeleteId.Value = "";
        BindGrid();
    }
}
