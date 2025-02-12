using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dealogikal.Database;
using Dealogikal.Utils;
using Dealogikal.Repository;
using Dealogikal.Models;


namespace Dealogikal.Controllers
{
    public class BaseController : Controller
    {
        public String ErrorMessage;
        public BaseRepository<userAccount> _userAcc;
        public AccountManager _AccManager;
         
        
        public BaseController()
        {
            _userAcc = new BaseRepository<userAccount>();
            _AccManager = new AccountManager();
        }

        public void IsUserLoggedSession()
        {
            UserLogged userLogged = new UserLogged();
            if (User != null)
            {
                if (User != null)
                {
                    if (User.Identity.IsAuthenticated)
                    {
                        userLogged.UserAccount = _AccManager.GetUserByEmployeeId(User.Identity.Name);
                        userLogged.employeeInfo = _AccManager.CreateOrRetrieve(userLogged.UserAccount.employeeId, ref ErrorMessage);
                    }
                }
                Session["User"] = userLogged;
            }

        }
    }
}