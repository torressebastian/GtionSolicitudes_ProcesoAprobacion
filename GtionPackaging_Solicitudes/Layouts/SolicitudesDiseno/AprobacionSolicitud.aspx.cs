using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Web.UI.WebControls;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Collections.Generic;
using System.Net;
using System.DirectoryServices.AccountManagement;
using System.Collections;
using Microsoft.SharePoint.Utilities;
using System.Collections.Specialized;
using System.Text;
using GtionPackaging_Solicitudes;

namespace SolicitudesDiseno_Solicitudes.Layouts.SolicitudesDiseno
{

    public class AttachmentsData
    {

        public string Title { get; set; }
        public string AttachmentTitle { get; set; }
        public string AttachmentURL { get; set; }
    }
    public partial class AprobacionSolicitud : LayoutsPageBase
    {
        protected void Page_Init(object sender, EventArgs e)
        { }
            protected void Page_Load(object sender, EventArgs e)
        {
            //Obtengo el Id de la solicitud
            Int32 idDocument = 0;
            idDocument = Convert.ToInt32(Request["ID"]);
            Boolean bInicioProceso = false;

            Guid siteId = SPContext.Current.Site.ID;
            Guid webId = SPContext.Current.Web.ID;
            SPUser currentUser = SPContext.Current.Web.CurrentUser;

            //SPSecurity.RunWithElevatedPrivileges(delegate ()
            //{
            using (SPSite site = new SPSite(siteId))
            {
                using (SPWeb web = site.OpenWeb(webId))
                {
                    SPList lDocumentos = web.Lists["Solicitudes"];
                    SPList lBitacora = web.Lists["Bitácora Solicitudes"];
                    SPListItem itmDocumento = lDocumentos.GetItemById(idDocument);

                    String sEstado = "";
                    String sTipoSolicitud = itmDocumento.ContentType.Name.ToString();
                    String sTipo = "";
                    if (Page.IsPostBack != true)
                    {
                        txtNombreSolicitud.Text = itmDocumento.Title.ToString();
                        txtTipoDocumento.Text = sTipoSolicitud;
                        if (itmDocumento["Detalle Solicitud"] is null) { txtComentarios.Text = ""; } else { txtComentarios.Text = itmDocumento["Detalle Solicitud"].ToString(); }
                        if (itmDocumento["Estado"] is null) { sEstado = ""; } else { sEstado = itmDocumento["Estado"].ToString(); }
                        if (itmDocumento["Fecha Inicio Solicitud"] is null) { txtInicioCircuito.Text = ""; } else { txtInicioCircuito.Text = Convert.ToDateTime(itmDocumento["Fecha Inicio Solicitud"].ToString()).ToShortDateString(); }
                        txtEstado.Text = sEstado;
                        if (sTipoSolicitud == "Lanzamiento Internacional")
                        {
                            if (itmDocumento["País"] is null) { txtPais.Text = ""; } else { txtPais.Text = Funciones_Comunes.RemoveCharacters(itmDocumento["País"].ToString()); }

                        }
                        else
                        {
                            txtPais.Text = "No aplica";
                        }
                        if (itmDocumento["Administrador"] is null) { txtAdministrador.Text = ""; }
                        else
                        {
                            String strResponsable = "";
                            string fieldValue = itmDocumento["Administrador"].ToString();
                            SPFieldUserValueCollection users = new SPFieldUserValueCollection(itmDocumento.Web, fieldValue);

                            foreach (SPFieldUserValue uv in users)
                            {
                                SPUser user = uv.User;
                                if (strResponsable != "")
                                {
                                    strResponsable = strResponsable + "; " + user.Name.ToString();
                                }
                                else
                                {
                                    strResponsable = user.Name.ToString();
                                }
                                // Process user
                            }

                            txtAdministrador.Text = strResponsable;
                        };
                    }

                    // Identifico el sector al que corresponde el usuario y lo comparo con el sector al que corresponde la solicitud.

                    Boolean bEsSectorAlta = false;


                    SPList lConfiguracionSolicitudes = web.Lists["Configuración Circuitos Solicitudes"];
                    SPQuery query = new SPQuery();
                    query.Query = "<Where><Eq><FieldRef Name=\"Title\" /><Value Type=\"Text\">" + sTipoSolicitud + "</Value></Eq></Where>";
                    query.RowLimit = 1;
                    query.ViewFields = "";
                    SPListItemCollection items = lConfiguracionSolicitudes.GetItems(query);
                    SPListItem item = items[0];
                    sTipo = item["Tipo Solicitud"].ToString();
                    StrTipoSolicitud.Value = sTipo;
                    SPFieldLookupValueCollection lkSectorAlta = new SPFieldLookupValueCollection(item["Sector alta"].ToString());

                    foreach (SPFieldLookupValue itmSectorAlta in lkSectorAlta)
                    {
                        Int32 idSectorAlta = itmSectorAlta.LookupId;
                        if (Funciones_Comunes.UsuarioGrupo(currentUser, idSectorAlta) == true)
                        {
                            bEsSectorAlta = true;
                        }
                    }

                    // Para las modificaciones, habilito los campos Comentario Desarrollo y Comentario Planificación

                    if (sTipo != "Lanzamiento")
                    {
                        if (Page.IsPostBack != true)
                        {
                            if (itmDocumento["Comentario desarrollo"] is null) { txtComentarioDesarrollo.Text = ""; } else { txtComentarioDesarrollo.Text = itmDocumento["Comentario desarrollo"].ToString(); }
                            if (itmDocumento["Comentario planificacion"] is null) { txtComentarioPlanificacion.Text = ""; } else { txtComentarioPlanificacion.Text = itmDocumento["Comentario planificacion"].ToString(); }
                            if (itmDocumento["Comentario packaging"] is null) { txtComentarioPackaging.Text = ""; } else { txtComentarioPackaging.Text = itmDocumento["Comentario packaging"].ToString(); }
                        }
                    }

                    if (sEstado == "") {
                        sEstado = txtEstado.Text.ToString();
                    }

                    
                    // Si el estado es Completado, oculto el panel de tareas activas.
                    if (sEstado == "Completado") {
                        pnlBitacoraDocumentoActual.Visible = false;
                    }
                                        
                    // Establezco si es inicio de proceso.
                    if (itmDocumento["Inicio Proceso"] is null)
                    {
                        bInicioProceso = true;
                    }
                    else
                    {
                        if (itmDocumento["Inicio Proceso"].ToString() == "SI")
                        {

                            bInicioProceso = bEsSectorAlta;
                            if (bEsSectorAlta == true)
                            {
                                btnCancelarProceso.Visible = true;
                                btnGuardarCambios.Visible = true;
                                txtNombreSolicitud.Enabled = true;
                                txtComentarios.Enabled = true;
                            }
                            else {
                                btnCancelarProceso.Visible = false;
                            }
                            if (sEstado == "Cancelado") {
                                btnGuardar.Visible = false;
                                btnCancelarProceso.Visible = false;
                                cellAdjuntoSolicitud.Visible = false;
                            }

                        }
                        else {
                            if (bEsSectorAlta == true && sEstado != "Cancelado")
                            {
                                btnCancelarProceso.Visible = true;
                                cellAdjuntoSolicitud.Visible = true;
                            }
                            else
                            {
                                btnCancelarProceso.Visible = false;
                                cellAdjuntoSolicitud.Visible = false;
                            }

                        }
                    }

                    // Verifico el tipo de solicitud (Lanzamiento / Modificación) para la botonera
                    if (sTipo != "Lanzamiento")
                    {
                        if (sEstado != "Completado")
                        {
                            tblCabeceraComentarioDesarrollo.Visible = false;
                            tblCabeceraComentarioPackaging.Visible = false;
                            tblCabeceraComentarioPlanificacion.Visible = false;
                            tblDetalleComentarioDesarrollo.Visible = false;
                            tblDetalleComentarioPackaging.Visible = false;
                            tblDetalleComentarioPlanificacion.Visible = false;
                        }
                        else {
                            tblCabeceraComentarioDesarrollo.Visible = true;
                            tblCabeceraComentarioPackaging.Visible = true;
                            tblCabeceraComentarioPlanificacion.Visible = true;
                            tblDetalleComentarioDesarrollo.Visible = true;
                            tblDetalleComentarioPackaging.Visible = true;
                            tblDetalleComentarioPlanificacion.Visible = true;
                        }

                        if (bInicioProceso == true && sEstado != "Cancelado")
                        {
                            btnInformacionMateriales.Text = "Cargar Materiales";
                            
                        }
                        else {
                            btnInformacionMateriales.Text = "Ver Materiales";
                        }
                        
                        btnEditarMateriales.Text = "Editar Materiales";
                    }
                    else
                    {
                        if (bInicioProceso == true && sEstado != "Cancelado")
                        {
                            btnInformacionMateriales.Text = "Cargar Productos / Materiales";
                            
                        }
                        else
                        {
                            btnInformacionMateriales.Text = "Ver Productos / Materiales";
                        }
                        btnEditarMateriales.Text = "Editar Productos / Materiales";
                    }

                    // La sección Materiales está dispobible en todo momento. 
                    btnInformacionMateriales.Visible = true;

                    if (sTipo != "Lanzamiento" && sEstado == "Pendiente Inicio Packaging")
                    {
                        if (Funciones_Comunes.UsuarioGrupo(currentUser, Funciones_Comunes.iDevolverIdSector("Packaging")) == true)

                        {
                            tblCabeceraComentarioDesarrollo.Visible = true;
                            tblCabeceraComentarioPackaging.Visible = true;
                            tblCabeceraComentarioPlanificacion.Visible = true;
                            tblDetalleComentarioDesarrollo.Visible = true;
                            tblDetalleComentarioPackaging.Visible = true;
                            tblDetalleComentarioPlanificacion.Visible = true;
                            txtComentarioPackaging.Enabled = true;
                            btnGuardarComentario.Visible = true; }
                        else {
                            tblCabeceraComentarioDesarrollo.Visible = false;
                            tblCabeceraComentarioPackaging.Visible = false;
                            tblCabeceraComentarioPlanificacion.Visible = false;
                            tblDetalleComentarioDesarrollo.Visible = false;
                            tblDetalleComentarioPackaging.Visible = false;
                            tblDetalleComentarioPlanificacion.Visible = false;
                            txtComentarioPackaging.Enabled = false;
                            btnGuardarComentario.Visible = false;
                        }              
                    }
                    else {
                        
                        btnGuardarComentario.Visible = false;
                    }

                    btnReenviarSolicitud.Visible = false;
                    if (sEstado == "Pendiente Inicio Packaging")
                    {
                        if (Funciones_Comunes.UsuarioGrupo(currentUser, Funciones_Comunes.iDevolverIdSector("Packaging")) == true)
                        {
                            btnReenviarSolicitud.Visible = true;
                        }
                    }

                    if (sTipo == "Lanzamiento") {
                        tblCabeceraComentarioDesarrollo.Visible = false;
                        tblCabeceraComentarioPackaging.Visible = false;
                        tblCabeceraComentarioPlanificacion.Visible = false;
                        tblDetalleComentarioDesarrollo.Visible = false;
                        tblDetalleComentarioPackaging.Visible = false;
                        tblDetalleComentarioPlanificacion.Visible = false;

                    }


                    if (sEstado == "Cancelado") { btnCancelarProceso.Visible = false; }

                    if (bInicioProceso == false || sEstado == "Cancelado")
                    {
                        btnIniciarProceso.Visible = false;
                    }
                    else
                    {

                        pnlBitacoraDocumentoActual.Visible = false;

                   }

                   
                    if (bEsSectorAlta == true && sEstado != "Cancelado")
                    {
                        CargarAdjuntos(idDocument);
                    }
                    else
                    {
                        CargarAdjuntosPanel(idDocument);
                    }
                    if (sEstado != "Cancelado" && sEstado != "Completado")
                    {
                        if (Page.IsPostBack != true)
                        {
                            CargarTareaActual(idDocument);
                        }
                    }

                    CargarTareasCumplidas(idDocument);


                }
            }
            //});


        }

