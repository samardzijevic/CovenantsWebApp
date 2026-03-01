<%@ Page Title="Edit Covenant" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Edit.aspx.cs" Inherits="Covenants_Edit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Edit Covenant</h2>

    <asp:Label ID="lblError" runat="server" CssClass="alert alert-danger" Visible="false" style="display:block; margin-bottom:15px;"></asp:Label>

    <asp:HiddenField ID="hfId" runat="server" />

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
                        <asp:TextBox ID="txtCurrency" runat="server" CssClass="form-control" MaxLength="10" />
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

    <div>
        <asp:Button ID="btnSave" runat="server" Text="Save Changes" CssClass="btn btn-primary" OnClick="btnSave_Click" />
        <asp:HyperLink ID="lnkDetails" runat="server" CssClass="btn btn-default" style="margin-left:8px;">Back to Details</asp:HyperLink>
        <a href="List.aspx" class="btn btn-link" style="margin-left:8px;">Back to List</a>
    </div>

</asp:Content>
