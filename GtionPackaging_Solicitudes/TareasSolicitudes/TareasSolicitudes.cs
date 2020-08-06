using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;
using System.Collections;
using System.DirectoryServices.AccountManagement;
using System.Collections.Specialized;
using System.Text;

namespace SolicitudesDiseno_Solicitudes.TareasSolicitudes
{
    /// <summary>
    /// List Item Events
    /// </summary>
    public class TareasSolicitudes : SPItemEventReceiver
    {
        public override void ItemUpdated(SPItemEventProperties properties)
        {
            Boolean bProcesar = false;
            if (properties.List.Title == "Bitácora Solicitudes")
            {
                String sEstado = properties.ListItem["Estado"].ToString();
                String sProcesado = properties.ListItem["Procesado"].ToString();
                if (sEstado != "Pendiente" && sProcesado == "NO")
                {
                    bProcesar = bProcesarTarea(properties);
                    if (bProcesar == true)
                    {
                        properties.ListItem["Procesado"] = "SI";
                        properties.ListItem["Fecha de Fin"] = DateTime.Now;
                        properties.ListItem.UpdateOverwriteVersion();
                    }
                }

            }



        }

        public Boolean bProcesarTarea(SPItemEventProperties properties)
        {

            String sEstado = properties.ListItem["Estado"].ToString();
            String sRevisorAsignado = "";
            String sCircuitoCompleto = "";

            Int32 iConfiguracionProceso = 0;
            Int32 iTareaSiguiente = 0;
            Int32 iSolicitudActual = 0;
            Int32 iIteracion = 0;

            String strVer;

            if (properties.ListItem["Configuracion Tarea"] is null) { iConfiguracionProceso = 0; } else { iConfiguracionProceso = Convert.ToInt32(properties.ListItem["Configuracion Tarea"].ToString().Split(';')[0]); };
            if (properties.ListItem["Solicitud asociada"] is null) { iSolicitudActual = 0; } else { iSolicitudActual = Convert.ToInt32(properties.ListItem["Solicitud asociada"].ToString().Split(';')[0]); };
            if (properties.ListItem["Iteración"] is null) { iIteracion = 0; } else { iIteracion = Convert.ToInt32(properties.ListItem["Iteración"].ToString()); }
            if (properties.ListItem["Ver"] is null) { strVer = ""; } else { strVer = properties.ListItem["Ver"].ToString(); }
            if (properties.ListItem["Circuito Completo"] is null) { sCircuitoCompleto = ""; } else { sCircuitoCompleto = properties.ListItem["Circuito Completo"].ToString(); }



            SPList lConfiguracionProceso = properties.Web.Lists["Configuración Proceso Solicitudes"];
            SPList lBitacora = properties.Web.Lists["Bitácora Solicitudes"];
            SPListItem itmConfiguracionProceso = lConfiguracionProceso.GetItemById(iConfiguracionProceso);


            if (sEstado == "Completado")
            {
                
                if (sCircuitoCompleto == "SI") { 
                    iTareaSiguiente = iTarea(iConfiguracionProceso, iSolicitudActual, properties);
                } else {
                    iTareaSiguiente = iTareaPackaging(iConfiguracionProceso, iSolicitudActual, properties);
                }


                if (iTareaSiguiente != 0)
                {
                    SPListItem itmConfiguracionProcesoSiguiente = lConfiguracionProceso.GetItemById(iTareaSiguiente);

                    Int32 idSector = Convert.ToInt32(itmConfiguracionProcesoSiguiente["Sector"].ToString().Split(';')[0].ToString());
                    SPList lSectores = properties.Web.Lists["Sectores"];
                    SPListItem imSector = lSectores.GetItemById(idSector);

                    if (Convert.ToBoolean(itmConfiguracionProcesoSiguiente["Desgloce tarea"].ToString()) == false) {

                        // Inicializo la tarea siguiente
                        SPListItem itmTareaBitacora = lBitacora.AddItem();
                        itmTareaBitacora["Title"] = itmConfiguracionProcesoSiguiente.Title.ToString();
                        itmTareaBitacora["Solicitud asociada"] = iSolicitudActual;
                        itmTareaBitacora["Iteración"] = iIteracion;
                        itmTareaBitacora["Asignado"] = imSector["Usuarios"];
                        itmTareaBitacora["Configuracion Tarea"] = itmConfiguracionProcesoSiguiente.ID.ToString();
                        itmTareaBitacora["Fecha de Fin"] = dtFechaVencimiento(Convert.ToInt32(itmConfiguracionProcesoSiguiente["Días Vencimiento"].ToString()));
                        itmTareaBitacora["Tarea Agrupadora"] = itmConfiguracionProcesoSiguiente["Tarea Agrupadora"];
                        itmTareaBitacora["Ver"] = strVer;
                        itmTareaBitacora["Sector"] = itmConfiguracionProcesoSiguiente["Sector"].ToString().Split('#')[1].ToString();
                        itmTareaBitacora.Update();
                    }
                    else {
                        SPQuery qryMateriales = new SPQuery();
                        SPList lProductosMateriales = properties.Web.Lists["Solicitud - Producto Material"];
                        String strQuery = "";
                        strQuery = "<Where><Eq><FieldRef Name='Solicitud' LookupId='TRUE' /><Value Type='Lookup'>" + iSolicitudActual.ToString() + "</Value></Eq></Where>";
                        qryMateriales.Query = strQuery;
                        SPListItemCollection lstProductosMateriales = lProductosMateriales.GetItems(qryMateriales);

                        foreach(SPListItem itmProductoMaterial in lstProductosMateriales) {

                            SPListItem itmProductoMaterialAux = lProductosMateriales.GetItemById(itmProductoMaterial.ID);

                            Boolean bProceso = true;



                            if (itmProductoMaterialAux["Código de Diseño"] != null) { 
                            if (itmProductoMaterialAux["Código SAP"].ToString() != itmProductoMaterialAux["Código de Diseño"].ToString())
                            {
                                    // Si el código SAP Difiere del Código de Material, busco si existe SAP con igual Código de Diseño para la Solicitud, si existe, no proceso la tarea.
                                    SPQuery qryMaterialesDiseno = new SPQuery();
                                    SPList lProductosMaterialesDiseno = properties.Web.Lists["Solicitud - Producto Material"];
                                    String strQueryDiseno = "";
                                    strQueryDiseno = "<Where><And><Eq><FieldRef Name='Solicitud' LookupId='TRUE' /><Value Type='Lookup'>" + iSolicitudActual.ToString() + "</Value></Eq><Eq><FieldRef Name='C_x00f3_digo_x0020_SAP' /><Value Type='Text'>" + itmProductoMaterialAux["Código de Diseño"].ToString() + "</Value></Eq></And></Where>";
                                    qryMaterialesDiseno.Query = strQueryDiseno;
                                    SPListItemCollection lstProductosMaterialesDiseno = lProductosMaterialesDiseno.GetItems(qryMaterialesDiseno);
                                    if (lstProductosMaterialesDiseno.Count >= 1)
                                    {
                                        bProceso = false;
                                    }
                                }
                            }

                            if (bProceso == true)
                            {
                                SPQuery qryMaterialesDiseno = new SPQuery();
                                SPList lProductosMaterialesDiseno = properties.Web.Lists["Bitácora Solicitudes"];
                                String strQueryDiseno = "";
                                strQueryDiseno = "<Where><And><Eq><FieldRef Name='Solicitud_x0020_asociada' LookupId='TRUE' /><Value Type='Lookup'>" + iSolicitudActual.ToString() + "</Value></Eq><Eq><FieldRef Name='C_x00f3_digo_x0020_SAP' /><Value Type='Text'>" + itmProductoMaterialAux["Código SAP"].ToString() + "</Value></Eq></And></Where>";
                                qryMaterialesDiseno.Query = strQueryDiseno;
                                SPListItemCollection lstProductosMaterialesDiseno = lProductosMaterialesDiseno.GetItems(qryMaterialesDiseno);
                                if (lstProductosMaterialesDiseno.Count >= 1)
                                {
                                    bProceso = false;
                                }
                            }


                            if (bProceso == true) { 
                                SPListItem itmTareaBitacora = lBitacora.AddItem();
                                itmTareaBitacora["Title"] = itmProductoMaterialAux["Código SAP"].ToString() + " - " + itmConfiguracionProcesoSiguiente.Title.ToString();
                                itmTareaBitacora["Solicitud asociada"] = iSolicitudActual;
                                itmTareaBitacora["Iteración"] = iIteracion;
                                itmTareaBitacora["Asignado"] = imSector["Usuarios"];
                                itmTareaBitacora["Configuracion Tarea"] = itmConfiguracionProcesoSiguiente.ID.ToString();
                                itmTareaBitacora["Fecha de Fin"] = dtFechaVencimiento(Convert.ToInt32(itmConfiguracionProcesoSiguiente["Días Vencimiento"].ToString()));
                                itmTareaBitacora["Tarea Agrupadora"] = itmConfiguracionProcesoSiguiente["Tarea Agrupadora"];
                                itmTareaBitacora["Ver"] = strVer;
                                itmTareaBitacora["Código SAP"] = itmProductoMaterialAux["Código SAP"].ToString();
                                itmTareaBitacora["Sector"] = itmConfiguracionProcesoSiguiente["Sector"].ToString().Split('#')[1].ToString();
                                itmTareaBitacora.Update();
                            }
                        }



                    }

                    if (Convert.ToBoolean(itmConfiguracionProcesoSiguiente["Tarea cierre circuito"].ToString()) == true)
                    {
                        SPList lDocumentos = properties.Web.Lists["Solicitudes"];
                        SPListItem itmDocumento = lDocumentos.GetItemById(iSolicitudActual);
                        itmDocumento["Sector actual"] = "Packaging";
                        itmDocumento["Estado"] = "Pendiente Inicio Packaging";
                        itmDocumento.Update();
                    }
                    else {
                        SPList lDocumentos = properties.Web.Lists["Solicitudes"];
                        SPListItem itmDocumento = lDocumentos.GetItemById(iSolicitudActual);
                        itmDocumento["Sector actual"] = strSectorPendiente(iSolicitudActual, properties);
                        itmDocumento["Estado"] = "En Curso";
                        itmDocumento.Update();
                    }

                }
                else
                {
                    if (itmConfiguracionProceso["Tarea cierre circuito"] is null)
                    {
                        SPList lDocumentos = properties.Web.Lists["Solicitudes"];
                        SPListItem itmDocumento = lDocumentos.GetItemById(iSolicitudActual);
                        itmDocumento["Sector actual"] = strSectorPendiente(iSolicitudActual, properties);
                        //itmDocumento["Estado"] = "En Curso";
                        itmDocumento.Update();
                    }
                    else
                    {
                        if (Convert.ToBoolean(itmConfiguracionProceso["Tarea cierre circuito"].ToString()) == false)
                        {
                            SPList lDocumentos = properties.Web.Lists["Solicitudes"];
                            SPListItem itmDocumento = lDocumentos.GetItemById(iSolicitudActual);
                            itmDocumento["Sector actual"] = strSectorPendiente(iSolicitudActual, properties);
                            //itmDocumento["Estado"] = "En Curso";
                            itmDocumento.Update();
                        }
                        else
                        {
                            // Valido que no haya tareas pendientes y doy por cerrada la solicitud

                            SPQuery queryDA = new SPQuery();
                            queryDA.Query = string.Concat("<Where><And><Eq><FieldRef Name='Solicitud_x0020_asociada' LookupId='TRUE'/>", "<Value Type='Lookup'>", iSolicitudActual, "</Value></Eq><Eq><FieldRef Name='Estado'/><Value Type='Text'>Pendiente</Value></Eq></And></Where>");
                            SPListItemCollection itemColl = null;
                            itemColl = lBitacora.GetItems(queryDA);
                            
                            if (itemColl.Count == 0) { 
                                SPList lDocumentos = properties.Web.Lists["Solicitudes"];
                                SPListItem itmDocumento = lDocumentos.GetItemById(iSolicitudActual);
                                itmDocumento["Estado"] = "Completado";
                                itmDocumento["Fecha Fin Solicitud"] = DateTime.Now;
                                itmDocumento.Update();
                            }
                            // Busco el Documento para Publicarlo
                        }

                    }
                }


            }
            

            return true;
        }

