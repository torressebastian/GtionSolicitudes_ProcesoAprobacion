<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CancelarProceso.aspx.cs" Inherits="GtionPackaging_Solicitudes.Layouts.SolicitudesDiseno.CancelarProceso" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">

</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <asp:Table ID="tblDatosAsignarUsuario" runat="server" Width="100%">
            <asp:TableHeaderRow BackColor="#009933">
                <asp:TableHeaderCell runat="server" HorizontalAlign="Left" ForeColor="White">¿Está seguro que desea cancelar el proceso de esta solicitud?</asp:TableHeaderCell>
            </asp:TableHeaderRow>
        <asp:TableRow>
            <asp:TableCell>
                <asp:TextBox ID="txtMotivoSuspension" runat="server" TextMode="MultiLine" Width="600px"></asp:TextBox>
            </asp:TableCell>
        </asp:TableRow>
        </asp:Table>
    <br />
    <asp:Label runat="server" ID="lblMensajeError" ForeColor="Red" Visible="false"></asp:Label><br />
    <asp:Button ID="btnAceptar" runat="server" Text="Aceptar" OnClick="btnAceptar_Click" />
    <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" OnClick="btnCancelar_Click" />
</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Cancelar Proceso
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
Cancelar Proceso
</asp:Content>
