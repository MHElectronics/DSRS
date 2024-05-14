using System;
using System.Security.Cryptography;
using System.Text;

namespace Services.Helpers;

public class SecurityHelper
{
    /// <summary>
    /// Create New Password Salt
    /// </summary>
    /// <returns></returns>
    public string CreateSalt()
    {
        byte[] buff = new byte[32];
        new RNGCryptoServiceProvider().GetBytes(buff);
        return Convert.ToBase64String(buff);
    }
    /// <summary>
    /// Hash Password
    /// </summary>
    /// <param name="password">Unhashed Password</param>
    /// <param name="salt">Password Salt</param>
    /// <returns></returns>
    public string CreatePasswordHash(string password, string salt)
    {
        MD5 md5 = new MD5CryptoServiceProvider();
        byte[] digest = md5.ComputeHash(Encoding.UTF8.GetBytes(password + salt));
        string base64Digest = Convert.ToBase64String(digest, 0, digest.Length);
        return base64Digest.Substring(0, base64Digest.Length - 2);
    }
}
