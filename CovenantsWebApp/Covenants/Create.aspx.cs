using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Covenants.BLL.Services;
using Covenants.DAL.Repositories;
using Covenants.Models;
using Microsoft.AspNet.Identity;

public partial class Covenants_Create : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindTypes();
            txtProcessingDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
            txtScheduleStart.Text  = DateTime.Today.ToString("yyyy-MM-dd");
        }
    }

    private void BindTypes()
    {
        var types = new CovenantTypeRepository().GetAllActive();
        ddlType.Items.Clear();
        ddlType.Items.Add(new ListItem("-- Select Type --", ""));
        foreach (var t in types)
            ddlType.Items.Add(new ListItem(t.Name, t.Id.ToString()));
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;

        string userId   = User.Identity.GetUserName();
        var histSvc     = new HistoryService(new CovenantHistoryRepository());
        var covenantSvc = new CovenantService(new CovenantRepository(), histSvc);

        var covenant = new Covenant
        {
            CovenantTypeId = int.Parse(ddlType.SelectedValue),
            Title          = txtTitle.Text.Trim(),
            Description    = txtDescription.Text.Trim(),
            ProcessingDate = DateTime.Parse(txtProcessingDate.Text),
            Value          = string.IsNullOrEmpty(txtValue.Text) ? (decimal?)null : decimal.Parse(txtValue.Text),
            Currency       = txtCurrency.Text.Trim(),
            Status         = ddlStatus.SelectedValue
        };

        var result = covenantSvc.Create(covenant, userId);
        if (!result.Success)
        {
            lblError.Text    = result.ErrorMessage;
            lblError.Visible = true;
            return;
        }

        int covenantId = result.Data;

        if (!string.IsNullOrEmpty(ddlScheduleType.SelectedValue) &&
            !string.IsNullOrEmpty(txtScheduleStart.Text))
        {
            var schedSvc = new ScheduleService(new CovenantScheduleRepository(), histSvc);
            schedSvc.CreateOrReplace(BuildSchedule(covenantId), userId);
        }

        Response.Redirect(string.Format("Details.aspx?id={0}&msg=created", covenantId));
    }

    private CovenantSchedule BuildSchedule(int covenantId)
    {
        string type = ddlScheduleType.SelectedValue;

        int interval = 1;
        int iv;
        if (int.TryParse(txtInterval.Text, out iv) && iv > 0)
            interval = iv;

        return new CovenantSchedule
        {
            CovenantId   = covenantId,
            ScheduleType = type,
            Interval     = interval,
            StartDate    = DateTime.Parse(txtScheduleStart.Text),
            EndDate      = string.IsNullOrEmpty(txtScheduleEnd.Text)
                            ? (DateTime?)null
                            : DateTime.Parse(txtScheduleEnd.Text),
            DaysOfWeek   = type == "Weekly"  ? CollectDaysOfWeek()  : null,
            DayOfMonth   = (type == "Monthly" || type == "Yearly") && !string.IsNullOrEmpty(txtDayOfMonth.Text)
                            ? int.Parse(txtDayOfMonth.Text)
                            : (int?)null,
            MonthOfYear  = type == "Yearly"
                            ? int.Parse(ddlMonth.SelectedValue)
                            : (int?)null
        };
    }

    private string CollectDaysOfWeek()
    {
        var days = new List<string>();
        if (chkMon.Checked) days.Add("1");
        if (chkTue.Checked) days.Add("2");
        if (chkWed.Checked) days.Add("3");
        if (chkThu.Checked) days.Add("4");
        if (chkFri.Checked) days.Add("5");
        if (chkSat.Checked) days.Add("6");
        if (chkSun.Checked) days.Add("0");
        return days.Count > 0 ? string.Join(",", days.ToArray()) : "1";
    }
}
