using System.Collections.Generic;

public static class WebhookEvents
{
    public const string RequestCreated = "Request Created";
    public const string RequestFinished = "Request Finished";
    public const string RequestAssigned = "Request Assigned";

    public static readonly HashSet<string> ValidEvents = new()
    {
        RequestCreated,
        RequestFinished,
        RequestAssigned
    };
}
