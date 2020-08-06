<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WP_BotoneraSolicitud.ascx.cs" Inherits="SolicitudesDiseno_Solicitudes.WP_BotoneraSolicitud.WP_BotoneraSolicitud" %>
<asp:Table ID="tblBar" runat="server">
    <asp:TableRow>
        <asp:TableCell ID="cellLanzInternacional" HorizontalAlign="Center" Width="100px">
            <asp:Button ID="btnLanzInternacional" runat="server" Text="Lanzamiento Internacional" OnClick="btnLanzInternacional_Click" />
        </asp:TableCell>
        <asp:TableCell ID="cellLanzNacional" HorizontalAlign="Center" Width="100px">
            <asp:Button ID="btnLanzNacional" runat="server" Text="Lanzamiento Nacional" OnClick="btnLanzNacional_Click" />
        </asp:TableCell>
        <asp:TableCell ID="cellMarketing" HorizontalAlign="Center" Width="100px">
            <asp:Button ID="btnMarketing" runat="server" Text="Modificación de Archivos (Marketing)" OnClick="btnMarketing_Click" />
        </asp:TableCell>
        <asp:TableCell ID="cellDesarrollo" HorizontalAlign="Center" Width="100px">
            <asp:Button ID="btnDesarrollo" runat="server" Text="Modificación de Archivos (Desarrollo)" OnClick="btnDesarrollo_Click" />
        </asp:TableCell>
        <asp:TableCell ID="cellPlanificacion" HorizontalAlign="Center" Width="100px">
            <asp:Button ID="btnPlanificacion" runat="server" Text="Modificación de Archivos (Planificación)" OnClick="btnPlanificacion_Click" />
        </asp:TableCell>
        <asp:TableCell ID="cellRegistro" HorizontalAlign="Center" Width="100px">
            <asp:Button ID="btnRegistro" runat="server" Text="Modificación de Archivos (Registro)" OnClick="btnRegistro_Click" />
        </asp:TableCell>
        <asp:TableCell ID="cellInternacional" HorizontalAlign="Center" Width="100px">
            <asp:Button ID="btnInternacional" runat="server" Text="Modificación de Archivos (Internacional)" OnClick="btnModInternacional_Click" />
        </asp:TableCell>
    </asp:TableRow>
</asp:Table>