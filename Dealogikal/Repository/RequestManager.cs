using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dealogikal.Utils;
using Dealogikal.Database;

namespace Dealogikal.Repository
{
    public class RequestManager
    {
        private BaseRepository<leaveRequest> _leaveReq;
        private BaseRepository<overtimeRequest> _overtReq;

        public RequestManager()
        {
            _leaveReq = new BaseRepository<leaveRequest>();
            _overtReq = new BaseRepository<overtimeRequest>();
        }

        public leaveRequest GetLeaveRequestbyRequestId(int requestId)
        {
            return _leaveReq.Get(requestId);
        }

        public List<leaveRequest> GetLeaveRequestByEmployeeId(string employeeId)
        {
            return _leaveReq._table.Where(l => l.employeeId == employeeId).OrderByDescending(l => l.dateFiled).ToList();
        }

        public List<leaveRequest> GetAllLeaveRequestsDesc() // Better naming
        {
            return _leaveReq.GetAll().OrderByDescending(l => l.dateFiled).ToList();
        }

        public List<leaveRequest> GetAllLeaveRequest()
        {
            return _leaveReq.GetAll();
        }

        public overtimeRequest GetOvertimeRequestbyRequestId(int requestId)
        {
            return _overtReq.Get(requestId);
        }

        public List<overtimeRequest> GetOvertimeRequestByEmployeeId(string employeeId)
        {
            return _overtReq._table.Where(o => o.employeeId == employeeId).OrderByDescending(o => o.dateFiled).ToList();
        }

        public List<overtimeRequest> GetAllOvertimeRequest()
        {
            return _overtReq.GetAll();
        }
        public List<overtimeRequest> GetAllOvertimeRequestsDesc() // Better naming
        {
            return _overtReq.GetAll().OrderByDescending(l => l.dateFiled).ToList();
        }

        public ErrorCode CreateLeave(leaveRequest lr, string employeeId, ref string errMsg)
        {
            try
            {
                DateTime serverTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Singapore Standard Time");

                lr.dateFiled = serverTime;
                lr.employeeId = employeeId;
                lr.status = 0;

                if (_leaveReq.Create(lr, out errMsg) != ErrorCode.Success)
                {
                    return ErrorCode.Error;
                }

                return ErrorCode.Success;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return ErrorCode.Error;
            }
        }

        public ErrorCode CreateOvertime(overtimeRequest or, string employeeId, ref string errMsg)
        {
            try
            {
                DateTime serverTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Singapore Standard Time");

                or.dateFiled = serverTime;
                or.employeeId = employeeId;
                or.status = 0;

                if (_overtReq.Create(or, out errMsg) != ErrorCode.Success)
                {
                    return ErrorCode.Error;
                }

                return ErrorCode.Success;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return ErrorCode.Error;
            }
        }

    }
}