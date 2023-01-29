using System;
using System.Security.Cryptography;
using System.Text;

namespace EtkBlazorApp.Extensions;

public static class StringHelper
{
    public static string GetMD5(string str)
    {
        if (str == null)
        {
            str = "";
        }

        using (var md5Hash = MD5.Create())
        {
            // Byte array representation of source string
            var sourceBytes = Encoding.UTF8.GetBytes(str);

            // Generate hash value(Byte Array) for input data
            var hashBytes = md5Hash.ComputeHash(sourceBytes);

            // Convert hash byte array to string
            var hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

            return hash;
        }
    }
}