        public Int32 iTarea(Int32 iConfiguracionProceso, Int32 iDocumento, SPItemEventProperties properties)
        {

            Int32 iTareaSiguiente = 0;
            Int32 iTareaAgrupadora = 0;
            Boolean bTareaSiguiente = false;
            Boolean bTareaAgrupadora = false;
            SPList lDocumentos = properties.Web.Lists["Solicitudes"];
            SPListItem itmDocumento = lDocumentos.GetItemById(iDocumento);
            String strCircuito = "";
            SPList lConfiguracionProceso = properties.Web.Lists["Configuración Proceso Solicitudes"];
            SPList lBitacora = properties.Web.Lists["Bitácora Solicitudes"];
            SPListItem itmConfiguracionProceso = lConfiguracionProceso.GetItemById(iConfiguracionProceso);
            SPListItem itmConfiguracionProcesoSiguiente;
            SPFieldLookupValueCollection itmCircuitos;

            strCircuito = itmDocumento.ContentType.Name.ToString();

            if (itmConfiguracionProceso["Tarea siguiente"].ToString() == "") { iTareaSiguiente = 0; } else { iTareaSiguiente = Convert.ToInt32(itmConfiguracionProceso["Tarea siguiente"].ToString().Split(';')[0]); };
            if (itmConfiguracionProceso["Tarea Agrupadora"] is null) { iTareaAgrupadora = 0; } else { iTareaAgrupadora = Convert.ToInt32(itmConfiguracionProceso["Tarea Agrupadora"].ToString().Split(';')[0]); };


            if (iTareaSiguiente != 0)
            {
                if (iTareaSiguiente == iTareaAgrupadora) { 
                

                    SPQuery queryDA = new SPQuery();
                    queryDA.Query = string.Concat("<Where><And><And><Eq><FieldRef Name='Solicitud_x0020_asociada' LookupId='TRUE'/>", "<Value Type='Lookup'>", iDocumento, "</Value></Eq><Eq><FieldRef Name='Estado'/>", "<Value Type='String'>Pendiente</Value></Eq></And><Eq><FieldRef Name='Tarea_x0020_Agrupadora' LookupId='TRUE'/>", "<Value Type='Lookup'>", iTareaAgrupadora, "</Value></Eq></And></Where><OrderBy><FieldRef Name='ID' Ascending='False'/></OrderBy>");
                    SPListItemCollection itemColl = null;
                    itemColl = lBitacora.GetItems(queryDA);
                    if (itemColl.Count != 0)
                    {
                        iTareaSiguiente = 0;
                        bTareaAgrupadora = true;
                    }


                    
                }

                if (bTareaAgrupadora == false)
                {
                    bTareaSiguiente = false;
                    while (bTareaSiguiente == false && iTareaSiguiente != 0)
                    {

                        itmConfiguracionProcesoSiguiente = lConfiguracionProceso.GetItemById(iTareaSiguiente);
                        itmCircuitos = new SPFieldLookupValueCollection(itmConfiguracionProcesoSiguiente["Circuito"].ToString());
                        foreach (SPFieldLookupValue value in itmCircuitos)
                        {
                            if (value.LookupValue == strCircuito)
                            {
                                bTareaSiguiente = true;
                            }
                        }

                        if (bTareaSiguiente == false)
                        {
                            if (itmConfiguracionProcesoSiguiente["Tarea siguiente"] is null) { iTareaSiguiente = 0; } else { iTareaSiguiente = Convert.ToInt32(itmConfiguracionProcesoSiguiente["Tarea siguiente"].ToString().Split(';')[0]); };
                        }
                    }
                }
            }
            return iTareaSiguiente;
        }

