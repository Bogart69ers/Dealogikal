using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dealogikal.Database;

namespace Dealogikal.ViewModel
{
    public class AccountViewModel
    {
        public userAccount userAccount { get; set; }

        public List<userAccount> userAccounts { get; set; } = new List<userAccount>();
        public employeeInfo employeeInfo { get; set; }

        public List<employeeInfo> employeeInfos { get; set; } = new List<employeeInfo>();

        public dtrRecords dtr { get; set; }

        public List<dtrRecords> dtrRecords { get; set; } = new List<dtrRecords>();

    }
}