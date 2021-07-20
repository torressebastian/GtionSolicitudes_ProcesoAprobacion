using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Data;
using System.Web.UI.WebControls;
using System.Collections;
using System.DirectoryServices.AccountManagement;
using GtionPackaging_Solicitudes;
using System.Text.RegularExpressions;

namespace SolicitudesDiseno_Solicitudes.Layouts.SolicitudesDiseno
{
    public partial class SolicitudProductoMaterial : LayoutsPageBase
    {
        Int32 idDocument = 0;
        String strEstadoSolicitud = "";
        Boolean bInicioProceso = false;
        String sTipoSolicitud = "";
        Int32 idSectorAlta = 0;


        protected void Page_Load(object sender, EventArgs e)
        {
            //Obtengo el Id de la solicitud
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
                        SPList lDocumentos = web.Lists["Solicitudes"];
                        SPList lBitacora = web.Lists["Bitácora Solicitudes"];
                        SPListItem itmDocumento = lDocumentos.GetItemById(idDocument);

                        sTipoSolicitud = itmDocumento.ContentType.Name.ToString();
                        txtNombreSolicitud.Text = itmDocumento.Title.ToString();
                        txtTipoDocumento.Text = sTipoSolicitud;

                        strEstadoSolicitud = itmDocumento["Estado"].ToString();


                        // Oculto los campos dependiendo si es o no Lanzamiento Internacional.
                        if (sTipoSolicitud == "Lanzamiento Internacional")
                        {
                            //tbrDatoCobertura.Visible = false;
                        }
                        else {
                            tbrCriterioUnificacion.Visible = false;
                        }

                        if (sTipoSolicitud == "Modificación de Archivos (Desarrollo)") {
                            lblProducto.Text = "Producto / Material:";

                        }

                        // Verifico si es inicio de proceso.
                        if (itmDocumento["Inicio Proceso"] is null)
                        {
                            bInicioProceso = true;
                        }
                        else
                        {
                            if (itmDocumento["Inicio Proceso"].ToString() == "SI")
                            {
                                bInicioProceso = true;
                            }
                        }

                        if (strEstadoSolicitud == "Reinicio Pendiente")
                        {
                            bInicioProceso = true;
                        }
                        

                        if (bInicioProceso == true) {
                           }

                        Errores.Text = Errores.Text + " - Inicio Proceso: " + bInicioProceso.ToString() + " - Grupo Inicio Proceso: " + idSectorAlta.ToString();

                    }
                }
            });

            if (!Page.IsPostBack)
            {
                // Armo la sección para alta y edición
                ListItem ddlItem;
                SPList lstList = SPContext.Current.Web.Lists["Tipo Material"];
                ddlItem = new ListItem();
                ddlItem.Value = "0";
                ddlItem.Text = "<-- Seleccione Material -->";


                ddlTipoMaterial.Items.Add(ddlItem);
                foreach (SPListItem str in lstList.GetItems())
                {
                    ddlItem = new ListItem();
                    ddlItem.Value = str.ID.ToString();
                    ddlItem.Text = str.Title.ToString();
                    ddlTipoMaterial.Items.Add(ddlItem);
                }


                ActualizarListaProductos();


            

            ArmarPanelProductos(0);
           
            if (bInicioProceso == true && sTipoSolicitud != "Modificación de Archivos (Desarrollo)")
            {
                h2CabeceraMateriales.Visible = false;
                pnlMateriales.Visible = false;
                pnlEdicionMaterial.Visible = false;
                SeguridadPanelProducto();
            }
            else
            {
                //Obtengo la lista de Productos
                ArmarPanelMateriales(0);
            }
            }



        }
        protected void ArmarPanelProductos(int sPage)
        {

            iPaginaProducto.Text = sPage.ToString();
            SPQuery qryTareas = new SPQuery();
            SPList lstList;
            String strQuery = "";
            lstList = SPContext.Current.Web.Lists["Solicitud - Producto"];
            qryTareas = new SPQuery(lstList.Views["Todos los elementos"]);
            String sOrden = string.Format("<OrderBy><FieldRef Name='{0}' Ascending='{1}' /></OrderBy>", "ID", "False");
            strQuery = "<Eq><FieldRef Name='Solicitud' LookupId='TRUE' /><Value Type='Lookup'>" + idDocument.ToString() + "</Value></Eq>";

            if (!string.IsNullOrEmpty(strQuery))
            {
                strQuery = "<Where>" + strQuery + "</Where>";
            }
            if (!string.IsNullOrEmpty(sOrden))
            {
                strQuery = strQuery + sOrden;
            }

            qryTareas.Query = strQuery;
            qryTareas.RowLimit = 500;

            if (lstList.GetItems(qryTareas).Count != 0)
            {
                DataTable tempTbl = lstList.GetItems(qryTareas).GetDataTable();
                CustomersGridView.DataSource = tempTbl;
                CustomersGridView.PageIndex = sPage;
                CustomersGridView.PageSize = 20;
                CustomersGridView.DataBind();
                CustomersGridView.Font.Size = 1;


                iPaginaProducto.Text = sPage.ToString();

                if (sPage == 0)
                {
                    imgAnterior.Visible = false;
                    lblPagina.Text = "1 - 20";
                }
                else
                {
                    imgAnterior.Visible = true;
                    lblPagina.Text = Convert.ToString(sPage * 20 + 1) + " - " + Convert.ToString((sPage + 1) * 20);
                }

                //Errores.Text = Errores.Text + "Total: " + CustomersGridView.PageCount.ToString();


                if (sPage == CustomersGridView.PageCount - 1)
                {
                    imgSiguiente.Visible = false;
                    lblPagina.Text = Convert.ToString(sPage * 20 + 1) + " - " + Convert.ToString(lstList.GetItems(qryTareas).Count.ToString());
                }
                else
                {
                    imgSiguiente.Visible = true;
                }
            }
            if (txtTipoDocumento.Text == "Modificación de Archivos (Desarrollo)") { 
                CustomersGridView.Columns[2].HeaderText = "Producto / Material";
            }

            //LimpiarPanelProducto();

            if (strEstadoSolicitud == "Pendiente Inicio Packaging" || strEstadoSolicitud == "Cancelado" || strEstadoSolicitud == "Completado")
            {
                btnAddProducto.Enabled = false;
                btnUpdProducto.Enabled = false;
                btnDelProducto.Enabled = false;
                btnAddMaterial.Enabled = false;
                btnUpdMaterial.Enabled = false;
                btnDelMaterial.Enabled = false;
            } else { 

            if (bInicioProceso == true)
            {
                    // Valido si el usuario es del grupo de alta de productos
                    SPUser currentUser = SPContext.Current.Web.CurrentUser;
                    Boolean bEsSectorAlta = false;


                    SPList lConfiguracionSolicitudes = SPContext.Current.Web.Lists["Configuración Circuitos Solicitudes"];
                    SPQuery query = new SPQuery();
                    query.Query = "<Where><Eq><FieldRef Name=\"Title\" /><Value Type=\"Text\">" + sTipoSolicitud + "</Value></Eq></Where>";
                    query.RowLimit = 1;
                    query.ViewFields = "";
                    SPListItemCollection items = lConfiguracionSolicitudes.GetItems(query);
                    SPListItem item = items[0];
                    SPFieldLookupValueCollection lkSectorAlta = new SPFieldLookupValueCollection(item["Sector alta"].ToString());

                    foreach (SPFieldLookupValue itmSectorAlta in lkSectorAlta)
                    {


                        Int32 idSectorAlta = itmSectorAlta.LookupId;

                        if (Funciones_Comunes.UsuarioGrupo(currentUser, idSectorAlta) == true)
                        {
                            bEsSectorAlta = true;
                        }
                    }


                    
                
                if (bEsSectorAlta == true)
                {
                    btnAddProducto.Enabled = true;
                }
                else
                {
                    btnAddProducto.Enabled = false;
                }
            }
            else
            {
                btnAddProducto.Enabled = false;
            }
            }
        }
        protected void CustomersGridViewProductos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditarProducto")
            {
                int index = Convert.ToInt32(e.CommandArgument);
                GridViewRow row = CustomersGridView.Rows[index];

                SPList lstList;
                lstList = SPContext.Current.Web.Lists["Solicitud - Producto"];
                SPListItem itmSolProducto;
                iProducto.Value = row.Cells[0].Text;
                itmSolProducto = lstList.GetItemById(Convert.ToInt32(iProducto.Value));

                if (itmSolProducto != null) {
                    txtProducto.Text = itmSolProducto.Title.ToString();
                    if (itmSolProducto["Código SAP"] is null) { txtProductoCodigoSAP.Text = ""; } else { txtProductoCodigoSAP.Text = itmSolProducto["Código SAP"].ToString(); }
                    if (itmSolProducto["Concentración"] is null) { txtConcentracion.Text = ""; } else { txtConcentracion.Text = itmSolProducto["Concentración"].ToString(); }
                    if (itmSolProducto["Presentación"] is null) { txtPresentacion.Text = ""; } else { txtPresentacion.Text = itmSolProducto["Presentación"].ToString(); }
                    if (itmSolProducto["Estimado Venta"] is null) { txtEstimadoVenta.Text = ""; } else { txtEstimadoVenta.Text = itmSolProducto["Estimado Venta"].ToString(); }
                    if (itmSolProducto["Vida Útil"] is null) { txtVidaUtil.Text = ""; } else { txtVidaUtil.Text = itmSolProducto["Vida Útil"].ToString(); }
                    if (itmSolProducto["Tipo de material de empaque"] is null) { txtTipoMaterialEmpaque.Text = ""; } else { txtTipoMaterialEmpaque.Text = itmSolProducto["Tipo de material de empaque"].ToString(); }
                    if (itmSolProducto["Recursos utilizados"] is null) { txtRecursosUtilizados.Text = ""; } else { txtRecursosUtilizados.Text = itmSolProducto["Recursos utilizados"].ToString(); }
                    if (itmSolProducto["Cantidad Blisters"] is null) { txtCantidadBlister.Text = ""; } else { txtCantidadBlister.Text = itmSolProducto["Cantidad Blisters"].ToString(); }
                    if (itmSolProducto["Criterio Unificación"] is null) { txtCriterioUnificacion.Text = ""; } else { txtCriterioUnificacion.Text = itmSolProducto["Criterio Unificación"].ToString(); }
                    if (itmSolProducto["Blister"] is null) { txtBlister.Text = ""; } else { txtBlister.Text = itmSolProducto["Blister"].ToString(); }
                    if (itmSolProducto["Unificación Aluminio"] is null) { txtUnificacionAluminio.Text = ""; } else { txtUnificacionAluminio.Text = itmSolProducto["Unificación Aluminio"].ToString(); }
                    if (itmSolProducto["Unificación Estuche"] is null) { txtUnificacionEstuche.Text = ""; } else { txtUnificacionEstuche.Text = itmSolProducto["Unificación Estuche"].ToString(); }
                    if (itmSolProducto["Unificación Prospecto"] is null) { txtUnificacionProspecto.Text = ""; } else { txtUnificacionProspecto.Text = itmSolProducto["Unificación Prospecto"].ToString(); }
                    if (itmSolProducto["Datos de Cobertura"] is null) { txtDatosCobertura.Text = ""; } else { txtDatosCobertura.Text = itmSolProducto["Datos de Cobertura"].ToString(); }
                    if (itmSolProducto["Tipo producto"] is null) { ddlTipoProducto.SelectedValue = ""; } else { ddlTipoProducto.SelectedValue = itmSolProducto["Tipo producto"].ToString(); }
                    
                }
                
                ArmarPanelMateriales(0);

                btnAddProducto.Enabled = false;
                btnUpdProducto.Enabled = true;
                btnDelProducto.Enabled = true;

                if (strEstadoSolicitud == "Pendiente Inicio Packaging" || strEstadoSolicitud == "Cancelado" || strEstadoSolicitud == "Completado")
                {
                    btnAddProducto.Enabled = false;
                    btnUpdProducto.Enabled = false;
                    btnDelProducto.Enabled = false;
                    btnAddMaterial.Enabled = false;
                    btnUpdMaterial.Enabled = false;
                    btnDelMaterial.Enabled = false;
                } else {

                    SeguridadPanelProducto();
                    if (bInicioProceso == true)
                {

                        SPUser currentUser1 = SPContext.Current.Web.CurrentUser;
                        Boolean bEsSectorAlta = false;


                        SPList lConfiguracionSolicitudes = SPContext.Current.Web.Lists["Configuración Circuitos Solicitudes"];
                        SPQuery query = new SPQuery();
                        query.Query = "<Where><Eq><FieldRef Name=\"Title\" /><Value Type=\"Text\">" + sTipoSolicitud + "</Value></Eq></Where>";
                        query.RowLimit = 1;
                        query.ViewFields = "";
                        SPListItemCollection items = lConfiguracionSolicitudes.GetItems(query);
                        SPListItem item = items[0];
                        SPFieldLookupValueCollection lkSectorAlta = new SPFieldLookupValueCollection(item["Sector alta"].ToString());

                        foreach (SPFieldLookupValue itmSectorAlta in lkSectorAlta)
                        {


                            Int32 idSectorAlta = itmSectorAlta.LookupId;

                            if (Funciones_Comunes.UsuarioGrupo(currentUser1, idSectorAlta) == true)
                            {
                                bEsSectorAlta = true;
                            }
                        }




                        if (bEsSectorAlta == true)
                        {
                            btnUpdProducto.Enabled = true;
                            btnDelProducto.Enabled = true;
                        }
                        else
                        {
                            btnUpdProducto.Enabled = false;
                            btnDelProducto.Enabled = false;
                        }

                    
                }
                else
                {
                    btnAddProducto.Enabled = false;
                    btnDelProducto.Enabled = false;
                }
                }

            }
        }
        protected void AddProducto(object sender, EventArgs e)
        {
            if (bValidarProducto() == true)
            {
                try
                {
                    SPList lstList;
                    lstList = SPContext.Current.Web.Lists["Solicitud - Producto"];

                    SPListItem addListProducto;

                    addListProducto = lstList.AddItem();

                    addListProducto["Solicitud"] = idDocument;
                    addListProducto["Title"] = txtProducto.Text;
                    addListProducto["Código SAP"] = txtProductoCodigoSAP.Text;
                    addListProducto["Concentración"] = txtConcentracion.Text;
                    addListProducto["Presentación"] = txtPresentacion.Text;
                    addListProducto["Vida Útil"] = txtVidaUtil.Text;
                    addListProducto["Estimado Venta"] = txtEstimadoVenta.Text;
                    addListProducto["Tipo de material de empaque"] = txtTipoMaterialEmpaque.Text;
                    addListProducto["Tipo producto"] = ddlTipoProducto.SelectedValue.ToString();
                    addListProducto["Recursos utilizados"] = txtRecursosUtilizados.Text;
                    addListProducto["Cantidad Blisters"] = txtCantidadBlister.Text;
                    addListProducto["Criterio Unificación"] = txtCriterioUnificacion.Text;
                    addListProducto["Blister"] = txtBlister.Text;
                    addListProducto["Unificación Aluminio"] = txtUnificacionAluminio.Text;
                    addListProducto["Unificación Estuche"] = txtUnificacionEstuche.Text;
                    addListProducto["Unificación Prospecto"] = txtUnificacionProspecto.Text;
                    addListProducto["Datos de Cobertura"] = txtDatosCobertura.Text;

                    addListProducto.Update();
                    System.Threading.Thread.Sleep(2500);

                    LimpiarPanelProducto();

                    ArmarPanelProductos(Convert.ToInt32(iPaginaProducto.Text));

                }
                catch (Exception ex)
                {
                    //Errores.Text = Errores.Text + " " + ex.Message;

                }
            }
            else {
                lblMensajeErrorProducto.Text = "Error: Datos obligatorios.";
                lblMensajeErrorProducto.Visible = true;
            }

        }
        protected void DelProducto(object sender, EventArgs e)
        {

            try
            {
                SPList lstList;
                lstList = SPContext.Current.Web.Lists["Solicitud - Producto"];

                SPListItem addListProducto;

                addListProducto = lstList.GetItemById(Convert.ToInt32(iProducto.Value));

                addListProducto.Delete();

                System.Threading.Thread.Sleep(2500);

                LimpiarPanelProducto();
                
                ArmarPanelProductos(Convert.ToInt32(iPaginaProducto.Text));

            }
            catch (Exception ex)
            {
                //Errores.Text = Errores.Text + " " + ex.Message;

            }
        }
        protected void UpdProducto(object sender, EventArgs e)
        {

            try
            {
                if (bValidarProducto() == true)
                {
                    SPList lstList;
                    lstList = SPContext.Current.Web.Lists["Solicitud - Producto"];

                    SPListItem addListProducto;

                    addListProducto = lstList.GetItemById(Convert.ToInt32(iProducto.Value));
                    addListProducto["Title"] = txtProducto.Text;
                    addListProducto["Código SAP"] = txtProductoCodigoSAP.Text;
                    addListProducto["Concentración"] = txtConcentracion.Text;
                    addListProducto["Presentación"] = txtPresentacion.Text;
                    addListProducto["Vida Útil"] = txtVidaUtil.Text;
                    addListProducto["Estimado Venta"] = txtEstimadoVenta.Text;
                    addListProducto["Tipo de material de empaque"] = txtTipoMaterialEmpaque.Text;
                    addListProducto["Tipo producto"] = ddlTipoProducto.SelectedValue.ToString();
                    addListProducto["Recursos utilizados"] = txtRecursosUtilizados.Text;
                    addListProducto["Cantidad Blisters"] = txtCantidadBlister.Text;
                    addListProducto["Criterio Unificación"] = txtCriterioUnificacion.Text;
                    addListProducto["Blister"] = txtBlister.Text;
                    addListProducto["Unificación Aluminio"] = txtUnificacionAluminio.Text;
                    addListProducto["Unificación Estuche"] = txtUnificacionEstuche.Text;
                    addListProducto["Unificación Prospecto"] = txtUnificacionProspecto.Text;
                    addListProducto["Datos de Cobertura"] = txtDatosCobertura.Text;


                    addListProducto.Update();
                    System.Threading.Thread.Sleep(2500);

                    LimpiarPanelProducto();

                    ArmarPanelProductos(Convert.ToInt32(iPaginaProducto.Text));
                }
                else {
                    lblMensajeErrorProducto.Text = "Error: Datos obligatorios.";
                    lblMensajeErrorProducto.Visible = true;
                }

            }
            catch (Exception ex)
            {
                //Errores.Text = Errores.Text + " " + ex.Message;

            }
        }
        protected void RefProducto(object sender, EventArgs e)
        {

            try
            {
                LimpiarPanelProducto();
            }
            catch (Exception ex)
            {
                //Errores.Text = Errores.Text + " " + ex.Message;

            }

        }
        protected void LimpiarPanelProducto() {
            iProducto.Value = "0";
            txtProducto.Text = "";
            txtProductoCodigoSAP.Text = "";
            txtPresentacion.Text = "";
            txtConcentracion.Text = "";
            txtVidaUtil.Text = "";
            txtEstimadoVenta.Text = "";
            txtTipoMaterialEmpaque.Text = "";
            txtRecursosUtilizados.Text = "";
            txtCantidadBlister.Text = "";
            ddlTipoProducto.SelectedIndex = 0;
            txtCriterioUnificacion.Text = "";
            txtBlister.Text = "";
            txtUnificacionAluminio.Text = "";
            txtUnificacionEstuche.Text = "";
            txtUnificacionProspecto.Text = "";
            txtDatosCobertura.Text = "";
            lblMensajeErrorProducto.Text = "";
            lblMensajeErrorProducto.Visible = false;

            if (bInicioProceso == true)
            {
                // Valido si el usuario es del grupo de alta de productos
                SPUser currentUser = SPContext.Current.Web.CurrentUser;
                Boolean bUsuarioGrupo = Funciones_Comunes.UsuarioGrupo(currentUser, idSectorAlta);
                if (bUsuarioGrupo == true)
                {
                    btnAddProducto.Enabled = true;
                }
                else {
                    btnAddProducto.Enabled = false;
                }
            }
            else {
                btnAddProducto.Enabled = false;
            }
            btnUpdProducto.Enabled = false;
            btnDelProducto.Enabled = false;

            if (strEstadoSolicitud == "Pendiente Inicio Packaging" || strEstadoSolicitud == "Cancelado" || strEstadoSolicitud == "Completado")
            {
                btnAddProducto.Enabled = false;
                btnUpdProducto.Enabled = false;
                btnDelProducto.Enabled = false;
                btnAddMaterial.Enabled = false;
                btnUpdMaterial.Enabled = false;
                btnDelMaterial.Enabled = false;
            }

            ActualizarListaProductos();

        }
        protected void SeguridadPanelProducto()
        {
            // Analizo campo por campo si el usuario tiene permisos para editar los campos

            if (bPuedeEditar("Producto") == true) { txtProducto.Enabled = true; lblProducto.ForeColor = System.Drawing.Color.Red; } else { txtProducto.Enabled = false; }
            if (bPuedeEditar("Concentración") == true) { txtConcentracion.Enabled = true; lblConcentracion.ForeColor = System.Drawing.Color.Red; } else { txtConcentracion.Enabled = false; }
            if (bPuedeEditar("Presentación") == true) { txtPresentacion.Enabled = true; lblPresentacion.ForeColor = System.Drawing.Color.Red; } else { txtPresentacion.Enabled = false; }
            if (bPuedeEditar("Estimado Venta") == true) { txtEstimadoVenta.Enabled = true; lblEstimadoVenta.ForeColor = System.Drawing.Color.Red; } else { txtEstimadoVenta.Enabled = false; }
            if (bPuedeEditar("Vida Útil") == true) { txtVidaUtil.Enabled = true; lblVidaUtil.ForeColor = System.Drawing.Color.Red; } else { txtVidaUtil.Enabled = false; }
            if (bPuedeEditar("Tipo de material de empaque") == true) { txtTipoMaterialEmpaque.Enabled = true; lblTipoMaterialEmpaque.ForeColor = System.Drawing.Color.Red; } else { txtTipoMaterialEmpaque.Enabled = false; }
            if (bPuedeEditar("Tipo producto") == true) { ddlTipoProducto.Enabled = true; lblTipoProducto.ForeColor = System.Drawing.Color.Red; } else { ddlTipoProducto.Enabled = false; }
            if (bPuedeEditar("Recursos utilizados") == true) { txtRecursosUtilizados.Enabled = true; lblRecursosUtilizados.ForeColor = System.Drawing.Color.Red; } else { txtRecursosUtilizados.Enabled = false; }
            if (bPuedeEditar("Cantidad Blisters") == true) { txtCantidadBlister.Enabled = true; lblCantidadBlister.ForeColor = System.Drawing.Color.Red; } else { txtCantidadBlister.Enabled = false; }
            if (bPuedeEditar("Criterio Unificación") == true) { txtCriterioUnificacion.Enabled = true; lblCriterioUnificacion.ForeColor = System.Drawing.Color.Red; } else { txtCriterioUnificacion.Enabled = false; }
            if (bPuedeEditar("Blister") == true) { txtBlister.Enabled = true; lblBlister.ForeColor = System.Drawing.Color.Red; } else { txtBlister.Enabled = false; }
            if (bPuedeEditar("Unificación Aluminio") == true) { txtUnificacionAluminio.Enabled = true; lblUnificacionAluminio.ForeColor = System.Drawing.Color.Red; } else { txtUnificacionAluminio.Enabled = false; }
            if (bPuedeEditar("Unificación Estuche") == true) { txtUnificacionEstuche.Enabled = true; lblUnificacionEstuche.ForeColor = System.Drawing.Color.Red; } else { txtUnificacionEstuche.Enabled = false; }
            if (bPuedeEditar("Unificación Prospecto") == true) { txtUnificacionProspecto.Enabled = true; lblUnificacionProspecto.ForeColor = System.Drawing.Color.Red; } else { txtUnificacionProspecto.Enabled = false; }
            if (bPuedeEditar("Código SAP") == true) { txtProductoCodigoSAP.Enabled = true; lblProductoCodigoSAP.ForeColor = System.Drawing.Color.Red; } else { txtProductoCodigoSAP.Enabled = false; }
            if (bPuedeEditar("Datos de Cobertura") == true) { txtDatosCobertura.Enabled = true; lblDatosCobertura.ForeColor = System.Drawing.Color.Red; } else { txtDatosCobertura.Enabled = false; }


        }

        protected Boolean bPuedeEditar(String strCampo) {

            Guid siteId = SPContext.Current.Site.ID;
            Guid webId = SPContext.Current.Web.ID;
            SPUser currentUser = SPContext.Current.Web.CurrentUser;
            Boolean bResult = false;

            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                using (SPSite site = new SPSite(siteId))
                {
                    using (SPWeb web = site.OpenWeb(webId))
                    {
                        Int32 idSectorAsociado = 0; 
                        SPList lConfiguracionProductoSector = web.Lists["Configuración Producto Sector"];
                        SPQuery query = new SPQuery();
                        query.Query = "<Where><Eq><FieldRef Name=\"Title\" /><Value Type=\"Text\">" + strCampo + "</Value></Eq></Where>";
                        query.RowLimit = 1;
                        query.ViewFields = "";
                        SPListItemCollection lstList = lConfiguracionProductoSector.GetItems(query);
                        foreach (SPListItem itmSector in lstList)
                        {
                            
                            SPFieldLookupValueCollection lkSectorAlta = new SPFieldLookupValueCollection(itmSector[sTipoSolicitud].ToString());

                            foreach (SPFieldLookupValue itmSectorAlta in lkSectorAlta)
                            {
                                idSectorAsociado = itmSectorAlta.LookupId;
                                if (bResult == false) { 
                                    bResult = Funciones_Comunes.UsuarioGrupo(currentUser, idSectorAsociado);
                                }
                            }
                        }
                        
                        
                    }
                }
            });


            return bResult;
        }
        protected Boolean bValidarProducto() {
            Boolean bAuxResultado = true;

            if (txtProducto.Enabled == true) { if (txtProducto.Text == "") { bAuxResultado = false; } }
            if (txtConcentracion.Enabled == true){ if(txtConcentracion.Text == "") { bAuxResultado = false; }        } 
            if (txtPresentacion.Enabled == true){ if( txtPresentacion.Text == "") { bAuxResultado = false;} }
            if (txtEstimadoVenta.Enabled == true) { if (txtEstimadoVenta.Text == "") { bAuxResultado = false; } }
            if (txtVidaUtil.Enabled == true) { if (txtVidaUtil.Text == "") { bAuxResultado = false; } }
            if (txtTipoMaterialEmpaque.Enabled == true) { if (txtTipoMaterialEmpaque.Text == "") { bAuxResultado = false; } }
            if (ddlTipoProducto.Enabled == true) { if (ddlTipoProducto.SelectedIndex == 0) { bAuxResultado = false; } }
            if (txtRecursosUtilizados.Enabled == true) { if (txtRecursosUtilizados.Text == "") { bAuxResultado = false; } }
            if (txtCantidadBlister.Enabled == true) { if (txtCantidadBlister.Text == "") { bAuxResultado = false; } }
            if (txtCriterioUnificacion.Enabled == true) { if (txtCriterioUnificacion.Text == "") { bAuxResultado = false; } }
            if (txtBlister.Enabled == true) { if (txtBlister.Text == "") { bAuxResultado = false; } }
            if (txtUnificacionAluminio.Enabled == true) { if (txtUnificacionAluminio.Text == "") { bAuxResultado = false; } }
            if (txtUnificacionEstuche.Enabled == true) { if (txtUnificacionEstuche.Text == "") { bAuxResultado = false; } }
            if (txtUnificacionProspecto.Enabled == true) { if (txtUnificacionProspecto.Text == "") { bAuxResultado = false; } }
            if (txtProductoCodigoSAP.Enabled == true) { if (txtProductoCodigoSAP.Text == "") { bAuxResultado = false; } }
            if (txtDatosCobertura.Enabled == true) { if (txtDatosCobertura.Text == "") { bAuxResultado = false; } }

            return bAuxResultado;

        }

        protected void btnAnterior_Click(object sender, EventArgs e)
        {
            iPaginaProducto.Text = (Convert.ToInt32(iPaginaProducto.Text) - 1).ToString();
            //ArmarPanelProductos(Convert.ToInt32(iPaginaProducto.Text) - 1);
        }
        protected void btnSiguiente_Click(object sender, EventArgs e)
        {
            iPaginaProducto.Text = (Convert.ToInt32(iPaginaProducto.Text) + 1).ToString();
            //ArmarPanelProductos(Convert.ToInt32(iPaginaProducto.Text) + 1);
        }
        protected void ArmarPanelMateriales(int sPage)
        {
            SeguridadPanelProductoMaterial();
            iPaginaMaterial.Text = sPage.ToString();
            SPQuery qryTareas = new SPQuery();
            SPList lstList;
            String strQuery = "";
            lstList = SPContext.Current.Web.Lists["Solicitud - Producto Material"];
            qryTareas = new SPQuery(lstList.Views["Todos los elementos"]);
            
            String sOrden = string.Format("<OrderBy><FieldRef Name='{0}' Ascending='{1}' /></OrderBy>", "ID", "False");
            strQuery = "<Eq><FieldRef Name='Solicitud' LookupId='TRUE' /><Value Type='Lookup'>" + idDocument.ToString() + "</Value></Eq>";

            if (!string.IsNullOrEmpty(strQuery))
            {
                strQuery = "<Where>" + strQuery + "</Where>";
            }
            if (!string.IsNullOrEmpty(sOrden))
            {
                strQuery = strQuery + sOrden;
            }

            qryTareas.Query = strQuery;
            qryTareas.RowLimit = 500;

            if (lstList.GetItems(qryTareas).Count != 0)
            {
                DataTable tempTbl = lstList.GetItems(qryTareas).GetDataTable();
                GridViewMateriales.DataSource = tempTbl;
                GridViewMateriales.PageIndex = sPage;
                GridViewMateriales.PageSize = 20;
                GridViewMateriales.DataBind();
                GridViewMateriales.Font.Size = 1;


                iPaginaMaterial.Text = sPage.ToString();

                if (sPage == 0)
                {
                    imgAnteriorMaterial.Visible = false;
                    lblPaginaMaterial.Text = "1 - 20";
                }
                else
                {
                    imgAnteriorMaterial.Visible = true;
                    lblPaginaMaterial.Text = Convert.ToString(sPage * 20 + 1) + " - " + Convert.ToString((sPage + 1) * 20);
                }

                //Errores.Text = Errores.Text + "Total: " + CustomersGridView.PageCount.ToString();


                if (sPage == GridViewMateriales.PageCount - 1)
                {
                    imgSiguienteMaterial.Visible = false;
                    lblPaginaMaterial.Text = Convert.ToString(sPage * 20 + 1) + " - " + Convert.ToString(lstList.GetItems(qryTareas).Count.ToString());
                }
                else
                {
                    imgSiguienteMaterial.Visible = true;
                }

                if (GridViewMateriales.Rows.Count != 0)
                {
                    foreach (GridViewRow row in GridViewMateriales.Rows)
                    {
                        if (row.Cells[16].Text.ToString() == "1")
                        {
                            row.BackColor = System.Drawing.Color.Orange;
                            row.Cells[16].ForeColor = System.Drawing.Color.Orange;
                            row.Cells[0].ForeColor = System.Drawing.Color.Orange;
                        }
                    }
                }


            }

            if (strEstadoSolicitud == "Pendiente Inicio Packaging")
            {
                btnAddProducto.Enabled = false;
                btnUpdProducto.Enabled = false;
                btnDelProducto.Enabled = false;
                btnAddMaterial.Enabled = false;
                btnUpdMaterial.Enabled = false;
                btnDelMaterial.Enabled = false;
            }
            else {

                SPUser currentUser = SPContext.Current.Web.CurrentUser;
                if (Funciones_Comunes.UsuarioGrupo(currentUser, Funciones_Comunes.iDevolverIdSector("Desarrollo")) == false)
                {
                    btnAddMaterial.Enabled = false;
                    btnUpdMaterial.Enabled = false;
                    btnDelMaterial.Enabled = false;
                }
                else {
                    lblProductoMaterial.ForeColor = System.Drawing.Color.Red;
                    lblMaterial.ForeColor = System.Drawing.Color.Red;
                    lblMaterialCodigoSAP.ForeColor = System.Drawing.Color.Red;
                    lblCodigoDiseno.ForeColor = System.Drawing.Color.Red;
                }
                 
            }


        }
        protected void ActualizarListaProductos() {
            lbxProductoMaterial.Items.Clear();
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
            foreach (SPListItem str in lstProductos)
            {
                ListItem ddlItem = new ListItem();
                ddlItem.Value = str.ID.ToString();
                ddlItem.Text = str.Title.ToString() + " - " + str["Concentración"].ToString() + " - " + str["Presentación"].ToString();
                lbxProductoMaterial.Items.Add(ddlItem);
            }
        }
        protected void CustomersGridViewMaterials_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditarMaterial")
            {

                LimpiarPanelMateriales();

                lblMensajeErrorMaterial.Visible = false;
                int index = Convert.ToInt32(e.CommandArgument);
                GridViewRow row = GridViewMateriales.Rows[index];

                SPList lstList;
                lstList = SPContext.Current.Web.Lists["Solicitud - Producto Material"];
                SPListItem itmSolMaterial;
                iMaterial.Value = row.Cells[0].Text;
                itmSolMaterial = lstList.GetItemById(Convert.ToInt32(iMaterial.Value));

                if (itmSolMaterial != null)
                {
                    txtMaterial.Text = itmSolMaterial.Title.ToString();
                    if (itmSolMaterial["Código SAP"] is null) { txtMaterialCodigoSAP.Text = ""; } else { txtMaterialCodigoSAP.Text = itmSolMaterial["Código SAP"].ToString(); }
                    if (itmSolMaterial["Cortante"] is null) { txtCortante.Text = ""; } else { txtCortante.Text = itmSolMaterial["Cortante"].ToString(); }
                    if (itmSolMaterial["Medida"] is null) { txtMedida.Text = ""; } else { txtMedida.Text = itmSolMaterial["Medida"].ToString(); }
                    if (itmSolMaterial["MDI"] is null) { txtMDI.Text = ""; } else { txtMDI.Text = itmSolMaterial["MDI"].ToString(); }
                    if (itmSolMaterial["Tipo Material"] is null) { ddlTipoMaterial.SelectedValue = "0"; } else { ddlTipoMaterial.SelectedValue = itmSolMaterial["Tipo Material"].ToString().Split(';')[0].ToString(); }
                    if (itmSolMaterial["Carga de Laca"] is null) { txtCargaLaca.Text = ""; } else { txtCargaLaca.Text = itmSolMaterial["Carga de Laca"].ToString(); }
                    if (itmSolMaterial["Plano"] is null) { txtPlano.Text = ""; } else { txtPlano.Text = itmSolMaterial["Plano"].ToString(); }
                    if (itmSolMaterial["Pharmacode"] is null) { txtPharmacode.Text = ""; } else { txtPharmacode.Text = itmSolMaterial["Pharmacode"].ToString(); }
                    if (itmSolMaterial["Nro Troquel"] is null) { txtTroquel.Text = ""; } else { txtTroquel.Text = itmSolMaterial["Nro Troquel"].ToString(); }
                    if (itmSolMaterial["Código de Diseño"] is null) { txtCodigoDiseno.Text = ""; } else { txtCodigoDiseno.Text = itmSolMaterial["Código de Diseño"].ToString(); }
                    SPFieldLookupValueCollection strProducto = itmSolMaterial["Producto"] as SPFieldLookupValueCollection;
                    foreach (SPFieldLookupValue iProducto in strProducto) {
                        foreach (ListItem xProducto in lbxProductoMaterial.Items) {
                            if (xProducto.Value == iProducto.LookupId.ToString()) {
                                xProducto.Selected = true;
                            }
                        }
                    }
                    if (itmSolMaterial["Datos de Cobertura"] is null) { txtCoberturaMaterial.Text = ""; } else { txtCoberturaMaterial.Text = itmSolMaterial["Datos de Cobertura"].ToString(); }
                    if (itmSolMaterial["Código de Especificación"] is null) { txtCodEspecificacion.Text = ""; } else { txtCodEspecificacion.Text = itmSolMaterial["Código de Especificación"].ToString(); }
                    if (itmSolMaterial["Código de Metodología Analítica"] is null) { txtCodMetodologia.Text = ""; } else { txtCodMetodologia.Text = itmSolMaterial["Código de Metodología Analítica"].ToString(); }

                }

                btnAddMaterial.Enabled = false;
                btnUpdMaterial.Enabled = true;
                btnDelMaterial.Enabled = true;


                if (strEstadoSolicitud == "Pendiente Inicio Packaging" || strEstadoSolicitud == "Cancelado" || strEstadoSolicitud == "Completado") 
                {
                    btnAddProducto.Enabled = false;
                    btnUpdProducto.Enabled = false;
                    btnDelProducto.Enabled = false;
                    btnAddMaterial.Enabled = false;
                    btnUpdMaterial.Enabled = false;
                    btnDelMaterial.Enabled = false;
                }
                else {
                    SPUser currentUser = SPContext.Current.Web.CurrentUser;
                    if (Funciones_Comunes.UsuarioGrupo(currentUser, Funciones_Comunes.iDevolverIdSector("Desarrollo")) == false)
                    {
                        btnAddMaterial.Enabled = false;
                        btnUpdMaterial.Enabled = false;
                        btnDelMaterial.Enabled = false;

                        if (Funciones_Comunes.UsuarioGrupo(currentUser, Funciones_Comunes.iDevolverIdSector("Planificación")) == false)
                        {
                            btnAddMaterial.Enabled = false;
                            btnUpdMaterial.Enabled = false;
                            btnDelMaterial.Enabled = false;
                        }
                        else
                        {
                            lblCoberturaMaterial.ForeColor = System.Drawing.Color.Red;
                            btnUpdMaterial.Enabled = true;
                        }
                    }
                    SeguridadPanelProductoMaterial();
                }

            }
        }
        protected void AddMaterial(object sender, EventArgs e)
        {
            if (bValidarMateriales() == true)
            {
                lblMensajeErrorMaterial.Visible = false;
                try
            {
                SPList lstList;
                lstList = SPContext.Current.Web.Lists["Solicitud - Producto Material"];

                SPListItem addListMaterial;

                addListMaterial = lstList.AddItem();


                SPFieldLookupValueCollection strProducto = new SPFieldLookupValueCollection();
                foreach (ListItem xProducto in lbxProductoMaterial.Items)
                {
                    if (xProducto.Selected == true)
                    {
                        strProducto.Add(new SPFieldLookupValue(xProducto.Value));
                    }
                }
                    addListMaterial["Solicitud"] = idDocument;
                addListMaterial["Producto"] = strProducto;
                addListMaterial["Title"] = txtMaterial.Text;
                addListMaterial["Código SAP"] = txtMaterialCodigoSAP.Text.ToString().TrimEnd();
                addListMaterial["Código de Diseño"] = txtCodigoDiseno.Text;
                addListMaterial["Cortante"] = txtCortante.Text;
                addListMaterial["Medida"] = txtMedida.Text;
                addListMaterial["MDI"] = txtMDI.Text;
                if (ddlTipoMaterial.SelectedItem.Value.ToString() != "0")
                {
                    addListMaterial["Tipo Material"] = ddlTipoMaterial.SelectedItem.Value.ToString();
                }
                else
                {
                    addListMaterial["Tipo Material"] = null;
                }
                addListMaterial["Carga de Laca"] = txtCargaLaca.Text;
                addListMaterial["Plano"] = txtPlano.Text;
                addListMaterial["Pharmacode"] = txtPharmacode.Text;
                addListMaterial["Nro Troquel"] = txtTroquel.Text;
                    addListMaterial["Datos de Cobertura"] = txtCoberturaMaterial.Text;
                    addListMaterial["Código de Especificación"] = txtCodEspecificacion.Text;
                    addListMaterial["Código de Metodología Analítica"] = txtCodMetodologia.Text;
                    addListMaterial.Update();
                System.Threading.Thread.Sleep(1500);

                    LimpiarPanelMateriales();


                }
            catch (Exception ex)
            {
                    //Errores.Text = Errores.Text + " " + ex.Message;

                }
            }
            else
            {

                lblMensajeErrorMaterial.Visible = true;
            }

        }
        protected void DelMaterial(object sender, EventArgs e)
        {

            try
            {
                SPList lstList;
                lstList = SPContext.Current.Web.Lists["Solicitud - Producto Material"];

                SPListItem addListMaterial;

                addListMaterial = lstList.GetItemById(Convert.ToInt32(iMaterial.Value));

                addListMaterial.Delete();

                System.Threading.Thread.Sleep(1500);

                LimpiarPanelMateriales();
            }
            catch (Exception ex)
            {
                //Errores.Text = Errores.Text + " " + ex.Message;

            }
        }
        protected void UpdMaterial(object sender, EventArgs e)
        {
            if (bValidarMateriales() == true)
            {
                lblMensajeErrorMaterial.Visible = false;
                try
                {
                    SPList lstList;
                    lstList = SPContext.Current.Web.Lists["Solicitud - Producto Material"];

                    SPListItem addListMaterial;

                    addListMaterial = lstList.GetItemById(Convert.ToInt32(iMaterial.Value));
                    SPFieldLookupValueCollection strProducto = new SPFieldLookupValueCollection();
                    foreach (ListItem xProducto in lbxProductoMaterial.Items)
                    {
                        if (xProducto.Selected == true)
                        {
                            strProducto.Add(new SPFieldLookupValue(xProducto.Value));
                        }
                    }
                    addListMaterial["Producto"] = strProducto;
                    addListMaterial["Title"] = txtMaterial.Text;
                    addListMaterial["Código SAP"] = txtMaterialCodigoSAP.Text.ToString().TrimEnd();
                    addListMaterial["Código de Diseño"] = txtCodigoDiseno.Text;
                    addListMaterial["Cortante"] = txtCortante.Text;
                    addListMaterial["Medida"] = txtMedida.Text;
                    addListMaterial["MDI"] = txtMDI.Text;
                    if (ddlTipoMaterial.SelectedItem.Value.ToString() != "0")
                    {
                        addListMaterial["Tipo Material"] = ddlTipoMaterial.SelectedItem.Value.ToString();
                    }
                    else
                    {
                        addListMaterial["Tipo Material"] = null;
                    }
                    addListMaterial["Carga de Laca"] = txtCargaLaca.Text;
                    addListMaterial["Plano"] = txtPlano.Text;
                    addListMaterial["Pharmacode"] = txtPharmacode.Text;
                    addListMaterial["Nro Troquel"] = txtTroquel.Text;
                    addListMaterial["Datos de Cobertura"] = txtCoberturaMaterial.Text;
                    addListMaterial["Código de Especificación"] = txtCodEspecificacion.Text;
                    addListMaterial["Código de Metodología Analítica"] = txtCodMetodologia.Text;
                    addListMaterial.Update();
                    System.Threading.Thread.Sleep(1500);

                    LimpiarPanelMateriales();


                }
                catch (Exception ex)
                {
                    //Errores.Text = Errores.Text + " " + ex.Message;

                }
            }
            else {

                lblMensajeErrorMaterial.Visible = true;
            }
        }

        protected void SeguridadPanelProductoMaterial()
        {
            SPUser currentUser = SPContext.Current.Web.CurrentUser;
            if (Funciones_Comunes.UsuarioGrupo(currentUser, Funciones_Comunes.iDevolverIdSector("Desarrollo")) == false)
            {
                lbxProductoMaterial.Enabled = false;
                txtMaterial.Enabled = false;
                txtMaterialCodigoSAP.Enabled = false;
                ddlTipoMaterial.Enabled = false;
                txtCodigoDiseno.Enabled = false;
                txtCortante.Enabled = false;
                txtMedida.Enabled = false;
                txtMDI.Enabled = false;
                txtCargaLaca.Enabled = false;
                txtPlano.Enabled = false;
                txtPharmacode.Enabled = false;
                txtTroquel.Enabled = false;
                txtCoberturaMaterial.Enabled = false;
                txtCodEspecificacion.Enabled = false;
                txtCodMetodologia.Enabled = false;
            }
            else{
                lbxProductoMaterial.Enabled = true;
                txtMaterial.Enabled = true;
                txtMaterialCodigoSAP.Enabled = true;
                ddlTipoMaterial.Enabled = true;
                txtCodigoDiseno.Enabled = true;
                txtCortante.Enabled = true;
                txtMedida.Enabled = true;
                txtMDI.Enabled = true;
                txtCargaLaca.Enabled = true;
                txtPlano.Enabled = true;
                txtPharmacode.Enabled = true;
                txtTroquel.Enabled = true;
                txtCoberturaMaterial.Enabled = false;
                txtCodEspecificacion.Enabled = true;
                txtCodMetodologia.Enabled = true;
            }

            if (Funciones_Comunes.UsuarioGrupo(currentUser, Funciones_Comunes.iDevolverIdSector("Planificación")) == true)
            {
                txtCoberturaMaterial.Enabled = true;
            }


            }

            protected void RefMaterial(object sender, EventArgs e)
        {

            try
            {

                LimpiarPanelMateriales();
            }
            catch (Exception ex)
            {
                //Errores.Text = Errores.Text + " " + ex.Message;

            }

        }
        protected bool bValidarMateriales() {
            Boolean bResult = true;
            bResult = false;
            foreach (ListItem xProducto in lbxProductoMaterial.Items)
            {
                if (xProducto.Selected == true)
                {
                    bResult = true;
                }
            }
            
            if (bResult == true) {
                if (txtMaterial.Text == "")
                {
                    bResult = false;
                    lblMensajeErrorMaterial.Text = "Datos obligatorios: Material";
                    txtMaterial.Focus();
                }
            }
            
            if (bResult == true) {
                if (txtMaterialCodigoSAP.Text == "")
                {
                    bResult = false;
                    lblMensajeErrorMaterial.Text = "Datos obligatorios: Código SAP";
                    txtMaterialCodigoSAP.Focus();
                }
                else {


                }
            }

            if (bResult == true)
            {
                if (txtCodigoDiseno.Text == "")
                {
                    bResult = false;
                    lblMensajeErrorMaterial.Text = "Datos obligatorios: Código Diseño";
                    txtCodigoDiseno.Focus();
                }
                else
                {


                }
            }
            if (bResult == true) {
                if (txtMaterialCodigoSAP.Text.ToString().TrimEnd() != "N/A")
                {
                    SPQuery qryTareas = new SPQuery();
                    SPList lstList;
                    String strQuery = "";
                    lstList = SPContext.Current.Web.Lists["Solicitud - Producto Material"];
                    qryTareas = new SPQuery(lstList.Views["Todos los elementos"]);

                    String sOrden = string.Format("<OrderBy><FieldRef Name='{0}' Ascending='{1}' /></OrderBy>", "ID", "False");
                    strQuery = "<Eq><FieldRef Name='C_x00f3_digo_x0020_SAP' /><Value Type='Text'>" + txtMaterialCodigoSAP.Text.ToString().TrimEnd() + "</Value></Eq>";

                    if (!string.IsNullOrEmpty(strQuery))
                    {
                        strQuery = "<Where>" + strQuery + "</Where>";
                    }

                    qryTareas.Query = strQuery;
                    qryTareas.RowLimit = 500;

                    SPListItemCollection lstMateriales = lstList.GetItems(qryTareas);
                    foreach (SPListItem str in lstMateriales)
                    {
                        if (Convert.ToInt32(str["Solicitud"].ToString().Split(';')[0].ToString()) != idDocument)
                        {
                            SPList lSolicitudes = SPContext.Current.Web.Lists["Solicitudes"];
                            SPListItem itmSolicitud = lSolicitudes.GetItemById(Convert.ToInt32(str["Solicitud"].ToString().Split(';')[0].ToString()));
                            if (itmSolicitud["Estado"].ToString() != "Cancelado")
                            {
                                bResult = false;
                                lblMensajeErrorMaterial.Text = "El Código SAP ingresado está asociado a otro material. Solicitud: " + itmSolicitud.Title.ToString();
                                txtMaterialCodigoSAP.Focus();
                                break;
                            }
                        }


                    }
                }

            }


            
            return bResult;
        }
        protected void LimpiarPanelMateriales() {
            iMaterial.Value = "0";
            txtMaterial.Text = "";
            txtMaterialCodigoSAP.Text = "";
            txtCortante.Text = "";
            txtMedida.Text = "";
            txtMDI.Text = "";
            ddlTipoMaterial.SelectedValue = "0";
            txtCodigoDiseno.Text = "";
            txtCargaLaca.Text = "";
            txtPlano.Text = "";
            txtPharmacode.Text = "";
            txtTroquel.Text = "";
            txtCoberturaMaterial.Text = "";
            txtCodEspecificacion.Text = "";
            txtCodMetodologia.Text = "";

            ActualizarListaProductos();

            btnAddMaterial.Enabled = true;
            btnUpdMaterial.Enabled = false;
            btnDelMaterial.Enabled = false;

            ArmarPanelMateriales(Convert.ToInt32(iPaginaMaterial.Text));
            lblCoberturaMaterial.ForeColor = System.Drawing.Color.Black ;
        }
        protected void btnAnteriorMaterial_Click(object sender, EventArgs e)
        {
            iPaginaMaterial.Text = (Convert.ToInt32(iPaginaMaterial.Text) - 1).ToString();
            //ArmarPanelMaterials(Convert.ToInt32(iPaginaMaterial.Text) - 1);
        }
        protected void btnSiguienteMaterial_Click(object sender, EventArgs e)
        {
            iPaginaMaterial.Text = (Convert.ToInt32(iPaginaMaterial.Text) + 1).ToString();
            //ArmarPanelMaterials(Convert.ToInt32(iPaginaMaterial.Text) + 1);
        }
        protected void btnVolver_Click(object sender, EventArgs e)
        {
            Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/SolicitudesDiseno/ProcesandoSolicitud.aspx?ID=" + Request["ID"]);
        }

        protected string RemoveCharacters(object String)
        {
            string s1 = String.ToString();
            string newString = Regex.Replace(s1, @"#[\d]\d+([,;\s]+\d+)*;", string.Empty);
            newString = Regex.Replace(newString, "#", " ");
            return newString.ToString();
        }
    }
}
