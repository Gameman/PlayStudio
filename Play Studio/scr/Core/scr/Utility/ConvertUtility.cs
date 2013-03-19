using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Play.Studio.Core.Utility
{
    public static partial class ConvertUtility
    {
        /// <summary> 
        /// 将 stream 转成 byte[] 
        /// </summary> 
        public static byte[] ToBytes(this Stream stream)
        {
            // 设置当前流的位置为流的开始 
            stream.Seek(0, SeekOrigin.Begin);

            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);

            stream.Seek(0, SeekOrigin.Begin);

            return bytes;
        }

        /// <summary>
        /// 将 stream 转成 string
        /// </summary>
        public static string ToString1(this Stream stream)
        {
            byte[] data = ToBytes(stream);
            return Convert.ToBase64String(data);
        }

        /// <summary> 
        /// 将 byte[] 转成 Stream 
        /// </summary> 
        public static Stream ToStream(this byte[] bytes)
        {
            Stream stream = new MemoryStream(bytes);
            return stream;
        }

        /// <summary> 
        /// 将 byte[] 转成 string
        /// </summary> 
        public static string ToString(this byte[] bytes) 
        {
            return new string(bytes.Select(X => (char)X).ToArray());
        }

        /// <summary>
        /// 将 string 转成 byte[]
        /// </summary>
        public static unsafe byte[] ToBytes(this string str)
        {
            return str.ToCharArray().Select(X => (byte)X).ToArray();
        }

        /// <summary>
        /// 将 string 转成 byte[]
        /// </summary>
        public static byte[] ToBytes(this string str, Encoding encoding) 
        {
            return encoding.GetBytes(str);
        }

        /// <summary>
        /// 将 string 转成 stream
        /// </summary>
        public static Stream ToStream(this string str) 
        {
            byte[] b = ToBytes(str);
            return ToStream(b);
        }

        /// <summary>
        /// 转换值为字符串
        /// </summary>
        public static T FromString<T>(this string v, T defaultValue)
        {
            if (string.IsNullOrEmpty(v))
                return defaultValue;
            if (typeof(T) == typeof(string))
                return (T)(object)v;
            try
            {
                TypeConverter c = TypeDescriptor.GetConverter(typeof(T));
                return (T)c.ConvertFromInvariantString(v);
            }
            catch
            {

                return defaultValue;
            }
        }

        /// <summary>
        /// 转换值为字符串
        /// </summary>
        public static string ToString<T>(this T val)
        {
            if (typeof(T) == typeof(string))
            {
                string s = (string)(object)val;
                return string.IsNullOrEmpty(s) ? null : s;
            }
            try
            {
                TypeConverter c = TypeDescriptor.GetConverter(typeof(T));
                string s = c.ConvertToInvariantString(val);
                return string.IsNullOrEmpty(s) ? null : s;
            }
            catch
            {

                return null;
            }
        }

    }
}