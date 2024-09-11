using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace W2.Settings
{
    public class CreateNewW2SettingValueDto
    {
        [Required]
        public string SettingCode { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Email { get; set; }
    }
}
