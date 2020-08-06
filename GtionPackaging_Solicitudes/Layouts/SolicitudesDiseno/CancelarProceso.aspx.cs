using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;

namespace GtionPackaging_Solicitudes.Layouts.SolicitudesDiseno
{
    public partial class CancelarProceso : LayoutsPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }


        protected void btnAceptar_Click(object sender, EventArgs e)
        {
            Int32 idDocument = Convert.ToInt32(Request["ID"]);
            
            using (SPSite site = new SPSite(SPContext.Current.Site.Url))
            {
                using (SPWeb web = site.OpenWeb("SolicitudesDiseno"))
                {
                    SPList lBitacora = web.Lists["Bitácora Solicitudes"];

                    SPQuery queryDA = new SPQuery();
                    queryDA.Query = string.Concat("<Where><Eq><FieldRef Name='Solicitud_x0020_asociada' LookupId='TRUE'/>", "<Value Type='Lookup'>", idDocument, "</Value></Eq></Where><OrderBy>  <FieldRef Name='ID' Ascending='False'/></OrderBy>");
                    SPListItemCollection itemColl = null;
                    itemColl = lBitacora.GetItems(queryDA);

                    foreach (SPListItem itmTarea in itemColl)
                    {
                        if (itmTarea["Estado"].ToString() == "Pendiente") {
                            itmTarea["Estado"] = "Cancelado";
                            itmTarea["Fecha de Fin"] = DateTime.Now;
                            itmTarea.Update();

                        }
                    }

                    

                    SPList lSolicitudes = web.Lists["Solicitudes"];
                    SPListItem itmSolicitud = lSolicitudes.GetItemById(idDocument);
                    itmSolicitud["Estado"] = "Cancelado";
                    if (txtMotivoSuspension.Text != "")
                    {
                        
                        itmSolicitud["Detalle Solicitud"] = itmSolicitud["Detalle Solicitud"].ToString() + "<br/> Cancelado el: " + DateTime.Now.ToShortDateString() + " - Motivo: " + txtMotivoSuspension.Text.ToString();
                    }
                    else {
                        itmSolicitud["Detalle Solicitud"] = itmSolicitud["Detalle Solicitud"].ToString() + "<br/> Cancelado el: " + DateTime.Now.ToShortDateString() ;

                    }
                    itmSolicitud.Update();




                }





                Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/SolicitudesDiseno/ProcesandoSolicitud.aspx?ID=" + Request["ID"]);
            }

        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/SolicitudesDiseno/ProcesandoSolicitud.aspx?ID=" + Request["ID"]);

        }

    }
}
