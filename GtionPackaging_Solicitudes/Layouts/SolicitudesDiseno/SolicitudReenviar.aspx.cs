using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Web.UI.WebControls;
using System.Text;

namespace GtionPackaging_Solicitudes.Layouts.SolicitudesDiseno
{
    public partial class SolictudReenviar : LayoutsPageBase
    {


        Int32 idDocument = 0;
        String strEstadoSolicitud = "";
        Boolean bInicioProceso = false;
        String sTipoSolicitud = "";
        Int32 idSectorAlta = 0;
        String strCargaMasiva = "No";
        String strMotivo = "";
        String strModificacion = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            idDocument = Convert.ToInt32(Request["ID"]);

            // Obtengo el usuario con el que me conecto
            SPUser currentUser = SPContext.Current.Web.CurrentUser;
            Errores.Text = Errores.Text + currentUser.Name.ToString();

            Guid siteId = SPContext.Current.Site.ID;
            Guid webId = SPContext.Current.Web.ID;
            if (Page.IsPostBack != true)
            {
                SPSecurity.RunWithElevatedPrivileges(delegate ()
                {
                    using (SPSite site = new SPSite(siteId))
                    {
                        using (SPWeb web = site.OpenWeb(webId))
                        {
                            SPList lDocumentos = web.Lists["Solicitudes"];
                            SPList lBitacora = web.Lists["Bitácora Solicitudes"];
                            SPListItem itmDocumento = lDocumentos.GetItemById(idDocument);


                            sTipoSolicitud = itmDocumento.ContentType.Name.ToString();
                            txtNombreSolicitud.Text = itmDocumento.Title.ToString();
                            txtTipoDocumento.Text = sTipoSolicitud;
                            if (itmDocumento["Carga masiva"] != null)
                            {
                                if (Convert.ToBoolean(itmDocumento["Carga masiva"].ToString()) == true)
                                {
                                    strCargaMasiva = "Si";
                                    if (itmDocumento["Modificación a Realizar"] != null) { strModificacion = itmDocumento["Modificación a Realizar"].ToString(); }
                                    if (itmDocumento["Motivo de la Modificación"] != null) { strMotivo = itmDocumento["Motivo de la Modificación"].ToString(); }
                                }
                            }

                            txtCargaMasiva.Text = strCargaMasiva;

                        // Cargo la lista de tareas pendientes

                        SPQuery queryDA = new SPQuery();
                        queryDA.Query = string.Concat("<Where><And><Eq><FieldRef Name='Solicitud_x0020_asociada' LookupId='TRUE'/>", "<Value Type='Lookup'>", idDocument, "</Value></Eq><Eq><FieldRef Name='Sector'/><Value Type='Text'>Packaging</Value></Eq></And></Where><OrderBy><FieldRef Name='ID' Ascending='False'/></OrderBy>");
                            //< Where >< And >< Eq >< FieldRef Name = 'Sector' />< Value Type = 'Text' > Packaging </ Value ></ Eq >< Eq >< FieldRef Name = 'Estado' />< Value Type = 'Text' > Pendiente </ Value ></ Eq ></ And ></ Where >

                            Boolean bRechazarSolictud = true;

                        SPListItemCollection itemColl = null;
                        itemColl = lBitacora.GetItems(queryDA);
                        if (itemColl.Count > 0)
                            {
                                foreach (SPListItem itmTarea in itemColl)
                                {
                                    if (itmTarea["Estado"].ToString() == "Pendiente")
                                    {
                                        ListItem itmTareaActiva = new ListItem();
                                        itmTareaActiva.Value = itmTarea.ID.ToString();
                                        itmTareaActiva.Text = itmTarea["Código SAP"].ToString();
                                        cblSeleccionMaterial.Items.Add(itmTareaActiva);
                                    } else
                                    {
                                        bRechazarSolictud = false;
                                    }
                                }
                            }

                        // Cargo la lista de tareas dispoibles a enviar. 
                        SPList lConfiguracionProceso = web.Lists["Configuración Proceso Solicitudes"];
                            SPQuery query = new SPQuery();
                            SPQuery queryConfig = new SPQuery();
                            queryConfig.Query = string.Concat("<Where><Contains><FieldRef Name='Circuito'/><Value Type='LookupMulti'>", txtTipoDocumento.Text, "</Value></Contains></Where>");
                            itemColl = null;
                            itemColl = lConfiguracionProceso.GetItems(queryConfig);

                            ListItem itmTarea0 = new ListItem();
                            itmTarea0.Value = "0";
                            itmTarea0.Text = "<-- Seleccione una tarea -->";
                            ddlSeleccioneTarea.Items.Add(itmTarea0);
                            itmTarea0 = new ListItem();
                            itmTarea0.Value = "S";
                            itmTarea0.Text = "Inicio Solicitud";
                            ddlSeleccioneTarea.Items.Add(itmTarea0);
                            int i = 0;
                            foreach (SPListItem itmTarea in itemColl)
                            {
                                if (itmTarea["Sector"] != null)
                                {
                                    if (itmTarea["Sector"].ToString().Split('#')[1] != "Packaging")
                                    {
                                        ListItem itmTareaActiva = new ListItem();
                                        itmTareaActiva.Value = itmTarea.ID.ToString();
                                        itmTareaActiva.Text = itmTarea.Title.ToString();
                                        ddlSeleccioneTarea.Items.Add(itmTareaActiva);
                                        i = i + 1;
                                    }
                                }
                            }

                            if (bRechazarSolictud ==false)
                            {
                                rblAccion.Items[0].Enabled = false;
                            }
                        }
                    }
                });
            }
        }

