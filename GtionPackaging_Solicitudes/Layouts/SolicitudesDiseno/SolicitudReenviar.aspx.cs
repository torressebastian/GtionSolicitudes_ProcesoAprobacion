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
                            itmTarea0.Text = "Solicitar Nuevo Producto o Material a Reemplazar";
                            ddlSeleccioneTarea.Items.Add(itmTarea0);
                            int i = 0;
                            foreach (SPListItem itmTarea in itemColl)
                            {
                                if (itmTarea["Sector"] != null)
                                {
                                    if (itmTarea["Sector"].ToString().Split('#')[1] == "Desarrollo")
                                    {
                                        ListItem itmTareaActiva = new ListItem();
                                        itmTareaActiva.Value = itmTarea.ID.ToString();
                                        itmTareaActiva.Text = itmTarea["Título Reinicio"].ToString();
                                        ddlSeleccioneTarea.Items.Add(itmTareaActiva);
                                        i = i + 1;
                                    }
                                }
                            }

                            if (bRechazarSolictud ==false)
                            {
                                rblAccion.Items[0].Enabled = false;
                            }

                            if (sTipoSolicitud != "Lanzamiento Nacional" && sTipoSolicitud != "Lanzamiento Internacional")
                            {
                                rblAccion.Items[3].Enabled = false;
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
                if (rblAccion.SelectedValue == "1" )
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
                                    //itmDocumento["Inicio Proceso"] = "SI";
                                    itmDocumento["Requiere Ajustes"] = 1;
                                    itmDocumento.Update();
                                    vGenerarTareaSiguiente("INICIO", "INICIO");
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

                                if (ddlSeleccioneTarea.SelectedValue != "S")
                                {
                                    itmDocumento["Estado"] = "Ajustes En Curso";
                                    itmDocumento["Requiere Ajustes"] = 1;
                                    itmDocumento.Update();
                                    vGenerarTareaSiguiente("Desarrollo", "Desarrollo");

                                }

                                else
                                {
                                    itmDocumento["Estado"] = "Reinicio Pendiente";
                                    //itmDocumento["Inicio Proceso"] = "SI";
                                    itmDocumento["Requiere Ajustes"] = 1;
                                    itmDocumento.Update();
                                    vGenerarTareaSiguiente("INICIO", "INICIO");
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
                                        vActualizarMaterialAjustes(itemTarea.Text.ToString());
                                    }



                                }
                                sbDelete.Append("</Batch>");
                                web.ProcessBatchData(sbDelete.ToString());
                                
                                //if (ddlSeleccioneTarea.SelectedValue != "S")
                                //{
                                    itmDocumento["Estado"] = "Ajustes En Curso";
                                    itmDocumento["Requiere Ajustes"] = 1;
                                    itmDocumento.Update();
                                    vGenerarTareaSiguiente("Desarrollo", "Desarrollo");
                                    
                                //}

                                //else
                                //{
                                //    itmDocumento["Estado"] = "Reinicio Pendiente";
                                //    //itmDocumento["Inicio Proceso"] = "SI";
                                //    itmDocumento["Requiere Ajustes"] = 1;
                                //    itmDocumento.Update();
                                //    vGenerarTareaSiguiente("INICIO", "INICIO");
                                //}

                                // Genero tarea de revisión
                                vGenerarTarea();

                            }
                        }
                    });

                }

                if (rblAccion.SelectedValue == "3")
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

                                StringBuilder sbDelete = new StringBuilder();
                                string xmlFormat = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
                                sbDelete.Append(xmlFormat);
                                sbDelete.Append("<Batch>");
                                string buildQuery = "<Method><SetList Scope=\"Request\">" + lBitacora.ID + "</SetList>";
                                buildQuery = buildQuery +
                                "<SetVar Name=\"ID\">{0}</SetVar><SetVar Name=\"Cmd\">Delete</SetVar></Method>";
                                foreach (ListItem itemTarea in cblSeleccionMaterial.Items)
                                {
                                    if (itemTarea.Selected == true)
                                    {

                                        sbDelete.Append(string.Format(buildQuery, itemTarea.Value.ToString()));
                                        vActualizarMaterialAjustes(itemTarea.Text.ToString());
                                    }



                                }
                                sbDelete.Append("</Batch>");
                                web.ProcessBatchData(sbDelete.ToString());

                                //if (ddlSeleccioneTarea.SelectedValue != "S")
                                //{
                                    itmDocumento["Estado"] = "Ajustes En Curso";
                                    itmDocumento["Requiere Ajustes"] = 1;
                                    itmDocumento.Update();
                                    vGenerarTareaSiguiente("Registro", "Registro Internacional");

                                //}

                                //else
                                //{
                                 //   itmDocumento["Estado"] = "Reinicio Pendiente";
                                    //itmDocumento["Inicio Proceso"] = "SI";
                                //    itmDocumento["Requiere Ajustes"] = 1;
                                //    itmDocumento.Update();
                                //    vGenerarTareaSiguiente(0);
                                //}

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
                        String strMensaje = "";
                        String strTarea = "";

                        if (rblAccion.SelectedValue == "1") { 
                            if (ddlSeleccioneTarea.SelectedValue != "S")
                            {
                                Int32 idTarea = Convert.ToInt32(ddlSeleccioneTarea.SelectedValue.ToString());
                                SPListItem itmConfiguracionProcesoSiguiente = lConfiguracionProceso.GetItemById(idTarea);

                                strTarea = " Realizar Ajustes - " + itmConfiguracionProcesoSiguiente["Sector"].ToString().Split('#')[1].ToString();



                            } else
                            {
                                strTarea = ddlSeleccioneTarea.SelectedItem.Text.ToString();
                            }
                        }
                        strMensaje = txtMensaje.Text.ToString();

                        if (rblAccion.SelectedValue == "0")
                        {
                            strMensaje = strMensaje + " - Tarea Destino: Reiniciar la solicitud.";
                        }
                        if (rblAccion.SelectedValue == "1")
                        {
                            strMensaje = strMensaje + " - Tarea Destino: Reiniciar la solicitud.";


                        }
                        if (rblAccion.SelectedValue == "2")
                        {
                            strMensaje = strMensaje + " - Materiales: ";
                            foreach (ListItem itemTarea in cblSeleccionMaterial.Items)
                            {
                                if (itemTarea.Selected == true)
                                {
                                    strMensaje = strMensaje + itemTarea.Text.ToString() + "; ";
                                }
                            }
                            strMensaje = strMensaje + " - Tarea Destino: Desarrollo";
                        }

                        if (rblAccion.SelectedValue == "3")
                        {
                            strMensaje = strMensaje + " - Materiales: ";
                            foreach (ListItem itemTarea in cblSeleccionMaterial.Items)
                            {
                                if (itemTarea.Selected == true)
                                {
                                    strMensaje = strMensaje + itemTarea.Text.ToString() + "; ";
                                }
                            }
                            strMensaje = strMensaje + " - Tarea Destino: Cargar Artes" ;
                        }

                        SPQuery query = new SPQuery();
                        query.Query = "<Where><Eq><FieldRef Name=\"Title\" /><Value Type=\"Text\">Ajustes en la Solicitud</Value></Eq></Where>";
                        query.RowLimit = 1;
                        query.ViewFields = "";
                        SPListItemCollection items = lConfiguracionProceso.GetItems(query);
                        SPListItem item = items[0];

                        SPListItem itmTareaResumen = lBitacora.AddItem();
                        itmTareaResumen["Title"] = "Ajustes en la Solicitud - " + rblAccion.SelectedItem.Text.ToString();
                        itmTareaResumen["Solicitud asociada"] = idDocument;
                        itmTareaResumen["Estado"] = "Completado";
                        itmTareaResumen["Procesado"] = "SI";
                        itmTareaResumen["Configuracion Tarea"] = item.ID.ToString();
                        itmTareaResumen["Comentarios"] = txtMensaje.Text.ToString();
                        itmTareaResumen["Fecha de Fin"] = Funciones_Comunes.dtFechaVencimiento(Convert.ToInt32(item["Días Vencimiento"].ToString()));
                        itmTareaResumen["Asignado"] = SPContext.Current.Web.CurrentUser;
                        itmTareaResumen["Mensaje"] = strMensaje;
                        itmTareaResumen.Update();
                    }
                }
            });

        }

        private void vGenerarTareaSiguiente(String strSectorNac, String strSectorInt)
        {

            idDocument = Convert.ToInt32(Request["ID"]);
            Int32 idTarea = 0;

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
                        SPList lSectores = web.Lists["Sectores"];

                        SPListItem itmDocumento = lDocumentos.GetItemById(idDocument);
                        sTipoSolicitud = itmDocumento.ContentType.Name.ToString();


                        // Busco la tarea siguiente de acuerdo al sector
                        if (strSectorNac != "INICIO"){ 
                            SPQuery query = new SPQuery();
                            SPQuery queryConfig = new SPQuery();
                            queryConfig.Query = string.Concat("<Where><Contains><FieldRef Name='Circuito'/><Value Type='LookupMulti'>", txtTipoDocumento.Text, "</Value></Contains></Where>");
                            SPListItemCollection itemColl = null;
                            itemColl = lConfiguracionProceso.GetItems(queryConfig);

                            foreach (SPListItem itmTarea in itemColl)
                            {
                                if (itmTarea["Sector"] != null)
                                {
                                    if (itmTarea["Sector"].ToString().Split('#')[1] == strSectorNac || itmTarea["Sector"].ToString().Split('#')[1] == strSectorInt)
                                    {

                                        if (strSectorNac == "Registro") // Si es Registro, busco la tarea arte
                                        {
                                            if (itmTarea["Adjunto Obligatorio"] is null)
                                            {
                                            
                                            }
                                            else
                                            {

                                                if (itmTarea["Adjunto Obligatorio"].ToString() == "True")
                                                {
                                                    idTarea = itmTarea.ID;
                                                }
                                                
                                            }
                                        } else
                                        {
                                            idTarea = itmTarea.ID;
                                        }

                                        
                                    }
                                }
                            }
                        }
                        
                        SPListItem itmConfiguracionProcesoSiguiente;
                        SPListItem imSector;
                        Int32 idSector;

                        if (idTarea != 0) { 
                            itmConfiguracionProcesoSiguiente = lConfiguracionProceso.GetItemById(idTarea);
                        } else
                        {
                            SPQuery query = new SPQuery();
                            query.Query = "<Where><Eq><FieldRef Name=\"Title\" /><Value Type=\"Text\">Reiniciar Solicitud</Value></Eq></Where>";
                            query.RowLimit = 1;
                            query.ViewFields = "";
                            SPListItemCollection items = lConfiguracionProceso.GetItems(query);
                            itmConfiguracionProcesoSiguiente = items[0];
                        }

                        if (idTarea != 0)
                        {
                            idSector = Convert.ToInt32(itmConfiguracionProcesoSiguiente["Sector"].ToString().Split(';')[0].ToString());
                        }
                        else
                        {
                            SPList lConfiguracionSolicitudes = SPContext.Current.Web.Lists["Configuración Circuitos Solicitudes"];
                            SPQuery query = new SPQuery();
                            query.Query = "<Where><Eq><FieldRef Name=\"Title\" /><Value Type=\"Text\">" + sTipoSolicitud + "</Value></Eq></Where>";
                            query.RowLimit = 1;
                            query.ViewFields = "";
                            SPListItemCollection items = lConfiguracionSolicitudes.GetItems(query);
                            SPListItem item = items[0];
                            SPFieldLookupValueCollection lkSectorAlta = new SPFieldLookupValueCollection(item["Sector alta"].ToString());
                            idSector = lkSectorAlta[0].LookupId;
                        }
                        
                        imSector = lSectores.GetItemById(idSector);

                        String strMensaje = txtMensaje.Text.ToString();
                        if (rblAccion.SelectedValue == "2")
                        {
                            strMensaje = strMensaje + " - Materiales: ";
                            foreach (ListItem itemTarea in cblSeleccionMaterial.Items)
                            {
                                if (itemTarea.Selected == true)
                                {
                                    strMensaje = strMensaje + itemTarea.Text.ToString() + "; ";
                                }
                            }
                            //strMensaje = strMensaje + " - Tarea Destino: " + ddlSeleccioneTarea.SelectedItem.Text.ToString();
                        }

                        if (rblAccion.SelectedValue == "3")
                        {
                            strMensaje = strMensaje + " - Materiales: ";
                            foreach (ListItem itemTarea in cblSeleccionMaterial.Items)
                            {
                                if (itemTarea.Selected == true)
                                {
                                    strMensaje = strMensaje + itemTarea.Text.ToString() + "; ";
                                }
                            }
                            //strMensaje = strMensaje + " - Tarea Destino: " + ddlSeleccioneTarea.SelectedItem.Text.ToString();
                        }


                        // Inicializo la tarea siguiente
                        SPListItem itmTareaBitacora = lBitacora.AddItem();

                        itmTareaBitacora["Title"] = itmConfiguracionProcesoSiguiente["Título Reinicio"];
                        itmTareaBitacora["Solicitud asociada"] = idDocument;
                        itmTareaBitacora["Iteración"] = 1;
                        itmTareaBitacora["Asignado"] = imSector["Usuarios"];
                        itmTareaBitacora["Configuracion Tarea"] = itmConfiguracionProcesoSiguiente.ID.ToString();
                        itmTareaBitacora["Fecha de Fin"] = Funciones_Comunes.dtFechaVencimiento(Convert.ToInt32(itmConfiguracionProcesoSiguiente["Días Vencimiento"].ToString()));
                        itmTareaBitacora["Tarea Agrupadora"] = itmConfiguracionProcesoSiguiente["Tarea Agrupadora"];
                        itmTareaBitacora["Ver"] = itmDocumento["Ver"].ToString();
                        itmTareaBitacora["Sector"] = imSector.Title.ToString();
                        itmTareaBitacora["Mensaje"] = strMensaje;
                        if (idTarea != 0) { 
                            itmTareaBitacora["Circuito Completo"] = "NO";
                        }
                        else
                        {
                            itmTareaBitacora["Circuito Completo"] = "SI";
                        }
                        itmTareaBitacora.Update();

                    }
                }
            });


        }

        private void vActualizarMaterialAjustes(String strCodigoSAP)
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

                        SPQuery qryMateriales = new SPQuery();
                        SPList lProductosMateriales = web.Lists["Solicitud - Producto Material"];
                        String strQuery = "";
                        strQuery = "<Where><And><Eq><FieldRef Name='Solicitud' LookupId='TRUE'/><Value Type='Lookup'>" + idDocument.ToString() + "</Value></Eq><Contains><FieldRef Name='C_x00f3_digo_x0020_SAP' /><Value Type='Text'>" + strCodigoSAP + "</Value></Contains></And></Where>";
                        qryMateriales.Query = strQuery;

                        SPListItemCollection lstProductosMateriales = lProductosMateriales.GetItems(qryMateriales);
                        if (lstProductosMateriales.Count != 0)
                        {
                            foreach (SPListItem itmProductoMaterial in lstProductosMateriales)
                            {
                                
                                itmProductoMaterial["Requiere Ajustes"] = 1;
                                itmProductoMaterial.Update();
                            }
                        }


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
                rblCircuito.Visible = false;
            }
            if (rblAccion.SelectedValue == "2")
            {
                lblSeleccioneTarea.Visible = false;
                ddlSeleccioneTarea.Visible = false;
                rblCircuito.Visible = false;
                cblSeleccionMaterial.Visible = true;
                ddlSeleccioneTarea.SelectedIndex = 0;
            }
            if (rblAccion.SelectedValue == "3")
            {
                lblSeleccioneTarea.Visible = false;
                cblSeleccionMaterial.Visible = true;
                ddlSeleccioneTarea.Visible = false;
                rblCircuito.Visible = false;
                ddlSeleccioneTarea.SelectedIndex = 0;
            }
        }
    }
}
