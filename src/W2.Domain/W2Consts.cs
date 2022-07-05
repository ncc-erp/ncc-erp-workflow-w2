namespace W2;

public static class W2Consts
{
    public const string DbTablePrefix = "App";

    public const string DbSchema = null;

    public const string TenantKey = "TenantIdentifier";

    public static class WorkflowSignals
    {
        public const string PMApproved = "PMApproved";
        public const string PMRejected = "PMRejected";
        public const string HoOApproved = "HoOApproved";
        public const string HoORejected = "HoORejected";
    }
}
