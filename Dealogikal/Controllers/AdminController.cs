using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Dealogikal.Utils;
using System.Web.Security;
using Dealogikal.Database;
using Dealogikal.ViewModel;

namespace Dealogikal.Controllers
{
    [Authorize(Roles = "HR")]
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
            var dtrRec = _DtrManager.GetRecordsByEmployeeId(user.employeeId);

            var currentDtr = _DtrManager.GetAllDtr().FirstOrDefault(r => r.employeeId == user.employeeId && r.date == DateTime.Now.Date);

            ViewBag.Name = user.firstName;

            var today = DateTime.Now.Date;
            var lateThreshold = new TimeSpan(8, 0, 0); // 8:00 AM cutoff

            var lateEmployeesCount = _DtrManager.GetAllDtr()
                 .Where(dtr => dtr.date == today &&
                               dtr.timeIn.HasValue &&
                               dtr.timeIn.Value.TimeOfDay > lateThreshold)
                 .Select(dtr => dtr.employeeId)
                 .Distinct()
                 .Count();


            ViewBag.LateEmployeesCount = lateEmployeesCount;

            var model = new AccountViewModel
            {
                employeeInfos = _AccManager.GetAllEmployee(),
                leaveRequests = _RequestManager.GetAllLeaveRequest(),
                overtimeRequests = _RequestManager.GetAllOvertimeRequest(),
                dtr = currentDtr,
                dtrRecords = _DtrManager.GetAllDtr()
            };

