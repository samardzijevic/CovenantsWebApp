<%@ Page Title="New Covenant" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Create.aspx.cs" Inherits="Covenants_Create" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>New Covenant</h2>

    <asp:Label ID="lblError" runat="server" CssClass="alert alert-danger" Visible="false" style="display:block; margin-bottom:15px;"></asp:Label>

    <div class="panel panel-default">
        <div class="panel-heading"><strong>Covenant Details</strong></div>
        <div class="panel-body">
            <div class="row">
                <div class="col-md-8">
                    <div class="form-group">
                        <label>Title <span class="text-danger">*</span></label>
                        <asp:TextBox ID="txtTitle" runat="server" CssClass="form-control" MaxLength="200" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtTitle" ErrorMessage="Title is required." CssClass="text-danger" Display="Dynamic" />
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="form-group">
                        <label>Covenant Type <span class="text-danger">*</span></label>
                        <asp:DropDownList ID="ddlType" runat="server" CssClass="form-control" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlType" InitialValue="" ErrorMessage="Type is required." CssClass="text-danger" Display="Dynamic" />
                    </div>
                </div>
            </div>
            <div class="form-group">
                <label>Description</label>
                <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
            </div>
            <div class="row">
                <div class="col-md-4">
                    <div class="form-group">
                        <label>Processing Date <span class="text-danger">*</span></label>
                        <asp:TextBox ID="txtProcessingDate" runat="server" CssClass="form-control" TextMode="Date" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtProcessingDate" ErrorMessage="Processing date is required." CssClass="text-danger" Display="Dynamic" />
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="form-group">
                        <label>Value</label>
                        <asp:TextBox ID="txtValue" runat="server" CssClass="form-control" TextMode="Number" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label>Currency</label>
                        <asp:TextBox ID="txtCurrency" runat="server" CssClass="form-control" MaxLength="10" placeholder="EUR" />
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="form-group">
                        <label>Status</label>
                        <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-control">
                            <asp:ListItem Value="Active">Active</asp:ListItem>
                            <asp:ListItem Value="Pending">Pending</asp:ListItem>
                            <asp:ListItem Value="Completed">Completed</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- ============================================================ -->
    <!-- TASK SCHEDULE                                                 -->
    <!-- ============================================================ -->
    <div class="panel panel-default">
        <div class="panel-heading"><strong>Task Schedule</strong></div>
        <div class="panel-body">

            <!-- Row 1: Type | Every N | Start | End -->
            <div class="row">
                <div class="col-md-3">
                    <div class="form-group">
                        <label>Schedule Type</label>
                        <asp:DropDownList ID="ddlScheduleType" runat="server" CssClass="form-control"
                            onchange="schedToggle(this.value)">
                            <asp:ListItem Value="">-- No Schedule --</asp:ListItem>
                            <asp:ListItem Value="Once">Once</asp:ListItem>
                            <asp:ListItem Value="Daily">Daily</asp:ListItem>
                            <asp:ListItem Value="Weekly">Weekly</asp:ListItem>
                            <asp:ListItem Value="Monthly">Monthly</asp:ListItem>
                            <asp:ListItem Value="Yearly">Yearly</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="col-md-2" id="divInterval" style="display:none;">
                    <div class="form-group">
                        <label>Every</label>
                        <div class="input-group">
                            <asp:TextBox ID="txtInterval" runat="server" CssClass="form-control" Text="1"
                                style="width:65px;" />
                            <span class="input-group-addon" id="spanIntervalUnit" style="min-width:55px;">days</span>
                        </div>
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="form-group">
                        <label>Start Date</label>
                        <asp:TextBox ID="txtScheduleStart" runat="server" CssClass="form-control" TextMode="Date" />
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="form-group">
                        <label>End Date <small class="text-muted">(optional)</small></label>
                        <asp:TextBox ID="txtScheduleEnd" runat="server" CssClass="form-control" TextMode="Date" />
                    </div>
                </div>
            </div>

            <!-- Weekly: day-of-week checkboxes -->
            <div id="divWeekly" style="display:none;">
                <div class="form-group">
                    <label>Days of Week <span class="text-danger">*</span></label>
                    <div>
                        <label class="checkbox-inline">
                            <asp:CheckBox ID="chkMon" runat="server" /> Monday
                        </label>
                        <label class="checkbox-inline">
                            <asp:CheckBox ID="chkTue" runat="server" /> Tuesday
                        </label>
                        <label class="checkbox-inline">
                            <asp:CheckBox ID="chkWed" runat="server" /> Wednesday
                        </label>
                        <label class="checkbox-inline">
                            <asp:CheckBox ID="chkThu" runat="server" /> Thursday
                        </label>
                        <label class="checkbox-inline">
                            <asp:CheckBox ID="chkFri" runat="server" /> Friday
                        </label>
                        <label class="checkbox-inline">
                            <asp:CheckBox ID="chkSat" runat="server" /> Saturday
                        </label>
                        <label class="checkbox-inline">
                            <asp:CheckBox ID="chkSun" runat="server" /> Sunday
                        </label>
                    </div>
                    <small class="text-muted">Select one or more days.</small>
                </div>
            </div>

            <!-- Monthly / Yearly: day of month -->
            <div id="divDayOfMonth" class="row" style="display:none;">
                <div class="col-md-3">
                    <div class="form-group">
                        <label>Day of Month <span class="text-muted">(1–28)</span></label>
                        <asp:TextBox ID="txtDayOfMonth" runat="server" CssClass="form-control"
                            TextMode="Number" placeholder="e.g. 15" />
                    </div>
                </div>
            </div>

            <!-- Yearly: month -->
            <div id="divMonth" class="row" style="display:none;">
                <div class="col-md-3">
                    <div class="form-group">
                        <label>Month of Year</label>
                        <asp:DropDownList ID="ddlMonth" runat="server" CssClass="form-control">
                            <asp:ListItem Value="1">January</asp:ListItem>
                            <asp:ListItem Value="2">February</asp:ListItem>
                            <asp:ListItem Value="3">March</asp:ListItem>
                            <asp:ListItem Value="4">April</asp:ListItem>
                            <asp:ListItem Value="5">May</asp:ListItem>
                            <asp:ListItem Value="6">June</asp:ListItem>
                            <asp:ListItem Value="7">July</asp:ListItem>
                            <asp:ListItem Value="8">August</asp:ListItem>
                            <asp:ListItem Value="9">September</asp:ListItem>
                            <asp:ListItem Value="10">October</asp:ListItem>
                            <asp:ListItem Value="11">November</asp:ListItem>
                            <asp:ListItem Value="12">December</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
            </div>

        </div>
    </div>

    <div>
        <asp:Button ID="btnSave" runat="server" Text="Create Covenant" CssClass="btn btn-primary" OnClick="btnSave_Click" />
        <a href="List.aspx" class="btn btn-default" style="margin-left:8px;">Cancel</a>
    </div>

    <script>
        var unitLabels = { 'Daily': 'days', 'Weekly': 'weeks', 'Monthly': 'months', 'Yearly': 'years' };

        function schedToggle(val) {
            var hasInterval = (val === 'Daily' || val === 'Weekly' || val === 'Monthly' || val === 'Yearly');
            document.getElementById('divInterval').style.display    = hasInterval ? '' : 'none';
            document.getElementById('divWeekly').style.display      = (val === 'Weekly')  ? '' : 'none';
            document.getElementById('divDayOfMonth').style.display  = (val === 'Monthly' || val === 'Yearly') ? '' : 'none';
            document.getElementById('divMonth').style.display       = (val === 'Yearly')  ? '' : 'none';

            var span = document.getElementById('spanIntervalUnit');
            if (span) span.innerHTML = unitLabels[val] || 'days';
        }
    </script>

</asp:Content>
