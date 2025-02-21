using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dealogikal.Utils;
using System.Web.Security;
using Dealogikal.Database;
using Dealogikal.ViewModel;

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
        public ActionResult CreateAccount(userAccount ua, string email, DateTime? birthdate, string firstName, string lastName ,string department, string position, string address, string barangay, string city, string zipcode, string phone, DateTime dateHired)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("CreateAccount");
                }

                if (_AccManager.EmployeeInfoSignup(birthdate, position, department, ua.employeeId, email, firstName, lastName, phone, address, zipcode, city, barangay, dateHired, ref ErrorMessage) != ErrorCode.Success)
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
            var model = new AccountViewModel
            {
                employeeInfos = _AccManager.GetAllEmployee() // Ensure this method returns a List<employeeInfo>
            };

            return View(model);
        }

        [HttpGet]
        public JsonResult GetEmployeeDetails(int id)
        {
            try
            {
                var employee = _AccManager.GetEmployeebyEmployeeId(id.ToString());

                if (employee == null)
                {
                    return Json(new { error = "Employee not found" }, JsonRequestBehavior.AllowGet);
                }

                var employeeDetails = new
                {
                    Email = employee.email,
                    Phone = employee.phone,
                    Address = $"{employee.address}, {employee.barangay}, {employee.city}, {employee.zipcode}",
                    Birthdate = employee.birthdate?.ToString("yyyy-MM-dd")
                };

                return Json(employeeDetails, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [Authorize]
        public ActionResult Dtr()
        {
            return View();
        }


    }
}