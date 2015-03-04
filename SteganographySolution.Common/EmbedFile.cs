using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Audio;
using Accord.Audio.Formats;
using SharpDX.Multimedia;

namespace SteganographySolution.Common
{
	public static class EmbedFile
	{
		private static short maxValue, minValue;
		private static void convertBuffer(byte[] rawBytes, short[] shortWav, int count = -1)
		{
			if (count < 0) count = shortWav.Length;
			float sample;
			for (int index = 0, length = count * 4, shortIndex = 0; index < length; index += 4, shortIndex++)
			{
				sample = BitConverter.ToSingle(rawBytes, index) * 32768;

				if (sample > 32767) sample = 32767;
				if (sample < -32768) sample = -32768;

				shortWav[shortIndex] = (short)sample;
			}
		}
		
		public static TimeSpan EstimateAudioTrackLength(int blobLength)
		{
			return TimeSpan.FromSeconds(Math.Ceiling(blobLength / 44100d) + 1);
		}
		public static void ClipWav(this short[] wav)
		{
			for (int index = 0; index < wav.Length; index++)
			{
				if (wav[index] > maxValue)
					wav[index] -= (short)((int)wav[index] - (int)maxValue);
				else if (wav[index] < minValue)
					wav[index] += (short)((int)minValue - (int)wav[index]);
			}
		}
		public static void ExtractFileFromFlacFile(this FileInfo audioOrVideoFile, string saveToLocation)
		{
			if (audioOrVideoFile == null) throw new ArgumentNullException("audioOrVideoFile");
			if (!File.Exists(audioOrVideoFile.FullName)) throw new FileNotFoundException("audioOrVideoFile");
			if (String.IsNullOrEmpty(saveToLocation)) throw new ArgumentNullException("saveToLocation");
			if (File.Exists(saveToLocation)) File.Delete(saveToLocation);

			var tempSourceWavLocation = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".wav");

