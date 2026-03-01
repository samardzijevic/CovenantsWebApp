<%@ Page Title="Notifications" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="List.aspx.cs" Inherits="Notifications_List" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="row" style="margin-bottom:10px;">
        <div class="col-md-8"><h2>Notifications</h2></div>
        <div class="col-md-4 text-right" style="padding-top:15px;">
            <asp:Button ID="btnMarkAllRead" runat="server" Text="Mark All Read" CssClass="btn btn-default btn-sm" OnClick="btnMarkAllRead_Click" />
        </div>
    </div>

    <asp:Label ID="lblMessage" runat="server" CssClass="alert alert-success" Visible="false" style="display:block; margin-bottom:10px;"></asp:Label>

    <asp:GridView ID="gvNotifications" runat="server"
        AutoGenerateColumns="false"
        CssClass="table table-bordered table-hover"
        DataKeyNames="Id"
        OnRowCommand="gvNotifications_RowCommand"
        OnRowDataBound="gvNotifications_RowDataBound"
        EmptyDataText="No notifications.">
        <Columns>
            <asp:TemplateField HeaderText="" ItemStyle-Width="10px">
                <ItemTemplate>
                    <asp:Label ID="lblUnread" runat="server" CssClass="glyphicon glyphicon-asterisk text-primary" Visible="false" title="Unread"></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="Message"       HeaderText="Message" />
            <asp:TemplateField HeaderText="Covenant">
                <ItemTemplate>
                    <asp:HyperLink ID="lnkCovenant" runat="server"></asp:HyperLink>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="Type"          HeaderText="Type"    ItemStyle-Width="200px" />
            <asp:BoundField DataField="CreatedAt"     HeaderText="Date"    DataFormatString="{0:dd MMM yyyy HH:mm}" ItemStyle-Width="140px" />
            <asp:TemplateField HeaderText="Actions"   ItemStyle-Width="140px">
                <ItemTemplate>
                    <asp:LinkButton ID="btnMarkRead" runat="server" CssClass="btn btn-xs btn-info"    CommandName="MarkRead"  CommandArgument='<%# Eval("Id") %>'>Mark Read</asp:LinkButton>
                    <asp:LinkButton ID="btnDismiss"  runat="server" CssClass="btn btn-xs btn-default" CommandName="Dismiss"   CommandArgument='<%# Eval("Id") %>'
                        OnClientClick="return confirm('Dismiss this notification?');">Dismiss</asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

</asp:Content>
