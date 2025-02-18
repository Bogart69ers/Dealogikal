using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dealogikal.Utils;
using System.Web.Security;
using Dealogikal.Database;

namespace Dealogikal.Controllers
{
    [Authorize]
    public class AdminController : BaseController
    {
        [Authorize]
        public ActionResult AdminDashboard()
        {
            return View();
        }



        [Authorize]
        public ActionResult CreateAccount()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateAccount(userAccount ua)
        {
            try
            {
                var currrentUser = _AccManager.GetUserByEmployeeId(User.Identity.Name);
                if (currrentUser == null)
                {
                    ModelState.AddModelError(string.Empty, "User not found,");
                    return View("CreateAccount");
                }

                if (_AccManager.CreateEmployee(ua, ref ErrorMessage) != ErrorCode.Success)
                {
                    ModelState.AddModelError(string.Empty, ErrorMessage);
                    return View("CreateAccount");
                }
                TempData["SuccessMessage"] = "Account created successfully.";
                return RedirectToAction("CreateAccount");

            }
            catch (Exception ex)
            {

                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                return View("CreateAccount");
            }
        }




        [Authorize]
        public ActionResult Accounts()
        {
            return View();
        }


        [Authorize]
        public ActionResult Dtr()
        {
            return View();
        }


    }
}