using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Accord.Audio;
using Accord.Audio.Formats;
using SharpDX.Multimedia;

namespace SteganographySolution.Common
{
	public static class EmbbedFile
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

		/// <summary>
		/// Reads the media file to find the duration of the file.
		/// </summary>
		/// <returns></returns>
		public static Task<TimeSpan> GetMediaFileLenth(this FileInfo mediaFile)
		{
			var returnValue = new TaskCompletionSource<TimeSpan>();
			ThreadPool.QueueUserWorkItem(state =>
			{
				try
				{
					using (var ffmpeg = new Process())
					{
						ffmpeg.StartInfo.Arguments = String.Format(
							"-i \"{0}\"",
							mediaFile.FullName);
						ffmpeg.StartInfo.CreateNoWindow = true;
						ffmpeg.StartInfo.FileName = "ffmpeg";
						ffmpeg.StartInfo.RedirectStandardError = true;
						ffmpeg.StartInfo.RedirectStandardOutput = true;
						ffmpeg.StartInfo.UseShellExecute = false;
						ffmpeg.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

						ffmpeg.Start();

						var results = ffmpeg.StandardError.ReadToEnd();

						ffmpeg.WaitForExit();

						string line;
						using (var reader = new StringReader(results))
							while (reader.Peek() != -1 && !String.IsNullOrEmpty(line = reader.ReadLine()))
								if (line.Contains("Duration:"))
								{
									var comma = line.IndexOf(',');
									var colon = line.IndexOf(':');

									if (colon == -1 || comma == -1 || comma < colon)
										throw new InvalidOperationException("Unable to find the time for this file.");

									var split = line
										.Substring(colon + 1, comma - colon - 1)
										.Split(':')
										.Select(item => Convert.ToDouble(item.Trim()))
										.ToArray();

									returnValue.SetResult(TimeSpan.FromSeconds(
										(split[0] * 3600) +
										(split[1] * 60) +
										split[2]));

									return;
								}

					}
					throw new InvalidOperationException("Unable to find the time for this file.");
				}
				catch (Exception error)
				{
					returnValue.TrySetException(error);
				}
			});

			return returnValue.Task;
		}
		/// <summary>
		/// Estimates the time of the audio track if this file was embedded in an media file.
		/// </summary>
		/// <param name="someFile">The source file.</param>
		/// <returns></returns>
		public static TimeSpan EstimateAudioTrackLength(this FileInfo someFile)
		{
			return TimeSpan.FromSeconds(Math.Ceiling(someFile.Length / 44100d) + 1);
		}
		/// <summary>
		/// Takes a blob stream (from a file or other binary stream) then embeds it into a flac audio file.
		/// </summary>
		/// <param name="blobStream">A blob stream (from a file or other binary stream).</param>
		/// <param name="blobLength">The length or the blob in bytes.</param>
		/// <param name="mixInThisAudioOrVideoFile">This file will be mixed into the otput flac file.</param>
		/// <param name="saveFlacAudioFileLocation">The output flac file location.</param>
		/// <returns></returns>
		public static async Task EmbedStreamIntoFlacFile(this Stream blobStream, int blobLength, FileInfo mixInThisAudioOrVideoFile, string saveFlacAudioFileLocation)
		{
			if (blobLength <= 0) throw new IndexOutOfRangeException("blobLength");
			if (blobStream == null) throw new ArgumentNullException("blobStream");
			if (mixInThisAudioOrVideoFile == null) throw new ArgumentNullException("mixInThisAudioOrVideoFile");
			if (String.IsNullOrEmpty(saveFlacAudioFileLocation)) throw new ArgumentNullException("saveFlacAudioFileLocation");
			if (File.Exists(saveFlacAudioFileLocation)) File.Delete(saveFlacAudioFileLocation);

			var tempSourceWavLocation = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".wav");
			var tempTargetWavLocation = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".wav");

			await Task.Factory.StartNew(() =>
			{
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
			});
		}
		/// <summary>
		/// Extracts a file from a media file by reading the audio track.
		/// </summary>
		/// <param name="mediaFile">The media file can be either a video or audio file.</param>
		/// <param name="saveToLocation">The export location for the extracted file.</param>
		/// <returns></returns>
		public static async Task ExtractFileFromMediaFile(this FileInfo mediaFile, string saveToLocation)
		{
			if (mediaFile == null) throw new ArgumentNullException("audioOrVideoFile");
			if (!File.Exists(mediaFile.FullName)) throw new FileNotFoundException("audioOrVideoFile");
			if (String.IsNullOrEmpty(saveToLocation)) throw new ArgumentNullException("saveToLocation");
			if (File.Exists(saveToLocation)) File.Delete(saveToLocation);

			var tempSourceWavLocation = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".wav");

			await Task.Factory.StartNew(() =>
			{
				try
				{
					#region Convert Source Audio to Wav
					using (var ffmpeg = new Process())
					{
						ffmpeg.StartInfo.Arguments = String.Format(
							"-i \"{0}\" -c:a pcm_f32le \"{1}\"",
							mediaFile.FullName, tempSourceWavLocation);
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
			});
		}
		/// <summary>
		/// Clips a wav segment to give overhead room for the embedded file.
		/// </summary>
		/// <param name="wav">The wav segment.</param>
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
		
		static EmbbedFile()
		{
			maxValue = short.MaxValue;
			minValue = short.MinValue;
		}
    }
}
