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
using ClosedXML.Excel;
using System.Globalization;



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


        [Authorize]
        [HttpPost]
        public JsonResult CreateFeedback(feedback fb)
        {
            try
            {
                if (fb == null)
                {
                    return Json(new { success = false, message = "Feedback data is null." });
                }

                // Manually set the dateCreated since it is not submitted from the form
                fb.dateCreated = DateTime.Now;
                fb.status = 0;

                if (_FeedbackManager.CreateFeedback(fb, ref ErrorMessage) != ErrorCode.Success)
                {
                    return Json(new { success = false, message = "Feedback Failed to create." });
                }

                return Json(new { success = true, message = "Thank you for your feedback!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }




        [Authorize]
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
                employeeInfos = _AccManager.GetAllEmployee(),
                images = _ImgManager.GetAllImages()               
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
                dtrRecords = _DtrManager.GetAllDtrDesc()
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


        [Authorize]
        public ActionResult DownloadEmployeeDTRExcel(string employeeId, int month, string cutoff)
        {
            var dtrRecords = _DtrManager.GetEmployeeDTR(employeeId, month, cutoff);
            var employee = _AccManager.GetEmployeebyEmployeeId(employeeId);

            if (employee == null)
            {
                TempData["Error"] = "Employee not found.";
                return RedirectToAction("EmployeeDtr");
            }

            string initials = $"{employee.firstName?.FirstOrDefault()}{employee.lastName?.FirstOrDefault()}".ToUpper();
            string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);

            // Get the cutoff date range
            List<DateTime> cutoffDates = GenerateCutoffDates(month, cutoff);
            int year = cutoffDates.First().Year;

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(initials);

                // TITLE ROW
                worksheet.Range("B1:J1").Merge().Value = "Bi-Weekly TimeSheet Calculator";
                worksheet.Cell("B1").Style.Font.SetBold().Font.FontSize = 16;
                worksheet.Cell("B1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // COMPANY NAME
                worksheet.Range("B2:J2").Merge().Value = "DEALOGIKAL CORP.";
                worksheet.Cell("B2").Style.Font.SetBold();
                worksheet.Cell("B2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // EMPLOYEE INFO
                worksheet.Cell("C3").Value = "Employee Name:";
                worksheet.Cell("D3").Value = $"{employee.firstName} {employee.lastName}";

                worksheet.Cell("C4").Value = "Department:";
                worksheet.Cell("D4").Value = employee.department;

                worksheet.Cell("C5").Value = "Paid Overtime:";
                worksheet.Cell("D5").Value = "No";

                worksheet.Cell("C7").Value = "Year";
                worksheet.Cell("C8").Value = year;

                worksheet.Cell("D7").Value = "Month";
                worksheet.Cell("D8").Value = monthName;

                worksheet.Cell("E7").Value = "Weekend";
                worksheet.Cell("E8").Value = "Sat & Sun";

                worksheet.Cell("E7").Style.Fill.BackgroundColor = XLColor.LightPink;
                worksheet.Range(7, 3, 7, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Range(8, 3, 8, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Range(7, 3, 7, 4).Style.Fill.BackgroundColor = XLColor.LightBlue;
                worksheet.Range(7, 3, 8, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range(7, 3, 8, 5).Style.Border.InsideBorder = XLBorderStyleValues.Thin;



                int startRow = 11;

                // HEADERS
                worksheet.Cell(startRow, 2).Value = "Day";
                worksheet.Cell(startRow, 3).Value = "Date";
                worksheet.Cell(startRow, 4).Value = "Time In";
                worksheet.Cell(startRow, 5).Value = "Break In";
                worksheet.Cell(startRow, 6).Value = "Break Out";
                worksheet.Cell(startRow, 7).Value = "Time Out";
                worksheet.Cell(startRow, 8).Value = "Break";

                worksheet.Range(startRow, 2, startRow, 8).Style.Font.SetBold().Font.FontColor = XLColor.DarkBlue;
                worksheet.Range(startRow, 2, startRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Range(startRow, 2, startRow, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range(startRow, 2, startRow, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                int row = startRow + 1;

                // Create a dictionary for quick lookup of DTR records by date
                var dtrDict = dtrRecords.ToDictionary(d => d.date.Date, d => d);

                foreach (var date in cutoffDates)
                {
                    worksheet.Cell(row, 2).Value = date.DayOfWeek.ToString();
                    worksheet.Cell(row, 3).Value = date.Day.ToString("00");

                    bool isSaturday = date.DayOfWeek == DayOfWeek.Saturday;
                    bool isSunday = date.DayOfWeek == DayOfWeek.Sunday;

                    dtrRecords dtr;

                    if (dtrDict.TryGetValue(date.Date, out dtr))
                    {
                        worksheet.Cell(row, 4).Value = dtr.timeIn.HasValue ? dtr.timeIn.Value.ToString("HH:mm") : "--";
                        worksheet.Cell(row, 5).Value = dtr.breakIn.HasValue ? dtr.breakIn.Value.ToString("HH:mm") : "--";
                        worksheet.Cell(row, 6).Value = dtr.breakOut.HasValue ? dtr.breakOut.Value.ToString("HH:mm") : "--";
                        worksheet.Cell(row, 7).Value = dtr.timeOut.HasValue ? dtr.timeOut.Value.ToString("HH:mm") : "--";
                        worksheet.Cell(row, 8).Value = "1.0"; // Or calculate dynamically
                    }
                    else
                    {
                        // No DTR record, mark as ABSENT
                        worksheet.Cell(row, 4).Value = "ABSENT";
                        worksheet.Cell(row, 5).Value = "--";
                        worksheet.Cell(row, 6).Value = "--";
                        worksheet.Cell(row, 7).Value = "--";
                        worksheet.Cell(row, 8).Value = "--";

                        // Fill LightPink for ABSENT days
                        worksheet.Range(row, 2, row, 8).Style.Fill.BackgroundColor = XLColor.LightPink;
                    }

                    // Weekend formatting
                    if (isSaturday)
                    {
                        worksheet.Range(row, 2, row, 8).Style.Fill.BackgroundColor = XLColor.LightPink;
                    }

                    if (isSunday)
                    {
                        worksheet.Range(row, 2, row, 8).Style.Fill.BackgroundColor = XLColor.LightPink;

                        // Mark as WEEKEND on Time In and clear others
                        worksheet.Cell(row, 4).Value = "WEEKEND";
                        worksheet.Cell(row, 5).Value = "--";
                        worksheet.Cell(row, 6).Value = "--";
                        worksheet.Cell(row, 7).Value = "--";
                        worksheet.Cell(row, 8).Value = "--";
                    }

                    // Center alignment for all columns in this row
                    worksheet.Range(row, 2, row, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Range(startRow + 1, 2, row - 1, 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(startRow + 1, 2, row - 1, 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    string fileName = $"DTR_{employee.lastName}_{monthName}_{year}_Cutoff-{cutoff}.xlsx";

                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }
        private List<DateTime> GenerateCutoffDates(int month, string cutoff)
        {
            List<DateTime> dates = new List<DateTime>();
            int year = DateTime.Now.Year; // Default year (or pass it from the front end)

            if (cutoff == "9-23")
            {
                for (int day = 9; day <= 23; day++)
                {
                    dates.Add(new DateTime(year, month, day));
                }
            }
            else if (cutoff == "24-8")
            {
                int daysInCurrentMonth = DateTime.DaysInMonth(year, month);
                for (int day = 24; day <= daysInCurrentMonth; day++)
                {
                    dates.Add(new DateTime(year, month, day));
                }

                int nextMonth = month == 12 ? 1 : month + 1;
                int nextYear = month == 12 ? year + 1 : year;

                for (int day = 1; day <= 8; day++)
                {
                    dates.Add(new DateTime(nextYear, nextMonth, day));
                }
            }

            return dates;
        }






    }
}