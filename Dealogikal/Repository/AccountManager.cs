using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dealogikal.Utils;
using Dealogikal.Database;

namespace Dealogikal.Repository
{
    public class AccountManager
    {
        private BaseRepository<userAccount> _userAcc;
        private BaseRepository<employeeInfo> _employeeInf;

        public AccountManager()
        {
            _userAcc = new BaseRepository<userAccount>();
            _employeeInf = new BaseRepository<employeeInfo>();
        }

        public userAccount GetUserByUserId(int userId)
        {
            return _userAcc.Get(userId);
        }
        
        public userAccount GetUserByEmployeeId(string employeeId)
        {
            return _userAcc._table.FirstOrDefault(m => m.employeeId == employeeId);
        }

        public List<userAccount> GetAllUsers()
        {
            return _userAcc.GetAll();
        }

        public List<employeeInfo> GetAllEmployee()
        {
            return _employeeInf.GetAll();
        }

        public employeeInfo GetEmployeebyEmployeeId(string employeeId)
        {
            return _employeeInf._table.FirstOrDefault(m => m.employeeId == employeeId);
        }
        public employeeInfo CreateOrRetrieve(String employeeId , ref String err)
        {
            var user = GetUserByEmployeeId(employeeId);
            if(user == null)
            {
                err = "User does not exist";
                return null;
            }

            var employeeInfo = GetEmployeebyEmployeeId(user.employeeId);
            if (employeeInfo != null)
            {
                return employeeInfo;
            }

            employeeInfo = new employeeInfo();
            employeeInfo.employeeId = user.employeeId;
            employeeInfo.createdAt = DateTime.Now;
            employeeInfo.status = (int)Status.Active;

            _employeeInf.Create(employeeInfo, out err);

            return GetEmployeebyEmployeeId(user.employeeId);
        }

        public ErrorCode DeleteUser(int userId, ref string errMsg)
        {
            try
            {
                var user = GetUserByUserId(userId);
                if (user == null)
                {
                    errMsg = "User not found";
                    return ErrorCode.Error;
                }
                return _userAcc.Delete(userId, out errMsg);
            }
            catch (Exception ex)
            {
                errMsg = $"An error occurred: {ex.Message}";
                return ErrorCode.Error; ;
            }
        }

        public ErrorCode UpdateEmployeeInformation(employeeInfo empinf , ref string errMsg)
        {
            return _employeeInf.Update(empinf.employeeId, empinf, out errMsg);
        }

        public ErrorCode UpdateUser(userAccount userac, ref string errMsg)
        {
            return _userAcc.Update(userac.userId, userac, out errMsg);
        }

        public ErrorCode SignIn(string employeeId, string password, ref string errMsg)
        {
            var userSignIn = GetUserByEmployeeId(employeeId);
            if (userSignIn == null)
            {
                errMsg = "User does not exist!";
                return ErrorCode.Error;
            }

            if (!userSignIn.password.Equals(password))
            {
                errMsg = "Password is incorrect";
                return ErrorCode.Error;
            }

            errMsg = "Login Successful";
            return ErrorCode.Success;
        }
    }
}