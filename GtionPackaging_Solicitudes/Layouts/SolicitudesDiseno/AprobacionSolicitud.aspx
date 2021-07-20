<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AprobacionSolicitud.aspx.cs" Inherits="SolicitudesDiseno_Solicitudes.Layouts.SolicitudesDiseno.AprobacionSolicitud" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <asp:Panel ID="pnlCabeceraDocumento" runat="server">
        <h2 runat="server" style="background-color:cadetblue;color:white" Font-Bold="true">&nbsp;&nbsp;Datos de la Solicitud</h2>
        <asp:Table ID="tblDatosDocumento" runat="server" Width="100%">
            <asp:TableHeaderRow BackColor="#144733">
                <asp:TableHeaderCell runat="server" ForeColor="White">Solicitud</asp:TableHeaderCell>
                <asp:TableHeaderCell runat="server" ForeColor="White">Circuito</asp:TableHeaderCell>
                <asp:TableHeaderCell runat="server" ForeColor="White">Estado</asp:TableHeaderCell>
            </asp:TableHeaderRow>
            <asp:TableRow>
                <asp:TableCell runat="server" Width="50%" BorderStyle="None" ><SharePoint:InputFormTextBox runat="server" BorderStyle="None" Width="100%" Enabled="false" Font-Bold="true" ID="txtNombreSolicitud" EnableViewState="true"  /></asp:TableCell>
                <asp:TableCell runat="server" Width="25%" BorderStyle="None"><SharePoint:InputFormTextBox runat="server" BorderStyle="None" Width="100%" Enabled="false" Font-Bold="true" ID="txtTipoDocumento" /></asp:TableCell>
                <asp:TableCell runat="server" Width="25%" BorderStyle="None"><SharePoint:InputFormTextBox runat="server" BorderStyle="None" Width="100%" Enabled="false" Font-Bold="true" ID="txtEstado"/></asp:TableCell>
            </asp:TableRow>
            <asp:TableHeaderRow BackColor="#144733">
                <asp:TableHeaderCell runat="server" ForeColor="White" ColumnSpan="2">Detalle Solicitud</asp:TableHeaderCell>
                
                <asp:TableHeaderCell runat="server" ForeColor="White">País</asp:TableHeaderCell>
            </asp:TableHeaderRow>
            
            <asp:TableRow>
                <asp:TableCell runat="server" Width="50%" ColumnSpan="2" BorderStyle="None"><SharePoint:InputFormTextBox runat="server" BorderStyle="None" Width="100%" Enabled="false" Font-Bold="true" ID="txtComentarios" TextMode="MultiLine" Rows="3" EnableViewState="true"/></asp:TableCell>
                <asp:TableCell runat="server" Width="20%" BorderStyle="None"><SharePoint:InputFormTextBox runat="server" BorderStyle="None" Width="100%" Enabled="false" Font-Bold="true" ID="txtPais" TextMode="MultiLine" Rows="3"/></asp:TableCell>
            </asp:TableRow>
            <asp:TableRow ID="tblRowGuardarCambios">
                <asp:TableCell ColumnSpan="3">
                    <asp:Button runat="server" Id="btnGuardarCambios" Visible="false" Text="Guardar Cambios" OnClick="btnGuardarCambios_Click" Width="200px" />

                </asp:TableCell></asp:TableRow>
             <asp:TableHeaderRow BackColor="#144733">
                <asp:TableHeaderCell runat="server" ColumnSpan="3" ForeColor="White">Adjuntos</asp:TableHeaderCell>
            </asp:TableHeaderRow>
            <asp:TableRow>
                <asp:TableCell ID="cellAdjuntoSolicitud"  runat="server" Width="40%" BorderStyle="None">
                    <asp:FileUpload ID="filUploadAdjuntoSolicitud" runat="server" />
                    <asp:Button runat="server" Id="btnAdjuntarSolicitud" Text="Adjuntar" OnClick="btnAdjuntarSolicitud_Click"/>
                    <br />
                    <asp:Label ID="Label3" runat="server" Text="Seleccione el archivo a adjuntar y presione el botón Adjuntar"></asp:Label>
                </asp:TableCell>
                <%--<asp:TableCell runat="server" ColumnSpan="2" Width ="30%" BorderStyle ="None"></asp:TableCell>--%>
            <asp:TableCell>
                <asp:Panel runat="server" id="pnlAdjuntos"></asp:Panel>    
                <asp:gridview CssClass="ms-listviewtable"  id="gridAdjuntosSolicitud" allowpaging="false" PagerSettings-Visible="false" autogeneratecolumns="false" runat="server" Width="100%" onrowcommand="SolicitudAdjuntosGridView_RowCommand" GridLines="None" >
                        <HeaderStyle ForeColor="DarkGray" Font-Bold="True" BackColor="#f3f3f3" Font-Names="Tahoma" HorizontalAlign="Left" Font-Size="8pt" />
                        <RowStyle Font-Size="8pt" Font-Names="Tahoma" BorderStyle="None" BorderWidth="0" BorderColor="White"  />
                        <AlternatingRowStyle CssClass="ms-alternating"/>
                            <columns>
                            <asp:HyperLinkField DataNavigateUrlFields="AttachmentURL" DataTextField="AttachmentTitle" headertext="Nombre" Visible="true" ><ItemStyle Width="89%" HorizontalAlign="Left" /></asp:HyperLinkField>
                            <asp:buttonfield ButtonType="Image" CommandName="VerAdjunto" DataTextField="AttachmentURL" headertext=""  ImageUrl="../images/open.gif" Visible="false"><ItemStyle Width="5%" HorizontalAlign="Left" /></asp:buttonfield>
                            <asp:buttonfield ButtonType="Image" CommandName="EliminarAdjunto" DataTextField="Title" headertext=""  ImageUrl="../images/delitem.gif" ><ItemStyle Width="5%" HorizontalAlign="Left" /></asp:buttonfield>
                            <asp:boundfield datafield="AttachmentTitle" headertext="" Visible="true" ><ItemStyle Width="1%" Font-Size="0" ForeColor="White" HorizontalAlign="Left" /></asp:boundfield>
                            </columns>
                        </asp:gridview>
                </asp:TableCell>
            
            
            </asp:TableRow>
        </asp:Table>

        <asp:Table ID="tblDatosInicioProceso" runat="server" Width="100%">
            <asp:TableHeaderRow BackColor="#144733">
                <asp:TableHeaderCell runat="server" ForeColor="White">Fecha de Inicio del circuito</asp:TableHeaderCell>
                <asp:TableHeaderCell runat="server" ForeColor="White">Iniciador</asp:TableHeaderCell>
            </asp:TableHeaderRow>
            <asp:TableRow>
                <asp:TableCell runat="server" Width="30%" BorderStyle="None"><SharePoint:InputFormTextBox runat="server" BorderStyle="None" Width="100%" Enabled="false" Font-Bold="true" ID="txtInicioCircuito" /></asp:TableCell>
                <asp:TableCell runat="server" Width="30%" BorderStyle="None"><SharePoint:InputFormTextBox runat="server" BorderStyle="None" Width="100%" Enabled="false" Font-Bold="true" ID="txtAdministrador" /></asp:TableCell>
            </asp:TableRow>
            <asp:TableHeaderRow BackColor="#144733" Visible="true" ID="tblCabeceraComentarioDesarrollo">
                <asp:TableHeaderCell runat="server" ForeColor="White" ColumnSpan="2">Comentarios hechos por Desarrollo</asp:TableHeaderCell>
            </asp:TableHeaderRow>
            <asp:TableRow Visible="true" ID="tblDetalleComentarioDesarrollo">
                <asp:TableCell runat="server" Width="50%" ColumnSpan="2" BorderStyle="None"><SharePoint:InputFormTextBox runat="server" BorderStyle="Solid" Width="99%" Enabled="false"  ID="txtComentarioDesarrollo" TextMode="MultiLine" Rows="3" /></asp:TableCell>
            </asp:TableRow>
            <asp:TableHeaderRow BackColor="#144733" Visible="true" ID="tblCabeceraComentarioPlanificacion">
                <asp:TableHeaderCell runat="server" ForeColor="White" ColumnSpan="2">Comentarios hechos por Planificación</asp:TableHeaderCell>
            </asp:TableHeaderRow>
            <asp:TableRow Visible="true" ID="tblDetalleComentarioPlanificacion">
                <asp:TableCell runat="server" Width="50%" ColumnSpan="2" BorderStyle="None"><SharePoint:InputFormTextBox runat="server" BorderStyle="Solid" Width="99%" Enabled="false"  ID="txtComentarioPlanificacion" TextMode="MultiLine" Rows="3" /></asp:TableCell>
            </asp:TableRow>
            <asp:TableHeaderRow BackColor="#144733" Visible="true" ID="tblCabeceraComentarioPackaging">
                <asp:TableHeaderCell runat="server" ForeColor="White" ColumnSpan="2">Comentarios hechos por Packaging</asp:TableHeaderCell>
            </asp:TableHeaderRow>
            <asp:TableRow Visible="true" ID="tblDetalleComentarioPackaging">
                <asp:TableCell runat="server" Width="50%" ColumnSpan="2" BorderStyle="None"><SharePoint:InputFormTextBox runat="server" BorderStyle="Solid" Width="99%" Enabled="false"  ID="txtComentarioPackaging" TextMode="MultiLine" Rows="3" /></asp:TableCell>
            </asp:TableRow>
            <asp:TableRow ID="tblRowInicioProceso">
                <asp:TableCell ColumnSpan="3">
                    <asp:Button runat="server" Id="btnGuardarComentario" Visible="true" Text="Guardar Comentario" OnClick="btnGuardarComentario_Click" Width="200px" />
                    <asp:Button runat="server" Id="btnInformacionMateriales" Text="Información Producto / Materiales"  OnClick="btnInformacionMateriales_Click" Width="200px" />
                    <asp:Button runat="server" Id="btnIniciarProceso" Text="Iniciar Proceso" OnClientClick="this.disabled=true;" UseSubmitBehavior="false" OnClick="btnIniciarProceso_Click" Width="200px" />
                    <asp:Button runat="server" Id="btnCancelarProceso" Text="Cancelar Proceso" OnClick="btnCancelarProceso_Click" Width="200px" />
                    <asp:Button runat="server" Id="btnReenviarSolicitud" Text="Requerir Ajustes a la Solicitud" OnClick="btnReenviarSolicitur_Click" Width="200px" />
                </asp:TableCell>
            </asp:TableRow>
        </asp:Table>    </asp:Panel>
    
    <asp:Panel ID="pnlBitacoraDocumentoActual" runat="server">
    <hr />
        <h2 runat="server" style="background-color:cadetblue;color:white" Font-Bold="true">&nbsp;&nbsp;Tareas activas</h2>
        <asp:Label ID="Label1" runat="server" Text="Seleccione la tarea a procesar: "></asp:Label><asp:DropDownList ID="ddlSeleccioneTarea"  OnSelectedIndexChanged="ddlSeleccioneTarea_SelectedIndexChanged" runat="server" AutoPostBack="True"></asp:DropDownList>
        <asp:Table ID="tblDatosTareaActual" runat="server" Width="100%">
            <asp:TableHeaderRow BackColor="#009933">
                <asp:TableHeaderCell runat="server" ForeColor="White">Etapa</asp:TableHeaderCell>
                <asp:TableHeaderCell runat="server" ForeColor="White">Estado</asp:TableHeaderCell>
                <asp:TableHeaderCell runat="server" ForeColor="White">Corrector</asp:TableHeaderCell>
                <asp:TableHeaderCell runat="server" ForeColor="White">Fecha de Inicio de tarea</asp:TableHeaderCell>
                <asp:TableHeaderCell runat="server" ForeColor="White">Fecha de Vencimiento</asp:TableHeaderCell>
            </asp:TableHeaderRow>
            <asp:TableRow>
                <asp:TableCell runat="server" Width="40%" BorderStyle="None"><SharePoint:InputFormTextBox runat="server" BorderStyle="None" Width="100%" Enabled="false" Font-Bold="true" ID="txtEtapaTarea" ></SharePoint:InputFormTextBox></asp:TableCell>
                <asp:TableCell runat="server" Width="10%" BorderStyle="None"><SharePoint:InputFormTextBox runat="server" BorderStyle="None" Width="100%" Enabled="false" Font-Bold="true" ID="txtEstadoTarea" /></asp:TableCell>
                <asp:TableCell runat="server" Width="20%" BorderStyle="None"><SharePoint:InputFormTextBox runat="server" BorderStyle="None" Width="100%" Enabled="false" Font-Bold="true" ID="txtCorrector" /></asp:TableCell>
                <asp:TableCell runat="server" Width="15%" BorderStyle="None"><SharePoint:InputFormTextBox runat="server" BorderStyle="None" Width="100%" Enabled="false" Font-Bold="true" ID="txtFechaInicio" /></asp:TableCell>
                <asp:TableCell runat="server" Width="15%" BorderStyle="None"><SharePoint:InputFormTextBox runat="server" BorderStyle="None" Width="100%" Enabled="false" Font-Bold="true" ID="txtFechaFin"/></asp:TableCell>
            </asp:TableRow>
        </asp:Table>  
        <asp:Table ID="tblMensajeReprocesar" runat="server" Width="100%">
            <asp:TableHeaderRow BackColor="#009933">
                <asp:TableHeaderCell runat="server" HorizontalAlign="Left" ForeColor="White">Ajustes Solicitados:</asp:TableHeaderCell>
            </asp:TableHeaderRow>
            <asp:TableRow>
                <asp:TableCell runat="server" Width="100%" BorderStyle="None"><SharePoint:InputFormTextBox runat="server" Width="99%" Enabled="false" ID="txtMensaje" TextMode="MultiLine" RichText="false" Font-Bold="true" Rows="3" ></SharePoint:InputFormTextBox></asp:TableCell>
                </asp:TableRow>
        </asp:Table>
                <asp:Table ID="tblDatosAprobacionTareaActual" runat="server" Width="100%">
            <asp:TableHeaderRow BackColor="#009933">
                <asp:TableHeaderCell runat="server" HorizontalAlign="Left" ForeColor="White">Comentarios:</asp:TableHeaderCell>
            </asp:TableHeaderRow>
            <asp:TableRow>
                <asp:TableCell runat="server" Width="100%" BorderStyle="None"><SharePoint:InputFormTextBox runat="server" Width="99%" Enabled="true" ID="txtDatosAprobacion" TextMode="MultiLine" RichText="false" Rows="3" ></SharePoint:InputFormTextBox></asp:TableCell>
                </asp:TableRow>
        </asp:Table>
        <asp:Table ID="tblAdjuntarDocumento" runat="server" Width="100%">
            <asp:TableHeaderRow BackColor="#009933" >
                <asp:TableHeaderCell runat="server" HorizontalAlign="Left" ForeColor="White">Adjuntar Documento:</asp:TableHeaderCell>
                <asp:TableHeaderCell runat="server" HorizontalAlign="Left" ForeColor="White">Documentos Adjuntos:</asp:TableHeaderCell>
            </asp:TableHeaderRow>
            <asp:TableRow>
                <asp:TableCell runat="server" Width="40%" BorderStyle="None">
                    <asp:FileUpload ID="filUploadAdjunto" runat="server" />
                    <asp:Button runat="server" Id="btnAdjuntar" Text="Adjuntar" OnClick="btnGuardar_Click"/>
                    <br />
                    <asp:Label ID="Label2" runat="server" Text="Seleccione el archivo a adjuntar y presione el botón Adjuntar"></asp:Label>
                </asp:TableCell>
                <asp:TableCell>
                    <asp:gridview CssClass="ms-listviewtable"  id="gridAdjuntoTarea" allowpaging="false" PagerSettings-Visible="false" autogeneratecolumns="false" runat="server" Width="100%" onrowcommand="AvisosPagoAdjuntosGridView_RowCommand" GridLines="None" >
                        <HeaderStyle ForeColor="DarkGray" Font-Bold="True" BackColor="#f3f3f3" Font-Names="Tahoma" HorizontalAlign="Left" Font-Size="8pt" />
                        <RowStyle Font-Size="8pt" Font-Names="Tahoma" BorderStyle="None" BorderWidth="0" BorderColor="White"  />
                        <AlternatingRowStyle CssClass="ms-alternating"/>
                            <columns>
                            <asp:HyperLinkField DataNavigateUrlFields="AttachmentURL" DataTextField="AttachmentTitle" headertext="Nombre" Visible="true" ><ItemStyle Width="89%" HorizontalAlign="Left" /></asp:HyperLinkField>
                            <asp:buttonfield ButtonType="Image" CommandName="VerAdjunto" DataTextField="AttachmentURL" headertext=""  ImageUrl="../images/open.gif" Visible="false"><ItemStyle Width="5%" HorizontalAlign="Left" /></asp:buttonfield>
                            <asp:buttonfield ButtonType="Image" CommandName="EliminarAdjunto" DataTextField="Title" headertext=""  ImageUrl="../images/delitem.gif" ><ItemStyle Width="5%" HorizontalAlign="Left" /></asp:buttonfield>
                            <asp:boundfield datafield="AttachmentTitle" headertext="" Visible="true" ><ItemStyle Width="1%" Font-Size="0" ForeColor="White" HorizontalAlign="Left" /></asp:boundfield>
                            </columns>
                        </asp:gridview>
                </asp:TableCell>
                </asp:TableRow>
        </asp:Table>

        <asp:Table ID="tblDatosAprobacion" runat="server" Width="100%">
            <asp:TableRow><asp:TableCell>
                <asp:Button runat="server" Id="btnEditarMateriales" Text="Editar Producto / Material" Width="150px" OnClick="btnInformacionMateriales_Click" />
                <asp:Button runat="server" Id="btnGuardar" Text="Guardar Borrador" Width="150px" OnClick="btnGuardar_Click"/>
                <asp:Button runat="server" Id="btnAprobar" Text="Completar Tarea" Width="150px" OnClick="btnAprobar_Click" />
                <asp:Button runat="server" Id="btnRechazar" Text="Cerrar Caso" Width="150px" OnClick="btnRechazar_Click"/>

                </asp:TableCell><asp:TableCell HorizontalAlign="Right">
                
                <asp:Button runat="server" Id="btnCambiarCorrector" Text="Reasignar Tarea" Width="150px" OnClick="btnCambiarCorrector_Click"/>
                    <asp:Button runat="server" ID="btnReenviar" Text="Reenvio Mail" Width="150px" OnClick="btnReenviar_Click" />
            </asp:TableCell></asp:TableRow>
            
        </asp:Table>


    </asp:Panel>
    <asp:Panel runat="server" ID="PanelError">
        <asp:Table runat="server" ID="tblError">
            <asp:TableRow>
                <asp:TableCell><asp:Label runat="server" ID="lblMensajeError" ForeColor="Red" Visible="true"></asp:Label></asp:TableCell>
            </asp:TableRow>
        </asp:Table>
    </asp:Panel>
    <hr />
        <asp:Panel ID="pnlBitacoraDocumentoHistoria" runat="server">
        <h2 runat="server" style="background-color:cadetblue;color:white" Font-Bold="true">&nbsp;&nbsp;Historial de Tareas</h2>
        <asp:Table ID="tblHistorialTareas" runat="server" Width="100%"></asp:Table>
    </asp:Panel>
    <asp:HiddenField ID="IdIteracion" runat="server" EnableViewState="true" />
    <asp:HiddenField ID="IdTareaBitacora" runat="server" EnableViewState="true" />
    <asp:HiddenField ID="AdjuntoObligatorio" runat="server" EnableViewState="true" />
    <asp:HiddenField ID="ValidaMateriales" runat="server" EnableViewState="true" />
    <asp:HiddenField ID="TareaReinicio" runat="server" EnableViewState="true" />
    <asp:HiddenField ID="Sector" runat="server" EnableViewState="true" />
    <asp:HiddenField ID="strNombreSector" runat="server" EnableViewState="true" />
    <asp:HiddenField ID="strNombreInterno" runat="server" EnableViewState="true" />
    <asp:HiddenField ID="StrTipoSolicitud" runat="server" EnableViewState="true" />

</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
    Solicitudes
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
    Solicitudes
    
<script type="text/javascript">
    function bValidar() {
            if (confirm('Está por cambiar de tarea y los cambios no guardados se pueden perder. ¿Desea continuar?')) {
                
                    
                    var ddltst = <%= ddlSeleccioneTarea.UniqueID %>;
                    __doPostBack(ddltst, '');
                
            } else {
                $('#ConfirmMessageResponse').val('No');
                
                return false;
            }
        }
    
   
　</script>
    </asp:Content>
