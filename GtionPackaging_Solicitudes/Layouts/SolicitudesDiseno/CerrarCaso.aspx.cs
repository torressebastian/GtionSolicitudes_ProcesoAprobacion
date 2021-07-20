using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;

namespace GtionPackaging_Solicitudes.Layouts
{
    public partial class CerrarCaso : LayoutsPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnContinuarSi_Click(object sender, EventArgs e)
        {
            btnContinuarSi.Visible = false;
            btnContinuarNo.Visible = false;
           
            btnAceptar.Visible = true;
            btnCancelar.Visible = true;
            lblMotivoCierre.Visible = true;
            txtMotivoCierre.Visible = true;


        }

        protected void btnContinuarNo_Click(object sender, EventArgs e)
        {
            Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/SolicitudesDiseno/ProcesandoSolicitud.aspx?ID=" + Request["ID"]);

        }

        protected void btnAceptar_Click(object sender, EventArgs e)
        {
            if (txtMotivoCierre.Text.ToString() != "")
            {
                Int32 idDocument = Convert.ToInt32(Request["ID"]);
                Int32 idTarea = Convert.ToInt32(Request["Tarea"]);
                using (SPSite site = new SPSite(SPContext.Current.Site.Url))
                {
                    using (SPWeb web = site.OpenWeb("SolicitudesDiseno"))
                    {
                        SPList lBitacora = web.Lists["Bitácora Solicitudes"];
                        SPListItem itmBitacora = lBitacora.GetItemById(idTarea);
                        itmBitacora["Comentarios"] = txtMotivoCierre.Text.ToString();
                        itmBitacora["Asignado"] = SPContext.Current.Web.CurrentUser;
                        itmBitacora["Estado"] = "Cerrado";
                        itmBitacora.Update();

                        SPList lDocumentos = web.Lists["Solicitudes"];
                        SPListItem itmDocumento = lDocumentos.GetItemById(idDocument);

                        itmDocumento["Estado"] = "Completado";
                        itmDocumento["Fecha Fin Solicitud"] = DateTime.Now;
                        itmDocumento.Update();

                        Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/SolicitudesDiseno/ProcesandoSolicitud.aspx?ID=" + Request["ID"]);
                    }

                }
            }
            else
            {
                lblMensajeError.Text = "Se debe indicar el motivo del cierre de la solicitud.";
                lblMensajeError.Visible = true;
                txtMotivoCierre.Focus();
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/SolicitudesDiseno/ProcesandoSolicitud.aspx?ID=" + Request["ID"]);
        }
    }
}
