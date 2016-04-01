using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

        public static String serializeObjectToJSON(Object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static Object deserializeJSONToObject(String json)
        {
            return JsonConvert.DeserializeObject(json);
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

        public static int randomNumber(int minValue, int maxValue)
        {
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                byte[] intBuffer = new byte[32];
                if (minValue > maxValue)
                    throw new ArgumentOutOfRangeException("minValue");
                if (minValue == maxValue) return minValue;
                long diff = maxValue - minValue;
                while (true)
                {
                    crypto.GetBytes(intBuffer);
                    UInt32 rand = BitConverter.ToUInt32(intBuffer, 0);

                    Int64 max = (1 + (Int64)UInt32.MaxValue);
                    Int64 remainder = max % diff;
                    if (rand < max - remainder)
                    {
                        return (Int32)(minValue + (rand % diff));
                    }
                }
            }
        }

        public static byte[] fetchForClient(Assembly assembly, String resourceName)
        {
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                byte[] buffer = new byte[16 * 1024];
                using (MemoryStream reader = new MemoryStream())
                {
                    int read;
                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        reader.Write(buffer, 0, read);
                    }
                    return reader.ToArray();
                }
            }
        }

        public static String fetchFromResource(bool isText, Assembly assembly, String resourceName)
        {
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
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
