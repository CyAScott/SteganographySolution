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
		public static async Task ReplaceAudioTrackWithFlacFile(this FileInfo videoFile, FileInfo flacAudioFile)
		{
			await Task.Factory.StartNew(() =>
			{
				using (var ffmpeg = new Process())
				{
					ffmpeg.StartInfo.Arguments = String.Format(
						//Todo: use ffmpeg to replace the audio track in a target video file
						"-i \"{0}\" PUT YOUR ARGUMENTS FOR FFMPEG HERE",
						flacAudioFile.FullName);
					ffmpeg.StartInfo.FileName = "ffmpeg";
					ffmpeg.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

					ffmpeg.Start();
					ffmpeg.WaitForExit();
				}
			});
		}
	}
}
