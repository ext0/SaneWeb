using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SaneWeb.Resources
{
    public static class Utility
    {
        public static string trustedRandomString(int maxSize)
        {
            char[] chars = new char[62];
            chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            byte[] data = new byte[1];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);
            }
            StringBuilder result = new StringBuilder(maxSize);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }

        public static int randomNumber()
        {
            byte[] data = new byte[1];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[31];
                crypto.GetNonZeroBytes(data);
            }
            return Math.Abs(BitConverter.ToInt32(data, 0));
        }
    }

    namespace SaneWeb.Resources.Arguments
    {
        public class HttpArgument
        {
            public readonly String key;
            public readonly String value;
            public HttpArgument(String key, String value)
            {
                this.key = key;
                this.value = value;
            }
            public override string ToString()
            {
                return String.Format("{0}={1}", key, value);
            }
        }
    }
}
