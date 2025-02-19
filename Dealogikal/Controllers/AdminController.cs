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
            var user = _AccManager.GetEmployeebyEmployeeId(User.Identity.Name);

            ViewBag.Name = user.firstName + " " + user.lastName;

            return View();
        }



        [Authorize]
        public ActionResult CreateAccount()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateAccount(userAccount ua, string email, DateTime birthdate, string firstName, string lastName ,string department, string position, string address, string barangay, string city, string zipcode, string phone)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("CreateAccount");
                }

                var empInfo = _AccManager.EmployeeInfoSignup(birthdate, position, department, ua.employeeId, email, firstName, lastName, phone, address, zipcode, city, barangay, ref ErrorMessage);


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