using Microsoft.SharePoint;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace GtionPackaging_Solicitudes
{
    public static class Funciones_Comunes
    {



        public static bool UsuarioGrupo(SPUser currentUser, Int32 idSector)
        {
            Guid siteId = SPContext.Current.Site.ID;
            Guid webId = SPContext.Current.Web.ID;
            Boolean bResult = false;

            if (idSector != 0)
            {
                SPSecurity.RunWithElevatedPrivileges(delegate ()
                {
                    using (SPSite site = new SPSite(siteId))
                    {
                        using (SPWeb web = site.OpenWeb(webId))
                        {
                            SPList lSectores = web.Lists["Sectores"];
                            SPListItem imSector = lSectores.GetItemById(idSector);

                            string fieldValue = imSector["Usuarios"].ToString();
                            SPFieldUserValueCollection users = new SPFieldUserValueCollection(web, fieldValue);

                            foreach (SPFieldUserValue uv in users)
                            {
                                if (uv.User != null)
                                {
                                    SPUser user = uv.User;
                                    if (user.LoginName == currentUser.LoginName)
                                    {
                                        bResult = true;
                                        break;
                                    }

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
                                                if (userName.Split('@')[0] == currentUser.Email.ToString().Split('@')[0])
                                                {
                                                    bResult = true;
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {

                                            if (user.LoginName == currentUser.LoginName)
                                            {
                                                bResult = true;
                                                break;
                                            }
                                        }
                                    }

                                }
                            }

                        }
                    }
                });
            }
            return bResult;
        }

        private static ArrayList GetADGroupUsers(string groupName)
        {
            ArrayList userNames = new ArrayList();
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain, "Baliarda.com", "sharepointservice", "Shrp8451");
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
                            userNames.Add(user.UserPrincipalName);
                        }
                    }
                }

            }
            return userNames;

        }

        public static Int32 iDevolverIdSector(String strSector)
        {
            Int32 iAuxSector = 0;
            using (SPSite site = new SPSite(SPContext.Current.Site.Url))
            {
                using (SPWeb web = site.OpenWeb("SolicitudesDiseno"))
                {
                    SPList lSectores = web.Lists["Sectores"];

                    SPQuery query = new SPQuery();
                    query.Query = "<Where><Eq><FieldRef Name=\"Title\" /><Value Type=\"Text\">" + strSector + "</Value></Eq></Where>";
                    query.RowLimit = 1;
                    query.ViewFields = "";
                    SPListItemCollection items = lSectores.GetItems(query);
                    SPListItem item = items[0];
                    iAuxSector = item.ID;

                }
            }
            return iAuxSector;

        }

        public static DateTime dtFechaVencimiento(Int32 iDiasVencimiento)
        {
            DateTime auxFechaVencimiento = DateTime.Now;

            auxFechaVencimiento = DateTime.Now.AddDays(iDiasVencimiento);

            if (auxFechaVencimiento.DayOfWeek == DayOfWeek.Saturday)
            {
                auxFechaVencimiento = auxFechaVencimiento.AddDays(2);
            }

            if (auxFechaVencimiento.DayOfWeek == DayOfWeek.Sunday)
            {
                auxFechaVencimiento = auxFechaVencimiento.AddDays(1);
            }

            return auxFechaVencimiento;
        }

        public static string RemoveCharacters(object String)
        {
            string s1 = String.ToString();
            string newString = Regex.Replace(s1, @"[\d](\d+)*;#", string.Empty);
            newString = Regex.Replace(newString, "#", " ");
            return newString.ToString();
        }

        public static string DevolverTipoSolicitud(String sTipoSolicitud) {
            string strAuxTipoSolicitud = "";

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
                    strAuxTipoSolicitud = item["Tipo Solicitud"].ToString();
                }
            }

                    return strAuxTipoSolicitud;

        }

    }
}
