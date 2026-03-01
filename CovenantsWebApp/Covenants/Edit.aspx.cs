using System;
using System.Web.UI.WebControls;
using Covenants.BLL.Services;
using Covenants.DAL.Repositories;
using Covenants.Models;
using Microsoft.AspNet.Identity;

public partial class Covenants_Edit : System.Web.UI.Page
{
    private int _id;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!int.TryParse(Request.QueryString["id"], out _id))
        {
            Response.Redirect("List.aspx");
            return;
        }

        if (!IsPostBack)
        {
            var covenant = new CovenantRepository().GetById(_id);
            if (covenant == null || covenant.IsDeleted)
            {
                Response.Redirect("List.aspx");
                return;
            }

            BindTypes(covenant.CovenantTypeId);
            PopulateForm(covenant);
            hfId.Value = _id.ToString();
            lnkDetails.NavigateUrl = $"Details.aspx?id={_id}";
        }
        else
        {
            _id = int.Parse(hfId.Value);
        }
    }

    private void BindTypes(int selectedId)
    {
        var types = new CovenantTypeRepository().GetAllActive();
        ddlType.Items.Clear();
        foreach (var t in types)
        {
            var item = new ListItem(t.Name, t.Id.ToString());
            if (t.Id == selectedId) item.Selected = true;
            ddlType.Items.Add(item);
        }
    }

    private void PopulateForm(Covenant c)
    {
        txtTitle.Text          = c.Title;
        txtDescription.Text    = c.Description;
        txtProcessingDate.Text = c.ProcessingDate.ToString("yyyy-MM-dd");
        txtValue.Text          = c.Value?.ToString();
        txtCurrency.Text       = c.Currency;
        ddlStatus.SelectedValue = c.Status;
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;

        string userId = User.Identity.GetUserName();
        var histSvc  = new HistoryService(new CovenantHistoryRepository());
        var svc      = new CovenantService(new CovenantRepository(), histSvc);

        var covenant = new Covenant
        {
            Id             = _id,
            CovenantTypeId = int.Parse(ddlType.SelectedValue),
            Title          = txtTitle.Text.Trim(),
            Description    = txtDescription.Text.Trim(),
            ProcessingDate = DateTime.Parse(txtProcessingDate.Text),
            Value          = string.IsNullOrEmpty(txtValue.Text) ? (decimal?)null : decimal.Parse(txtValue.Text),
            Currency       = txtCurrency.Text.Trim(),
            Status         = ddlStatus.SelectedValue,
            UpdatedBy      = userId
        };

        var result = svc.Update(covenant, userId);
        if (!result.Success)
        {
            lblError.Text    = result.ErrorMessage;
            lblError.Visible = true;
            return;
        }

        Response.Redirect($"Details.aspx?id={_id}&msg=updated");
    }
}
