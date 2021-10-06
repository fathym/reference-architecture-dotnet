using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
	public static class StringExtensions
	{
		public static string Base64Decode(this string value)
		{
			return Encoding.ASCII.GetString(Convert.FromBase64String(value));
		}

		public static string Base64Encode(this string value)
		{
			return Convert.ToBase64String(Encoding.ASCII.GetBytes(value));
		}
		
		public static dynamic FromJSON(this string value)
		{
			if (value.IsNullOrEmpty())
				return null;

			return JsonConvert.DeserializeObject(value);
		}

		public static T FromJSON<T>(this string value, JsonSerializerSettings serializationSettings = null)
		{
			if (value.IsNullOrEmpty())
				return default(T);

			return JsonConvert.DeserializeObject<T>(value, serializationSettings);
		}

		public static bool IsNullOrEmpty(this string value)
		{
			return String.IsNullOrEmpty(value);
		}

		public static string ReplaceFirst(this string text, string search, string replace)
		{
			int pos = text.IndexOf(search);

			if (pos < 0)
				return text;

			return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
		}

		public static string ReplaceStart(this string text, string search, string replace)
		{
			int pos = text.IndexOf(search);

			if (pos != 0)
				return text;

			return replace + text.Substring(search.Length);
		}

		public static string ToApexHost(this string host)
		{
			var apexHostParts = host.Split('.');

			var apexHost = apexHostParts.Length > 1 ? $"{apexHostParts.ElementAt(apexHostParts.Length - 2)}.{apexHostParts.ElementAt(apexHostParts.Length - 1)}"
				: host;

			return apexHost;
		}

		public static T ToEnum<T>(this string value)
		{
			return (T)ToEnum(value, typeof(T));
		}

		public static object ToEnum(this string value, Type type)
		{
			return Enum.Parse(type, value);
		}

		public static string ToMD5Hash(this string value, bool lowerCaseHash = false)
		{
			var md5 = System.Security.Cryptography.MD5.Create();

			var inputBytes = System.Text.Encoding.ASCII.GetBytes(value);

			var hash = md5.ComputeHash(inputBytes);

			var sb = new StringBuilder();

			for (var i = 0; i < hash.Length; i++)
				sb.Append(hash[i].ToString("X2"));

			var md5Hash = sb.ToString();

			return lowerCaseHash ? md5Hash.ToLower() : md5Hash.ToUpper();
		}
		
		public static string URLEncode(this string value)
		{
			return System.Text.Encodings.Web.UrlEncoder.Default.Encode(value);
		}
	}
}
