using System;
using System.IO;
using SteganographySolution.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SteganographySolution.Tests
{
	[TestClass]
	public class EncryptionTests
	{
		[TestMethod]
		public void TestEncryption()
		{
			byte[] decryptedBlob, encryptedBlob;
			var exception = new InvalidProgramException("It didn't work.");
			var key = Encryption.GenerateKey();
			var rnd = new Random();
			var unencryptedBlob = new byte[UInt16.MaxValue];

			rnd.NextBytes(unencryptedBlob);

			using (var readStream = new MemoryStream(unencryptedBlob))
			using (var writeStream = new MemoryStream())
			{
				readStream.EncryptStream(writeStream, key);

				encryptedBlob = writeStream.ToArray();
			}

			using (var readStream = new MemoryStream(encryptedBlob))
			using (var writeStream = new MemoryStream())
			{
				readStream.DecryptStream(writeStream, key);

				decryptedBlob = writeStream.ToArray();
			}

			if (decryptedBlob.Length != unencryptedBlob.Length)
				throw exception;

			for (int index = 0; index < decryptedBlob.Length; index++)
				if (decryptedBlob[index] != unencryptedBlob[index])
					throw exception;
		}
	}
}
