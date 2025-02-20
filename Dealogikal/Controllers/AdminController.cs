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
        public ActionResult Index()
        {
            return RedirectToAction("AdminDashboard");
        }

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
        public ActionResult CreateAccount(userAccount ua, string email, DateTime? birthdate, string firstName, string lastName ,string department, string position, string address, string barangay, string city, string zipcode, string phone)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("CreateAccount");
                }

                if (_AccManager.EmployeeInfoSignup(birthdate, position, department, ua.employeeId, email, firstName, lastName, phone, address, zipcode, city, barangay, ref ErrorMessage) != ErrorCode.Success)
                {
                    ViewBag.ErrorMessage = ErrorMessage;
                    return View("CreateAccount");
                }


                if (_AccManager.CreateEmployee(ua, ref ErrorMessage) != ErrorCode.Success)
                {
                    ViewBag.ErrorMessage = "Employee Already Exist";
                    return View("CreateAccount");
                }


                TempData["SuccessMessage"] = "Account created successfully.";

                ViewBag.SuccessMessage = TempData["SuccessMessage"];
                ViewBag.ErrorMessage = ErrorMessage;

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