using System;
using System.Web.UI.WebControls;
using Covenants.DAL.Repositories;
using Covenants.Models;
using Microsoft.AspNet.Identity;

public partial class CovenantTypes_List : System.Web.UI.Page
{
    private CovenantTypeRepository _repo;

    protected void Page_Load(object sender, EventArgs e)
    {
        _repo = new CovenantTypeRepository();
        if (!IsPostBack) BindGrid();
    }

    private void BindGrid()
    {
        gvTypes.DataSource = _repo.GetAll();
        gvTypes.DataBind();
    }

    protected void btnAdd_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;

        _repo.Insert(new CovenantType
        {
            Name        = txtName.Text.Trim(),
            Description = txtDescription.Text.Trim(),
            IsActive    = true,
            CreatedBy   = User.Identity.GetUserName()
        });

        txtName.Text = txtDescription.Text = "";
        lblMessage.Text    = "Type added.";
        lblMessage.Visible = true;
        BindGrid();
    }

    protected void gvTypes_RowEditing(object sender, GridViewEditEventArgs e)
    {
        gvTypes.EditIndex = e.NewEditIndex;
        BindGrid();
    }

    protected void gvTypes_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
        gvTypes.EditIndex = -1;
        BindGrid();
    }

    protected void gvTypes_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        int id   = (int)gvTypes.DataKeys[e.RowIndex].Value;
        var row  = gvTypes.Rows[e.RowIndex];
        string name = ((TextBox)row.FindControl("txtEditName")).Text.Trim();
        string desc = ((TextBox)row.FindControl("txtEditDesc")).Text.Trim();

        var existing = _repo.GetById(id);
        existing.Name        = name;
        existing.Description = desc;
        _repo.Update(existing);

        gvTypes.EditIndex  = -1;
        lblMessage.Text    = "Type updated.";
        lblMessage.Visible = true;
        BindGrid();
    }

    protected void gvTypes_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName != "Deactivate" && e.CommandName != "Activate") return;

        int id = int.Parse(e.CommandArgument.ToString());
        _repo.SetActive(id, e.CommandName == "Activate");

        lblMessage.Text    = e.CommandName == "Activate" ? "Type activated." : "Type deactivated.";
        lblMessage.Visible = true;
        BindGrid();
    }

    protected void gvTypes_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow) return;
        var t = (CovenantType)e.Row.DataItem;

        var lblActive    = (Label)e.Row.FindControl("lblActive");
        var btnDeactivate = e.Row.FindControl("btnDeactivate") as LinkButton;
        var btnActivate   = e.Row.FindControl("btnActivate")   as LinkButton;

        if (lblActive != null)
        {
            lblActive.Text     = t.IsActive ? "Yes" : "No";
            lblActive.CssClass = t.IsActive ? "label label-success" : "label label-default";
        }

        if (btnDeactivate != null) btnDeactivate.Visible = t.IsActive;
        if (btnActivate   != null) btnActivate.Visible   = !t.IsActive;
    }
}
