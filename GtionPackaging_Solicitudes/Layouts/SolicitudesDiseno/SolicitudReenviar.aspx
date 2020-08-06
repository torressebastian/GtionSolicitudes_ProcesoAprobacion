<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SolicitudReenviar.aspx.cs" Inherits="GtionPackaging_Solicitudes.Layouts.SolicitudesDiseno.SolictudReenviar" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">

</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <asp:Panel ID="pnlCabeceraDocumento" runat="server">
        <h2 runat="server" style="background-color:cadetblue;color:white" Font-Bold="true">&nbsp;&nbsp;Datos de la Solicitud</h2>
        <asp:Table ID="tblDatosDocumento" runat="server" Width="100%">
            <asp:TableHeaderRow BackColor="#144733">
                <asp:TableHeaderCell runat="server" ForeColor="White">Solicitud</asp:TableHeaderCell>
                <asp:TableHeaderCell runat="server" ForeColor="White">Circuito</asp:TableHeaderCell>
                <asp:TableHeaderCell runat="server" ForeColor="White">Carga Masiva</asp:TableHeaderCell>
                </asp:TableHeaderRow>
            <asp:TableRow>
                <asp:TableCell runat="server" Width="50%" BorderStyle="None" ><SharePoint:InputFormTextBox runat="server" BorderStyle="None" Width="100%" Enabled="false" Font-Bold="true" ID="txtNombreSolicitud" /></asp:TableCell>
                <asp:TableCell runat="server" Width="30%" BorderStyle="None"><SharePoint:InputFormTextBox runat="server" BorderStyle="None" Width="100%" Enabled="false" Font-Bold="true" ID="txtTipoDocumento" /></asp:TableCell>
                <asp:TableCell runat="server" Width="20%" BorderStyle="None"><SharePoint:InputFormTextBox runat="server" BorderStyle="None" Width="100%" Enabled="false" Font-Bold="true" ID="txtCargaMasiva" /></asp:TableCell>
                </asp:TableRow>
             <asp:TableRow>
                <asp:TableCell runat="server" Width="60%" BorderStyle="None" >
                    
                </asp:TableCell>
            </asp:TableRow>
            </asp:Table>
        </asp:Panel>
    <br />
    <h2 runat="server" style="background-color:cadetblue;color:white" Font-Bold="true">&nbsp;&nbsp;Acción a Realizar</h2>
    <asp:RadioButtonList ID="rblAccion" runat="server" OnSelectedIndexChanged="rblAccion_SelectedIndexChanged" AutoPostBack="true" >
        <asp:ListItem Selected="False" Value="0" Text="Reiniciar Solicitud"></asp:ListItem>
        <asp:ListItem Selected="False" Value="1" Text="Solicitar Datos Adicionales"></asp:ListItem>
        <asp:ListItem Selected="False" Value="2" Text="Solicitar Ajustes de Materiales Nuevo o Reemplazantes"></asp:ListItem>
        
    </asp:RadioButtonList>

    <asp:CheckBoxList ID="cblSeleccionMaterial" Visible="false" runat="server" ></asp:CheckBoxList><br />

    <asp:Label ID="lblSeleccioneTarea" Visible="false"  runat="server" Text="Seleccione la tarea destino: "></asp:Label><asp:DropDownList ID="ddlSeleccioneTarea" Visible="false"  runat="server" AutoPostBack="False"></asp:DropDownList>
    <asp:RadioButtonList ID="rblCircuito" runat="server" Visible="false"  >
        <asp:ListItem Selected="true" Value="SI" Text="Circuito Completo"></asp:ListItem>
        <asp:ListItem Selected="False" Value="NO" Text="Circuito Corto"></asp:ListItem>
        
    </asp:RadioButtonList>
    <br /><br />
    <h2 runat="server" style="background-color:cadetblue;color:white" Font-Bold="true">&nbsp;&nbsp;Mensaje</h2>
    <SharePoint:InputFormTextBox runat="server" Width="99%" Enabled="true" ID="txtMensaje" TextMode="MultiLine" RichText="false" Rows="3" ></SharePoint:InputFormTextBox>
    <br /><br /><asp:Label ID="lblErrores" runat="server" Text="" ForeColor="Red" ></asp:Label><br />
    <asp:Button ID="btnGuardar" runat="server" Text="Aceptar" OnClick="btnGuardar_Click" Width="150px"/>
    <asp:Button ID="btnVolver" runat="server" Text="Volver a Solicitud" OnClick="btnVolver_Click" Width="150px"/>
    

    <asp:TextBox ID="Errores" runat="server" TextMode="MultiLine" Rows="3" Visible="false" />

</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Requerir Ajustes a la Solicitud
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
Requerir Ajustes a la Solicitud
</asp:Content>
