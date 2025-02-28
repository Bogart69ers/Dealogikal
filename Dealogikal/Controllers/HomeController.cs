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
using Dealogikal.ViewModel;



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
            var dtrRec = _DtrManager.GetRecordsByEmployeeId(user.employeeId);

            var currentDtr = _DtrManager.GetAllDtr().FirstOrDefault(r => r.employeeId == user.employeeId && r.date == DateTime.Now.Date);

            ViewBag.Name = user.firstName + " " + user.lastName;

            var model = new AccountViewModel
            {
                employeeInfos = _AccManager.GetAllEmployee(),
                dtr = currentDtr,
                dtrRecords = _DtrManager.GetAllDtr()
            };

            return View(model);

        }

        [Authorize]
        public ActionResult Dtr()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult Dtr(dtrRecords dtr, int? recordId, string action)
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
                    return RedirectToAction("Dashboard");
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
                        return RedirectToAction("Dashboard");
                    }
                }
                else
                {
                    ViewBag.Error = "Record ID is missing for Break In.";
                    return RedirectToAction("Dashboard");
                }
            }
            else if (action == "BreakOut")
            {
                result = _DtrManager.UpdateBreakOut(currentUser, recordId.Value, ref errMsg);
                if (result != ErrorCode.Success)
                {
                    ViewBag.Error = "Error Updating Break Out: " + errMsg;
                    return RedirectToAction("Dashboard");
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
                        return RedirectToAction("Dashboard");
                    }
                }
                else
                {
                    ViewBag.Error = "Record ID is missing for Time Out.";
                    return RedirectToAction("Dashboard");
                }
            }

            return RedirectToAction("Dashboard");
        }


        [Authorize]
        public ActionResult LeaveRequest()
        {
            var user = _AccManager.GetEmployeebyEmployeeId(User.Identity.Name);
            var currentUserId = User.Identity.Name;

            ViewBag.LeaveCount = user.leaveCount;

            var requests = _RequestManager.GetLeaveRequestByEmployeeId(currentUserId);

            var model = new AccountViewModel
            {
                leaveRequests = requests,

            };
           
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult LeaveRequest(leaveRequest lr)
        {
            try
            {
                var user = User.Identity.Name;
                var userInfo = _AccManager.GetEmployeebyEmployeeId(user);
                string errMsg = string.Empty;

                if (userInfo == null)
                {
                    ViewBag.ErrorMessage = "User not found.";
                    return View("LeaveRequest");
                }

                // Check if Leave Type is "With Pay" and if user has available leave credits
                if (lr.leaveType == "leavewithpay")
                {
                    if (userInfo.leaveCount <= 0)
                    {
                        ViewBag.ErrorMessage = "You have no remaining leave balance.";
                        return View("LeaveRequest");
                    }
                }

                // Create Leave Request
                if (_RequestManager.CreateLeave(lr, user, ref errMsg) != ErrorCode.Success)
                {
                    ViewBag.ErrorMessage = errMsg;
                    return View("LeaveRequest");
                }

                // If Leave is "With Pay", Deduct Leave Count
                if (lr.leaveType == "leavewithpay")
                {
                    if (_AccManager.UpdateEmployeeLeaveCount(user, ref errMsg) != ErrorCode.Success)
                    {
                        ViewBag.ErrorMessage = errMsg;
                        return View("LeaveRequest");
                    }
                }

                return RedirectToAction("LeaveRequest");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("LeaveRequest");
            }
        }


        [Authorize]
        public ActionResult OvertimeRequest()
        {
            var user = _AccManager.GetEmployeebyEmployeeId(User.Identity.Name);
            var currentUserId = User.Identity.Name;

            ViewBag.LeaveCount = user.leaveCount;

            var requests = _RequestManager.GetOvertimeRequestByEmployeeId(currentUserId);

            var model = new AccountViewModel
            {
                overtimeRequests = requests,

            };

            return View(model);
        }


        [Authorize]
        [HttpPost]

        public ActionResult OvertimeRequest(overtimeRequest ot)
        {
            try
            {
                var user = User.Identity.Name;
                var userInfo = _AccManager.GetEmployeebyEmployeeId(user);
                string errMsg = string.Empty;

                if (userInfo == null)
                {
                    ViewBag.ErrorMessage = "User not found.";
                    return View("OvertimeRequest");
                }

                if (_RequestManager.CreateOvertime(ot, user,  ref errMsg) != ErrorCode.Success)
                {
                    ViewBag.ErrorMessage = errMsg;
                    return View("OvertimeRequest");
                }

                return RedirectToAction("OvertimeRequest");
                

            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("OvertimeRequest");
            }
        }

        [Authorize]
        public ActionResult DTRHistory()
        {
            var currentUserId = User.Identity.Name;
            var dtrHistory = _DtrManager.GetDtrHistoryByEmployeeId(currentUserId);

            var model = new AccountViewModel
            {
                dtrRecords = dtrHistory
            };

            return View(model);
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