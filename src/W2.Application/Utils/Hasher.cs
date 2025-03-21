using System;
using System.Security.Cryptography;

public class Hasher
{
    public static byte[] HMAC_SHA256(byte[] key, byte[] data)
    {
        using (var hmac = new HMACSHA256(key))
        {
            return hmac.ComputeHash(data);
        }
    }

    public static string HEX(byte[] data)
    {
        return BitConverter.ToString(data).Replace("-", "").ToLower();
    }
}