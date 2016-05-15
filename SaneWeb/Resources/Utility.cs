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
        /// <summary>
        /// Builds a cryptographically secure random String of the size maxSize
        /// </summary>
        /// <param name="maxSize">Size of the generated String</param>
        /// <returns>The randomally generated String object</returns>
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

        /// <summary>
        /// Serializes an object to a JSON string
        /// </summary>
        /// <param name="obj">Object to be serialized</param>
        /// <returns>JSON representation of the object</returns>
        public static String serializeObjectToJSON(Object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// Deserializes an object from its JSON representation into a .NET object
        /// </summary>
        /// <param name="json">JSON String to be deserialized</param>
        /// <returns>Object represented in the String (must be cast to expected type)</returns>
        public static Object deserializeJSONToObject(String json)
        {
            return JsonConvert.DeserializeObject(json);
        }

        /// <summary>
        /// Generates a cryptographically secure random integer
        /// </summary>
        /// <returns>https://xkcd.com/221/</returns>
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
 
        /// <summary>
        /// Generates a random integer within a specified range
        /// </summary>
        /// <param name="minValue">Inclusive minimum value of the requested integer</param>
        /// <param name="maxValue">Inclusive maximum value of the requested integer</param>
        /// <returns>A random number between minValue and maxValue</returns>
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
        
        /// <summary>
        /// Gets data from an embedded resource in its binary representation
        /// </summary>
        /// <param name="assembly">Assembly to search in</param>
        /// <param name="resourceName">Full name of the resource</param>
        /// <returns>Byte array of the contents of the resource</returns>
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

        /// <summary>
        /// Gets data from an embedded resource in its String representation
        /// </summary>
        /// <param name="assembly">Assembly to search in</param>
        /// <param name="resourceName">Full name of the resource</param>
        /// <returns>String representation of the specified resource</returns>
        public static String fetchFromResource(Assembly assembly, String resourceName)
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
}
