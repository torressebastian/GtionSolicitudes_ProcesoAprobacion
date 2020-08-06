using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;

namespace SolicitudesDiseno_Solicitudes.Layouts.SolicitudesDiseno
{
    public partial class ProcesandoSolicitud : LayoutsPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        
        {
            System.Threading.Thread.Sleep(1000);

            Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/SolicitudesDiseno/AprobacionSolicitud.aspx?ID=" + Request["ID"] + "&Origen=T");

        
        }
    }
}