			try
			{
				#region Convert Source Audio to Wav
				using (var ffmpeg = new Process())
				{
					ffmpeg.StartInfo.Arguments = String.Format(
						"-i \"{0}\" -c:a pcm_f32le \"{1}\"",
						audioOrVideoFile.FullName, tempSourceWavLocation);
					ffmpeg.StartInfo.FileName = "ffmpeg";
					ffmpeg.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

					ffmpeg.Start();
					ffmpeg.WaitForExit();
				}

				if (!File.Exists(tempSourceWavLocation)) throw new InvalidOperationException("Unable to read the flacFile file.");
				#endregion

				using (var reader = File.OpenRead(tempSourceWavLocation))
				using (var decoder = new SoundStream(reader))
				{
					#region Variables
					int bufferIndex, index, length, readLength, shortWavIndex;
					var byteWav = new byte[256];
					var rawBytes = new byte[256];
					var shortWav = new short[64];
					#endregion

					#region Read File Length
					decoder.Read(rawBytes, 0, 32);
					convertBuffer(rawBytes, shortWav, 8);

					for (bufferIndex = 0, shortWavIndex = 0; bufferIndex < 4; bufferIndex++, shortWavIndex += 2)
						byteWav[bufferIndex] = Convert.ToByte(shortWav[shortWavIndex + 1] - shortWav[shortWavIndex] + 96);

					length = BitConverter.ToInt32(byteWav, 0);

					if (decoder.Length < length * 8) throw new ArgumentException("The provided file is too short.");
					#endregion

					#region Extract File
					using (var writer = File.OpenWrite(saveToLocation))
						for (index = 0; index < length; index += byteWav.Length)
						{
							readLength = decoder.Read(rawBytes, 0, rawBytes.Length) / 8;
							
							if (byteWav.Length != readLength) byteWav = new byte[readLength];

							readLength *= 2;

							if (shortWav.Length != readLength) shortWav = new short[readLength];

							convertBuffer(rawBytes, shortWav);

							for (bufferIndex = 0, shortWavIndex = 0; shortWavIndex < readLength; bufferIndex++, shortWavIndex += 2)
								byteWav[bufferIndex] = Convert.ToByte(shortWav[shortWavIndex + 1] - shortWav[shortWavIndex] + 96);

							writer.Write(byteWav, 0, Math.Min(byteWav.Length, length - index));
						}
					#endregion

					decoder.Close();
				}
			}
			catch
			{
				if (File.Exists(saveToLocation)) File.Delete(saveToLocation);
				throw;
			}
			finally
			{
				if (File.Exists(tempSourceWavLocation)) File.Delete(tempSourceWavLocation);
			}
			
		}
		public static void SaveToFlacFile(this Stream blobStream, int blobLength, FileInfo mixInThisAudioOrVideoFile, string saveFlacAudioFileLocation)
		{
			if (blobLength <= 0) throw new IndexOutOfRangeException("blobLength");
			if (blobStream == null) throw new ArgumentNullException("blobStream");
			if (mixInThisAudioOrVideoFile == null) throw new ArgumentNullException("mixInThisAudioOrVideoFile");
			if (String.IsNullOrEmpty(saveFlacAudioFileLocation)) throw new ArgumentNullException("saveFlacAudioFileLocation");
			if (File.Exists(saveFlacAudioFileLocation)) File.Delete(saveFlacAudioFileLocation);

			var tempSourceWavLocation = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".wav");
			var tempTargetWavLocation = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".wav");

			#region Convert Source Audio to Wav
			using (var ffmpeg = new Process())
			{
				ffmpeg.StartInfo.Arguments = String.Format(
					"-i \"{0}\" -ac 1 -ar 44100 -acodec pcm_s16le \"{1}\"",
					mixInThisAudioOrVideoFile.FullName, tempSourceWavLocation);
				ffmpeg.StartInfo.FileName = "ffmpeg";
				ffmpeg.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

				ffmpeg.Start();
				ffmpeg.WaitForExit();
			}

			if (!File.Exists(tempSourceWavLocation)) throw new InvalidOperationException("Unable to read the mixInThisAudioOrVideoFile file.");
			#endregion

			try
			{
				using (var sourceWav = File.OpenRead(tempSourceWavLocation))
				using (var targetWav = File.OpenWrite(tempTargetWavLocation))
				{
					#region Variables
					int bufferIndex, index, readLength, shortWavIndex, sourceWavIndex;
					var byteWav = new byte[256];
					var decoder = new WaveDecoder(sourceWav);
					var encoder = new WaveEncoder(targetWav);
					var floatWav = new float[8];
					var lengthBytes = BitConverter.GetBytes(blobLength);
					var shortWav = new short[8];
					var sourcefloatWav = new float[4];
					var sourceShortWav = new short[4];

					if (decoder.Frames < blobLength + 4) throw new ArgumentException("The source audio file is not long enough.");
					#endregion

					#region Mix File Length with Audio
					using (var readSignal = decoder.Decode(4))
					{
						readSignal.CopyTo(sourcefloatWav);

						SampleConverter.Convert(sourcefloatWav, sourceShortWav);

						sourceShortWav.ClipWav();

						for (bufferIndex = 0, shortWavIndex = 0, sourceWavIndex = 0; bufferIndex < lengthBytes.Length; bufferIndex++, shortWavIndex += 2, sourceWavIndex++)
						{
							//Left Channel
							shortWav[shortWavIndex] =
								Convert.ToInt16(-1 * (Math.Min(128, (int)lengthBytes[bufferIndex]) - 96) +
									sourceShortWav[sourceWavIndex]);

							//Right Channel
							shortWav[shortWavIndex + 1] =
								Convert.ToInt16(Math.Max(0, (int)lengthBytes[bufferIndex] - 128) +
									sourceShortWav[sourceWavIndex]);
						}

						SampleConverter.Convert(shortWav, floatWav);

						using (var writeSignal = Signal.FromArray(floatWav, 2, 44100, SampleFormat.Format32BitIeeeFloat))
							encoder.Encode(writeSignal);
					}
					#endregion

					#region Mix File with Audio
					for (index = 0; index < blobLength; index += byteWav.Length)
					{
						readLength = blobStream.Read(byteWav, 0, Math.Min(byteWav.Length, blobLength - index));

						using (var readSignal = decoder.Decode(readLength))
						{
							if (sourcefloatWav.Length != readLength) sourcefloatWav = new float[readLength];
							if (sourceShortWav.Length != readLength) sourceShortWav = new short[readLength];

							readSignal.CopyTo(sourcefloatWav);

							SampleConverter.Convert(sourcefloatWav, sourceShortWav);

							sourceShortWav.ClipWav();

							if (byteWav.Length != readLength) Array.Resize(ref byteWav, readLength);

							readLength *= 2;

							if (floatWav.Length != readLength) floatWav = new float[readLength];
							if (shortWav.Length != readLength) shortWav = new short[readLength];

							for (bufferIndex = 0, shortWavIndex = 0, sourceWavIndex = 0; bufferIndex < byteWav.Length; bufferIndex++, shortWavIndex += 2, sourceWavIndex++)
							{
								//Left Channel
								shortWav[shortWavIndex] =
									Convert.ToInt16(-1 * (Math.Min(128, (int)byteWav[bufferIndex]) - 96) - 1 +
										sourceShortWav[sourceWavIndex]);

								//Right Channel
								shortWav[shortWavIndex + 1] =
									Convert.ToInt16(Math.Max(0, (int)byteWav[bufferIndex] - 128) - 1 +
										sourceShortWav[sourceWavIndex]);
							}

							SampleConverter.Convert(shortWav, floatWav);

							using (var writeSignal = Signal.FromArray(floatWav, 2, 44100, SampleFormat.Format32BitIeeeFloat))
								encoder.Encode(writeSignal);
						}
					}
					#endregion

					#region Copy Remaining Audio
					while (decoder.Position < decoder.Frames)
						using (var readSignal = decoder.Decode(readLength = Math.Min(128, decoder.Frames - decoder.Position)))
						{
							if (sourcefloatWav.Length != readLength)
							{
								sourcefloatWav = new float[readLength];
								sourceShortWav = new short[readLength];
							}

							readSignal.CopyTo(sourcefloatWav);

							SampleConverter.Convert(sourcefloatWav, sourceShortWav);

							readLength *= 2;

							if (floatWav.Length != readLength)
							{
								floatWav = new float[readLength];
								shortWav = new short[readLength];
							}

							for (shortWavIndex = 0, sourceWavIndex = 0; sourceWavIndex < sourceShortWav.Length; shortWavIndex += 2, sourceWavIndex++)
							{
								//Left Channel
								shortWav[shortWavIndex] = sourceShortWav[sourceWavIndex];

								//Right Channel
								shortWav[shortWavIndex + 1] = sourceShortWav[sourceWavIndex];
							}

							SampleConverter.Convert(shortWav, floatWav);

							using (var writeSignal = Signal.FromArray(floatWav, 2, 44100, SampleFormat.Format32BitIeeeFloat))
								encoder.Encode(writeSignal);
						}
					#endregion

					encoder.Close();
				}

				#region Convert to Flac
				using (var ffmpeg = new Process())
				{
					ffmpeg.StartInfo.Arguments = String.Format(
						"-i \"{0}\" -c:a flac \"{1}\"",
						tempTargetWavLocation, saveFlacAudioFileLocation);
					ffmpeg.StartInfo.FileName = "ffmpeg";
					ffmpeg.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

					ffmpeg.Start();
					ffmpeg.WaitForExit();
				}
				#endregion
			}
			catch
			{
				if (File.Exists(saveFlacAudioFileLocation)) File.Delete(saveFlacAudioFileLocation);
				throw;
			}
			finally
			{
				if (File.Exists(tempSourceWavLocation)) File.Delete(tempSourceWavLocation);
				if (File.Exists(tempTargetWavLocation)) File.Delete(tempTargetWavLocation);
			}
		}
		
		static EmbedFile()
		{
			maxValue = short.MaxValue;
			minValue = short.MinValue;
		}
    }
}
