using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace Dealogikal
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

        }

        protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        {
            HttpCookie authCookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie != null)
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket != null && !ticket.Expired)
                {
                    // Example: Assign roles based on the user's role number
                    string[] roles;
                    switch (ticket.Name) // ticket.Name contains employeeId
                    {
                        case "admin":
                            roles = new string[] { "HR" }; // Admin Role
                            break;
                        case "employee":
                            roles = new string[] { "Employee" }; // Employee Role
                            break;
                        default:
                            roles = new string[] { "Employee" }; // Default role if undefined
                            break;
                    }

                    // Set the user context with the roles
                    Context.User = new System.Security.Principal.GenericPrincipal(
                        new FormsIdentity(ticket), roles
                    );
                }
            }
        }


    }
}
