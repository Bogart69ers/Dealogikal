using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dealogikal.Database;
using Dealogikal.Utils;
using Dealogikal.Repository;
using Dealogikal.ViewModel;



namespace Dealogikal.Controllers
{
    public class BaseController : Controller
    {
        public String ErrorMessage;
        public BaseRepository<userAccount> _userAcc;
        public AccountManager _AccManager;
        public DtrManager _DtrManager;
         
        
        public BaseController()
        {
            _userAcc = new BaseRepository<userAccount>();
            _AccManager = new AccountManager();
            _DtrManager = new DtrManager();
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if (User.Identity.IsAuthenticated)
            {
                var userAccount = _AccManager.GetUserByEmployeeId(User.Identity.Name);
                if (userAccount != null)
                {
                    var employeeInfo = _AccManager.CreateOrRetrieve(userAccount.employeeId, ref ErrorMessage);

                    // Create the AccountViewModel instance
                    var accountViewModel = new AccountViewModel
                    {
                        userAccount = userAccount,
                        employeeInfo = employeeInfo
                    };

                    // Pass the model data to all views using ViewBag
                    ViewBag.AccountViewModel = accountViewModel;
                    // Optionally, you can pass the employee info directly if your layout references it:
                    ViewBag.EmployeeInfo = employeeInfo;
                }
            }
        }
    }
}