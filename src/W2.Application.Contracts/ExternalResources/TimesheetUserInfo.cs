using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace W2.ExternalResources
{
    public class TimesheetUserInfo
    {
        [JsonProperty("fullName")]
        public string FullName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("sex")]
        public int Sex { get; set; }

        [JsonProperty("statusName")]
        public string StatusName { get; set; }

        [JsonProperty("userTypeName")]
        public string UserTypeName { get; set; }

        [JsonProperty("skillNames")]
        public List<object> SkillNames { get; set; }

        [JsonProperty("teams")]
        public List<string> Teams { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("birthday")]
        public DateTime Birthday { get; set; }

        [JsonProperty("idCard")]
        public string IdCard { get; set; }

        [JsonProperty("issuedOn")]
        public DateTime IssuedOn { get; set; }

        [JsonProperty("issuedBy")]
        public string IssuedBy { get; set; }

        [JsonProperty("placeOfPermanent")]
        public string PlaceOfPermanent { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("bankAccountNumber")]
        public string BankAccountNumber { get; set; }

        [JsonProperty("remainLeaveDay")]
        public int RemainLeaveDay { get; set; }

        [JsonProperty("taxCode")]
        public string TaxCode { get; set; }

        [JsonProperty("insuranceStatus")]
        public int InsuranceStatus { get; set; }

        [JsonProperty("insuranceStatusName")]
        public string InsuranceStatusName { get; set; }

        [JsonProperty("branch")]
        public string Branch { get; set; }

        [JsonProperty("level")]
        public string Level { get; set; }

        [JsonProperty("jobPosition")]
        public string JobPosition { get; set; }

        [JsonProperty("bank")]
        public string Bank { get; set; }

        [JsonProperty("bankId")]
        public int BankId { get; set; }
    }
}
