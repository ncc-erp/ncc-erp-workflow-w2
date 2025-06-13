namespace W2.Application.Contracts.IMS;
public class CommonUtil
{
    public static string GetNameByFullName(string fullName)
    {
        return fullName.Substring(fullName.LastIndexOf(" ") + 1);
    }

    public static string GetSurNameByFullName(string fullName)
    {
        return fullName.Substring(0, fullName.LastIndexOf(" "));
    }
}