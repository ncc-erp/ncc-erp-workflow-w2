using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace W2.ExternalResources
{
    public class ExternalAuthDto
    {
        public string? Provider { get; set; }
        public string? IdToken { get; set; }
    }

    public class ExternalAuthUser
    {
        public string Token { get; set; }
    }
}
