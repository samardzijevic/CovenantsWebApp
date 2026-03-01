<%@ Page Title="Covenant Details" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Details.aspx.cs" Inherits="Covenants_Details" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <asp:HiddenField ID="hfId" runat="server" />
    <asp:HiddenField ID="hfActiveTab" runat="server" Value="info" />

    <div class="row" style="margin-bottom:10px;">
        <div class="col-md-8">
            <h2>
                <asp:Label ID="lblTitle" runat="server"></asp:Label>
                <asp:Label ID="lblDeletedBadge" runat="server" CssClass="label label-danger" Visible="false" style="font-size:14px; vertical-align:middle; margin-left:8px;">DELETED</asp:Label>
            </h2>
        </div>
        <div class="col-md-4 text-right" style="padding-top:15px;">
            <asp:HyperLink ID="lnkEdit" runat="server" CssClass="btn btn-warning btn-sm">
                <span class="glyphicon glyphicon-pencil"></span> Edit
            </asp:HyperLink>
            <asp:LinkButton ID="btnDelete" runat="server" CssClass="btn btn-danger btn-sm" OnClick="btnDelete_Click"
                OnClientClick="return confirm('Soft-delete this covenant?');">
                <span class="glyphicon glyphicon-trash"></span> Delete
            </asp:LinkButton>
            <asp:LinkButton ID="btnRestore" runat="server" CssClass="btn btn-success btn-sm" Visible="false" OnClick="btnRestore_Click">
                <span class="glyphicon glyphicon-refresh"></span> Restore
            </asp:LinkButton>
            <a href="List.aspx" class="btn btn-default btn-sm">Back to List</a>
        </div>
    </div>

    <asp:Label ID="lblMessage" runat="server" CssClass="alert alert-success" Visible="false" style="display:block; margin-bottom:10px;"></asp:Label>
    <asp:Label ID="lblError"   runat="server" CssClass="alert alert-danger"  Visible="false" style="display:block; margin-bottom:10px;"></asp:Label>

    <!-- Tabs -->
    <ul class="nav nav-tabs" role="tablist">
        <li class="active"><a href="#tab-info"     data-toggle="tab">Info</a></li>
        <li><a href="#tab-schedule"  data-toggle="tab">Schedule</a></li>
        <li><a href="#tab-followups" data-toggle="tab">Follow-Ups <span class="badge" id="followUpCount">0</span></a></li>
        <li><a href="#tab-history"   data-toggle="tab">History</a></li>
    </ul>

    <div class="tab-content" style="border:1px solid #ddd; border-top:none; padding:20px;">

        <!-- TAB 1: INFO -->
        <div class="tab-pane active" id="tab-info">
            <div class="row">
                <div class="col-md-6">
                    <table class="table table-condensed">
                        <tr><th style="width:140px;">Type</th><td><asp:Label ID="lblType" runat="server"></asp:Label></td></tr>
                        <tr><th>Processing Date</th><td><asp:Label ID="lblProcessingDate" runat="server"></asp:Label></td></tr>
                        <tr><th>Status</th><td><asp:Label ID="lblStatus" runat="server"></asp:Label></td></tr>
                        <tr><th>Value</th><td><asp:Label ID="lblValue" runat="server"></asp:Label></td></tr>
                        <tr><th>Currency</th><td><asp:Label ID="lblCurrency" runat="server"></asp:Label></td></tr>
                    </table>
                </div>
                <div class="col-md-6">
                    <table class="table table-condensed">
                        <tr><th style="width:140px;">Created By</th><td><asp:Label ID="lblCreatedBy" runat="server"></asp:Label></td></tr>
                        <tr><th>Created At</th><td><asp:Label ID="lblCreatedAt" runat="server"></asp:Label></td></tr>
                        <tr><th>Last Updated</th><td><asp:Label ID="lblUpdatedAt" runat="server"></asp:Label></td></tr>
                        <tr><th>Updated By</th><td><asp:Label ID="lblUpdatedBy" runat="server"></asp:Label></td></tr>
                    </table>
                </div>
            </div>
            <div class="form-group">
                <strong>Description</strong>
                <div class="well well-sm" style="min-height:50px;">
                    <asp:Label ID="lblDescription" runat="server"></asp:Label>
                </div>
            </div>
        </div>

        <!-- TAB 2: SCHEDULE -->
        <div class="tab-pane" id="tab-schedule">
            <asp:Panel ID="pnlNoSchedule" runat="server">
                <div class="alert alert-info">No schedule configured for this covenant.</div>
            </asp:Panel>
            <asp:Panel ID="pnlScheduleDetails" runat="server" Visible="false">
                <table class="table table-condensed" style="max-width:520px;">
                    <tr><th style="width:140px;">Schedule</th><td><asp:Label ID="lblSchedType" runat="server"></asp:Label></td></tr>
                    <tr><th>Start Date</th><td><asp:Label ID="lblSchedStart" runat="server"></asp:Label></td></tr>
                    <tr><th>End Date</th><td><asp:Label ID="lblSchedEnd" runat="server"></asp:Label></td></tr>
                    <tr><th>Next Run</th><td><asp:Label ID="lblSchedNext" runat="server"></asp:Label></td></tr>
                    <tr><th>Last Run</th><td><asp:Label ID="lblSchedLast" runat="server"></asp:Label></td></tr>
                    <tr><th>Active</th><td><asp:Label ID="lblSchedActive" runat="server"></asp:Label></td></tr>
                </table>
            </asp:Panel>

            <asp:Panel ID="pnlChangeSchedule" runat="server" CssClass="panel panel-default" style="margin-top:20px;">
                <div class="panel-heading">
                    <a data-toggle="collapse" href="#collapseSchedule" style="text-decoration:none;">
                        <strong>Change Schedule</strong> <span class="glyphicon glyphicon-chevron-down"></span>
                    </a>
                </div>
                <div id="collapseSchedule" class="panel-collapse collapse">
                    <div class="panel-body">

                        <!-- Row: Type | Every N | Start | End -->
                        <div class="row">
                            <div class="col-md-3">
                                <div class="form-group">
                                    <label>Schedule Type</label>
                                    <asp:DropDownList ID="ddlNewScheduleType" runat="server" CssClass="form-control"
                                        onchange="detailsSchedToggle(this.value)">
                                        <asp:ListItem Value="">-- Remove Schedule --</asp:ListItem>
                                        <asp:ListItem Value="Once">Once</asp:ListItem>
                                        <asp:ListItem Value="Daily">Daily</asp:ListItem>
                                        <asp:ListItem Value="Weekly">Weekly</asp:ListItem>
                                        <asp:ListItem Value="Monthly">Monthly</asp:ListItem>
                                        <asp:ListItem Value="Yearly">Yearly</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="col-md-2" id="divNewInterval" style="display:none;">
                                <div class="form-group">
                                    <label>Every</label>
                                    <div class="input-group">
                                        <asp:TextBox ID="txtNewInterval" runat="server" CssClass="form-control" Text="1"
                                            style="width:65px;" />
                                        <span class="input-group-addon" id="spanNewIntervalUnit" style="min-width:55px;">days</span>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="form-group">
                                    <label>Start Date</label>
                                    <asp:TextBox ID="txtNewSchedStart" runat="server" CssClass="form-control" TextMode="Date" />
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="form-group">
                                    <label>End Date <small class="text-muted">(optional)</small></label>
                                    <asp:TextBox ID="txtNewSchedEnd" runat="server" CssClass="form-control" TextMode="Date" />
                                </div>
                            </div>
                        </div>

                        <!-- Weekly: day checkboxes -->
                        <div id="divNewWeekly" style="display:none;">
                            <div class="form-group">
                                <label>Days of Week</label>
                                <div>
                                    <label class="checkbox-inline"><asp:CheckBox ID="chkNewMon" runat="server" /> Monday</label>
                                    <label class="checkbox-inline"><asp:CheckBox ID="chkNewTue" runat="server" /> Tuesday</label>
                                    <label class="checkbox-inline"><asp:CheckBox ID="chkNewWed" runat="server" /> Wednesday</label>
                                    <label class="checkbox-inline"><asp:CheckBox ID="chkNewThu" runat="server" /> Thursday</label>
                                    <label class="checkbox-inline"><asp:CheckBox ID="chkNewFri" runat="server" /> Friday</label>
                                    <label class="checkbox-inline"><asp:CheckBox ID="chkNewSat" runat="server" /> Saturday</label>
                                    <label class="checkbox-inline"><asp:CheckBox ID="chkNewSun" runat="server" /> Sunday</label>
                                </div>
                                <small class="text-muted">Select one or more days.</small>
                            </div>
                        </div>

                        <!-- Monthly / Yearly: day of month -->
                        <div id="divNewDayOfMonth" class="row" style="display:none;">
                            <div class="col-md-3">
                                <div class="form-group">
                                    <label>Day of Month <span class="text-muted">(1–28)</span></label>
                                    <asp:TextBox ID="txtNewDayOfMonth" runat="server" CssClass="form-control"
                                        TextMode="Number" placeholder="e.g. 15" />
                                </div>
                            </div>
                        </div>

                        <!-- Yearly: month -->
                        <div id="divNewMonth" class="row" style="display:none;">
                            <div class="col-md-3">
                                <div class="form-group">
                                    <label>Month of Year</label>
                                    <asp:DropDownList ID="ddlNewMonth" runat="server" CssClass="form-control">
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

                        <asp:Button ID="btnSaveSchedule" runat="server" Text="Save Schedule"
                            CssClass="btn btn-primary" OnClick="btnSaveSchedule_Click" />
                    </div>
                </div>
            </asp:Panel>
            <script>
                var newUnitLabels = { 'Daily': 'days', 'Weekly': 'weeks', 'Monthly': 'months', 'Yearly': 'years' };
                function detailsSchedToggle(val) {
                    var hasInterval = (val === 'Daily' || val === 'Weekly' || val === 'Monthly' || val === 'Yearly');
                    document.getElementById('divNewInterval').style.display     = hasInterval ? '' : 'none';
                    document.getElementById('divNewWeekly').style.display       = (val === 'Weekly')  ? '' : 'none';
                    document.getElementById('divNewDayOfMonth').style.display   = (val === 'Monthly' || val === 'Yearly') ? '' : 'none';
                    document.getElementById('divNewMonth').style.display        = (val === 'Yearly')  ? '' : 'none';
                    var span = document.getElementById('spanNewIntervalUnit');
                    if (span) span.innerHTML = newUnitLabels[val] || 'days';
                }
            </script>
        </div>

        <!-- TAB 3: FOLLOW-UPS -->
        <div class="tab-pane" id="tab-followups">
            <div class="row" style="margin-bottom:15px;">
                <div class="col-md-12">
                    <a data-toggle="collapse" href="#collapseAddFollowUp" class="btn btn-primary btn-sm">
                        <span class="glyphicon glyphicon-plus"></span> Add Follow-Up
                    </a>
                </div>
            </div>

            <!-- Add follow-up form (collapsible) -->
            <div id="collapseAddFollowUp" class="panel panel-default panel-collapse collapse" style="margin-bottom:20px;">
                <div class="panel-body">
                    <div class="row">
                        <div class="col-md-8">
                            <div class="form-group">
                                <label>Title <span class="text-danger">*</span></label>
                                <asp:TextBox ID="txtFuTitle" runat="server" CssClass="form-control" MaxLength="200" />
                            </div>
                        </div>
                    </div>
                    <div class="form-group">
                        <label>Description</label>
                        <asp:TextBox ID="txtFuDescription" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" />
                    </div>
                    <div class="row">
                        <div class="col-md-3">
                            <div class="form-group">
                                <label>Start Date <span class="text-danger">*</span></label>
                                <asp:TextBox ID="txtFuStart" runat="server" CssClass="form-control" TextMode="Date" />
                            </div>
                        </div>
                        <div class="col-md-3">
                            <div class="form-group">
                                <label>End Date (Deadline) <span class="text-danger">*</span></label>
                                <asp:TextBox ID="txtFuEnd" runat="server" CssClass="form-control" TextMode="Date" />
                            </div>
                        </div>
                    </div>
                    <div class="form-group">
                        <label>Notes</label>
                        <asp:TextBox ID="txtFuNotes" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" />
                    </div>
                    <asp:Button ID="btnAddFollowUp" runat="server" Text="Add Follow-Up" CssClass="btn btn-success" OnClick="btnAddFollowUp_Click" />
                </div>
            </div>

            <!-- Follow-up list -->
            <asp:GridView ID="gvFollowUps" runat="server"
                AutoGenerateColumns="false"
                CssClass="table table-bordered"
                DataKeyNames="Id"
                OnRowCommand="gvFollowUps_RowCommand"
                OnRowDataBound="gvFollowUps_RowDataBound"
                EmptyDataText="No follow-ups yet.">
                <Columns>
                    <asp:BoundField DataField="Title"       HeaderText="Title" />
                    <asp:BoundField DataField="StartDate"   HeaderText="Start"    DataFormatString="{0:dd MMM yyyy}" />
                    <asp:BoundField DataField="EndDate"     HeaderText="Deadline" DataFormatString="{0:dd MMM yyyy}" />
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate>
                            <asp:Label ID="lblFuStatus" runat="server"></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="StartedBy"   HeaderText="Started By"   NullDisplayText="-" />
                    <asp:BoundField DataField="CompletedBy" HeaderText="Completed By" NullDisplayText="-" />
                    <asp:BoundField DataField="Notes"       HeaderText="Notes"        NullDisplayText="-" />
                    <asp:TemplateField HeaderText="Actions" ItemStyle-Width="200px">
                        <ItemTemplate>
                            <asp:LinkButton ID="btnFuStart"    runat="server" CssClass="btn btn-xs btn-info"    CommandName="FuStart"    CommandArgument='<%# Eval("Id") %>'>Start</asp:LinkButton>
                            <asp:LinkButton ID="btnFuComplete" runat="server" CssClass="btn btn-xs btn-success" CommandName="FuComplete" CommandArgument='<%# Eval("Id") %>'>Complete</asp:LinkButton>
                            <asp:LinkButton ID="btnFuCancel"   runat="server" CssClass="btn btn-xs btn-danger"  CommandName="FuCancel"   CommandArgument='<%# Eval("Id") %>'
                                OnClientClick="return confirm('Cancel this follow-up?');">Cancel</asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>

            <!-- Complete modal -->
            <div id="modalComplete" class="modal fade" tabindex="-1">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header"><h4 class="modal-title">Complete Follow-Up</h4></div>
                        <div class="modal-body">
                            <asp:HiddenField ID="hfCompleteId" runat="server" />
                            <div class="form-group">
                                <label>Completion Notes</label>
                                <asp:TextBox ID="txtCompletionNotes" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
                            </div>
                        </div>
                        <div class="modal-footer">
                            <asp:Button ID="btnConfirmComplete" runat="server" Text="Mark Complete" CssClass="btn btn-success" OnClick="btnConfirmComplete_Click" />
                            <button type="button" class="btn btn-default" data-dismiss="modal">Cancel</button>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- TAB 4: HISTORY -->
        <div class="tab-pane" id="tab-history">
            <asp:GridView ID="gvHistory" runat="server"
                AutoGenerateColumns="false"
                CssClass="table table-condensed table-bordered"
                EmptyDataText="No history yet.">
                <Columns>
                    <asp:BoundField DataField="ChangedAt" HeaderText="When"      DataFormatString="{0:dd MMM yyyy HH:mm}" ItemStyle-Width="140px" />
                    <asp:BoundField DataField="ChangedBy" HeaderText="By"        NullDisplayText="-" ItemStyle-Width="120px" />
                    <asp:BoundField DataField="Action"    HeaderText="Action"    ItemStyle-Width="120px" />
                    <asp:BoundField DataField="FieldName" HeaderText="Field"     NullDisplayText="-" ItemStyle-Width="120px" />
                    <asp:BoundField DataField="OldValue"  HeaderText="Old Value" NullDisplayText="-" />
                    <asp:BoundField DataField="NewValue"  HeaderText="New Value" NullDisplayText="-" />
                    <asp:BoundField DataField="Notes"     HeaderText="Notes"     NullDisplayText="-" />
                </Columns>
            </asp:GridView>
        </div>

    </div><!-- end tab-content -->

    <script>
        // Restore active tab after postback
        $(document).ready(function () {
            var activeTab = $('#<%= hfActiveTab.ClientID %>').val();
            if (activeTab) {
                $('a[href="#tab-' + activeTab + '"]').tab('show');
            }
            $('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
                var id = $(e.target).attr('href').replace('#tab-', '');
                $('#<%= hfActiveTab.ClientID %>').val(id);
            });

            // Show complete modal when FuComplete is clicked
            $('[id$="btnFuComplete"]').on('click', function (e) {
                e.preventDefault();
                var id = $(this).data('fuId');
                $('#<%= hfCompleteId.ClientID %>').val($(this).closest('tr').find('[id$="btnFuComplete"]').attr('data-fu-id'));
                $('#modalComplete').modal('show');
                return false;
            });
        });
    </script>

</asp:Content>