        protected void btnVolver_Click(object sender, EventArgs e)
        {
            Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/SolicitudesDiseno/ProcesandoSolicitud.aspx?ID=" + Request["ID"]);
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            int iResultado = 0;
            // Valido que al menos se haya seleccionado una de las opciones de acción
            Boolean bContinuar = true;
            lblErrores.Text = "";

            if (rblAccion.SelectedValue == "")
            {
                bContinuar = false;

            }

            if (bContinuar == true)
            {
                if (rblAccion.SelectedValue == "1" || rblAccion.SelectedValue == "2")
                {
                    if (ddlSeleccioneTarea.SelectedIndex == 0)
                    {
                        bContinuar = false;
                        iResultado = 1;
                    }
                }
            }

            if (bContinuar == true)
            {
                if (txtMensaje.Text.Trim().ToString() == "")
                {
                    bContinuar = false;
                    iResultado = 2;
                }
            }

            if (bContinuar == true)
            {
                if (rblAccion.SelectedValue == "0")
                {
                    idDocument = Convert.ToInt32(Request["ID"]);

                    // Obtengo el usuario con el que me conecto
                    SPUser currentUser = SPContext.Current.Web.CurrentUser;
                    Errores.Text = Errores.Text + currentUser.Name.ToString();

                    Guid siteId = SPContext.Current.Site.ID;
                    Guid webId = SPContext.Current.Web.ID;
                    SPSecurity.RunWithElevatedPrivileges(delegate ()
                        {
                            using (SPSite site = new SPSite(siteId))
                            {
                                using (SPWeb web = site.OpenWeb(webId))
                                {
                                    web.AllowUnsafeUpdates = true;

                                    SPList lDocumentos = web.Lists["Solicitudes"];
                                    SPList lBitacora = web.Lists["Bitácora Solicitudes"];
                                    SPListItem itmDocumento = lDocumentos.GetItemById(idDocument);

                                    // Elimino las tareas pendientes de packaging
                                    StringBuilder batchString = new StringBuilder();
                                    SPQuery queryDA = new SPQuery();
                                    queryDA.Query = string.Concat("<Where><And><Eq><FieldRef Name='Solicitud_x0020_asociada' LookupId='TRUE'/>", "<Value Type='Lookup'>", idDocument, "</Value></Eq><Eq><FieldRef Name='Sector'/><Value Type='Text'>Packaging</Value></Eq></And></Where><OrderBy><FieldRef Name='ID' Ascending='False'/></OrderBy>");
                                    //< Where >< And >< Eq >< FieldRef Name = 'Sector' />< Value Type = 'Text' > Packaging </ Value ></ Eq >< Eq >< FieldRef Name = 'Estado' />< Value Type = 'Text' > Pendiente </ Value ></ Eq ></ And ></ Where >

                                    SPListItemCollection itemColl = null;
                                    itemColl = lBitacora.GetItems(queryDA);
                                    if (itemColl.Count > 0)
                                    {
                                        StringBuilder sbDelete = new StringBuilder();
                                        string xmlFormat = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
                                        sbDelete.Append(xmlFormat);
                                        sbDelete.Append("<Batch>");
                                        string buildQuery = "<Method><SetList Scope=\"Request\">" + lBitacora.ID + "</SetList>";
                                        buildQuery = buildQuery +
                                        "<SetVar Name=\"ID\">{0}</SetVar><SetVar Name=\"Cmd\">Delete</SetVar></Method>";
                                        foreach (SPListItem itemBitacora in itemColl)
                                        {
                                            sbDelete.Append(string.Format(buildQuery, itemBitacora.ID.ToString()));
                                        }
                                        sbDelete.Append("</Batch>");
                                        web.ProcessBatchData(sbDelete.ToString());

                                    }

                                    vGenerarTarea();

                                    // Actualizo el estado de la solicitud y habilito el inicio
                                    
                                    itmDocumento["Estado"] = "Reinicio Pendiente";
                                    itmDocumento["Inicio Proceso"] = "SI";
                                    itmDocumento.Update();
                                }
                            }
                        });

                                    

                                    

                                    


                                }

                if (rblAccion.SelectedValue == "1")
                {

                    idDocument = Convert.ToInt32(Request["ID"]);

                    // Obtengo el usuario con el que me conecto
                    SPUser currentUser = SPContext.Current.Web.CurrentUser;
                    Errores.Text = Errores.Text + currentUser.Name.ToString();

                    Guid siteId = SPContext.Current.Site.ID;
                    Guid webId = SPContext.Current.Web.ID;
                    SPSecurity.RunWithElevatedPrivileges(delegate ()
                    {
                        using (SPSite site = new SPSite(siteId))
                        {
                            using (SPWeb web = site.OpenWeb(webId))
                            {
                                web.AllowUnsafeUpdates = true;

                                SPList lDocumentos = web.Lists["Solicitudes"];
                                SPList lBitacora = web.Lists["Bitácora Solicitudes"];
                                SPListItem itmDocumento = lDocumentos.GetItemById(idDocument);

                                if (ddlSeleccioneTarea.SelectedValue != "S") { 
                                    vGenerarTareaSiguiente(Convert.ToInt32(ddlSeleccioneTarea.SelectedValue.ToString()));
                                    itmDocumento["Estado"] = "En Curso";
                                    itmDocumento.Update();
                                } else
                                {
                                    itmDocumento["Estado"] = "Reinicio Pendiente";
                                    itmDocumento["Inicio Proceso"] = "SI";
                                    itmDocumento.Update();
                                }

                                // Genero tarea de revisión
                                vGenerarTarea();
                                
                            }
                        }
                    });


                }
                if (rblAccion.SelectedValue == "2") {

                    idDocument = Convert.ToInt32(Request["ID"]);

                    // Obtengo el usuario con el que me conecto
                    SPUser currentUser = SPContext.Current.Web.CurrentUser;
                    Errores.Text = Errores.Text + currentUser.Name.ToString();

                    Guid siteId = SPContext.Current.Site.ID;
                    Guid webId = SPContext.Current.Web.ID;
                    SPSecurity.RunWithElevatedPrivileges(delegate ()
                    {
                        using (SPSite site = new SPSite(siteId))
                        {
                            using (SPWeb web = site.OpenWeb(webId))
                            {
                                web.AllowUnsafeUpdates = true;

                                SPList lDocumentos = web.Lists["Solicitudes"];
                                SPList lBitacora = web.Lists["Bitácora Solicitudes"];
                                SPListItem itmDocumento = lDocumentos.GetItemById(idDocument);
                                
                                StringBuilder sbDelete = new StringBuilder();
                                string xmlFormat = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
                                sbDelete.Append(xmlFormat);
                                sbDelete.Append("<Batch>");
                                string buildQuery = "<Method><SetList Scope=\"Request\">" + lBitacora.ID + "</SetList>";
                                buildQuery = buildQuery +
                                "<SetVar Name=\"ID\">{0}</SetVar><SetVar Name=\"Cmd\">Delete</SetVar></Method>";
                                foreach (ListItem itemTarea in cblSeleccionMaterial.Items)
                                {
                                    if (itemTarea.Selected == true) { 
                                        sbDelete.Append(string.Format(buildQuery, itemTarea.Value.ToString()));
                                    }
                                }
                                sbDelete.Append("</Batch>");
                                web.ProcessBatchData(sbDelete.ToString());
                                
                                if (ddlSeleccioneTarea.SelectedValue != "S")
                                {
                                    vGenerarTareaSiguiente(Convert.ToInt32(ddlSeleccioneTarea.SelectedValue.ToString()));
                                    itmDocumento["Estado"] = "En Curso";

                                    itmDocumento.Update();
                                }

                                else
                                {
                                    itmDocumento["Estado"] = "Reinicio Pendiente";
                                    itmDocumento["Inicio Proceso"] = "SI";
                                    itmDocumento.Update();
                                }

                                // Genero tarea de revisión
                                vGenerarTarea();

                            }
                        }
                    });

                }

                Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/SolicitudesDiseno/ProcesandoSolicitud.aspx?ID=" + Request["ID"]);

            } else
            {
                if (iResultado == 0) { 
                    lblErrores.Text = "Se debe seleccionar una acción a ejecutar";
                }
                if (iResultado == 1)
                {
                    lblErrores.Text = "Se debe seleccionar la tarea destino si se selecciona reenviar material";
                }
                if (iResultado == 2)
                {
                    lblErrores.Text = "El mensaje es obligatorio";
                }

            }
        


        }

