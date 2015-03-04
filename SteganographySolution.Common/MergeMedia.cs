using System;
using System.Diagnostics;
using System.IO;

namespace SteganographySolution.Common
{
	public static class MergeMedia
	{
		/// <summary>
		/// Use this method to extract the audio track form some video file to a audio flac file.
		/// </summary>
		public static void ExtractFlacAudioToFile(this FileInfo videoFile, string saveToLocation)
		{
			//Todo: use ffmpeg to extract the audio file from the video file
			//the command line for this should look like:
			//ffmpeg -i somevideofile.avi -c:a flac output.flac
		}
		/// <summary>
		/// Use this method to replace the audio track for a video with an audio flac file.
		/// </summary>
		public static void ReplaceAudioTrackWithFlacFile(this FileInfo videoFile, FileInfo flacAudioFile)
		{
			//Todo: use ffmpeg to add the audio file from the video file
		}
	}
}
