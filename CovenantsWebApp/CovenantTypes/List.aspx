<%@ Page Title="Covenant Types" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="List.aspx.cs" Inherits="CovenantTypes_List" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Covenant Types</h2>

    <asp:Label ID="lblMessage" runat="server" CssClass="alert alert-success" Visible="false" style="display:block; margin-bottom:10px;"></asp:Label>
    <asp:Label ID="lblError"   runat="server" CssClass="alert alert-danger"  Visible="false" style="display:block; margin-bottom:10px;"></asp:Label>

    <!-- Add new type -->
    <div class="panel panel-default" style="margin-bottom:20px;">
        <div class="panel-heading"><strong>Add New Type</strong></div>
        <div class="panel-body">
            <div class="row">
                <div class="col-md-4">
                    <div class="form-group">
                        <label>Name <span class="text-danger">*</span></label>
                        <asp:TextBox ID="txtName" runat="server" CssClass="form-control" MaxLength="100" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtName" ErrorMessage="Name is required." CssClass="text-danger" Display="Dynamic" />
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="form-group">
                        <label>Description</label>
                        <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" MaxLength="500" />
                    </div>
                </div>
                <div class="col-md-2" style="padding-top:25px;">
                    <asp:Button ID="btnAdd" runat="server" Text="Add" CssClass="btn btn-primary" OnClick="btnAdd_Click" />
                </div>
            </div>
        </div>
    </div>

    <asp:GridView ID="gvTypes" runat="server"
        AutoGenerateColumns="false"
        CssClass="table table-bordered table-hover"
        DataKeyNames="Id"
        OnRowEditing="gvTypes_RowEditing"
        OnRowUpdating="gvTypes_RowUpdating"
        OnRowCancelingEdit="gvTypes_RowCancelingEdit"
        OnRowCommand="gvTypes_RowCommand"
        EmptyDataText="No covenant types found.">
        <Columns>
            <asp:TemplateField HeaderText="Name">
                <ItemTemplate><%# Eval("Name") %></ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtEditName" runat="server" Text='<%# Eval("Name") %>' CssClass="form-control" />
                </EditItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Description">
                <ItemTemplate><%# Eval("Description") %></ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtEditDesc" runat="server" Text='<%# Eval("Description") %>' CssClass="form-control" />
                </EditItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Active">
                <ItemTemplate>
                    <asp:Label ID="lblActive" runat="server"></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Actions" ItemStyle-Width="160px">
                <ItemTemplate>
                    <asp:LinkButton ID="btnEdit"       runat="server" CssClass="btn btn-xs btn-warning" CommandName="Edit">Edit</asp:LinkButton>
                    <asp:LinkButton ID="btnDeactivate" runat="server" CssClass="btn btn-xs btn-danger"  CommandName="Deactivate" CommandArgument='<%# Eval("Id") %>'
                        OnClientClick="return confirm('Deactivate this type?');">Deactivate</asp:LinkButton>
                    <asp:LinkButton ID="btnActivate"   runat="server" CssClass="btn btn-xs btn-success" CommandName="Activate"   CommandArgument='<%# Eval("Id") %>'>Activate</asp:LinkButton>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:LinkButton runat="server" CssClass="btn btn-xs btn-primary" CommandName="Update">Save</asp:LinkButton>
                    <asp:LinkButton runat="server" CssClass="btn btn-xs btn-default" CommandName="Cancel">Cancel</asp:LinkButton>
                </EditItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

</asp:Content>