            return View(model);
        }



        [Authorize]
        public ActionResult CreateAccount()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateAccount(userAccount ua, string email, DateTime? birthdate, string firstName, string lastName ,string department, string position, string address, string barangay, string city, string phone, DateTime dateHired, string corporation)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("CreateAccount");
                }

                if (_AccManager.EmployeeInfoSignup(birthdate, position, department, ua.employeeId, email, firstName, lastName, phone, address, city, barangay, dateHired, corporation, ref ErrorMessage) != ErrorCode.Success)
                {
                    ViewBag.ErrorMessage = ErrorMessage;
                    return View("CreateAccount");
                }


                if (_AccManager.CreateEmployee(ua, department, ref ErrorMessage) != ErrorCode.Success)
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
                    Address = $"{employee.address}, {employee.barangay}, {employee.city}",
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
            var currentUserId = User.Identity.Name;
            var dtrHistory = _DtrManager.GetDtrHistoryByEmployeeId(currentUserId);

            var model = new AccountViewModel
            {
                dtrRecords = dtrHistory
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Dtr(dtrRecords dtr, int? recordId,  string action)
        {
            var currentUser = User.Identity.Name;
            string errMsg = string.Empty;
            ErrorCode result;

            if (action == "TimeIn")
            {
                // Create a new record for the morning Time In.
                result = _DtrManager.CreateDtr(dtr, currentUser, ref errMsg);
                if (result != ErrorCode.Success)
                {
                    ViewBag.Error = "Error Creating DTR: " + errMsg;
                    return RedirectToAction("AdminDashboard");
                }
            }
            else if (action == "BreakIn")
            {
                // Update the current record with Break In time.
                if (recordId.HasValue)
                {
                    result = _DtrManager.UpdateBreakIn(currentUser, recordId.Value, ref errMsg);
                    if (result != ErrorCode.Success)
                    {
                        ViewBag.Error = "Error Updating Break In: " + errMsg;
                        return RedirectToAction("AdminDashboard");
                    }
                }
                else
                {
                    ViewBag.Error = "Record ID is missing for Break In.";
                    return RedirectToAction("AdminDashboard");
                }
            }
            else if (action == "BreakOut")
            {
                result = _DtrManager.UpdateBreakOut(currentUser, recordId.Value, dtr.workMode, ref errMsg);
                if (result != ErrorCode.Success)
                {
                    ViewBag.Error = "Error Updating Break Out: " + errMsg;
                    return RedirectToAction("AdminDashboard");
                }
            }
            else if (action == "TimeOut")
            {
                // Update the current record with Time Out.
                if (recordId.HasValue)
                {
                    result = _DtrManager.UpdateTimeOut(currentUser, recordId.Value, ref errMsg);
                    if (result != ErrorCode.Success)
                    {
                        ViewBag.Error = "Error Updating Time Out: " + errMsg;
                        return RedirectToAction("AdminDashboard");
                    }
                }
                else
                {
                    ViewBag.Error = "Record ID is missing for Time Out.";
                    return RedirectToAction("AdminDashboard");
                }
            }

            return RedirectToAction("AdminDashboard");
        }

        [Authorize]
        public ActionResult EmployeeDtr()
        {
            var model = new AccountViewModel
            {
                employeeInfos = _AccManager.GetAllEmployee(), 
                dtrRecords = _DtrManager.GetAllDtr()
            };

            return View(model);
        }

        [Authorize]
        public ActionResult LeaveRequest()
        {
            var model = new AccountViewModel
            {
                employeeInfos = _AccManager.GetAllEmployee(),
                leaveRequests = _RequestManager.GetAllLeaveRequestsDesc()
            };

            return View(model);
        } 

        [Authorize]
        public ActionResult OvertimeRequests()
        {
            var model = new AccountViewModel
            {
                employeeInfos = _AccManager.GetAllEmployee(),
                overtimeRequests = _RequestManager.GetAllOvertimeRequestsDesc()
            };

            return View(model);
        }

        [Authorize]
        public ActionResult MyProfile()
        {
            var currentUser = User.Identity.Name;
            var employee = _AccManager.GetEmployeebyEmployeeId(currentUser);
            var user = _AccManager.GetUserByEmployeeId(currentUser);

            var model = new AccountViewModel
            {
                employeeInfo = employee,
                userAccount = user
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult MyProfile(string phone, string email, string address, string barangay, string city, HttpPostedFileBase profilePicture)
        {
            if (ModelState.IsValid)
            {
                var currentUser = User.Identity.Name;

                // Retrieve the existing employee and user records
                var image = _ImgManager.GetImagebyEmployeeId(currentUser);
                var employee = _AccManager.GetEmployeebyEmployeeId(currentUser);
                var user = _AccManager.GetUserByEmployeeId(currentUser);

                if (employee == null || user == null)
                {
                    ModelState.AddModelError(String.Empty, "User not found.");
                    return View();
                }

                // Profile Picture Upload Handling
                if (profilePicture != null && profilePicture.ContentLength > 0)
                {
                    var uploadsFolderPath = Server.MapPath("~/UploadedFiles/");
                    if (!Directory.Exists(uploadsFolderPath))
                        Directory.CreateDirectory(uploadsFolderPath);

                    var profileFileName = Path.GetFileName(profilePicture.FileName);
                    var profileSavePath = Path.Combine(uploadsFolderPath, profileFileName);
                    profilePicture.SaveAs(profileSavePath);

                    var existingImage = _ImgManager.ListImageByEmployeeId(employee.employeeId).FirstOrDefault();
                    if (existingImage != null)
                    {
                        existingImage.imageFile = profileFileName;
                        if (_ImgManager.UpdateImg(existingImage, ref ErrorMessage) == ErrorCode.Error)
                        {
                            ModelState.AddModelError(String.Empty, ErrorMessage);
                            return View();
                        }
                    }
                    else
                    {
                        images img = new images
                        {
                            imageFile = profileFileName,
                            employeeId = employee.employeeId
                        };

                        if (_ImgManager.CreateImg(img, ref ErrorMessage) == ErrorCode.Error)
                        {
                            ModelState.AddModelError(String.Empty, ErrorMessage);
                            return View();
                        }
                    }

                }

                // Update Employee Information ONLY IF new values are provided
                employee.phone = !string.IsNullOrEmpty(phone) ? phone : employee.phone;
                employee.email = !string.IsNullOrEmpty(email) ? email : employee.email;
                employee.address = !string.IsNullOrEmpty(address) ? address : employee.address;
                employee.barangay = !string.IsNullOrEmpty(barangay) ? barangay : employee.barangay;
                employee.city = !string.IsNullOrEmpty(city) ? city : employee.city;

                if (_AccManager.UpdateEmployeeInformation(employee, ref ErrorMessage) == ErrorCode.Error)
                {
                    ModelState.AddModelError(String.Empty, ErrorMessage);
                    return View();
                }

                TempData["SuccessMessage"] = "Profile updated successfully.";
                return RedirectToAction("MyProfile");
            }

            return View();
        }










    }
}