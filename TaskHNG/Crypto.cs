using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;

namespace TaskHNG
{
    public static class Crypto
    {
        public static string Hash(String value)
        {
            return Convert.ToBase64String(
                SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(value))
                );
            
        }
    }
}