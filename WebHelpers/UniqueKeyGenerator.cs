using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Common
{
    public class UniqueKeyGenerator
    {
        public static string RNGTicks(int rngSize)
        {
            var result = UniqueKeyGenerator.UsingTicks() + UniqueKeyGenerator.RNGCharacterMask(rngSize);
            return result;
        }
        public static  string UsingGuid()
        {
            var result = Guid.NewGuid().ToString().GetHashCode().ToString("x");
            return result;
        }


        public static  string UsingTicks()
        {
            var val = DateTime.Now.Ticks.ToString("x");
            return val;
        }
        public static string RNGCharacterMask(int maxSize)
        {
            
            //var minSize = 5;
            var chars = new char[36];
            string a;
            a = "abcdefghijklmnopqrstuvwxyz1234567890";
            chars = a.ToCharArray();
            int size = maxSize;
            byte[] data = new byte[1];
            var crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            size = maxSize;
            data = new byte[size];
            crypto.GetNonZeroBytes(data);
            var result = new StringBuilder(size);
            foreach (byte b in data)
            { result.Append(chars[b % (chars.Length - 1)]); }
            return result.ToString();
        }

        public static  string RNGCharacterMask()
        {
            const int maxSize = 24;
            var minSize = 5;
            var chars = new char[36];
            string a;
            a = "abcdefghijklmnopqrstuvwxyz1234567890";
            chars = a.ToCharArray();
            int size = maxSize;
            byte[] data = new byte[1];
            var crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            size = maxSize;
            data = new byte[size];
            crypto.GetNonZeroBytes(data);
            var result = new StringBuilder(size);
            foreach (byte b in data)
            { result.Append(chars[b % (chars.Length - 1)]); }
            return result.ToString();
        }


        public static  string UsingDateTime()
        {
            return DateTime.Now.ToString().GetHashCode().ToString("x");
        }


        public static  Hashtable Frequency(string[] keys)
        {
            const int LEN = 1000000000;
            var freq = new Hashtable(LEN);

            foreach (string key in keys)
            {
                if (freq[key] == null)
                {
                    freq.Add(key, 0);
                }
                else
                {
                    freq[key] = (int)freq[key] + 1;
                }
            }
            return freq;
        }
    }
}
