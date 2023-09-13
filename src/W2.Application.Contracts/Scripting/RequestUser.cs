using System;

namespace W2.Scripting
{
    public class RequestUser
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Project { get; set; }
        public string PM { get; set; }
        public string HeadOfOfficeEmail { get; set; }
        public string ProjectCode { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public Guid? Id { get; set; }
    }
}
