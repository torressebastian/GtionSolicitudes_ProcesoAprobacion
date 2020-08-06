using System;
using System.ComponentModel;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using GtionPackaging_Solicitudes;

namespace SolicitudesDiseno_Solicitudes.WP_BotoneraSolicitud
{
    [ToolboxItemAttribute(false)]
    public partial class WP_BotoneraSolicitud : WebPart
    {
        // Uncomment the following SecurityPermission attribute only when doing Performance Profiling on a farm solution
        // using the Instrumentation method, and then remove the SecurityPermission attribute when the code is ready
        // for production. Because the SecurityPermission attribute bypasses the security check for callers of
        // your constructor, it's not recommended for production purposes.
        // [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Assert, UnmanagedCode = true)]
        public WP_BotoneraSolicitud()
        {
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            InitializeControl();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (bPuedeIniciar("Lanzamiento Internacional") == false) { cellLanzInternacional.Visible = false; }
            if (bPuedeIniciar("Lanzamiento Nacional") == false) { cellLanzNacional.Visible = false; }
            if (bPuedeIniciar("Modificación de Archivos (Desarrollo)") == false) { cellDesarrollo.Visible = false; }
            if (bPuedeIniciar("Modificación de Archivos (Marketing)") == false) { cellMarketing.Visible = false; }
            if (bPuedeIniciar("Modificación de Archivos (Planificación)") == false) { cellPlanificacion.Visible = false; }
            if (bPuedeIniciar("Modificación de Archivos (Registro)") == false) { cellRegistro.Visible = false; }
            if (bPuedeIniciar("Modificación de Archivos (Internacional)") == false) { cellInternacional.Visible = false; }

        }

        private Boolean bPuedeIniciar(String sTipoSolicitud) {
            Boolean bAuxResultado = false;

            Guid siteId = SPContext.Current.Site.ID;
            Guid webId = SPContext.Current.Web.ID;
            SPUser currentUser = SPContext.Current.Web.CurrentUser;

            //SPSecurity.RunWithElevatedPrivileges(delegate ()
            //{
            using (SPSite site = new SPSite(siteId))
            {
                using (SPWeb web = site.OpenWeb(webId))
                {

                    SPList lConfiguracionSolicitudes = web.Lists["Configuración Circuitos Solicitudes"];
                    SPQuery query = new SPQuery();
                    query.Query = "<Where><Eq><FieldRef Name=\"Title\" /><Value Type=\"Text\">" + sTipoSolicitud + "</Value></Eq></Where>";
                    query.RowLimit = 1;
                    query.ViewFields = "";
                    SPListItemCollection items = lConfiguracionSolicitudes.GetItems(query);
                    SPListItem item = items[0];
                    SPFieldLookupValueCollection lkSectorAlta = new SPFieldLookupValueCollection(item["Sector alta"].ToString());

                    foreach (SPFieldLookupValue itmSectorAlta in lkSectorAlta) {

                    
                    Int32 idSectorAlta = itmSectorAlta.LookupId;

                    if (Funciones_Comunes.UsuarioGrupo(currentUser, idSectorAlta) == true)
                    {
                        bAuxResultado = true;
                    }
                    }
                }
            }

            return bAuxResultado;

        }

        protected void btnLanzNacional_Click(object sender, EventArgs e)
        {
            Context.Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/start.aspx#/Lists/Solicitudes/NewForm.aspx?ContentTypeId=" + strGuidTipoContenido("Lanzamiento Nacional"));

        }

        protected void btnLanzInternacional_Click(object sender, EventArgs e)
        {
            Context.Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/start.aspx#/Lists/Solicitudes/NewForm.aspx?ContentTypeId=" + strGuidTipoContenido("Lanzamiento Internacional"));
        }

        protected String strGuidTipoContenido(String strTipoContenido) {

            String strAuxGuid = "";
            Guid siteId = SPContext.Current.Site.ID;
            Guid webId = SPContext.Current.Web.ID;

            using (SPSite site = new SPSite(siteId))
            {
                using (SPWeb web = site.OpenWeb(webId))
                {
                    SPList lDocumentos = web.Lists["Solicitudes"];
                    SPContentType spct = lDocumentos.ContentTypes[strTipoContenido];

                    strAuxGuid = spct.Id.ToString();

                }
            }


        return strAuxGuid;
        }

        protected void btnMarketing_Click(object sender, EventArgs e)
        {
            Context.Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/start.aspx#/Lists/Solicitudes/NewForm.aspx?ContentTypeId=" + strGuidTipoContenido("Modificación de Archivos (Marketing)"));

        }

        protected void btnDesarrollo_Click(object sender, EventArgs e)
        {
            Context.Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/start.aspx#/Lists/Solicitudes/NewForm.aspx?ContentTypeId=" + strGuidTipoContenido("Modificación de Archivos (Desarrollo)"));

        }

        protected void btnPlanificacion_Click(object sender, EventArgs e)
        {
            Context.Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/start.aspx#/Lists/Solicitudes/NewForm.aspx?ContentTypeId=" + strGuidTipoContenido("Modificación de Archivos (Planificación)"));

        }

        protected void btnRegistro_Click(object sender, EventArgs e)
        {
            Context.Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/start.aspx#/Lists/Solicitudes/NewForm.aspx?ContentTypeId=" + strGuidTipoContenido("Modificación de Archivos (Registro)"));

        }

        protected void btnModInternacional_Click(object sender, EventArgs e)
        {
            Context.Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/start.aspx#/Lists/Solicitudes/NewForm.aspx?ContentTypeId=" + strGuidTipoContenido("Modificación de Archivos (Internacional)"));
        }
    }
}
