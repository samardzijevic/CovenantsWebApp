<%@ Page Title="Covenants" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="List.aspx.cs" Inherits="Covenants_List" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Covenants</h2>

    <asp:Label ID="lblMessage" runat="server" CssClass="alert alert-success" Visible="false" style="display:block; margin-bottom:10px;"></asp:Label>
    <asp:Label ID="lblError"   runat="server" CssClass="alert alert-danger"  Visible="false" style="display:block; margin-bottom:10px;"></asp:Label>

    <!-- Toolbar -->
    <div class="row" style="margin-bottom:12px;">
        <div class="col-md-12">
            <button type="button" class="btn btn-primary" data-toggle="modal" data-target="#modalAdd">
                <span class="glyphicon glyphicon-plus"></span> New Covenant
            </button>
        </div>
    </div>

    <!-- Filters -->
    <div class="panel panel-default" style="margin-bottom:12px;">
        <div class="panel-body">
            <div class="row">
                <div class="col-md-3">
                    <label>Status</label>
                    <asp:DropDownList ID="ddlFilterStatus" runat="server" CssClass="form-control">
                        <asp:ListItem Value="">-- All --</asp:ListItem>
                        <asp:ListItem Value="Active">Active</asp:ListItem>
                        <asp:ListItem Value="Pending">Pending</asp:ListItem>
                        <asp:ListItem Value="Completed">Completed</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-md-3">
                    <label>Type</label>
                    <asp:DropDownList ID="ddlFilterType" runat="server" CssClass="form-control"></asp:DropDownList>
                </div>
                <div class="col-md-2" style="padding-top:24px;">
                    <asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="btn btn-default" OnClick="btnFilter_Click" />
                    <asp:Button ID="btnClear"  runat="server" Text="Clear"  CssClass="btn btn-link"    OnClick="btnClear_Click" />
                </div>
                <div class="col-md-2" style="padding-top:28px;">
                    <label>
                        <asp:CheckBox ID="chkShowDeleted" runat="server" AutoPostBack="true" OnCheckedChanged="btnFilter_Click" />
                        Show deleted
                    </label>
                </div>
            </div>
        </div>
    </div>

    <!-- Grid -->
    <asp:GridView ID="gvCovenants" runat="server"
        AutoGenerateColumns="false"
        CssClass="table table-bordered table-hover"
        DataKeyNames="Id"
        OnRowCommand="gvCovenants_RowCommand"
        OnRowDataBound="gvCovenants_RowDataBound"
        EmptyDataText="No covenants found.">
        <Columns>
            <asp:BoundField DataField="Title"            HeaderText="Title" />
            <asp:BoundField DataField="CovenantTypeName" HeaderText="Type" />
            <asp:BoundField DataField="ProcessingDate"   HeaderText="Processing Date" DataFormatString="{0:dd MMM yyyy}" />
            <asp:BoundField DataField="Value"            HeaderText="Value"    DataFormatString="{0:N2}" NullDisplayText="-" />
            <asp:BoundField DataField="Currency"         HeaderText="Currency" NullDisplayText="-" />
            <asp:TemplateField HeaderText="Status">
                <ItemTemplate>
                    <asp:Label ID="lblStatus" runat="server"></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Actions" ItemStyle-Width="195px">
                <ItemTemplate>
                    <asp:HyperLink  ID="lnkDetails" runat="server" CssClass="btn btn-xs btn-info">Details</asp:HyperLink>
                    <asp:LinkButton ID="btnEdit"    runat="server" CssClass="btn btn-xs btn-warning" CommandName="LoadEdit"  CommandArgument='<%# Eval("Id") %>'>Edit</asp:LinkButton>
                    <asp:LinkButton ID="btnDelete"  runat="server" CssClass="btn btn-xs btn-danger"  CommandName="AskDelete" CommandArgument='<%# Eval("Id") %>'>Delete</asp:LinkButton>
                    <asp:LinkButton ID="btnRestore" runat="server" CssClass="btn btn-xs btn-success" CommandName="Restore"   CommandArgument='<%# Eval("Id") %>' Visible="false">Restore</asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

    <!-- Hidden state for modals -->
    <asp:HiddenField ID="hfEditId"   runat="server" />
    <asp:HiddenField ID="hfDeleteId" runat="server" />

    <!-- ============================================================ -->
    <!-- ADD MODAL                                                     -->
    <!-- ============================================================ -->
    <div class="modal fade" id="modalAdd" tabindex="-1" role="dialog">
        <div class="modal-dialog modal-lg">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                    <h4 class="modal-title">New Covenant</h4>
                </div>
                <div class="modal-body">
                    <asp:Label ID="lblAddError" runat="server" CssClass="alert alert-danger" Visible="false" style="display:block; margin-bottom:10px;"></asp:Label>
                    <div class="row">
                        <div class="col-md-8">
                            <div class="form-group">
                                <label>Title <span class="text-danger">*</span></label>
                                <asp:TextBox ID="txtAddTitle" runat="server" CssClass="form-control" MaxLength="200" />
                            </div>
                        </div>
                        <div class="col-md-4">
                            <div class="form-group">
                                <label>Type <span class="text-danger">*</span></label>
                                <asp:DropDownList ID="ddlAddType" runat="server" CssClass="form-control" />
                            </div>
                        </div>
                    </div>
                    <div class="form-group">
                        <label>Description</label>
                        <asp:TextBox ID="txtAddDescription" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" />
                    </div>
                    <div class="row">
                        <div class="col-md-4">
                            <div class="form-group">
                                <label>Processing Date <span class="text-danger">*</span></label>
                                <asp:TextBox ID="txtAddProcessingDate" runat="server" CssClass="form-control" TextMode="Date" />
                            </div>
                        </div>
                        <div class="col-md-3">
                            <div class="form-group">
                                <label>Value</label>
                                <asp:TextBox ID="txtAddValue" runat="server" CssClass="form-control" />
                            </div>
                        </div>
                        <div class="col-md-2">
                            <div class="form-group">
                                <label>Currency</label>
                                <asp:TextBox ID="txtAddCurrency" runat="server" CssClass="form-control" MaxLength="10" placeholder="EUR" />
                            </div>
                        </div>
                        <div class="col-md-3">
                            <div class="form-group">
                                <label>Status</label>
                                <asp:DropDownList ID="ddlAddStatus" runat="server" CssClass="form-control">
                                    <asp:ListItem Value="Active">Active</asp:ListItem>
                                    <asp:ListItem Value="Pending">Pending</asp:ListItem>
                                    <asp:ListItem Value="Completed">Completed</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <asp:Button ID="btnAddSave" runat="server" Text="Create" CssClass="btn btn-primary" OnClick="btnAddSave_Click" />
                    <button type="button" class="btn btn-default" data-dismiss="modal">Cancel</button>
                </div>
            </div>
        </div>
    </div>

    <!-- ============================================================ -->
    <!-- EDIT MODAL                                                    -->
    <!-- ============================================================ -->
    <div class="modal fade" id="modalEdit" tabindex="-1" role="dialog">
        <div class="modal-dialog modal-lg">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                    <h4 class="modal-title">Edit Covenant</h4>
                </div>
                <div class="modal-body">
                    <asp:Label ID="lblEditError" runat="server" CssClass="alert alert-danger" Visible="false" style="display:block; margin-bottom:10px;"></asp:Label>
                    <div class="row">
                        <div class="col-md-8">
                            <div class="form-group">
                                <label>Title <span class="text-danger">*</span></label>
                                <asp:TextBox ID="txtEditTitle" runat="server" CssClass="form-control" MaxLength="200" />
                            </div>
                        </div>
                        <div class="col-md-4">
                            <div class="form-group">
                                <label>Type <span class="text-danger">*</span></label>
                                <asp:DropDownList ID="ddlEditType" runat="server" CssClass="form-control" />
                            </div>
                        </div>
                    </div>
                    <div class="form-group">
                        <label>Description</label>
                        <asp:TextBox ID="txtEditDescription" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" />
                    </div>
                    <div class="row">
                        <div class="col-md-4">
                            <div class="form-group">
                                <label>Processing Date <span class="text-danger">*</span></label>
                                <asp:TextBox ID="txtEditProcessingDate" runat="server" CssClass="form-control" TextMode="Date" />
                            </div>
                        </div>
                        <div class="col-md-3">
                            <div class="form-group">
                                <label>Value</label>
                                <asp:TextBox ID="txtEditValue" runat="server" CssClass="form-control" />
                            </div>
                        </div>
                        <div class="col-md-2">
                            <div class="form-group">
                                <label>Currency</label>
                                <asp:TextBox ID="txtEditCurrency" runat="server" CssClass="form-control" MaxLength="10" />
                            </div>
                        </div>
                        <div class="col-md-3">
                            <div class="form-group">
                                <label>Status</label>
                                <asp:DropDownList ID="ddlEditStatus" runat="server" CssClass="form-control">
                                    <asp:ListItem Value="Active">Active</asp:ListItem>
                                    <asp:ListItem Value="Pending">Pending</asp:ListItem>
                                    <asp:ListItem Value="Completed">Completed</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <asp:Button ID="btnEditSave" runat="server" Text="Save Changes" CssClass="btn btn-primary" OnClick="btnEditSave_Click" />
                    <button type="button" class="btn btn-default" data-dismiss="modal">Cancel</button>
                </div>
            </div>
        </div>
    </div>

    <!-- ============================================================ -->
    <!-- DELETE MODAL                                                  -->
    <!-- ============================================================ -->
    <div class="modal fade" id="modalDelete" tabindex="-1" role="dialog">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                    <h4 class="modal-title">Delete Covenant</h4>
                </div>
                <div class="modal-body">
                    <p>Are you sure you want to delete <strong><asp:Label ID="lblDeleteName" runat="server"></asp:Label></strong>?</p>
                    <p class="text-muted">The covenant will be soft-deleted and can be restored at any time.</p>
                </div>
                <div class="modal-footer">
                    <asp:Button ID="btnDeleteConfirm" runat="server" Text="Delete" CssClass="btn btn-danger" OnClick="btnDeleteConfirm_Click" />
                    <button type="button" class="btn btn-default" data-dismiss="modal">Cancel</button>
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        function showModal(id) { $(id).modal('show'); }
    </script>

</asp:Content>
