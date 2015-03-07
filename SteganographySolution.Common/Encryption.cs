using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace SteganographySolution.Common
{
	public static class Encryption
	{
		private static Random rnd;
		private static SymmetricAlgorithm newEncryptionAlgorithm()
		{
			return new RijndaelManaged();
		}
		private static SymmetricAlgorithm newEncryptionAlgorithm(byte[] keyData)
		{
			if (keyData == null) throw new ArgumentNullException("keyData");
			if (keyData.Length != SymmetricKeyLength) throw new ArgumentException("Invalid key length.");

			var algorithm = new RijndaelManaged();

			byte[]
				key = new byte[keyLength],
				iv = new byte[ivLength];

			Buffer.BlockCopy(keyData, 0, iv, 0, iv.Length);
			Buffer.BlockCopy(keyData, iv.Length, key, 0, key.Length);
			algorithm.Key = key;
			algorithm.IV = iv;

			return algorithm;
		}
		private static int keyLength, ivLength;

		/// <summary>
		/// Decrypts a blob or binary stream.
		/// </summary>
		/// <param name="stream">The source stream.</param>
		/// <param name="saveToStream">The export stream.</param>
		/// <param name="keyData">The key to decrypt the source stream.</param>
		/// <returns></returns>
		public static async Task DecryptStream(this Stream stream, Stream saveToStream, byte[] keyData)
		{
			if (saveToStream == null) throw new ArgumentNullException("saveToStream");
			if (stream == null) throw new ArgumentNullException("stream");

			using (var algorithm = newEncryptionAlgorithm(keyData))
			using (var decryptor = algorithm.CreateDecryptor())
			using (var crypto = new CryptoStream(stream, decryptor, CryptoStreamMode.Read))
				await crypto.CopyToAsync(saveToStream);
		}
		/// <summary>
		/// Encrypts a blob or stream.
		/// </summary>
		/// <param name="stream">The source stream.</param>
		/// <param name="saveToStream">The export stream.</param>
		/// <param name="keyData">The key to encrypt the source stream.</param>
		/// <returns></returns>
		public static async Task EncryptStream(this Stream stream, Stream saveToStream, byte[] keyData)
		{
			if (saveToStream == null) throw new ArgumentNullException("saveToStream");
			if (stream == null) throw new ArgumentNullException("stream");

			using (var algorithm = newEncryptionAlgorithm(keyData))
			using (var encryptor = algorithm.CreateEncryptor())
			using (var crypto = new CryptoStream(saveToStream, encryptor, CryptoStreamMode.Write))
				await stream.CopyToAsync(crypto);
		}
		/// <summary>
		/// Convert a base64 encoded string into a byte array.
		/// </summary>
		/// <param name="text">The source base64 encoded string.</param>
		/// <returns></returns>
		public static byte[] ConvertFromBase64(this string text)
		{
			return Convert.FromBase64String(text);
		}
		/// <summary>
		/// Generates a key to use for encryption. This key will contain both the symetric encryption key and IV.
		/// </summary>
		/// <returns></returns>
		public static byte[] GenerateKey()
		{
			var returnValue = new byte[SymmetricKeyLength];
			rnd.NextBytes(returnValue);
			return returnValue;
		}
		/// <summary>
		/// The required length for the a symmetric key.
		/// </summary>
		public static int SymmetricKeyLength
		{
			get;
			private set;
		}
		/// <summary>
		/// Converts a byte array to a base64 encoded string.
		/// </summary>
		/// <param name="bytes">The source byte array.</param>
		/// <returns></returns>
		public static string ConvertToBase64(this byte[] bytes)
		{
			return Convert.ToBase64String(bytes);
		}
		
		static Encryption()
		{
			rnd = new Random();
			using (var algorithm = newEncryptionAlgorithm())
			{
				keyLength = Convert.ToInt32(algorithm.LegalKeySizes.Max(length => length.MaxSize) / 8);
				ivLength = Convert.ToInt32(algorithm.BlockSize / 8);
			}
			SymmetricKeyLength = keyLength + ivLength;
		}
	}
}
