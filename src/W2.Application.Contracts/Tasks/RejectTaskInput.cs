using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace W2.Tasks
{
    public class RejectTaskInput
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string Reason { get; set; }

        [Required]
        public string Email { get; set; }
    }
}
