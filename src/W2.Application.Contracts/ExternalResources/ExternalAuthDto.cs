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

    public class GetMezonOauthTokenDto
    {
        public string code { get; set; }
        public string state { get; set; }
        public string grant_type { get; set; }
        public string redirect_uri { get; set; }
        public string scope { get; set; }
        public string client_id { get; set; }
        public string client_secret { get; set; }
    }

    public class MezonOauthTokenResponse
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }
        public string token_type { get; set; }
    }

    public class MezonAuthDto
    {
        public string code { get; set; }
        public string state { get; set; }
        public string? scope { get; set; }
    }

    public class MezonAuthUserDto
    {

    public List<string> aud { get; set; }
        public long authTime { get; set; }  
        public long iat { get; set; }    
        public string iss { get; set; }    
        public long rat { get; set; }       
        public string sub { get; set; }    
        public string user_id { get; set; }

    }
}





