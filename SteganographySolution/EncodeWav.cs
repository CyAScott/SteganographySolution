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
	public static class EncodeWav
	{
		public static void CopyToWavStream(this Stream blobStream, int length, Stream wavStream)
		{
			int bufferIndex, index, readLength, shortWavIndex;
			var byteWav = new byte[256];
			var encoder = new WaveEncoder(wavStream);
			var floatWav = new float[8];
			var shortWav = new short[8];

			var lengthBytes = BitConverter.GetBytes(length);

			for (bufferIndex = 0, shortWavIndex = 0; bufferIndex < lengthBytes.Length; bufferIndex++, shortWavIndex += 2)
			{
				//Left Channel
				shortWav[shortWavIndex] = Convert.ToInt16(-1 * (Math.Min(128, (int)lengthBytes[bufferIndex]) - 96));
				//Right Channel
				shortWav[shortWavIndex + 1] = Convert.ToInt16(Math.Max(0, (int)lengthBytes[bufferIndex] - 128));
			}

			shortWav.PadWav();

			SampleConverter.Convert(shortWav, floatWav);

			using (var signal = Signal.FromArray(floatWav, 2, 44100, SampleFormat.Format32BitIeeeFloat))
				encoder.Encode(signal);

			for (index = 0; index < length; index += byteWav.Length)
			{
				readLength = blobStream.Read(byteWav, 0, Math.Min(byteWav.Length, length - index));

				if (byteWav.Length != readLength) Array.Resize(ref byteWav, readLength);

				readLength *= 2;

				if (floatWav.Length != readLength) floatWav = new float[readLength];
				if (shortWav.Length != readLength) shortWav = new short[readLength];

				for (bufferIndex = 0, shortWavIndex = 0; bufferIndex < byteWav.Length; bufferIndex++, shortWavIndex += 2)
				{
					//Left Channel
					shortWav[shortWavIndex] = Convert.ToInt16(-1 * (Math.Min(128, (int)byteWav[bufferIndex]) - 96) - 1);
					//Right Channel
					shortWav[shortWavIndex + 1] = Convert.ToInt16(Math.Max(0, (int)byteWav[bufferIndex] - 128) - 1);
				}

				shortWav.PadWav();

				SampleConverter.Convert(shortWav, floatWav);

				using (var signal = Signal.FromArray(floatWav, 2, 44100, SampleFormat.Format32BitIeeeFloat))
					encoder.Encode(signal);
			}

			encoder.Close();
		}
		public static void CopyToBytes(this Stream wavStream, Stream blobStream)
		{
			int bufferIndex, index, length, readLength, shortWavIndex;
			var byteWav = new byte[256];
			var decoder = new WaveDecoder(wavStream);
			var floatWav = new float[8];
			var shortWav = new short[8];

			using (var signal = decoder.Decode(4))
			{
				signal.CopyTo(floatWav);
				SampleConverter.Convert(floatWav, shortWav);

				for (bufferIndex = 0, shortWavIndex = 0; bufferIndex < 4; bufferIndex++, shortWavIndex += 2)
				{
					byteWav[bufferIndex] = Convert.ToByte(shortWav[shortWavIndex + 1] - shortWav[shortWavIndex] + 96);
				}

				length = BitConverter.ToInt32(byteWav, 0);
			}

			for (index = 0; index < length; index += byteWav.Length)
				using (var signal = decoder.Decode(readLength = Math.Min(byteWav.Length, decoder.Frames - decoder.Position)))
				{
					if (byteWav.Length != readLength) byteWav = new byte[readLength];

					readLength *= 2;

					if (floatWav.Length != readLength)
					{
						floatWav = new float[readLength];
						shortWav = new short[readLength];
					}

					signal.CopyTo(floatWav);
					SampleConverter.Convert(floatWav, shortWav);

					for (bufferIndex = 0, shortWavIndex = 0; shortWavIndex < readLength; bufferIndex++, shortWavIndex += 2)
					{
						byteWav[bufferIndex] = Convert.ToByte(shortWav[shortWavIndex + 1] - shortWav[shortWavIndex] + 96);
					}

					blobStream.Write(byteWav, 0, byteWav.Length);
				}

			decoder.Close();
		}
		public static void PadWav(this short[] wav)
		{
			for (int index = 0; index < wav.Length; index++)
			{
				if (wav[index] > 0)
					wav[index]++;
				else if (wav[index] < 0)
					wav[index]--;
			}
		}
	}
}