        public Int32 iTareaPackaging(Int32 iConfiguracionProceso, Int32 iDocumento, SPItemEventProperties properties)
        {

            Int32 iTareaSiguiente = 0;
            SPList lDocumentos = properties.Web.Lists["Solicitudes"];
            SPListItem itmDocumento = lDocumentos.GetItemById(iDocumento);
            String strCircuito = "";
            SPList lConfiguracionProceso = properties.Web.Lists["Configuración Proceso Solicitudes"];
            SPListItem itmConfiguracionProceso = lConfiguracionProceso.GetItemById(iConfiguracionProceso);


            strCircuito = itmDocumento.ContentType.Name.ToString();

            SPQuery queryConfig = new SPQuery();
            queryConfig.Query = string.Concat("<Where><And><Eq><FieldRef Name='Tarea_x0020_cierre_x0020_circuit' /><Value Type='Boolean'>1</Value></Eq><Contains><FieldRef Name='Circuito'/><Value Type='LookupMulti'>", strCircuito, "</Value></Contains></And></Where>");
            SPListItemCollection itemColl = null;
            itemColl = lConfiguracionProceso.GetItems(queryConfig);

            if (itemColl.Count > 0)
            {
                iTareaSiguiente = itemColl[0].ID;
            }
                    return iTareaSiguiente;
        }

