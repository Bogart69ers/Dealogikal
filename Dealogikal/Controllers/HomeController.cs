using System;
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
    [Authorize]
    public class HomeController : BaseController
    {
        // GET: Home
        [Authorize]
        public ActionResult Index()
        {
            return View();
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

                FormsAuthentication.SetAuthCookie(employeeId, false);

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
                        return RedirectToAction("HRDashboard", "Admin");
                    case Constant.Role_Employee:
                        return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Error = ErrorMessage;

            return View();

        }

        [AllowAnonymous]
        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
    }
}