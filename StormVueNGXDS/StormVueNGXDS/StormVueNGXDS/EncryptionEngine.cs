using System;
using System.Security.Cryptography;
using System.Text;

namespace StormVue2RTCM
{
	internal class EncryptionEngine
	{
		private static string key = "svuengxencryptengine2012";

		public static string Encrypt(string ToEncrypt, bool useHash)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(ToEncrypt);
			byte[] array;
			if (useHash)
			{
				MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
				array = mD5CryptoServiceProvider.ComputeHash(Encoding.UTF8.GetBytes(EncryptionEngine.key));
				mD5CryptoServiceProvider.Clear();
			}
			else
			{
				array = Encoding.UTF8.GetBytes(EncryptionEngine.key);
			}
			TripleDESCryptoServiceProvider obj = new TripleDESCryptoServiceProvider
			{
				Key = array,
				Mode = CipherMode.ECB,
				Padding = PaddingMode.PKCS7
			};
			byte[] array2 = obj.CreateEncryptor().TransformFinalBlock(bytes, 0, bytes.Length);
			obj.Clear();
			return Convert.ToBase64String(array2, 0, array2.Length);
		}

		public static string Decrypt(string cypherString, bool useHash)
		{
			byte[] array = Convert.FromBase64String(cypherString);
			string s = "svuengxencryptengine2012";
			byte[] array2;
			if (useHash)
			{
				MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
				array2 = mD5CryptoServiceProvider.ComputeHash(Encoding.UTF8.GetBytes(s));
				mD5CryptoServiceProvider.Clear();
			}
			else
			{
				array2 = Encoding.UTF8.GetBytes(EncryptionEngine.key);
			}
			TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
			tripleDESCryptoServiceProvider.Key = array2;
			tripleDESCryptoServiceProvider.Mode = CipherMode.ECB;
			tripleDESCryptoServiceProvider.Padding = PaddingMode.PKCS7;
			ICryptoTransform cryptoTransform = tripleDESCryptoServiceProvider.CreateDecryptor();
			try
			{
				byte[] array3 = cryptoTransform.TransformFinalBlock(array, 0, array.Length);
				tripleDESCryptoServiceProvider.Clear();
				return Encoding.UTF8.GetString(array3, 0, array3.Length);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public static string R13(string value)
		{
			char[] array = value.ToCharArray();
			for (int i = 0; i < array.Length; i++)
			{
				int num = array[i];
				if (num >= 97 && num <= 122)
				{
					num = ((num <= 109) ? (num + 13) : (num - 13));
				}
				else if (num >= 65 && num <= 90)
				{
					num = ((num <= 77) ? (num + 13) : (num - 13));
				}
				array[i] = (char)num;
			}
			return new string(array);
		}
	}
}
