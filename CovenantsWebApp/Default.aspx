<%@ Page Title="Home" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="row" style="margin-bottom:10px;">
        <div class="col-md-8">
            <h2>Dashboard</h2>
        </div>
        <div class="col-md-4 text-right" style="padding-top:15px;">
            <a href="~/Covenants/Create.aspx" runat="server" class="btn btn-primary">
                <span class="glyphicon glyphicon-plus"></span> New Covenant
            </a>
        </div>
    </div>

    <div class="row">

        <!-- LEFT: Active / Unfinished -->
        <div class="col-md-7">
            <div class="panel panel-warning">
                <div class="panel-heading">
                    <strong><span class="glyphicon glyphicon-time"></span> Active Covenants</strong>
                    <span class="badge pull-right">
                        <asp:Label ID="lblActiveCount" runat="server">0</asp:Label>
                    </span>
                </div>
                <div class="panel-body" style="padding:0;">
                    <asp:GridView ID="gvActive" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="table table-condensed"
                        style="margin-bottom:0;"
                        OnRowDataBound="gvActive_RowDataBound"
                        EmptyDataText="No active covenants.">
                        <Columns>
                            <asp:TemplateField HeaderText="Title">
                                <ItemTemplate>
                                    <asp:HyperLink ID="lnkActive" runat="server" CssClass="text-primary"></asp:HyperLink>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="CovenantTypeName" HeaderText="Type" />
                            <asp:BoundField DataField="ProcessingDate"   HeaderText="Processing Date" DataFormatString="{0:dd MMM yyyy}" />
                            <asp:TemplateField HeaderText="Status">
                                <ItemTemplate>
                                    <asp:Label ID="lblActiveStatus" runat="server"></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>

        <!-- RIGHT: Completed -->
        <div class="col-md-5">
            <div class="panel panel-success">
                <div class="panel-heading">
                    <strong><span class="glyphicon glyphicon-ok-circle"></span> Completed Covenants</strong>
                    <span class="badge pull-right">
                        <asp:Label ID="lblCompletedCount" runat="server">0</asp:Label>
                    </span>
                </div>
                <div class="panel-body" style="padding:0;">
                    <asp:GridView ID="gvCompleted" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="table table-condensed"
                        style="margin-bottom:0;"
                        OnRowDataBound="gvCompleted_RowDataBound"
                        EmptyDataText="No completed covenants.">
                        <Columns>
                            <asp:TemplateField HeaderText="Title">
                                <ItemTemplate>
                                    <asp:HyperLink ID="lnkCompleted" runat="server" CssClass="text-success"></asp:HyperLink>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="CovenantTypeName" HeaderText="Type" />
                            <asp:BoundField DataField="ProcessingDate"   HeaderText="Processing Date" DataFormatString="{0:dd MMM yyyy}" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>

    </div><!-- end row -->

    <small class="text-muted">
        Rows highlighted in <span style="background:#fcf8e3; padding:1px 5px;">yellow</span>
        have processing dates within <%: NotificationThreshold %> days.
    </small>

</asp:Content>