        protected void CargarAdjuntos(Int32 idDocument) {
            try
            {

                //cellAdjuntosAvisoPago.Visible = true;
                gridAdjuntosSolicitud.DataSource = getAttachmentsData("Solicitudes", idDocument);
                gridAdjuntosSolicitud.DataBind();

            }
            catch (Exception ex)
            {
                //Errores.Text = Errores.Text + " - Error Listar Adjuntos: " + ex.Message;
            }

            
        }

        protected void CargarAdjuntosPanel(Int32 idDocument)
        {
            Guid siteId = SPContext.Current.Site.ID;
            Guid webId = SPContext.Current.Web.ID;
            using (SPSite site = new SPSite(SPContext.Current.Site.Url))
            {
                using (SPWeb web = site.OpenWeb("SolicitudesDiseno"))
                {
                    SPList lSolicitudes = web.Lists["Solicitudes"];
                    SPListItem itmDocumento = lSolicitudes.GetItemById(idDocument);
                    String auxAttach = "";
                    // Busco los adjuntos
                    string fileurl = (string)itmDocumento["EncodedAbsUrl"];

                    if (itmDocumento["Attachments"].ToString() == "True")
                    {
                        SPAttachmentCollection attachments = itmDocumento.Attachments;
                        foreach (string fileName in attachments)
                        {
                            SPFile file = itmDocumento.ParentList.ParentWeb.GetFile(
                            itmDocumento.Attachments.UrlPrefix + fileName);
                            String strExtension = fileName.Split('.')[1].ToString();

                            auxAttach = auxAttach + @"<a href='" + web.Url.ToString() + "/" + file.Url.ToString() + "'>" + file.Name.ToString() + "</a><br/>";

                        }
                    }
                    else { auxAttach = "Sin adjuntos"; }



                    pnlAdjuntos.Controls.Add(new LiteralControl(auxAttach));
                }
            }
        }
        protected void CargarTareaActual(Int32 idDocument) {
            Guid siteId = SPContext.Current.Site.ID;
            Guid webId = SPContext.Current.Web.ID;

            //ddlSeleccioneTarea.Items.Clear();

            using (SPSite site = new SPSite(SPContext.Current.Site.Url))
            {
                using (SPWeb web = site.OpenWeb("SolicitudesDiseno"))
                {
                    SPList lSolicitudes = web.Lists["Solicitudes"];
                    SPList lBitacora = web.Lists["Bitácora Solicitudes"];
                    SPListItem itmDocumento = lSolicitudes.GetItemById(idDocument);
                    String strOrigen = "T";
                    strOrigen = Request["Origen"];

                    SPQuery queryDA = new SPQuery();
                    queryDA.Query = string.Concat("<Where><And><And><Or><Membership Type='CurrentUserGroups'><FieldRef Name='Asignado'/></Membership><Eq> <FieldRef Name='Asignado'></FieldRef><Value Type='Integer'><UserID Type='Integer'/></Value></Eq></Or><Eq><FieldRef Name='Solicitud_x0020_asociada' LookupId='TRUE'/>", "<Value Type='Lookup'>", idDocument, "</Value></Eq></And><Eq><FieldRef Name='Estado'/>", "<Value Type='String'>Pendiente</Value></Eq></And></Where>");
                    //queryDA.Query = string.Concat("<Where><And><Eq><FieldRef Name='Solicitud_x0020_asociada' LookupId='TRUE'/>", "<Value Type='Lookup'>", idDocument, "</Value></Eq><Eq><FieldRef Name='Estado'/>", "<Value Type='String'>Pendiente</Value></Eq></And></Where><OrderBy><FieldRef Name='ID' Ascending='False'/></OrderBy>");

                    SPListItemCollection itemColl = null;
                    itemColl = lBitacora.GetItems(queryDA);
                    if (itemColl.Count > 0)
                    {

                        ListItem itmTarea0 = new ListItem();
                        itmTarea0.Value = "0";
                        itmTarea0.Text = "<-- Seleccione una tarea -->";
                        ddlSeleccioneTarea.Items.Add(itmTarea0);
                        int i = 0;
                        foreach (SPListItem itmTarea in itemColl)
                        {
                            ListItem itmTareaActiva = new ListItem();
                            itmTareaActiva.Value = itmTarea.ID.ToString();
                            itmTareaActiva.Text = itmTarea.Title.ToString();
                            ddlSeleccioneTarea.Items.Add(itmTareaActiva);
                            i = i + 1;
                        }

                        if (i == 1)
                        {
                            ddlSeleccioneTarea.SelectedIndex = 1;
                            CargarTareaEdicion(Convert.ToInt32(ddlSeleccioneTarea.SelectedItem.Value.ToString()));

                        }
                        else {
                            tblDatosTareaActual.Visible = false;
                            tblDatosAprobacionTareaActual.Visible = false;
                            tblAdjuntarDocumento.Visible = false;
                            tblDatosAprobacion.Visible = false;
                        }

                    }
                    else {

                        ddlSeleccioneTarea.Visible = false;
                        pnlBitacoraDocumentoActual.Visible = false;


                    }

                }
            }
        }
        protected void CargarTareaEdicion(Int32 idTarea)
        {
            Guid siteId = SPContext.Current.Site.ID;
            Guid webId = SPContext.Current.Web.ID;
            tblDatosTareaActual.Visible = true;
            tblAdjuntarDocumento.Visible = true;
            IdTareaBitacora.Value = idTarea.ToString();

            tblDatosTareaActual.Visible = true;
            tblDatosAprobacionTareaActual.Visible = true;
            tblAdjuntarDocumento.Visible = true;
            tblDatosAprobacion.Visible = true;


            //SPSecurity.RunWithElevatedPrivileges(delegate ()
            //{
            using (SPSite site = new SPSite(siteId))
            {
                using (SPWeb web = site.OpenWeb(webId))
                {
                    SPList lSolicitudes = web.Lists["Solicitudes"];
                    SPList lBitacora = web.Lists["Bitácora Solicitudes"];
                    SPList lConfiguracionProceso = web.Lists["Configuración Proceso Solicitudes"];
                    Int32 iConfiguracionProceso;
                    SPListItem itmConfiguracionProceso;
                    SPListItem itmTarea = lBitacora.GetItemById(idTarea);
                    String strOrigen = "T";
                    strOrigen = Request["Origen"];

                    String strEtapaTarea, strEstadoTarea, strFechaInicio, strFechaFin, strCorrector, strSector;

                    strEtapaTarea = itmTarea.Title.ToString();
                    txtEtapaTarea.Text = strEtapaTarea;
                    if (itmTarea["Estado"] is null) { txtEstadoTarea.Text = ""; } else { txtEstadoTarea.Text = itmTarea["Estado"].ToString(); };
                    if (itmTarea["Fecha de Inicio"] is null) { txtFechaInicio.Text = ""; } else { txtFechaInicio.Text = Convert.ToDateTime(itmTarea["Fecha de Inicio"].ToString()).ToShortDateString(); };
                    if (itmTarea["Fecha de Fin"] is null) { txtFechaFin.Text = ""; }
                    if (itmTarea["Comentarios"] is null) { txtDatosAprobacion.Text = ""; } else { txtDatosAprobacion.Text = itmTarea["Comentarios"].ToString(); };

                    if (itmTarea["Asignado"] is null) { strCorrector = ""; }
                    else
                    {
                        String strResponsable = "";
                        try
                        {
                            string fieldValue = itmTarea["Asignado"].ToString();
                            SPFieldUserValueCollection users = new SPFieldUserValueCollection(itmTarea.Web, fieldValue);
                            foreach (SPFieldUserValue uv in users)
                            {
                                if (uv != null)
                                {
                                    if (uv.User != null)
                                    {
                                        SPUser user = uv.User;
                                        if (strResponsable != "")
                                        {
                                            strResponsable = strResponsable + "; " + user.Name.ToString();
                                        }
                                        else
                                        {
                                            strResponsable = user.Name.ToString();
                                        }
                                    }
                                    else
                                    {
                                        if (web.Groups[uv.LookupValue] != null)
                                        {
                                            SPGroup sGroup = web.Groups[uv.LookupValue];
                                            if (strResponsable != "")
                                            {
                                                strResponsable = strResponsable + "; " + sGroup.Name.ToString();
                                            }
                                            else
                                            {
                                                strResponsable = sGroup.Name.ToString();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch { }

                        strCorrector = strResponsable;
                    };
                    txtCorrector.Text = strCorrector;

                    listarAdjuntos("Bitácora Solicitudes", Convert.ToInt32(IdTareaBitacora.Value.ToString()));

                    if (itmTarea["Configuracion Tarea"] is null) { iConfiguracionProceso = 0; } else { iConfiguracionProceso = Convert.ToInt32(itmTarea["Configuracion Tarea"].ToString().Split(';')[0]); };
                    if (iConfiguracionProceso != 0)
                    {
                        itmConfiguracionProceso = lConfiguracionProceso.GetItemById(iConfiguracionProceso);
                        if (itmConfiguracionProceso["Adjunto Obligatorio"] is null)
                        {
                            AdjuntoObligatorio.Value = "NO";
                        }
                        else
                        {

                            if (itmConfiguracionProceso["Adjunto Obligatorio"].ToString() == "False")
                            {
                                AdjuntoObligatorio.Value = "NO";
                            }
                            else
                            {
                                AdjuntoObligatorio.Value = "SI";
                            }
                        }
                        if (itmConfiguracionProceso["Agrega materiales"] is null)
                        {
                            ValidaMateriales.Value = "NO";
                        }
                        else
                        {

                            if (itmConfiguracionProceso["Agrega materiales"].ToString() == "False")
                            {
                                ValidaMateriales.Value = "NO";
                            }
                            else
                            {
                                ValidaMateriales.Value = "SI";
                            }
                        }

                        if (itmConfiguracionProceso["Sector"] is null)
                        {
                            Sector.Value = "0";
                            strNombreSector.Value = "";
                        }
                        else
                        {
                            Sector.Value = itmConfiguracionProceso["Sector"].ToString().Split(';')[0].ToString();
                            strNombreSector.Value = itmConfiguracionProceso["Sector"].ToString().Split('#')[1].ToString();
                        }
                    }


                }
            }
            //});
        }

        protected void CargarTareasCumplidas(Int32 idDocument)
        {
            Guid siteId = SPContext.Current.Site.ID;
            Guid webId = SPContext.Current.Web.ID;

            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                using (SPSite site = new SPSite(siteId))
                {
                    using (SPWeb web = site.OpenWeb(webId))
                    {
                        SPList lSolicitudes = web.Lists["Solicitudes"];
                        SPList lBitacora = web.Lists["Bitácora Solicitudes"];
                        SPListItem itmDocumento = lSolicitudes.GetItemById(idDocument);
                        String strOrigen = "T";
                        strOrigen = Request["Origen"];

                        String strEtapaTarea, strEstadoTarea, strFechaInicio, strFechaFin, strCorrector, strDatosAprobacion, strDatosMensaje, strAdjuntos;

                        SPQuery queryDA = new SPQuery();
                        queryDA.Query = string.Concat("<Where><Eq><FieldRef Name='Solicitud_x0020_asociada' LookupId='TRUE'/>", "<Value Type='Lookup'>", idDocument, "</Value></Eq></Where><OrderBy>  <FieldRef Name='ID' Ascending='False'/></OrderBy>");
                        SPListItemCollection itemColl = null;
                        itemColl = lBitacora.GetItems(queryDA);



                        if (itemColl.Count > 0)
                        {
                            int i = 2;
                            foreach (SPListItem itmTarea in itemColl)
                            {

                                if (itmTarea["Estado"].ToString() != "")
                                {
                                    if (i == 2)
                                    {
                                        if (itmTarea["Iteración"] is null) { IdIteracion.Value = "0"; } else { IdIteracion.Value = itmTarea["Iteración"].ToString(); };
                                    }

                                    strAdjuntos = "";

                                    if (strOrigen == "T" || (strOrigen == "P" && IdIteracion.Value == itmTarea["Iteración"].ToString()))
                                    {
                                        //if (itmTarea["Etapa"] is null) { strEtapaTarea = ""; } else { strEtapaTarea = itmTarea["Etapa"].ToString() + " - " + itmTarea["Configuracion Tarea"].ToString().Split('#')[1]; };
                                        strEtapaTarea = itmTarea.Title.ToString();
                                        if (itmTarea["Estado"] is null) { strEstadoTarea = ""; } else { strEstadoTarea = itmTarea["Estado"].ToString(); };
                                        if (itmTarea["Fecha de Inicio"] is null) { strFechaInicio = ""; } else { strFechaInicio = Convert.ToDateTime(itmTarea["Fecha de Inicio"].ToString()).ToShortDateString(); };
                                        if (itmTarea["Fecha de Fin"] is null) { strFechaFin = ""; } else { strFechaFin = Convert.ToDateTime(itmTarea["Fecha de Fin"].ToString()).ToShortDateString(); };
                                        //if (itmTarea["Asignado"] is null) { strCorrector = ""; } else { strCorrector = itmTarea["Asignado"].ToString().Split('#')[1]; };
                                        if (itmTarea["Asignado"] is null) { strCorrector = ""; }
                                        else
                                        {
                                            String strResponsable = "";
                                            try
                                            {
                                                string fieldValue = itmTarea["Asignado"].ToString();
                                                SPFieldUserValueCollection users = new SPFieldUserValueCollection(itmTarea.Web, fieldValue);
                                                foreach (SPFieldUserValue uv in users)
                                                {
                                                    if (uv != null)
                                                    {
                                                        if (uv.User != null)
                                                        {
                                                            SPUser user = uv.User;
                                                            if (strResponsable != "")
                                                            {
                                                                strResponsable = strResponsable + "; " + user.Name.ToString();
                                                            }
                                                            else
                                                            {
                                                                strResponsable = user.Name.ToString();
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (web.Groups[uv.LookupValue] != null)
                                                            {
                                                                SPGroup sGroup = web.Groups[uv.LookupValue];
                                                                if (strResponsable != "")
                                                                {
                                                                    strResponsable = strResponsable + "; " + sGroup.Name.ToString();
                                                                }
                                                                else
                                                                {
                                                                    strResponsable = sGroup.Name.ToString();
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            catch { }

                                            strCorrector = strResponsable;
                                        };
                                        if (itmTarea["Comentarios"] is null) { strDatosAprobacion = "Sin Datos aprobación."; } else { strDatosAprobacion = itmTarea["Comentarios"].ToString(); };
                                        if (itmTarea["Attachments"].ToString() == "True")
                                        {
                                            SPAttachmentCollection attachments = itmTarea.Attachments;
                                            foreach (string fileName in itmTarea.Attachments)
                                            {
                                                SPFile file = itmTarea.ParentList.ParentWeb.GetFile(
                                                itmTarea.Attachments.UrlPrefix + fileName);
                                                strAdjuntos = strAdjuntos + @"<a href=' " + itmTarea.Attachments.UrlPrefix + fileName + "'>" + file.Name.ToString() + "</a><br/>";
                                            }
                                        }
                                        else
                                        {
                                            strAdjuntos = "Sin documentos adjuntos.";
                                        }

                                        Int32 iConfiguracionProceso = 0;
                                        SPListItem itmConfiguracionProceso;
                                        SPList lConfiguracionProceso = web.Lists["Configuración Proceso Solicitudes"];
                                        if (itmTarea["Configuracion Tarea"] is null) { iConfiguracionProceso = 0; } else { iConfiguracionProceso = Convert.ToInt32(itmTarea["Configuracion Tarea"].ToString().Split(';')[0]); };
                                        itmConfiguracionProceso = lConfiguracionProceso.GetItemById(iConfiguracionProceso);

                                        if (itmConfiguracionProceso["Tarea Resumen"].ToString() == "True")
                                        {
                                            TableHeaderRow tblRowCabecera = new TableHeaderRow();
                                            tblRowCabecera.BackColor = System.Drawing.Color.FromName("#144733");
                                            TableCell tblCellCabeceraEtapa = new TableCell();
                                            TableCell tblCellCabeceraFechaInicio = new TableCell();
                                            TableCell tblCellCabeceraAsignado = new TableCell();

                                            tblCellCabeceraAsignado.ColumnSpan = 3;
                                            tblCellCabeceraAsignado.Text = "Iniciador";
                                            tblCellCabeceraAsignado.ForeColor = System.Drawing.Color.White;
                                            tblCellCabeceraFechaInicio.ColumnSpan = 2;
                                            tblCellCabeceraFechaInicio.Text = "Fecha de Inicio";
                                            tblCellCabeceraFechaInicio.ForeColor = System.Drawing.Color.White;
                                            TableCell tblCellCabecerAdjuntos = new TableCell();



                                            tblRowCabecera.Cells.Add(tblCellCabeceraAsignado);
                                            tblRowCabecera.Cells.Add(tblCellCabeceraFechaInicio);


                                            TableRow tblRowFila1 = new TableRow();
                                            TableCell tblCellFechaInicio = new TableCell();
                                            TableCell tblCellAsignado = new TableCell();


                                            tblCellFechaInicio.Text = strFechaInicio;
                                            tblCellFechaInicio.ColumnSpan = 2;
                                            tblCellAsignado.ColumnSpan = 3;
                                            tblCellAsignado.Text = strCorrector;
                                            tblRowFila1.Cells.Add(tblCellAsignado);
                                            tblRowFila1.Cells.Add(tblCellFechaInicio);





                                            tblHistorialTareas.Rows.Add(tblRowCabecera);
                                            tblHistorialTareas.Rows.Add(tblRowFila1);
                                        }
                                        else
                                        {
                                            TableHeaderRow tblRowCabecera = new TableHeaderRow();
                                            if (i % 2 == 0)
                                            {
                                                tblRowCabecera.BackColor = System.Drawing.Color.FromName("#009933");
                                            }
                                            else
                                            {
                                                tblRowCabecera.BackColor = System.Drawing.Color.CadetBlue;
                                            }
                                            TableCell tblCellCabeceraEtapa = new TableCell();
                                            TableCell tblCellCabeceraEstado = new TableCell();
                                            TableCell tblCellCabeceraFechaInicio = new TableCell();
                                            TableCell tblCellCabeceraFechaFin = new TableCell();
                                            TableCell tblCellCabeceraAsignado = new TableCell();

                                            tblCellCabeceraEtapa.Text = "Tarea";
                                            tblCellCabeceraEtapa.ForeColor = System.Drawing.Color.White;
                                            tblCellCabeceraEstado.Text = "Estado";
                                            tblCellCabeceraEstado.ForeColor = System.Drawing.Color.White;
                                            tblCellCabeceraAsignado.Text = "Usuario";
                                            tblCellCabeceraAsignado.ForeColor = System.Drawing.Color.White;
                                            tblCellCabeceraFechaInicio.Text = "Fecha de Inicio de tarea";
                                            tblCellCabeceraFechaInicio.ForeColor = System.Drawing.Color.White;
                                            tblCellCabeceraFechaFin.Text = "Fecha de cumplimentado";
                                            tblCellCabeceraFechaFin.ForeColor = System.Drawing.Color.White;
                                            tblRowCabecera.Cells.Add(tblCellCabeceraEtapa);
                                            tblRowCabecera.Cells.Add(tblCellCabeceraEstado);
                                            tblRowCabecera.Cells.Add(tblCellCabeceraAsignado);
                                            tblRowCabecera.Cells.Add(tblCellCabeceraFechaInicio);
                                            tblRowCabecera.Cells.Add(tblCellCabeceraFechaFin);


                                            TableRow tblRowFila1 = new TableRow();
                                            TableCell tblCellEtapa = new TableCell();
                                            TableCell tblCellEstado = new TableCell();
                                            TableCell tblCellFechaInicio = new TableCell();
                                            TableCell tblCellFechaFin = new TableCell();
                                            TableCell tblCellAsignado = new TableCell();

                                            tblCellEtapa.Text = strEtapaTarea;
                                            tblCellEstado.Text = strEstadoTarea;
                                            tblCellFechaInicio.Text = strFechaInicio;
                                            tblCellFechaFin.Text = strFechaFin;
                                            tblCellAsignado.Text = strCorrector;
                                            tblRowFila1.Cells.Add(tblCellEtapa);
                                            tblRowFila1.Cells.Add(tblCellEstado);
                                            tblRowFila1.Cells.Add(tblCellAsignado);
                                            tblRowFila1.Cells.Add(tblCellFechaInicio);
                                            tblRowFila1.Cells.Add(tblCellFechaFin);

                                            TableRow tblRowCabeceraFila2 = new TableRow();
                                            if (i % 2 == 0)
                                            {
                                                tblRowCabeceraFila2.BackColor = System.Drawing.Color.FromName("#009933");
                                            }
                                            else
                                            {
                                                tblRowCabeceraFila2.BackColor = System.Drawing.Color.CadetBlue;
                                            }

                                            if (strOrigen == "T" || itmConfiguracionProceso["Tarea Publicación"].ToString() == "True")
                                            {
                                                TableCell tblCellCabeceraComentarios = new TableCell();
                                                tblCellCabeceraComentarios.ColumnSpan = 3;
                                                if (strOrigen == "T")
                                                {
                                                    tblCellCabeceraComentarios.Text = "Comentarios";
                                                    tblCellCabeceraComentarios.ForeColor = System.Drawing.Color.White;
                                                    tblRowCabeceraFila2.Cells.Add(tblCellCabeceraComentarios);
                                                }
                                                TableCell tblCellCabecerAdjuntos = new TableCell();

                                                tblCellCabecerAdjuntos.ColumnSpan = 2;

                                                tblCellCabecerAdjuntos.Text = "Documentos Adjuntos";
                                                tblCellCabecerAdjuntos.ForeColor = System.Drawing.Color.White;
                                                tblRowCabeceraFila2.Cells.Add(tblCellCabecerAdjuntos);

                                            }


                                            TableRow tblRowFila2 = new TableRow();
                                            TableCell tblCellComentarios = new TableCell();
                                            TableCell tblCellAdjuntos = new TableCell();

                                            if (strOrigen == "T")
                                            {
                                                tblCellComentarios.ColumnSpan = 3;
                                                tblCellComentarios.Text = strDatosAprobacion;
                                                tblRowFila2.Cells.Add(tblCellComentarios);
                                                tblCellAdjuntos.ColumnSpan = 2;
                                            }
                                            else
                                            {
                                                tblCellAdjuntos.ColumnSpan = 5;
                                            }

                                            tblCellAdjuntos.Text = strAdjuntos;
                                            tblRowFila2.Cells.Add(tblCellAdjuntos);


                                            tblHistorialTareas.Rows.Add(tblRowCabecera);
                                            tblHistorialTareas.Rows.Add(tblRowFila1);
                                            tblHistorialTareas.Rows.Add(tblRowCabeceraFila2);
                                            tblHistorialTareas.Rows.Add(tblRowFila2);


                                            i = i + 1;
                                        }

                                    }

                                }

                            }
                        }
                        else
                        {
                            pnlBitacoraDocumentoHistoria.Visible = false;
                            IdIteracion.Value = "0";
                        }
                    }
                }
            });
        }
        protected void btnIniciarProceso_Click(object sender, EventArgs e)
        {
            Int32 idSolicitud = Convert.ToInt32(Request["ID"]);
            if (bGuardarCambios() == true) { 
            Int32 iValidarProducto = bValidarProductos(false, false);

            if (txtTipoDocumento.Text == "Modificación de Archivos (Desarrollo)"){
                iValidarProducto = bValidarProductos(true, true); 
            }
                if (iValidarProducto == 1)
                {
                    using (SPSite site = new SPSite(SPContext.Current.Site.Url))
                    {
                        using (SPWeb web = site.OpenWeb("SolicitudesDiseno"))
                        {
                            SPList lSolicitudes = web.Lists["Solicitudes"];
                            SPList lBitacora = web.Lists["Bitácora Solicitudes"];
                            SPList lConfiguracionProceso = web.Lists["Configuración Proceso Solicitudes"];
                            String strSectorActual = "";

                            SPListItem itmSolicitud = lSolicitudes.GetItemById(idSolicitud);

                            SPQuery query = new SPQuery();
                            query.Query = "<Where><Eq><FieldRef Name=\"Title\" /><Value Type=\"Text\">Inicio Proceso Solicitud</Value></Eq></Where>";
                            query.RowLimit = 1;
                            query.ViewFields = "";
                            SPListItemCollection items = lConfiguracionProceso.GetItems(query);
                            SPListItem item = items[0];

                            SPListItem itmTareaResumen = lBitacora.AddItem();
                            itmTareaResumen["Title"] = "Inicio Proceso";
                            itmTareaResumen["Solicitud asociada"] = idSolicitud;
                            itmTareaResumen["Estado"] = "Completado";
                            itmTareaResumen["Procesado"] = "SI";
                            itmTareaResumen["Configuracion Tarea"] = item.ID.ToString();
                            itmTareaResumen["Fecha de Fin"] = Funciones_Comunes.dtFechaVencimiento(Convert.ToInt32(item["Días Vencimiento"].ToString()));
                            itmTareaResumen["Asignado"] = SPContext.Current.Web.CurrentUser;
                            itmTareaResumen.Update();

                            SPQuery queryConfig = new SPQuery();
                            queryConfig.Query = string.Concat("<Where><And><Eq><FieldRef Name='Tarea_x0020_inicial_x0020_circui' /><Value Type='Boolean'>1</Value></Eq><Contains><FieldRef Name='Circuito'/><Value Type='LookupMulti'>", txtTipoDocumento.Text, "</Value></Contains></And></Where>");
                            SPListItemCollection itemColl = null;
                            itemColl = lConfiguracionProceso.GetItems(queryConfig);

                            if (itemColl.Count > 0)
                            {
                                foreach (SPListItem itmTarea in itemColl)
                                {

                                    Int32 idSector = Convert.ToInt32(itmTarea["Sector"].ToString().Split(';')[0].ToString());
                                    SPList lSectores = web.Lists["Sectores"];
                                    SPListItem imSector = lSectores.GetItemById(idSector);

                                    SPListItem itmTareaBitacora = lBitacora.AddItem();
                                    itmTareaBitacora["Title"] = itmTarea.Title.ToString();
                                    itmTareaBitacora["Solicitud asociada"] = idSolicitud;
                                    itmTareaBitacora["Asignado"] = imSector["Usuarios"];
                                    itmTareaBitacora["Configuracion Tarea"] = itmTarea.ID.ToString();
                                    itmTareaBitacora["Fecha de Fin"] = Funciones_Comunes.dtFechaVencimiento(Convert.ToInt32(itmTarea["Días Vencimiento"].ToString()));
                                    itmTareaBitacora["Iteración"] = Convert.ToInt32(IdIteracion.Value) + 1;
                                    itmTareaBitacora["Tarea Agrupadora"] = itmTarea["Tarea Agrupadora"];
                                    itmTareaBitacora["Ver"] = itmSolicitud["Ver"];
                                    itmTareaBitacora["Sector"] = itmTarea["Sector"].ToString().Split('#')[1].ToString();
                                    itmTareaBitacora.Update();

                                    if (strSectorActual != "")
                                    {
                                        strSectorActual = strSectorActual + "; " + itmTarea["Sector"].ToString().Split('#')[1].ToString();
                                    }
                                    else
                                    {
                                        strSectorActual = itmTarea["Sector"].ToString().Split('#')[1].ToString();
                                    }

                                }
                                //itmDocumento["Usuario Asignado"] = itemColl[0]["Usuario asignado"];
                                //itmDocumento["Fecha Vencimiento"] = DateTime.Now.AddDays(Convert.ToInt32(itemColl[0]["Días Vencimiento"].ToString()));

                            }


                            itmSolicitud["Fecha Inicio Solicitud"] = DateTime.Now;
                            SPUser oUser = SPContext.Current.Web.CurrentUser;
                            itmSolicitud["Administrador"] = oUser;
                            itmSolicitud["Sector actual"] = strSectorActual;
                            itmSolicitud["Inicio Proceso"] = "NO";
                            itmSolicitud["Estado"] = "En Curso";
                            itmSolicitud.Update();

                            System.Threading.Thread.Sleep(10000);

                            Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/SolicitudesDiseno/AprobacionSolicitud.aspx?ID=" + Request["ID"]);

                            //CargarTareaActual(idDocument);
                            //CargarTareasCumplidas(idDocument);

                        }

                    }
                }
                else
                {

                    if (iValidarProducto == 0)
                    {
                        lblMensajeError.Text = "Se debe indicar al menos un producto para iniciar el proceso.";
                        if (txtTipoDocumento.Text == "Modificación de Archivos (Desarrollo)")
                        {
                            lblMensajeError.Text = "Se debe indicar al menos un material a reemplazar para iniciar el proceso.";
                        }


                        lblMensajeError.Visible = true;

                    }

                    if (iValidarProducto == 3)
                    {
                        lblMensajeError.Text = "Los datos de materiales asociados al inicio no están completos. Verifique los datos de Productos.";
                        lblMensajeError.Visible = true;

                    }

                    if (iValidarProducto == 2)
                    {
                        lblMensajeError.Text = "Se deben cargar al menos un material de reemplazo por cada material antes de iniciar el proceso.";
                        lblMensajeError.Visible = true;
                    }


                }
            }
        }
        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            Boolean bProcesar = true;
            Int32 idDocument = Convert.ToInt32(Request["ID"]);

            if (IdTareaBitacora.Value.ToString() == "" || IdTareaBitacora.Value.ToString() == "0")
            {
                lblMensajeError.Text = "Se debe seleccionar la tarea a aprobar.";
                lblMensajeError.Visible = true;
                //txtUsuario.Focus();
                bProcesar = false;
            }


            if (bProcesar == true) {
                using (SPSite site = new SPSite(SPContext.Current.Site.Url))
                {
                    using (SPWeb web = site.OpenWeb("SolicitudesDiseno"))
                    {
                        SPList lBitacora = web.Lists["Bitácora Solicitudes"];
                        SPListItem itmBitacora = lBitacora.GetItemById(Convert.ToInt32(IdTareaBitacora.Value.ToString()));
                        itmBitacora["Comentarios"] = txtDatosAprobacion.Text.ToString();
                        itmBitacora.Update();
                        AdjuntarAvisoPago(Convert.ToInt32(IdTareaBitacora.Value.ToString()));
                    }

                }

                CargarTareaEdicion(Convert.ToInt32(IdTareaBitacora.Value.ToString()));
            }
            CargarTareasCumplidas(idDocument);
        }
        protected void btnAprobar_Click(object sender, EventArgs e)
        {
            Boolean bProcesar = true;
            Int32 idDocument = Convert.ToInt32(Request["ID"]);

            if (IdTareaBitacora.Value.ToString() == "" || IdTareaBitacora.Value.ToString() == "0")
            {
                lblMensajeError.Text = "Se debe seleccionar la tarea a aprobar.";
                lblMensajeError.Visible = true;
                //txtUsuario.Focus();
                bProcesar = false;
            }

            if (AdjuntoObligatorio.Value == "SI")
            {
                if (gridAdjuntoTarea.Rows.Count == 0)
                {
                    lblMensajeError.Text = "Se debe adjuntar el archivo correspondiente antes de completar esta tarea.";
                    lblMensajeError.Visible = true;
                    //txtUsuario.Focus();
                    bProcesar = false;
                }

            }
            else {

                Int32 iResultadoValidar = bValidarProductos(true, true);

                if (iResultadoValidar == 3)
                {
                    lblMensajeError.Text = "Los datos de productos asociados a la tarea no están completos. Verifique los datos de Productos.";
                    lblMensajeError.Visible = true;
                    bProcesar = false;
                }

                if (ValidaMateriales.Value == "SI")
                {
                    if (iResultadoValidar == 2)
                    {
                        lblMensajeError.Text = "Se deben cargar al menos un material por cada producto antes de completar esta tarea.";
                        lblMensajeError.Visible = true;
                        bProcesar = false;
                    }
                }
            }

            // Valido la existencia de materiales para cada producto.


            if (bProcesar == true)
            {

                using (SPSite site = new SPSite(SPContext.Current.Site.Url))
                {
                    using (SPWeb web = site.OpenWeb("SolicitudesDiseno"))
                    {
                        SPList lBitacora = web.Lists["Bitácora Solicitudes"];
                        SPListItem itmBitacora = lBitacora.GetItemById(Convert.ToInt32(IdTareaBitacora.Value.ToString()));
                        //itmBitacora["Comentario de Revisión / Aprobación"] = txtDatosAprobacion.Text.ToString();
                        itmBitacora["Comentarios"] = txtDatosAprobacion.Text.ToString();
                        itmBitacora["Asignado"] = SPContext.Current.Web.CurrentUser;
                        itmBitacora["Estado"] = "Completado";
                        itmBitacora.Update();

                        String sTipo = Funciones_Comunes.DevolverTipoSolicitud(txtTipoDocumento.Text.ToString());

                        if (sTipo == "Modificación") {
                            // Valido el tipo de tarea que corresponda
                            if (strNombreSector.Value == "Desarrollo") {
                                txtComentarioDesarrollo.Text = txtDatosAprobacion.Text.ToString();
                            }
                            if (strNombreSector.Value == "Planificación") {
                                txtComentarioPlanificacion.Text = txtDatosAprobacion.Text.ToString();
                            }

                            SPList lstList = SPContext.Current.Web.Lists["Solicitudes"];
                            SPListItem itmAdjunto;
                            itmAdjunto = lstList.GetItemById(Convert.ToInt32(Request["ID"]));
                            itmAdjunto["Comentario desarrollo"] = txtComentarioDesarrollo.Text.ToString();
                            itmAdjunto["Comentario planificacion"] = txtComentarioPlanificacion.Text.ToString();
                            itmAdjunto.Update();

                        }

                    }

                }

                CargarTareaEdicion(Convert.ToInt32(IdTareaBitacora.Value.ToString()));
                CargarTareasCumplidas(idDocument);
                Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/SolicitudesDiseno/ProcesandoSolicitud.aspx?ID=" + Request["ID"]);
            }
        }
        protected void btnRechazar_Click(object sender, EventArgs e)
        {
            if (txtDatosAprobacion.Text.ToString() != "")
            {
                Int32 idDocument = Convert.ToInt32(Request["ID"]);
                using (SPSite site = new SPSite(SPContext.Current.Site.Url))
                {
                    using (SPWeb web = site.OpenWeb("SolicitudesDiseno"))
                    {
                        SPList lBitacora = web.Lists["Bitácora Solicitudes"];
                        SPListItem itmBitacora = lBitacora.GetItemById(Convert.ToInt32(IdTareaBitacora.Value.ToString()));
                        itmBitacora["Asignado"] = SPContext.Current.Web.CurrentUser;
                        itmBitacora["Estado"] = "Rechazado";
                        itmBitacora.Update();

                        SPList lDocumentos = web.Lists["Solicitudes"];
                        SPListItem itmDocumento = lDocumentos.GetItemById(idDocument);

                        itmDocumento["Inicio Proceso"] = "SI";
                        itmDocumento["Estado"] = "Rechazado";
                        itmDocumento["Usuario Asignado"] = null;
                        itmDocumento["Fecha Vencimiento"] = null;
                        itmDocumento.Update();

                        Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/SolicitudesDiseno/ProcesandoSolicitud.aspx?ID=" + Request["ID"]);
                    }

                }
                CargarTareaEdicion(Convert.ToInt32(IdTareaBitacora.Value.ToString()));
                CargarTareasCumplidas(idDocument);
            }
            else
            {
                lblMensajeError.Text = "Se debe indicar el motivo del rechazo de la tarea.";
                lblMensajeError.Visible = true;
                txtDatosAprobacion.Focus();
            }



        }
        protected void btnReenviar_Click(object sender, EventArgs e)
        {
            Boolean bProcesar = true;
            Int32 idDocument = Convert.ToInt32(Request["ID"]);

            if (IdTareaBitacora.Value.ToString() == "" || IdTareaBitacora.Value.ToString() == "0")
            {
                lblMensajeError.Text = "Se debe seleccionar la tarea a reenviar.";
                lblMensajeError.Visible = true;
                //txtUsuario.Focus();
                bProcesar = false;
            }


            if (bProcesar == true)
            {
                using (SPSite site = new SPSite(SPContext.Current.Site.Url))
                {
                    using (SPWeb web = site.OpenWeb("SolicitudesDiseno"))
                    {


                        Int32 iConfiguracionProceso;
                        SPList lConfiguracionProceso = web.Lists["Configuración Proceso Solicitudes"];
                        SPList lBitacora = web.Lists["Bitácora Solicitudes"];
                        SPListItem itmBitacora = lBitacora.GetItemById(Convert.ToInt32(IdTareaBitacora.Value.ToString()));

                        if (itmBitacora["Configuracion Tarea"] is null) { iConfiguracionProceso = 0; } else { iConfiguracionProceso = Convert.ToInt32(itmBitacora["Configuracion Tarea"].ToString().Split(';')[0]); };
                        SPListItem itmConfiguracionProceso = lConfiguracionProceso.GetItemById(iConfiguracionProceso);

                        string strDocumentoAsociado, strIdDocumentoAsociado, strTipoCircuito, strTituloCircuito;
                        strDocumentoAsociado = itmBitacora["Solicitud asociada"].ToString().Split('#')[1].ToString();
                        strIdDocumentoAsociado = itmBitacora["Solicitud asociada"].ToString().Split(';')[0].ToString();
                        SPList lDocumentos = web.Lists["Solicitudes"];
                        SPListItem itmDocumento = lDocumentos.GetItemById(Convert.ToInt32(strIdDocumentoAsociado));

                        strTipoCircuito = itmDocumento.ContentType.Name.ToString();

                        if (itmConfiguracionProceso["Tarea Resumen"].ToString() == "False")
                        {

                            StringBuilder strCuerpoAnuncio = new StringBuilder();
                            String strCabeceraMail = "";
                            strCuerpoAnuncio = strCuerpoAnuncio.Append("</tr>");
                            string strResponsable = "";
                            string strCopiaMail = "";

                            string strLinkPaginaTarea = web.Url + "/_layouts/15/SolicitudesDiseno/AprobacionSolicitud.aspx?ID=" + strIdDocumentoAsociado + "&Origen=T";
                            strCabeceraMail = "Se le recuerda la tarea asignada " + itmBitacora.Title.ToString() + " para la solicitud " + strDocumentoAsociado + ".";
                            strCuerpoAnuncio = strCuerpoAnuncio.Append("<b>Circuito:</b> " + strTipoCircuito + "<br /><br />");
                            strCuerpoAnuncio = strCuerpoAnuncio.Append("<b>Fecha de Vencimiento de la tarea:</b> " + Convert.ToDateTime(itmBitacora["Fecha de Fin"].ToString()).ToShortDateString() + "<br /><br />");
                            strCuerpoAnuncio = strCuerpoAnuncio.Append("Para continuar con el proceso, ingrese a la tarea para su completitud: " + @"<a href='" + strLinkPaginaTarea + "'>" + itmBitacora.Title.ToString() + "</a><br/>");

                            string fieldValue = itmBitacora["Asignado"].ToString();
                            SPFieldUserValueCollection users = new SPFieldUserValueCollection(itmBitacora.Web, fieldValue);

                            foreach (SPFieldUserValue uv in users)
                            {
                                if (uv.User != null)
                                {
                                    SPUser user = uv.User;
                                    strResponsable = strResponsable + user.Email.ToString() + ";";
                                }
                                else
                                {
                                    SPGroup sGroup = web.Groups[uv.LookupValue];
                                    foreach (SPUser user in sGroup.Users)
                                    {
                                        if (user.IsDomainGroup == true)
                                        {
                                            ArrayList ADMembers = GetADGroupUsers(user.Name.ToString());
                                            foreach (string userName in ADMembers)
                                            {
                                                strResponsable = strResponsable + userName + ";";
                                            }
                                        }
                                        else
                                        {

                                            strResponsable = strResponsable + user.Email.ToString() + ";";
                                        }
                                    }

                                }

                                // Process user
                            }



                            string emailBody = " ";
                            emailBody = emailBody + "</tr></table>";
                            StringDictionary headers = new StringDictionary();
                            headers.Add("to", strResponsable);// sDevolverMailUsuario(strResponsable, properties));
                            headers.Add("from", web.Title.ToString() + "<sharepoint@baliarda.com.ar>");
                            if (strCopiaMail != "") { headers.Add("cc", strCopiaMail); }
                            headers.Add("subject", itmBitacora.Title.ToString() + " - " + strDocumentoAsociado);
                            headers.Add("content-type", "text/html");
                            SPUtility.SendEmail(web, headers, strCabeceraMail + "<br /><br />" + strCuerpoAnuncio.ToString() + emailBody);
                            emailBody = "";

                            Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/SolicitudesDiseno/ProcesandoSolicitud.aspx?ID=" + Request["ID"]);

                        }

                    }
                }
            }

        }
        protected void btnCambiarCorrector_Click(object sender, EventArgs e)
        {
            Boolean bProcesar = true;
            Int32 idDocument = Convert.ToInt32(Request["ID"]);

            if (IdTareaBitacora.Value.ToString() == "" || IdTareaBitacora.Value.ToString() == "0")
            {
                lblMensajeError.Text = "Se debe seleccionar la tarea a asignar.";
                lblMensajeError.Visible = true;
                //txtUsuario.Focus();
                bProcesar = false;
            }


            if (bProcesar == true)
            {
                Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/SolicitudesDiseno/AsignarTareaSolicitud.aspx?ID=" + Request["ID"] + "&IDTarea=" + IdTareaBitacora.Value.ToString());
            }
        }
        protected void listarAdjuntos(String strLista, Int32 idElemento)
        {
            //if (!Page.IsPostBack)
            //{
            try
            {

                //cellAdjuntosAvisoPago.Visible = true;
                gridAdjuntoTarea.DataSource = getAttachmentsData(strLista, idElemento);
                gridAdjuntoTarea.DataBind();

            }
            catch (Exception ex)
            {
                //Errores.Text = Errores.Text + " - Error Listar Adjuntos: " + ex.Message;
            }
            //}
        }
        protected void AdjuntarAvisoPago(Int32 iTarea)
        {
            try
            {
                if (filUploadAdjunto.FileName != "")
                {
                    SPList lstList;
                    lstList = SPContext.Current.Web.Lists["Bitácora Solicitudes"];
                    SPListItem itmAdjunto;
                    itmAdjunto = lstList.GetItemById(Convert.ToInt32(IdTareaBitacora.Value.ToString()));
                    itmAdjunto.Attachments.Add(filUploadAdjunto.FileName, filUploadAdjunto.FileBytes);
                    itmAdjunto.UpdateOverwriteVersion();
                }

                CargarTareaEdicion(Convert.ToInt32(IdTareaBitacora.Value.ToString()));
                Int32 idDocument = 0;
                idDocument = Convert.ToInt32(Request["ID"]);
                CargarTareasCumplidas(idDocument);
            }
            catch (Exception ex)
            {
                //Errores.Text = Errores.Text + " - Error Adjuntar: " + ex.Message;
            }
        }
        public List<AttachmentsData> getAttachmentsData(String strLista, Int32 iElemento)
        {
            List<AttachmentsData> AttachmentsData = new List<AttachmentsData>();

            Guid siteId = SPContext.Current.Site.ID;
            Guid webId = SPContext.Current.Web.ID;

            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                using (SPSite site = new SPSite(siteId))
                {
                    using (SPWeb currentWeb = site.OpenWeb(webId))
                    {
                        SPList lst = currentWeb.Lists[strLista];
                        SPListItem item = lst.GetItemById(iElemento);

                        if (item["Attachments"].ToString() == "True")
                        {
                            SPAttachmentCollection attachments = item.Attachments;
                            //SPFolder folder = item.Attachments.UrlPrefix;
                            //SPFolder folder = currentWeb.Folders[item.Attachments.UrlPrefix];
                            //SPFolder folder = currentWeb.Folders["Lists"].SubFolders[lst.Title].SubFolders["Attachments"].SubFolders[item.ID.ToString()];

                            foreach (string fileName in item.Attachments)
                            {
                                SPFile file = item.ParentList.ParentWeb.GetFile(
                                item.Attachments.UrlPrefix + fileName);

                                AttachmentsData.Add(new AttachmentsData()
                                {
                                    Title = item["Title"].ToString(),
                                    AttachmentTitle = file.Name.ToString(),
                                    AttachmentURL = currentWeb.Url + "/" + file.Url.ToString()
                                });
                            }
                        }
                        else if (item["Attachments"].ToString() == "False")
                        {
                            /* AttachmentsData.Add(new AttachmentsData()
                             {
                                 Title = item["Title"].ToString(),
                                 AttachmentTitle = "--",
                                 AttachmentURL = Page.Request.Url.ToString() + "#"
                             });*/
                        }
                    }
                }
            });
            return AttachmentsData;
        }
        protected void AvisosPagoAdjuntosGridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {

            try
            {

                int index = Convert.ToInt32(e.CommandArgument);
                GridViewRow row = gridAdjuntoTarea.Rows[index];
                SPList lstList = SPContext.Current.Web.Lists["Bitácora Solicitudes"];
                SPListItem itmAdjunto;
                itmAdjunto = lstList.GetItemById(Convert.ToInt32(IdTareaBitacora.Value));
                SPAttachmentCollection atcItem;
                atcItem = itmAdjunto.Attachments;
                //Errores.Text = Errores.Text + row.Cells[0].Text;
                if (e.CommandName == "EliminarAdjunto")
                {
                    itmAdjunto.Attachments.Delete(row.Cells[3].Text);
                    itmAdjunto.Update();
                }
                else
                {
                    String sPath;
                    sPath = SPContext.Current.Web.ServerRelativeUrl;
                    sPath = row.Cells[3].Text;

                    SPFile file = SPContext.Current.Web.GetFile(sPath);
                    string filePath = Path.Combine(@"C:\SharePoint", row.Cells[0].Text);

                    byte[] binFile = file.OpenBinary();
                    System.IO.FileStream fs = System.IO.File.Create(filePath);
                    fs.Write(binFile, 0, binFile.Length);
                    fs.Close();



                }

                CargarTareaEdicion(Convert.ToInt32(IdTareaBitacora.Value.ToString()));
                Int32 idDocument = 0;
                idDocument = Convert.ToInt32(Request["ID"]);
                CargarTareasCumplidas(idDocument);

                //Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/SolicitudesDiseno/Procesando.aspx?ID=" + Request["ID"]);

            }
            catch (Exception ex)
            {
                //Errores.Text = Errores.Text + " " + ex.Message;

            }

        }
        protected void btnMailPublicacion_Click(object sender, EventArgs e)
        {

        }
        public bool IsUserAuthorized(string groupName)
        {

            SPUser currentUser = SPContext.Current.Web.CurrentUser;

            Guid siteId = SPContext.Current.Site.ID;
            Guid webId = SPContext.Current.Web.ID;
            Boolean bResult = false;

            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                using (SPSite site = new SPSite(siteId))
                {
                    using (SPWeb web = site.OpenWeb(webId))
                    {

                        SPGroupCollection userGroups = currentUser.Groups;
                        foreach (SPGroup group in userGroups)
                        {
                            if (group.Name.Contains(groupName))
                                bResult = true;
                        }

                        //SPGroup sGroup = web.Groups[groupName];
                        //foreach (SPUser user in sGroup.Users)
                        //{
                        //    if (user.IsDomainGroup == true)
                        //    {
                        //        ArrayList ADMembers = GetADGroupUsers(user.Name.ToString());
                        //        foreach (string userName in ADMembers)
                        //        {
                        //            if (currentUser.LoginName.ToString() == user.LoginName.ToString())
                        //            {
                        //                bResult = true;
                        //            }
                        //        }
                        //    }
                        //    else
                        //    {
                        //        if (currentUser.LoginName.ToString() == user.LoginName.ToString())
                        //        {
                        //            bResult = true;
                        //        }
                        //    }
                        //}


                    }
                    //bResult = false;
                }
            });
            return bResult;
        }
        private ArrayList GetADGroupUsers(string groupName)
        {
            ArrayList userNames = new ArrayList();
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
            GroupPrincipal group = GroupPrincipal.FindByIdentity(ctx, groupName.Replace("Baliarda\\", "").ToString());

            if (group != null)
            {
                foreach (Principal p in group.GetMembers())
                {
                    UserPrincipal theUser = p as UserPrincipal;
                    if (theUser != null)
                    {
                        var user = UserPrincipal.FindByIdentity(ctx, p.SamAccountName);
                        if (user != null)
                        {
                            userNames.Add(user.EmailAddress);
                        }
                    }
                }

            }
            return userNames;

        }

        protected void ddlSeleccioneTarea_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (ddlSeleccioneTarea.SelectedItem.Value != "0")
            {
                
                CargarTareaEdicion(Convert.ToInt32(ddlSeleccioneTarea.SelectedItem.Value));
            }
            else {
                tblDatosTareaActual.Visible = false;
                tblDatosAprobacionTareaActual.Visible = false;
                tblAdjuntarDocumento.Visible = false;
                tblDatosAprobacion.Visible = false;
                IdTareaBitacora.Value = "0";
            }

            Int32 idDocument = 0;
            idDocument = Convert.ToInt32(Request["ID"]);
            CargarTareasCumplidas(idDocument);

        }

        protected void btnInformacionMateriales_Click(object sender, EventArgs e)
        {

            Boolean bProcesar = true;
            Int32 idDocument = Convert.ToInt32(Request["ID"]);

            if (IdTareaBitacora.Value.ToString() == "" || IdTareaBitacora.Value.ToString() == "0")
            {
                bProcesar = false;
            }
            
            if (bProcesar == true)
            {
                using (SPSite site = new SPSite(SPContext.Current.Site.Url))
                {
                    using (SPWeb web = site.OpenWeb("SolicitudesDiseno"))
                    {
                        SPList lBitacora = web.Lists["Bitácora Solicitudes"];
                        SPListItem itmBitacora = lBitacora.GetItemById(Convert.ToInt32(IdTareaBitacora.Value.ToString()));
                        itmBitacora["Comentarios"] = txtDatosAprobacion.Text.ToString();
                        itmBitacora.Update();
                        AdjuntarAvisoPago(Convert.ToInt32(IdTareaBitacora.Value.ToString()));
                    }

                }
            }

            if (bGuardarCambios() == true) { 

            if (btnInformacionMateriales.Text == "Ver Materiales" || btnInformacionMateriales.Text == "Cargar Materiales") {
                Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/SolicitudesDiseno/SolicitudProductoMaterialModif.aspx?ID=" + Request["ID"]);
            }
            
            else {
                Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/SolicitudesDiseno/SolicitudProductoMaterial.aspx?ID=" + Request["ID"]);
            }
            }

        }

        protected Int32 bValidarProductos(Boolean bValidarObligatorio, Boolean bValidarMaterial) {
            Int32 bAuxResultado = 0;

            Int32 idDocument = 0;
            idDocument = Convert.ToInt32(Request["ID"]);

            SPList lConfiguracionSolicitudes = SPContext.Current.Web.Lists["Configuración Circuitos Solicitudes"];
            SPQuery query = new SPQuery();
            query.Query = "<Where><Eq><FieldRef Name=\"Title\" /><Value Type=\"Text\">" + txtTipoDocumento.Text.ToString() + "</Value></Eq></Where>";
            query.RowLimit = 1;
            query.ViewFields = "";
            SPListItemCollection items = lConfiguracionSolicitudes.GetItems(query);
            SPListItem item = items[0];
            String strNombreInterno = item["Nombre Interno"].ToString();



            SPQuery qryProductos = new SPQuery();
            String strQuery = "";
            SPList lstListProductos = SPContext.Current.Web.Lists["Solicitud - Producto"];
            strQuery = "<Eq><FieldRef Name='Solicitud' LookupId='TRUE' /><Value Type='Lookup'>" + idDocument.ToString() + "</Value></Eq>";
            if (!string.IsNullOrEmpty(strQuery))
            {
                strQuery = "<Where>" + strQuery + "</Where>";
            }
            qryProductos.Query = strQuery;

            SPListItemCollection lstProductos = lstListProductos.GetItems(qryProductos);

            if (lstProductos.Count != 0) {
                bAuxResultado = 1;



                if (bValidarMaterial == true || bValidarObligatorio == true)
                {
                    foreach (SPListItem itmProducto in lstProductos) {

                        SPQuery qryCampos = new SPQuery();
                        String strQueryCampo = "";
                        SPList lConfiguracionProductoSector = SPContext.Current.Web.Lists["Configuración Producto Sector"];
                        strQueryCampo = "<Where><Contains><FieldRef Name='" + strNombreInterno + "' LookupId='TRUE' /><Value Type='LookupMulti'>" + Sector.Value.ToString() + "</Value></Contains></Where>";
                        qryCampos.Query = strQueryCampo;
                        SPListItemCollection lstCampos = lConfiguracionProductoSector.GetItems(qryCampos);
                        foreach (SPListItem itmCampo in lstCampos) {
                            Boolean bValidar = true;
                            //if (StrTipoSolicitud.Value != "Lanzamiento" && itmCampo.Title.ToString() == "Datos de Cobertura") {
                            //    bValidar = false;
                            //}
                            if (bValidar == true) { 
                            if (itmProducto[itmCampo.Title.ToString()] is null) {
                                bAuxResultado = 3;
                            }
                            else
                            {
                                if (itmProducto[itmCampo.Title.ToString()].ToString() == "")
                                {
                                    bAuxResultado = 3;
                                }
                            }
                            }
                        }


                        if (bAuxResultado != 3) {
                            SPQuery qryMaterial = new SPQuery();
                            String strQueryMaterial = "";
                            SPList lstListMateriales = SPContext.Current.Web.Lists["Solicitud - Producto Material"];
                            strQueryMaterial = "<Where><Contains><FieldRef Name='Producto' LookupId='TRUE' /><Value Type='LookupMulti'>" + itmProducto.ID.ToString() + "</Value></Contains></Where>";

                            qryMaterial.Query = strQueryMaterial;
                            SPListItemCollection lstMateriales = lstListMateriales.GetItems(qryMaterial);

                            if (lstMateriales.Count == 0) {
                                bAuxResultado = 2;

                            }
                        }
                        else
                        {
                            break;
                        }

                    }
                }
            }



            return bAuxResultado;
        }

        protected void btnCancelarProceso_Click(object sender, EventArgs e)
        {
            Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/SolicitudesDiseno/CancelarProceso.aspx?ID=" + Request["ID"]);
        }

        protected void btnAdjuntarSolicitud_Click(object sender, EventArgs e)
        {
            try
            {
                if (filUploadAdjuntoSolicitud.FileName != "")
                {
                    SPList lstList;
                    lstList = SPContext.Current.Web.Lists["Solicitudes"];
                    SPListItem itmAdjunto;
                    itmAdjunto = lstList.GetItemById(Convert.ToInt32(Request["ID"].ToString()));
                    itmAdjunto.Attachments.Add(filUploadAdjuntoSolicitud.FileName, filUploadAdjuntoSolicitud.FileBytes);
                    itmAdjunto.UpdateOverwriteVersion();
                }

                CargarAdjuntos(Convert.ToInt32(Request["ID"].ToString()));
                //Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/SolicitudesDiseno/Procesando.aspx?ID=" + Request["ID"]);

            }
            catch (Exception ex)
            {

                //Errores.Text = Errores.Text + " - Error Adjuntar: " + ex.Message;
            }
        }

        protected void EliminarAdjuntoSolicitud(String strAdjunto)
        {
            try
            {
                    SPList lstList;
                    lstList = SPContext.Current.Web.Lists["Solicitudes"];
                    SPListItem itmAdjunto;
                    itmAdjunto = lstList.GetItemById(Convert.ToInt32(Request["ID"].ToString()));
                    itmAdjunto.Attachments.Delete(strAdjunto);
                    itmAdjunto.UpdateOverwriteVersion();

                CargarAdjuntos(Convert.ToInt32(Request["ID"].ToString()));
                //Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/SolicitudesDiseno/Procesando.aspx?ID=" + Request["ID"]);

            }
            catch (Exception ex)
            {
                //Errores.Text = Errores.Text + " - Error Adjuntar: " + ex.Message;
            }
        }

        protected void SolicitudAdjuntosGridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {

            try
            {

                int index = Convert.ToInt32(e.CommandArgument);
                GridViewRow row = gridAdjuntosSolicitud.Rows[index];
                SPList lstList = SPContext.Current.Web.Lists["Solicitudes"];
                SPListItem itmAdjunto;
                itmAdjunto = lstList.GetItemById(Convert.ToInt32(Request["ID"]));
                SPAttachmentCollection atcItem;
                atcItem = itmAdjunto.Attachments;
                //Errores.Text = Errores.Text + row.Cells[0].Text;
                if (e.CommandName == "EliminarAdjunto")
                {
                    itmAdjunto.Attachments.Delete(row.Cells[3].Text);
                    itmAdjunto.Update();
                }
                else
                {
                    String sPath;
                    sPath = SPContext.Current.Web.ServerRelativeUrl;
                    sPath = row.Cells[3].Text;

                    SPFile file = SPContext.Current.Web.GetFile(sPath);
                    string filePath = Path.Combine(@"C:\SharePoint", row.Cells[0].Text);

                    byte[] binFile = file.OpenBinary();
                    System.IO.FileStream fs = System.IO.File.Create(filePath);
                    fs.Write(binFile, 0, binFile.Length);
                    fs.Close();



                }
                CargarAdjuntos(Convert.ToInt32(Request["ID"].ToString()));

                //Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/SolicitudesDiseno/Procesando.aspx?ID=" + Request["ID"]);

            }
            catch (Exception ex)
            {
                //Errores.Text = Errores.Text + " " + ex.Message;

            }

        }

        protected void btnGuardarComentario_Click(object sender, EventArgs e)
        {
            SPList lstList = SPContext.Current.Web.Lists["Solicitudes"];
            SPListItem itmAdjunto;
            itmAdjunto = lstList.GetItemById(Convert.ToInt32(Request["ID"]));
            itmAdjunto["Comentario packaging"] = txtComentarioPackaging.Text.ToString();
            itmAdjunto.Update();

        }

        protected void btnGuardarCambios_Click(object sender, EventArgs e)
        {

            Boolean bResultado = bGuardarCambios();
        }

        protected Boolean bGuardarCambios() {
            Boolean bResultado = true;

            lblMensajeError.Visible = false;
            if (txtEstado.Text == "No Iniciada") { 
            if (txtNombreSolicitud.Text != "")
            {
                SPList lstList = SPContext.Current.Web.Lists["Solicitudes"];
                SPListItem itmSolicitud;
                itmSolicitud = lstList.GetItemById(Convert.ToInt32(Request["ID"]));
                itmSolicitud["Title"] = txtNombreSolicitud.Text.ToString();
                itmSolicitud["Detalle Solicitud"] = txtComentarios.Text.ToString();
                itmSolicitud.Update();
            }
            else
            {
                bResultado = false;
                lblMensajeError.Text = "El nombre de la solicitud es obligatorio";
                lblMensajeError.Visible = true;
            }
            }

            return bResultado;

        }

        protected void btnReenviarSolicitur_Click(object sender, EventArgs e)
        {
            Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/SolicitudesDiseno/SolicitudReenviar.aspx?ID=" + Request["ID"]);
        }
    }
}