        /// <summary>
        /// An item was added
        /// </summary>
        public override void ItemAdded(SPItemEventProperties properties)
        {

            if (properties.List.Title == "Solicitudes")
            {
                properties.ListItem["Ver"] = @"<p><a href='/SolicitudesDiseno/_layouts/15/SolicitudesDiseno/AprobacionSolicitud.aspx?ID=" + properties.ListItemId.ToString() + "&Origen=T'><img alt='Ver' src='/SolicitudesDiseno/Imagenes/Dashboard.png' style = 'margin: 0px; width: 16px; '/></a></p>";
                properties.ListItem["Estado"] = "No Iniciada";
                properties.ListItem.SystemUpdate();
            }

            if (properties.List.Title == "Procesos Solicitudes Packaging") {
                if (properties.ListItem.Title == "Inicio Proceso Diseño Packaging") {
                    ProcesarProcesoPackaging(properties);

                }

            }

            if (properties.List.Title == "Bitácora Solicitudes")
            {
                Int32 iConfiguracionProceso;
                SPList lConfiguracionProceso = properties.Web.Lists["Configuración Proceso Solicitudes"];
                if (properties.ListItem["Configuracion Tarea"] is null) { iConfiguracionProceso = 0; } else { iConfiguracionProceso = Convert.ToInt32(properties.ListItem["Configuracion Tarea"].ToString().Split(';')[0]); };
                SPListItem itmConfiguracionProceso = lConfiguracionProceso.GetItemById(iConfiguracionProceso);

                string strSolicitudAsociada, strIdSolicitudAsociada, strTipoCircuito;
                strSolicitudAsociada = properties.ListItem["Solicitud asociada"].ToString().Split('#')[1].ToString();
                strIdSolicitudAsociada = properties.ListItem["Solicitud asociada"].ToString().Split(';')[0].ToString();
                SPList lSolicitud = properties.Web.Lists["Solicitudes"];
                SPListItem itmDocumento = lSolicitud.GetItemById(Convert.ToInt32(strIdSolicitudAsociada));

                strTipoCircuito = itmDocumento.ContentType.Name.ToString();


                if (itmConfiguracionProceso["Tarea Resumen"].ToString() == "False")
                {

                    StringBuilder strCuerpoAnuncio = new StringBuilder();
                    String strCabeceraMail = "";
                    strCuerpoAnuncio = strCuerpoAnuncio.Append("</tr>");
                    string strResponsable = "";
                    string strCopiaMail = "";
                    string strMensajeReinicio = "";

                    string strLinkPaginaTarea = properties.WebUrl + "/_layouts/15/SolicitudesDiseno/AprobacionSolicitud.aspx?ID=" + strIdSolicitudAsociada + "&Origen=T";
                    if (itmConfiguracionProceso["Tarea Reinicio"].ToString() == "False")
                    {
                        strCabeceraMail = "Se le ha asignado la tarea " + properties.ListItem.Title.ToString() + " para la solicitud " + itmDocumento.Title.ToString() + ".";
                    }
                    else {
                        strCabeceraMail = "Se ha requerido " + properties.ListItem.Title.ToString() + " para la solicitud " + itmDocumento.Title.ToString() + ".";
                    }
                    strCuerpoAnuncio = strCuerpoAnuncio.Append("<b>Circuito:</b> " + strTipoCircuito + "<br /><br />");

                    if (properties.ListItem["Mensaje"] != null)
                    {
                        if (properties.ListItem["Mensaje"].ToString() != "")
                        {
                            strCuerpoAnuncio = strCuerpoAnuncio.Append("<b>Mensaje adicional:</b> " + properties.ListItem["Mensaje"].ToString() + "<br /><br />");
                        }
                    }

                    if (itmConfiguracionProceso["Tarea Reinicio"].ToString() == "False") { 
                        strCuerpoAnuncio = strCuerpoAnuncio.Append("<b>Fecha de Vencimiento de la tarea:</b> " + Convert.ToDateTime(properties.ListItem["Fecha de Fin"].ToString()).ToShortDateString() + "<br /><br />");
                        strCuerpoAnuncio = strCuerpoAnuncio.Append("Para continuar con el proceso, ingrese a la tarea para completarla: " + @"<a href='" + strLinkPaginaTarea + "'>" + properties.ListItem.Title.ToString() + "</a><br/>");
                    }

                    string fieldValue = properties.ListItem["Asignado"].ToString();
                    if (itmConfiguracionProceso["Tarea Reinicio"].ToString() == "True")
                    {
                        fieldValue = itmDocumento["Author"].ToString();
                    }
                    SPFieldUserValueCollection users = new SPFieldUserValueCollection(properties.ListItem.Web, fieldValue);

                    foreach (SPFieldUserValue uv in users)
                    {
                        if (uv.User != null)
                        {
                            SPUser user = uv.User;
                            strResponsable = strResponsable + user.Email.ToString() + ";";
                        }
                        else
                        {
                            SPGroup sGroup = properties.Web.Groups[uv.LookupValue];
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
                    headers.Add("from", properties.Web.Title.ToString() + "<sharepoint@baliarda.com.ar>");
                    if (strCopiaMail != "") { headers.Add("cc", strCopiaMail); }
                    headers.Add("subject", properties.ListItem.Title.ToString() + " - " + strTipoCircuito + " - " + strSolicitudAsociada);
                    headers.Add("content-type", "text/html");
                    SPUtility.SendEmail(properties.Web, headers, strCabeceraMail + "<br /><br />" + strCuerpoAnuncio.ToString() + emailBody);
                    emailBody = "";

                    

                }


            }
        }

        public static void ProcesarProcesoPackaging(SPItemEventProperties properties) {
            try
            {
                SPList lBitacora = properties.Web.Lists["Bitácora Solicitudes"];
                SPList lProductos = properties.Web.Lists["Solicitud - Producto Material"];

                String strCodigoSAP = "";
                Int32 idSolicitud = 0;
                if (properties.ListItem["Código SAP"] != null) { strCodigoSAP = properties.ListItem["Código SAP"].ToString(); }
                
                SPQuery qryMateriales = new SPQuery();
                SPList lProductosMateriales = properties.Web.Lists["Solicitud - Producto Material"];
                String strQuery = "";
                String strDiseno = "";
                strQuery = "<Where><Contains><FieldRef Name='C_x00f3_digo_x0020_SAP' /><Value Type='Text'>" + strCodigoSAP + "</Value></Contains></Where>";
                qryMateriales.Query = strQuery;

                SPListItemCollection lstProductosMateriales = lProductosMateriales.GetItems(qryMateriales);
                if (lstProductosMateriales.Count != 0)
                {
                    foreach (SPListItem itmProductoMaterial in lstProductosMateriales)
                    {
                        idSolicitud = Convert.ToInt32(itmProductoMaterial["Solicitud"].ToString().Split(';')[0].ToString());
                        itmProductoMaterial["Estado"] = "Iniciado";
                        itmProductoMaterial["Fecha de Fin"] = DateTime.Now;
                        itmProductoMaterial.Update();
                        strDiseno = itmProductoMaterial["Código de Diseño"].ToString();
                        AgregarEvento(properties, "Código SAP", strCodigoSAP);
                        AgregarEvento(properties, "Código Diseño", strDiseno);

                    // Busco los códigos SAP que se correspondan con el mismo Código de Diseño
                    // Por cada código SAP diferente, cargo un nuevo registro en la lista de relación
                    SPQuery qryCodigoDiseno = new SPQuery();
                    SPList lProductosMaterialesDiseno = properties.Web.Lists["Solicitud - Producto Material"];
                    String strQueryDiseno = "";
                    strQueryDiseno = "<Where><Eq><FieldRef Name='C_x00f3_digo_x0020_de_x0020_Dise' /><Value Type='Text'>" + strDiseno + "</Value></Eq></Where>";
                    qryCodigoDiseno.Query = strQueryDiseno;

                    SPListItemCollection lstProductosMaterialesDiseno = lProductosMaterialesDiseno.GetItems(qryCodigoDiseno);
                        if (lstProductosMaterialesDiseno.Count != 0)
                        {
                            AgregarEvento(properties, "# Diseño", lstProductosMaterialesDiseno.Count.ToString());
                            foreach (SPListItem itmProductoMaterialDiseno in lstProductosMaterialesDiseno)
                            {
                                if (itmProductoMaterialDiseno["Código SAP"].ToString() != strCodigoSAP) {
                                    if(itmProductoMaterialDiseno["Estado"].ToString() != "Iniciado") { 
                                    SPList lProcesosPackaging = properties.Web.Lists["Procesos Solicitudes Packaging"];
                                    SPListItem itmProcesoPackaging;
                                    itmProcesoPackaging = lProcesosPackaging.AddItem();
                                    itmProcesoPackaging["Title"] = properties.ListItem.Title.ToString();
                                    itmProcesoPackaging["Id Documento"] = properties.ListItem["Id Documento"].ToString();
                                    itmProcesoPackaging["Código SAP"] = itmProductoMaterialDiseno["Código SAP"].ToString();
                                    itmProcesoPackaging.Update();
                                    }

                                }
                            }

                        }
                    }
                }

                SPQuery qryBitacora = new SPQuery();
                strQuery = "";
                strQuery = "<Where><Contains><FieldRef Name='C_x00f3_digo_x0020_SAP' /><Value Type='Text'>" + strCodigoSAP + "</Value></Contains></Where>";
                qryBitacora.Query = strQuery;

                SPListItemCollection lstBitacora = lBitacora.GetItems(qryBitacora);
                if (lstBitacora.Count != 0)
                {
                    foreach (SPListItem itmBitacora in lstBitacora) {
                        if (itmBitacora["Estado"].ToString() == "Pendiente") { 
                        itmBitacora["Estado"] = "Completado";
                        itmBitacora["Fecha de Fin"] = DateTime.Now;
                        itmBitacora["Asignado"] = properties.ListItem["Author"];
                        itmBitacora.SystemUpdate();
                        }
                    }
                    //idSolicitud = Convert.ToInt32(lstBitacora[0]["Solicitud asociada"].ToString().Split(';')[0].ToString());
                    
                }

                properties.ListItem["Resultado"] = "Procesado OK";
                properties.ListItem.Update();




            }
            catch (Exception ex) {
                properties.ListItem["Resultado"] = "Error - " + ex.Message.ToString();
                properties.ListItem.Update();
            }
            }

        public static string sDevolverMailUsuario(string strUsuario, SPItemEventProperties properties)
        {
            string auxValor = "";

            auxValor = strUsuario.Split(';')[0].ToString();

            SPSite site = new SPSite(properties.Site.Url.ToString()); //.Settings.Default.UrlSite.ToString());
            SPWeb myweb = site.OpenWeb();
            SPUser sUsuario = myweb.AllUsers.GetByID(Convert.ToInt32(auxValor));

            auxValor = sUsuario.Email.ToString();

            return auxValor;
        }

        public static string sDevolverDatosUsuario(string strUsuario, SPItemEventProperties properties)
        {
            string auxValor = "";
            auxValor = strUsuario.Split(';')[0].ToString();

            SPSite site = new SPSite(properties.Site.Url.ToString()); //.Settings.Default.UrlSite.ToString());
            SPWeb myweb = site.OpenWeb();
            SPUser sUsuario = myweb.AllUsers.GetByID(Convert.ToInt32(auxValor));

            auxValor = sUsuario.Name.ToString();

            return auxValor;
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

        private DateTime dtFechaVencimiento(Int32 iDiasVencimiento) {
            DateTime auxFechaVencimiento = DateTime.Now;

            auxFechaVencimiento = DateTime.Now.AddDays(iDiasVencimiento);

            if (auxFechaVencimiento.DayOfWeek == DayOfWeek.Saturday) {
                auxFechaVencimiento = auxFechaVencimiento.AddDays(2);
            }

            if (auxFechaVencimiento.DayOfWeek == DayOfWeek.Sunday) {
                auxFechaVencimiento = auxFechaVencimiento.AddDays(1);
            }

            return auxFechaVencimiento;
        }

        private String strSectorPendiente(Int32 idSolicitud, SPItemEventProperties properties) {
            String strAuxSector = "";

            SPList lBitacora = properties.Web.Lists["Bitácora Solicitudes"];
            SPQuery queryDA = new SPQuery();
            queryDA.Query = string.Concat("<Where><Eq><FieldRef Name='Solicitud_x0020_asociada' LookupId='TRUE'/>", "<Value Type='Lookup'>", idSolicitud, "</Value></Eq></Where><OrderBy>  <FieldRef Name='ID' Ascending='False'/></OrderBy>");
            SPListItemCollection itemColl = null;
            itemColl = lBitacora.GetItems(queryDA);
            foreach (SPListItem itmTarea in itemColl)
            {
                if (itmTarea["Estado"].ToString() == "Pendiente") {
                    if (itmTarea["Sector"] != null) {
                        if (strAuxSector != "")
                        {
                            if (strAuxSector != "Packaging") {
                                if (strAuxSector != "Registro") { 
                            strAuxSector = strAuxSector + "; " + itmTarea["Sector"].ToString();
                                } else
                                {
                                    strAuxSector = "Registro";
                                }
                            } else {
                                strAuxSector = "Packaging";
                            }

                        }
                        else {
                            strAuxSector = itmTarea["Sector"].ToString();
                        }

                    }

                }
            }

                return strAuxSector;

        }

        public static void AgregarEvento(SPItemEventProperties properties, string strTitulo, string strMensaje) {
            SPList lEventos = properties.Web.Lists["Eventos"];
            SPListItem iEvento = lEventos.AddItem();
            iEvento["Título"] = strTitulo;
            iEvento["Descripcion"] = strMensaje;
            iEvento.Update();
        }

    }
}