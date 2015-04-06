using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace SteganographySolution.Common
{
	public static class MergeMedia
	{
		/// <summary>
		/// Use this method to replace the audio track for a video with an audio flac file.
		/// </summary>
		/// <param name="videoFile">The target video file.</param>
		/// <param name="flacAudioFile">The source audio track.</param>
		/// <param name="output">The save to location for the new video file.</param>
		public static Task ReplaceAudioTrackWithFlacFile(this FileInfo videoFile, FileInfo flacAudioFile, string output)
		{
			return Task.Factory.StartNew(() =>
			{
				using (var ffmpeg = new Process())
				{
					ffmpeg.StartInfo.Arguments = String.Format(
						"-i \"{0}\" -i \"{1}\" -c:v copy -c:a flac -strict experimental -map 0:v:0 -map 1:a:0 \"{2}\"",
						videoFile.FullName, flacAudioFile.FullName, output);
					ffmpeg.StartInfo.FileName = "ffmpeg";
					ffmpeg.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

					ffmpeg.Start();
					ffmpeg.WaitForExit();
				}
			});
		}
	}
}
