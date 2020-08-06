using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Text;
using System.Collections.Specialized;
using Microsoft.SharePoint.Utilities;

namespace SolicitudesDiseno_Solicitudes.Layouts.SolicitudesDiseno
{
    public partial class AsignarTareaSolicitud : LayoutsPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnAceptar_Click(object sender, EventArgs e)
        {
            Int32 idDocument = Convert.ToInt32(Request["ID"]);
            Int32 IdTareaBitacora = Convert.ToInt32(Request["IDTarea"]);
            if (txtUsuario.CommaSeparatedAccounts.ToString() != "")
            {
                using (SPSite site = new SPSite(SPContext.Current.Site.Url))
                {
                    using (SPWeb web = site.OpenWeb("SolicitudesDiseno"))
                    {
                        SPList lBitacora = web.Lists["Bitácora Solicitudes"];
                        SPListItem itmBitacora = lBitacora.GetItemById(IdTareaBitacora);
                        if (tblDatosAsignarUsuario.Visible != false)
                        {
                            SPUser oUser = web.EnsureUser(txtUsuario.CommaSeparatedAccounts.ToString());
                            itmBitacora["Asignado"] = oUser;
                            //itmBitacora["Revisor Asignado"] = oUser;
                        }

                        itmBitacora.Update();

                        StringBuilder strCuerpoAnuncio = new StringBuilder();
                        String strCabeceraMail = "";
                        strCuerpoAnuncio = strCuerpoAnuncio.Append("</tr>");
                        string strResponsable = "";
                        string strDocumentoAsociado, strIdDocumentoAsociado;
                        strDocumentoAsociado = itmBitacora["Solicitud asociada"].ToString().Split('#')[1].ToString();
                        strIdDocumentoAsociado = itmBitacora["Solicitud asociada"].ToString().Split(';')[0].ToString();
                        string strLinkPaginaTarea = SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/SolicitudesDiseno/AprobacionSolicitud.aspx?ID=" + strIdDocumentoAsociado + "&Origen=T";
                        strCabeceraMail = "Se le ha asignado la tarea " + itmBitacora.Title.ToString() + " para la solicitud " + strDocumentoAsociado + ".";
                        strCuerpoAnuncio = strCuerpoAnuncio.Append("<b>Fecha de Vencimiento de la tarea:</b> " + Convert.ToDateTime(itmBitacora["Fecha de Fin"].ToString()).ToShortDateString() + "<br /><br />");
                        strCuerpoAnuncio = strCuerpoAnuncio.Append("Para continuar con el proceso, ingrese a la tarea para completarla: " + @"<a href='" + strLinkPaginaTarea + "'>" + itmBitacora.Title.ToString() + "</a><br/>");

                        string fieldValue = itmBitacora["Asignado"].ToString();
                        SPFieldUserValueCollection users = new SPFieldUserValueCollection(web, fieldValue);

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
                                    strResponsable = strResponsable + user.Email.ToString() + ";";
                                }

                            }

                            // Process user
                        }

                        string emailBody = " ";
                        emailBody = emailBody + "</tr></table>";
                        StringDictionary headers = new StringDictionary();
                        headers.Add("to", strResponsable);// sDevolverMailUsuario(strResponsable, properties));
                        headers.Add("from", web.Title.ToString());
                        headers.Add("subject", itmBitacora.Title.ToString() + " - " + strDocumentoAsociado);
                        headers.Add("content-type", "text/html");
                        SPUtility.SendEmail(web, headers, strCabeceraMail + "<br /><br />" + strCuerpoAnuncio.ToString() + emailBody);
                        emailBody = "";



                    }

                }



                Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/SolicitudesDiseno/ProcesandoSolicitud.aspx?ID=" + Request["ID"]);
            }
            else
            {
                lblMensajeError.Text = "Se debe indicar el usuario al que se va a asignar a tarea.";
                lblMensajeError.Visible = true;
                txtUsuario.Focus();
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect(SPContext.Current.Site.Url + "/SolicitudesDiseno/_layouts/15/SolicitudesDiseno/ProcesandoSolicitud.aspx?ID=" + Request["ID"]);

        }
    }
}
