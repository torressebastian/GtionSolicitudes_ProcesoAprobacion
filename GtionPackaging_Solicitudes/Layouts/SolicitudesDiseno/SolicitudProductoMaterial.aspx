<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SolicitudProductoMaterial.aspx.cs" Inherits="SolicitudesDiseno_Solicitudes.Layouts.SolicitudesDiseno.SolicitudProductoMaterial" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">

</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <asp:Panel ID="pnlCabeceraDocumento" runat="server">
        <h2 runat="server" style="background-color:cadetblue;color:white" Font-Bold="true">&nbsp;&nbsp;Datos de la Solicitud</h2>
        <asp:Table ID="tblDatosDocumento" runat="server" Width="100%">
            <asp:TableHeaderRow BackColor="#144733">
                <asp:TableHeaderCell runat="server" ForeColor="White">Solicitud</asp:TableHeaderCell>
                <asp:TableHeaderCell runat="server" ForeColor="White">Circuito</asp:TableHeaderCell>
                </asp:TableHeaderRow>
            <asp:TableRow>
                <asp:TableCell runat="server" Width="60%" BorderStyle="None" ><SharePoint:InputFormTextBox runat="server" BorderStyle="None" Width="100%" Enabled="false" Font-Bold="true" ID="txtNombreSolicitud" /></asp:TableCell>
                <asp:TableCell runat="server" Width="20%" BorderStyle="None"><SharePoint:InputFormTextBox runat="server" BorderStyle="None" Width="100%" Enabled="false" Font-Bold="true" ID="txtTipoDocumento" /></asp:TableCell>
                </asp:TableRow>
             <asp:TableRow>
                <asp:TableCell runat="server" Width="60%" BorderStyle="None" >
                    <asp:Button ID="btnVolver" runat="server" Text="Volver a Solicitud" OnClick="btnVolver_Click" Width="150px"/>
                </asp:TableCell>
            </asp:TableRow>
            </asp:Table>
        </asp:Panel>
    <br />
    <h2 runat="server" style="background-color:cadetblue;color:white" Font-Bold="true">&nbsp;&nbsp;Datos de los Productos</h2>
    <asp:Panel runat="server" ID="pnlProductos" Font-Size="Small">
        <asp:gridview CssClass="ms-listviewtable"  id="CustomersGridView" allowpaging="True" PagerSettings-Visible="false" autogeneratecolumns="false" runat="server" onrowcommand="CustomersGridViewProductos_RowCommand" GridLines="Both" >
        <HeaderStyle ForeColor="DarkGray" Font-Bold="True" BackColor="#f3f3f3" Font-Names="Tahoma" HorizontalAlign="Left" Font-Size="10pt" />
        <RowStyle Font-Size="10pt" Font-Names="Tahoma" BorderStyle="Solid" BorderWidth="1" BorderColor="#339966"  />
        <AlternatingRowStyle CssClass="ms-alternating"/>
            <columns>
                <asp:boundfield datafield="ID" headertext="-" Visible="true" ItemStyle-ForeColor="White"><ItemStyle Width="1px" HorizontalAlign="Left" /></asp:boundfield>
                <asp:buttonfield ButtonType="Image" CommandName="EditarProducto" DataTextField="ID" headertext=""  ImageUrl="../images/edititem.gif" ><ItemStyle Width="20px" HorizontalAlign="Center" /></asp:buttonfield>
                <asp:boundfield datafield="Title" headertext="Producto"  />
                <asp:boundfield datafield="C_x00f3_digo_x0020_SAP" headertext="Código SAP"  />
                <asp:boundfield datafield="Concentraci_x00f3_n" headertext="Concentración"  />
                <asp:boundfield datafield="Presentaci_x00f3_n" headertext="Presentación"  />
                <asp:boundfield datafield="Estimado_x0020_Venta" headertext="Estimado"  />
                <asp:boundfield datafield="Vida_x0020__x00da_til" headertext="Vida Útil (meses)"  />
                <asp:boundfield datafield="Tipo_x0020_de_x0020_material_x00" headertext="Tipo Material Empaque"  />
                <asp:boundfield datafield="Tipo_x0020_producto" headertext="Tipo Producto"  />
                <asp:boundfield datafield="Recursos_x0020_utilizados" headertext="Recursos Utilizados"  />
                <asp:boundfield datafield="Cantidad_x0020_Blisters" headertext="Cantidad de Blisters"  />
                <asp:boundfield datafield="Blister" headertext="Blister"  />
                <asp:boundfield datafield="Criterio_x0020_Unificaci_x00f3_n" headertext="Criterio Unificación"  />
                <asp:boundfield datafield="Unificaci_x00f3_n_x0020_Aluminio" headertext="Unificación Aluminio"  />
                <asp:boundfield datafield="Unificaci_x00f3_n_x0020_Estuche" headertext="Unificación Estuche"  />
                <asp:boundfield datafield="Unificaci_x00f3_n_x0020_Prospect" headertext="Unificación Prospecto"  />
                <asp:boundfield datafield="Datos_x0020_de_x0020_Cobertura" headertext="Datos de Cobertura"  />
            </columns>
        </asp:gridview> 
        <asp:Table ID="tblPaginado" runat="server" Width="100%" CssClass="ms-bottompaging">
            <asp:TableRow ID="TableRow1" runat="server" Height="15px">
            <asp:TableCell ID="TableCell5" runat="server" CssClass="ms-vb" HorizontalAlign="Center" VerticalAlign="Middle">
                <asp:imageButton OnClick="btnAnterior_Click" runat="server" ImageUrl="/_layouts/3082/images/prev.gif" AlternateText="Anterior" ID="imgAnterior"  />
                <asp:Label ID="lblPagina" runat="server" Text="" ></asp:Label>
                <asp:imageButton runat="server" OnClick="btnSiguiente_Click" ImageUrl="/_layouts/3082/images/next.gif" AlternateText="Siguiente" ID="imgSiguiente"/>
            </asp:TableCell>
            </asp:TableRow>
   </asp:Table>
   <asp:TextBox ID="iPaginaProducto" runat="server" Visible="false" ></asp:TextBox>
    </asp:Panel>
    <asp:Panel runat="server" ID="pnlEdicionProducto" Font-Size="Small" >
        <asp:Table ID="tblEdicionProducto" runat="server" Width="92%" CellPadding="2">
            <asp:TableRow>
                <asp:TableCell ColumnSpan="2">
                    <asp:Label ID="lblProducto" runat="server">Producto:</asp:Label><br />
                    <asp:TextBox ID="txtProducto" runat="server" CssClass="ms-long" Width="92%"></asp:TextBox>                
                </asp:TableCell>
                <asp:TableCell>
                    <asp:Label ID="lblProductoCodigoSAP" runat="server">Código SAP:</asp:Label><br />
                    <asp:TextBox ID="txtProductoCodigoSAP" runat="server" CssClass="ms-input"></asp:TextBox>
                </asp:TableCell>
                <asp:TableCell ColumnSpan="2">
                    <asp:Label ID="lblConcentracion" runat="server">Concentración:</asp:Label><br />
                    <asp:TextBox ID="txtConcentracion" runat="server" CssClass="ms-long" Width="92%"></asp:TextBox>                
                </asp:TableCell>
                <asp:TableCell ColumnSpan="2">
                    <asp:Label ID="lblPresentacion" runat="server">Presentación:</asp:Label><br />
                    <asp:TextBox ID="txtPresentacion" runat="server" CssClass="ms-long" Width="92%"></asp:TextBox>                
                </asp:TableCell>
                
                </asp:TableRow>
            <asp:TableRow>
            
                <asp:TableCell >
                    <asp:Label ID="lblEstimadoVenta" runat="server">Estimado (mensual):</asp:Label><br />
                    <asp:TextBox ID="txtEstimadoVenta" runat="server" CssClass="ms-input" ></asp:TextBox>
                </asp:TableCell>
                <asp:TableCell> 
                    <asp:Label ID="lblVidaUtil" runat="server">Vida Útil (meses):</asp:Label><br />
                    <asp:TextBox ID="txtVidaUtil" runat="server" CssClass="ms-input" ></asp:TextBox>
                </asp:TableCell>
                <asp:TableCell> 
                    <asp:Label ID="lblTipoMaterialEmpaque" runat="server">Tipo Material Empaque:</asp:Label><br />
                    <asp:TextBox ID="txtTipoMaterialEmpaque" runat="server" CssClass="ms-input" ></asp:TextBox>
                </asp:TableCell>
                <asp:TableCell> 
                    <asp:Label ID="lblTipoProducto" runat="server">Tipo Producto:</asp:Label><br />
                    <asp:DropDownList ID="ddlTipoProducto" runat="server" CssClass="ms-input" >
                        <asp:ListItem Text="<-- Seleccione -->" Value="" Selected="True" />
                        <asp:ListItem Text="Venta" Value="Venta"  />
                        <asp:ListItem Text="Muestra médica" Value="Muestra médica"  />
                    </asp:DropDownList>
                </asp:TableCell>
                <asp:TableCell> 
                    <asp:Label ID="lblRecursosUtilizados" runat="server">Recursos Utilizados:</asp:Label><br />
                    <asp:TextBox ID="txtRecursosUtilizados" runat="server" CssClass="ms-input" ></asp:TextBox>
                </asp:TableCell>
                <asp:TableCell> 
                    <asp:Label ID="lblCantidadBlister" runat="server">Cantidad de Blisters:</asp:Label><br />
                    <asp:TextBox ID="txtCantidadBlister" runat="server" CssClass="ms-input" ></asp:TextBox>
                </asp:TableCell>
                <asp:TableCell> 
                    <asp:Label ID="lblBlister" runat="server">Blister:</asp:Label><br />
                    <asp:TextBox ID="txtBlister" runat="server" CssClass="ms-input"></asp:TextBox>
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow ID="tbrCriterioUnificacion">
                <asp:TableCell>
                    <asp:Label ID="lblCriterioUnificacion" runat="server">Criterio de Unificación:</asp:Label><br />
                    <asp:TextBox ID="txtCriterioUnificacion" runat="server" CssClass="ms-input" ></asp:TextBox>
                </asp:TableCell>
                <asp:TableCell ColumnSpan="2">
                    <asp:Label ID="lblUnificacionAluminio" runat="server">Unificación Aluminio:</asp:Label><br />
                    <asp:TextBox ID="txtUnificacionAluminio" runat="server" CssClass="ms-long" Width="92%"></asp:TextBox>                
                </asp:TableCell>
                
                <asp:TableCell ColumnSpan="2">
                    <asp:Label ID="lblUnificacionEstuche" runat="server">Unificación Estuche:</asp:Label><br />
                    <asp:TextBox ID="txtUnificacionEstuche" runat="server" CssClass="ms-long" Width="92%"></asp:TextBox>                
                </asp:TableCell>
                <asp:TableCell ColumnSpan="2">
                    <asp:Label ID="lblUnificacionProspecto" runat="server">Unificación Prospecto:</asp:Label><br />
                    <asp:TextBox ID="txtUnificacionProspecto" runat="server" CssClass="ms-long" Width="92%"></asp:TextBox>                
                </asp:TableCell>
                
                </asp:TableRow>
            <asp:TableRow ID="tbrDatoCobertura">
                <asp:TableCell ColumnSpan="2">
                    <asp:Label ID="lblDatosCobertura" runat="server">Datos de Cobertura:</asp:Label><br />
                    <asp:TextBox ID="txtDatosCobertura" runat="server" CssClass="ms-long" Width="90%"></asp:TextBox>                
                </asp:TableCell>
                </asp:TableRow>
            <asp:TableRow>
            <asp:TableCell ColumnSpan="5">
                    <asp:label id="lblMensajeErrorProducto" runat="server" Text=" " Visible="True" style="color:Red;" />  <br />
                    <asp:Button ID="btnAddProducto" runat="server" Text="Agregar" OnClick="AddProducto"  Enabled="True" CssClass="ms-NarrowButtonHeightWidth" Width="150px"/>&nbsp;&nbsp;
                    <asp:Button ID="btnUpdProducto" runat="server" Text="Actualizar" OnClick="UpdProducto" Enabled="False" CssClass="ms-NarrowButtonHeightWidth" Width="150px" />&nbsp;&nbsp;
                    <asp:Button ID="btnDelProducto" runat="server" Text="Eliminar" OnClick="DelProducto" Enabled="False" CssClass="ms-NarrowButtonHeightWidth" Width="150px"/>&nbsp;&nbsp;
                    <asp:Button ID="btnRefProducto" runat="server" Text="Limpiar" OnClick="RefProducto" Enabled="True" CssClass="ms-NarrowButtonHeightWidth" Width="150px"/>
                </asp:TableCell>
            </asp:TableRow>
        </asp:Table>
    </asp:Panel>
    <br />
    <h2 runat="server" id="h2CabeceraMateriales" style="background-color:cadetblue;color:white" Font-Bold="true">&nbsp;&nbsp;Datos de los Materiales</h2>
    <asp:Panel runat="server" ID="pnlMateriales" Font-Size="Small">
        <asp:gridview CssClass="ms-listviewtable"  id="GridViewMateriales" allowpaging="True" PagerSettings-Visible="false" autogeneratecolumns="false" runat="server" Width="100%" onrowcommand="CustomersGridViewMaterials_RowCommand" GridLines="Both" >
        <HeaderStyle ForeColor="DarkGray" Font-Bold="True" BackColor="#f3f3f3" Font-Names="Tahoma" HorizontalAlign="Left" Font-Size="10pt" Wrap="true" />
        <RowStyle Font-Size="10pt" Font-Names="Tahoma" BorderStyle="Solid" BorderWidth="1" BorderColor="#339966"  />
        <AlternatingRowStyle CssClass="ms-alternating"/>
            <columns>
            <asp:boundfield datafield="ID" headertext="-" Visible="true" ItemStyle-ForeColor="White"><ItemStyle Width="1px" HorizontalAlign="Left" /></asp:boundfield>
            <asp:buttonfield ButtonType="Image" CommandName="EditarMaterial" DataTextField="ID" headertext=""  ImageUrl="../images/edititem.gif" ><ItemStyle Width="20px" HorizontalAlign="Center" /></asp:buttonfield>
                <%--<asp:boundfield datafield="Producto_x003a_Nombre" headertext="Producto"  />--%>  
                <asp:TemplateField HeaderText="Producto">
                    <ItemTemplate>
                        <%# RemoveCharacters(Eval("Producto_x003a_Nombre").ToString())%>
                    </ItemTemplate>
                </asp:TemplateField>
	            <asp:boundfield datafield="Title" headertext="Material"  />
                <asp:boundfield datafield="Tipo_x0020_Material" headertext="Tipo Material"  />
                <asp:boundfield datafield="C_x00f3_digo_x0020_SAP" headertext="Código SAP"  />
                <asp:boundfield datafield="C_x00f3_digo_x0020_de_x0020_Dise" headertext="Código Diseño"  />
                <asp:boundfield datafield="Cortante" headertext="Cortante"  />
                <asp:boundfield datafield="Medida" headertext="Medida"  />
                <asp:boundfield datafield="Carga_x0020_de_x0020_Laca" headertext="Carga de Laca"  />
                <asp:boundfield datafield="Plano" headertext="Plano"  />
                <asp:boundfield datafield="Pharmacode" headertext="Pharmacode"  />
                <asp:boundfield datafield="Nro_x0020_Troquel" headertext="Nro Troquel"  />
                <asp:boundfield datafield="C_x00f3_digo_x0020_de_x0020_Espe" headertext="Código Especificación"  />
                <asp:boundfield datafield="C_x00f3_digo_x0020_de_x0020_Meto" headertext="Código Metodología Analítica"  />
                <asp:boundfield datafield="Datos_x0020_de_x0020_Cobertura" headertext="Datos de Cobertura"  />

            </columns>
        </asp:gridview> 
        <asp:Table ID="Table1" runat="server" Width="100%" CssClass="ms-bottompaging">
            <asp:TableRow ID="TableRow2" runat="server" Height="15px">
            <asp:TableCell ID="TableCell1" runat="server" CssClass="ms-vb" HorizontalAlign="Center" VerticalAlign="Middle">
                <asp:imageButton OnClick="btnAnteriorMaterial_Click" runat="server" ImageUrl="/_layouts/3082/images/prev.gif" AlternateText="Anterior" ID="imgAnteriorMaterial"  />
                <asp:Label ID="lblPaginaMaterial" runat="server" Text="" ></asp:Label>
                <asp:imageButton runat="server" OnClick="btnSiguienteMaterial_Click" ImageUrl="/_layouts/3082/images/next.gif" AlternateText="Siguiente" ID="imgSiguienteMaterial"/>
            </asp:TableCell>
            </asp:TableRow>
   </asp:Table>
   <asp:TextBox ID="iPaginaMaterial" runat="server" Visible="false" ></asp:TextBox>
    </asp:Panel>
    <asp:Panel runat="server" ID="pnlEdicionMaterial" Font-Size="Small" >
        <asp:Table ID="tblEdicionMaterial" runat="server" Width="92%" CellPadding="2">
            <asp:TableRow>
                <asp:TableCell ColumnSpan="2" RowSpan="3" VerticalAlign="Top">
                    <asp:Label ID="lblProductoMaterial" runat="server" >Producto:</asp:Label><br />
                    <asp:ListBox ID="lbxProductoMaterial" runat="server" SelectionMode="Multiple" CssClass="ms-long"></asp:ListBox>
                </asp:TableCell>
                <asp:TableCell ColumnSpan="2">
                    <asp:Label ID="lblMaterial" runat="server" >Material:</asp:Label><br />
                    <asp:TextBox ID="txtMaterial" runat="server" CssClass="ms-long" Width="90%"></asp:TextBox>                
                </asp:TableCell>
    		    <asp:TableCell>
                    <asp:Label ID="lblTipoMaterial" runat="server" >Tipo Material:</asp:Label><br />
                    <asp:DropDownList ID="ddlTipoMaterial" runat="server" ></asp:DropDownList>
                </asp:TableCell>
                <asp:TableCell>
                    <asp:Label ID="lblMaterialCodigoSAP" runat="server" >Código SAP:</asp:Label><br />
                    <asp:TextBox ID="txtMaterialCodigoSAP" runat="server" CssClass="ms-input"></asp:TextBox>
                </asp:TableCell>
                <asp:TableCell>
                    <asp:Label ID="lblCodigoDiseno" runat="server">Código Diseño:</asp:Label><br />
                    <asp:TextBox ID="txtCodigoDiseno" runat="server" CssClass="ms-input"></asp:TextBox>
                </asp:TableCell>
                <asp:TableCell >
                    <asp:Label ID="lblCortante" runat="server">Cortante:</asp:Label><br />
                    <asp:TextBox ID="txtCortante" runat="server" CssClass="ms-input" ></asp:TextBox>
                </asp:TableCell>
                </asp:TableRow>
            <asp:TableRow>
                <asp:TableCell> 
                    <asp:Label ID="lblMedida" runat="server">Medida:</asp:Label><br />
                    <asp:TextBox ID="txtMedida" runat="server" CssClass="ms-input" ></asp:TextBox>
                </asp:TableCell>
            	<asp:TableCell>
                    <asp:Label ID="lblMDI" runat="server">MDI:</asp:Label><br />
                    <asp:TextBox ID="txtMDI" runat="server" CssClass="ms-input"></asp:TextBox>
                </asp:TableCell>
                <asp:TableCell>
                    <asp:Label ID="lblCargaLaca" runat="server">Carga de Laca:</asp:Label><br />
                    <asp:TextBox ID="txtCargaLaca" runat="server" CssClass="ms-input"></asp:TextBox>
                </asp:TableCell>
                <asp:TableCell>
                    <asp:Label ID="lblPlano" runat="server">Plano:</asp:Label><br />
                    <asp:TextBox ID="txtPlano" runat="server" CssClass="ms-input"></asp:TextBox>
                </asp:TableCell>
                <asp:TableCell>
                    <asp:Label ID="lblPharmacode" runat="server">Pharmacode:</asp:Label><br />
                    <asp:TextBox ID="txtPharmacode" runat="server" CssClass="ms-input"></asp:TextBox>
                </asp:TableCell>
                <asp:TableCell>
                    <asp:Label ID="lblTroquel" runat="server">Número Troquel:</asp:Label><br />
                    <asp:TextBox ID="txtTroquel" runat="server" CssClass="ms-input"></asp:TextBox>
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow>
                <asp:TableCell>
                    <asp:Label ID="lblCodEspecificacion" runat="server">Código de Especificación:</asp:Label><br />
                    <asp:TextBox ID="txtCodEspecificacion" runat="server" CssClass="ms-input"></asp:TextBox>
                </asp:TableCell>
                <asp:TableCell>
                    <asp:Label ID="lblCodMetodologia" runat="server">Cód. Metodología Analítica:</asp:Label><br />
                    <asp:TextBox ID="txtCodMetodologia" runat="server" CssClass="ms-input"></asp:TextBox>
                </asp:TableCell>
                <asp:TableCell ColumnSpan="2">
                    <asp:Label ID="lblCoberturaMaterial" runat="server">Datos de Cobertura:</asp:Label><br />
                    <asp:TextBox ID="txtCoberturaMaterial" runat="server" CssClass="ms-input"></asp:TextBox>
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow>
            <asp:TableCell ColumnSpan="7">
                    <asp:label id="lblMensajeErrorMaterial" runat="server" Text=" " Visible="True" style="color:Red;" />  <br />
                    <asp:Button ID="btnAddMaterial" runat="server" Text="Agregar" OnClick="AddMaterial"  Enabled="True" CssClass="ms-NarrowButtonHeightWidth" Width="150px"/>&nbsp;&nbsp;
                    <asp:Button ID="btnUpdMaterial" runat="server" Text="Actualizar" OnClick="UpdMaterial" Enabled="False" CssClass="ms-NarrowButtonHeightWidth" Width="150px"/>&nbsp;&nbsp;
                    <asp:Button ID="btnDelMaterial" runat="server" Text="Eliminar" OnClick="DelMaterial" Enabled="False" CssClass="ms-NarrowButtonHeightWidth" Width="150px"/>&nbsp;&nbsp;
                    <asp:Button ID="btnRefMaterial" runat="server" Text="Limpiar" OnClick="RefMaterial" Enabled="True" CssClass="ms-NarrowButtonHeightWidth" Width="150px"/>
                </asp:TableCell>
            </asp:TableRow>
        </asp:Table>
    </asp:Panel>    
    <asp:HiddenField ID="iProducto" runat="server" EnableViewState="true" />
    <asp:HiddenField ID="iMaterial" runat="server" EnableViewState="true" />
    <asp:TextBox ID="Errores" runat="server" TextMode="MultiLine" Rows="3" Visible="false" />

</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Solicitud - Producto / Materiales
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
Solicitud - Producto / Materiales
</asp:Content>
