using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Accord.Audio;
using Accord.Audio.Formats;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteganographySolution.Common;
using SharpDX.Multimedia;

namespace SteganographySolution.Tests
{
	[TestClass]
	public class EmbedFileTests
	{
		public EmbedFileTests()
		{
			decodedFile = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
			encodedFile = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".flac");
			randomFlacFile = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".flac");
			randomWavFile = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".wav");
		}
		private string decodedFile;
		private string encodedFile;
		private string randomFlacFile;
		private string randomWavFile;
		private void deleteFiles()
		{
			if (File.Exists(decodedFile)) File.Delete(decodedFile);
			if (File.Exists(encodedFile)) File.Delete(encodedFile);
			if (File.Exists(randomFlacFile)) File.Delete(randomFlacFile);
			if (File.Exists(randomWavFile)) File.Delete(randomWavFile);
		}

		[TestMethod]
		public void TestEncodeAndDecode()
		{
			try
			{
				deleteFiles();

				#region Create Blob
				var blob = new byte[byte.MaxValue + 1];

				for (int b = 0; b <= byte.MaxValue; b++)
					blob[b] = (byte)b;

				//var blob = new byte[UInt16.MaxValue];
				//var rnd = new Random();
				//rnd.NextBytes(blob);
				#endregion

				#region Create Random Stereo Flac Track for Mixing
				var randomFloatWav = new float[(blob.Length + 4) * 2 + 44100];
				var randomShortWav = new short[randomFloatWav.Length];

				#region Create Wav File
				using (var writer = File.OpenWrite(randomWavFile))
				{
					var randomWavEncoder = new WaveEncoder(writer);

					SampleConverter.Convert(randomShortWav, randomFloatWav);

					using (var writeSignal = Signal.FromArray(randomFloatWav, 2, 44100, SampleFormat.Format32BitIeeeFloat))
						randomWavEncoder.Encode(writeSignal);

					randomWavEncoder.Close();
				}
				#endregion

				#region Convert to Flac
				using (var ffmpeg = new Process())
				{
					ffmpeg.StartInfo.Arguments = String.Format(
						"-i \"{0}\" -c:a flac \"{1}\"",
						randomWavFile, randomFlacFile);
					ffmpeg.StartInfo.FileName = "ffmpeg";
					ffmpeg.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

					ffmpeg.Start();
					ffmpeg.WaitForExit();
				}
				#endregion
				#endregion

				#region Encode Audio File
				using (var reader = new MemoryStream(blob))
					reader.SaveToFlacFile(blob.Length, new FileInfo(randomFlacFile), encodedFile);
				#endregion

				#region Decode Audio File
				(new FileInfo(encodedFile)).ExtractFileFromFlacFile(decodedFile);
				var decodedBlob = File.ReadAllBytes(decodedFile);
				#endregion

				#region Check Results
				var exception = new Exception("It didn't work.");
				if (blob.Length != decodedBlob.Length)
					throw exception;
				for (int index = 0; index < blob.Length; index++)
					if (blob[index] != decodedBlob[index])
						throw exception;
				#endregion
			}
			catch (Exception error)
			{
				Console.WriteLine(error);
				Console.Read();
			}
			finally
			{
				deleteFiles();
			}
		}
	}
}
