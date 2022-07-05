namespace W2.Configurations
{
    public class ElsaConfiguration
    {
        public string ConnectionString { get; set; } = string.Empty;
        public ElsaConfigurationServerSection Server { get; set; }
        public ElsaConfigurationSmtpSection Smtp { get; set; }
    }

    public class ElsaConfigurationServerSection
    {
        public string BaseUrl { get; set; } = string.Empty;
    }

    public class ElsaConfigurationSmtpSection
    {
        public string Host { get; set; } = string.Empty;
        public string Port { get; set; } = string.Empty;
        public string DefaultSender { get; set; } = string.Empty;
    }
}
