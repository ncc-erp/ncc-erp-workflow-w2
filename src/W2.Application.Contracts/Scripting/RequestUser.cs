using System;
using System.Collections.Generic;

namespace W2.Scripting
{
    public class RequestUser
    {

        public string Email { get; set; }
        public string TargetStaffEmail { get; set; }
        public string Name { get; set; }
        public string Project { get; set; }
        public string PM { get; set; }
        public string HeadOfOfficeEmail { get; set; }
        public string ProjectCode { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public Guid? Id { get; set; }
        public List<string> CEOEmails { get; set; }
        public List<string> ITEmails { get; set; }
        public List<string> DirectorEmails { get; set; }
        public List<string> HREmails { get; set; }
        public List<string> SaleEmails { get; set; }
        public List<string> HPMEmails { get; set; }
        public List<string> SaoDoEmails { get; set; }
    }
}
