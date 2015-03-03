using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Audio;
using Accord.Audio.Formats;

namespace SteganographySolution
{
	public class Program
	{
		public static void Main(string[] args)
		{
			byte[] bytes = new byte[UInt16.MaxValue];
			byte[] decodedBytes = null;
			var rnd = new Random();
			rnd.NextBytes(bytes);

			const string output = @"D:\output2.wav";
			const string outputBytes = @"D:\output";

			//if (File.Exists(output)) File.Delete(output);
			//if (File.Exists(outputBytes)) File.Delete(outputBytes);

			//rnd.NextBytes(bytes);
			//File.WriteAllBytes(outputBytes, bytes);
			bytes = File.ReadAllBytes(outputBytes);

			//using (var target = File.OpenWrite(output))
			//using (var source = new MemoryStream(bytes))
			//	source.CopyToWavStream(bytes.Length, target);

			using (var source = File.OpenRead(output))
			using (var target = new MemoryStream())
			{
				source.CopyToBytes(target);
				decodedBytes = target.ToArray();
			}

			for (int index = 0; index < bytes.Length; index++)
			{
				if (bytes[index] != decodedBytes[index])
				{
					Console.WriteLine("It didn't work.");
					Console.Read();
				}
			}

			//if (File.Exists(output)) File.Delete(output);
			//if (File.Exists(outputBytes)) File.Delete(outputBytes);
		}
	}
}
