namespace W2.Configurations
{
    public static class EmailProviders
    {
        public const string Smtp = "Smtp";
        public const string AwsSes = "AwsSes";
    }

    public class EmailConfiguration
    {
        /// <summary>
        /// Email provider to use. Supported values: "Smtp" (default) and "AwsSes".
        /// </summary>
        public string Provider { get; set; } = EmailProviders.Smtp;

        public AwsSesConfiguration AwsSes { get; set; } = new AwsSesConfiguration();
    }

    public class AwsSesConfiguration
    {
        /// <summary>
        /// AWS region of the SES service, e.g. "ap-southeast-1".
        /// </summary>
        public string Region { get; set; } = "ap-southeast-1";

        /// <summary>
        /// Optional explicit credentials. Leave empty to use the default AWS
        /// credential chain (environment variables, shared profile, IAM role, ...).
        /// </summary>
        public string AccessKeyId { get; set; }

        public string SecretAccessKey { get; set; }

        public string SessionToken { get; set; }

        /// <summary>
        /// Optional SES configuration set name.
        /// </summary>
        public string ConfigurationSetName { get; set; }

        /// <summary>
        /// Optional sender address. When empty, the address configured in
        /// "ElsaConfiguration:Smtp:DefaultSender" is used.
        /// </summary>
        public string FromAddress { get; set; }

        /// <summary>
        /// Optional sender display name.
        /// </summary>
        public string FromDisplayName { get; set; }
    }
}
