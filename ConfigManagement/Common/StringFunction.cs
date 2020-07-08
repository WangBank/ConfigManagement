using System;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;

namespace ConfigManagement.Common
{
    public static class StringFunction
	{
		public static bool GetBoolValue(this string source)
		{
			return (source.Equals("True", StringComparison.OrdinalIgnoreCase) ? true : source.Equals("1"));
		}

		public static DateTime GetDateTime(this string source)
		{
			DateTime.TryParse(source, out DateTime now);
			return now;
		}

		public static int GetIntValue(this string source, int defaultValue)
		{
			int.TryParse(source, out int num);
			return num;
		}

		public static int GetIntValue(this string source)
		{
			return source.GetIntValue(0);
		}

		public static string ParseArrayToString(this string source, string spliter, object[] array)
		{
			StringBuilder stringBuilder = new StringBuilder(source);
			int num = 0;
			int length = (int)array.Length;
			while (num < length)
			{
				if (num > 0)
				{
					stringBuilder.Append(spliter);
				}
				stringBuilder.Append(array[num]);
				num++;
			}
			return stringBuilder.ToString();
		}

		public static string[] Split(this string source, string separator)
		{
			return source.Split(new string[] { separator }, StringSplitOptions.None);
		}

		public static byte[] ToBytes(this string source)
		{
			return Encoding.UTF8.GetBytes(source);
		}

		public static string ToDencodeString(this string source)
		{
			string str = source.Replace("-", "=").Replace("_", "+").Replace("|", "/");
			byte[] numArray = Convert.FromBase64String(str);
			return Encoding.UTF8.GetString(numArray);
		}

		public static string ToEncodeString(this string source)
		{
			string str = Convert.ToBase64String(source.ToBytes()).Replace("=", "-").Replace("+", "_").Replace("/", "|");
			return str;
		}

		public static string ToMd5EncodingString(this string source)
		{
			byte[] bytes = source.ToBytes();
			using (MD5 mD5CryptoServiceProvider = new MD5CryptoServiceProvider())
			{
				bytes = mD5CryptoServiceProvider.ComputeHash(bytes);
			}
			string str = Convert.ToBase64String(bytes).Replace("/", "|").Replace("=", "-").Replace("+", "_");
			return str;
		}

		public static NameValueCollection ToNameValueCollection(this string source, string spliter1, string spliter2)
		{
			NameValueCollection nameValueCollection = new NameValueCollection();
			string[] strArrays = source.Split(spliter1);
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string[] strArrays1 = strArrays[i].Split(spliter2);
				if ((int)strArrays1.Length >= 2)
				{
					string str = strArrays1[1];
					if ((int)strArrays1.Length > 2)
					{
						StringBuilder stringBuilder = new StringBuilder();
						for (int j = 1; j < (int)strArrays1.Length; j++)
						{
							if (j > 1)
							{
								stringBuilder.Append(",");
							}
							stringBuilder.Append(strArrays1[j]);
						}
						str = stringBuilder.ToString();
					}
					nameValueCollection.Add(strArrays1[0], str);
				}
			}
			return nameValueCollection;
		}

		public static NameValueCollection ToNameValueCollection(this string source)
		{
			return source.ToNameValueCollection("&", "=");
		}
	}
}