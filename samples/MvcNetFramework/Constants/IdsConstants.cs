using System;
using System.Security.Cryptography;
using System.Text;

namespace MvcNetFramework.Constants
{
    public static class IdsConstants
    {
        public static string IdsClientId = "urmia_ir_fish";
        public static string IdsClientSecretNotHashed = "f2f9e5e4-3a83-dbb7-0f53-dc8338aed92e";
        public static string IdsClientSecret = Sha256_hash(IdsClientSecretNotHashed);
        public static string IdsServerUrl = "https://sso.urmia.ir";
        public static string ApisServerUrl = "https://ssoapi.urmia.ir";

        public static String Sha256_hash(String input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                string hashBase64 = Convert.ToBase64String(hashBytes);
                return hashBase64;
            }
        }
    }
}