        private void vGenerarTarea()
        {
            idDocument = Convert.ToInt32(Request["ID"]);

            // Obtengo el usuario con el que me conecto
            SPUser currentUser = SPContext.Current.Web.CurrentUser;
            Errores.Text = Errores.Text + currentUser.Name.ToString();

            Guid siteId = SPContext.Current.Site.ID;
            Guid webId = SPContext.Current.Web.ID;
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                using (SPSite site = new SPSite(siteId))
                {
                    using (SPWeb web = site.OpenWeb(webId))
                    {
                        web.AllowUnsafeUpdates = true;

                        // Genero una tarea para indicar que fue Reiniciado
                        SPList lConfiguracionProceso = web.Lists["Configuración Proceso Solicitudes"];
                        SPList lBitacora = web.Lists["Bitácora Solicitudes"];

                        SPQuery query = new SPQuery();
                        query.Query = "<Where><Eq><FieldRef Name=\"Title\" /><Value Type=\"Text\">Requerir Ajustes en la Solicitud</Value></Eq></Where>";
                        query.RowLimit = 1;
                        query.ViewFields = "";
                        SPListItemCollection items = lConfiguracionProceso.GetItems(query);
                        SPListItem item = items[0];

                        SPListItem itmTareaResumen = lBitacora.AddItem();
                        itmTareaResumen["Title"] = "Requerir Ajustes en la Solicitud - " + rblAccion.SelectedItem.Text.ToString();
                        itmTareaResumen["Solicitud asociada"] = idDocument;
                        itmTareaResumen["Estado"] = "Completado";
                        itmTareaResumen["Procesado"] = "SI";
                        itmTareaResumen["Configuracion Tarea"] = item.ID.ToString();
                        itmTareaResumen["Comentarios"] = txtMensaje.Text.ToString();
                        itmTareaResumen["Fecha de Fin"] = Funciones_Comunes.dtFechaVencimiento(Convert.ToInt32(item["Días Vencimiento"].ToString()));
                        itmTareaResumen["Asignado"] = SPContext.Current.Web.CurrentUser;
                        itmTareaResumen["Mensaje"] = txtMensaje.Text.ToString();
                        itmTareaResumen.Update();
                    }
                }
            });

        }

        private void vGenerarTareaSiguiente(Int32 idTarea)
        {

            idDocument = Convert.ToInt32(Request["ID"]);

            // Obtengo el usuario con el que me conecto
            SPUser currentUser = SPContext.Current.Web.CurrentUser;
            Errores.Text = Errores.Text + currentUser.Name.ToString();

            Guid siteId = SPContext.Current.Site.ID;
            Guid webId = SPContext.Current.Web.ID;
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                using (SPSite site = new SPSite(siteId))
                {
                    using (SPWeb web = site.OpenWeb(webId))
                    {
                        web.AllowUnsafeUpdates = true;

                        // Genero una tarea para indicar que fue Reiniciado
                        SPList lConfiguracionProceso = web.Lists["Configuración Proceso Solicitudes"];
                        SPList lBitacora = web.Lists["Bitácora Solicitudes"];
                        SPList lDocumentos = web.Lists["Solicitudes"];

                        SPListItem itmDocumento = lDocumentos.GetItemById(idDocument);

                        SPListItem itmConfiguracionProcesoSiguiente = lConfiguracionProceso.GetItemById(idTarea);

                        Int32 idSector = Convert.ToInt32(itmConfiguracionProcesoSiguiente["Sector"].ToString().Split(';')[0].ToString());
                        SPList lSectores = web.Lists["Sectores"];
                        SPListItem imSector = lSectores.GetItemById(idSector);


                            // Inicializo la tarea siguiente
                        SPListItem itmTareaBitacora = lBitacora.AddItem();
                        itmTareaBitacora["Title"] = itmConfiguracionProcesoSiguiente.Title.ToString();
                        itmTareaBitacora["Solicitud asociada"] = idDocument;
                        itmTareaBitacora["Iteración"] = 1;
                        itmTareaBitacora["Asignado"] = imSector["Usuarios"];
                        itmTareaBitacora["Configuracion Tarea"] = itmConfiguracionProcesoSiguiente.ID.ToString();
                        itmTareaBitacora["Fecha de Fin"] = Funciones_Comunes.dtFechaVencimiento(Convert.ToInt32(itmConfiguracionProcesoSiguiente["Días Vencimiento"].ToString()));
                        itmTareaBitacora["Tarea Agrupadora"] = itmConfiguracionProcesoSiguiente["Tarea Agrupadora"];
                        itmTareaBitacora["Ver"] = itmDocumento["Ver"].ToString();
                        itmTareaBitacora["Sector"] = itmConfiguracionProcesoSiguiente["Sector"].ToString().Split('#')[1].ToString();
                        itmTareaBitacora["Mensaje"] = txtMensaje.Text.ToString();
                        itmTareaBitacora["Circuito Completo"] = rblCircuito.SelectedValue.ToString();
                        itmTareaBitacora.Update();

                    }
                }
            });


        }
        
        protected void rblAccion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rblAccion.SelectedValue == "0"){
                lblSeleccioneTarea.Visible = false;
                ddlSeleccioneTarea.Visible = false;
                cblSeleccionMaterial.Visible = false;
                rblCircuito.Visible = false;
            }
            if (rblAccion.SelectedValue == "1")
            {
                lblSeleccioneTarea.Visible = true;
                ddlSeleccioneTarea.Visible = true;
                ddlSeleccioneTarea.SelectedIndex  = 0;
                cblSeleccionMaterial.Visible = false;
                rblCircuito.Visible = true;
            }
            if (rblAccion.SelectedValue == "2")
            {
                lblSeleccioneTarea.Visible = true;
                cblSeleccionMaterial.Visible = true;
                ddlSeleccioneTarea.Visible = true;
                rblCircuito.Visible = true;
                ddlSeleccioneTarea.SelectedIndex = 0;
            }
        }
    }
}
