﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dealogikal.Database;
using System.Web.Security;
using System.IO;
using Dealogikal.Repository;
using System.Globalization;
using Dealogikal.Utils;



namespace Dealogikal.Controllers
{
    [Authorize(Roles = "Employee")]
    public class HomeController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            var user = _AccManager.GetUserByEmployeeId(User.Identity.Name);

            if (user == null)
            {
                return RedirectToAction("Login", "Home"); // Redirect to login if no user is found
            }

            switch (user.role1.roleName)
            {
                case Constant.Role_HR:
                    return RedirectToAction("AdminDashboard", "Admin");

                case Constant.Role_Employee:
                    return RedirectToAction("Dashboard", "Home");

                default:
                    return RedirectToAction("Login", "Home"); // Handle unexpected roles
            }
        }


        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Error = string.Empty;
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(string employeeId, string password, string returnUrl)
        {
            if (_AccManager == null)
            {
                ViewBag.Error = "Account manager is not initialized";
                return View();
            }

            if (_AccManager.SignIn(employeeId, password, ref ErrorMessage) == ErrorCode.Success)
            {
                var user = _AccManager.GetUserByEmployeeId(employeeId);
                if (user == null)
                {
                    ViewBag.Error = "User not found";
                    return View();
                }
                var info = _AccManager.GetEmployeebyEmployeeId(employeeId);
                if (info == null)
                {
                    ViewBag.Error = "Employee Information not found";
                    return View();
                }

                if (info != null && info.status == 0)
                {
                    return RedirectToAction("InActiveAccount", "Home");
                }

                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                    1, 
                    employeeId, 
                    DateTime.Now, 
                    DateTime.Now.AddDays(30), 
                    true, 
                    "", 
                    FormsAuthentication.FormsCookiePath 
                );

                // Encrypt the ticket
                string encryptedTicket = FormsAuthentication.Encrypt(ticket);

                // Create the authentication cookie
                HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                authCookie.Expires = ticket.Expiration; // Set expiration
                authCookie.HttpOnly = true; // Prevent JavaScript access

                // Add the cookie to the response
                Response.Cookies.Add(authCookie);


                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                if (user.role1 == null)
                {
                    ViewBag.Error = "User role is not defined.";
                    return View();
                }

                switch (user.role1.roleName)
                {
                    case Constant.Role_HR:
                        return RedirectToAction("AdminDashboard", "Admin");
                    case Constant.Role_Employee:
                        return RedirectToAction("Dashboard", "Home");
                }
            }

            ViewBag.Error = ErrorMessage;

            return View();

        }

        [Authorize]
        public ActionResult Dashboard()
        {
            var user = _AccManager.GetEmployeebyEmployeeId(User.Identity.Name);

            ViewBag.Name = user.firstName + " " + user.lastName;

            return View();

        }


        [Authorize]
        public ActionResult Dtr()
        {
            return View();
        }


        [Authorize]
        public ActionResult LeaveRequest()
        {
            return View();
        }

        [Authorize]
        public ActionResult OvertimeRequest()
        {
            return View();
        }

        [Authorize]
        public ActionResult DTRHistory()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public ActionResult PageNotFound()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult InActiveAccount()
        {
            return View();
        }
    }
}