using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SteganographySolution.Common
{
	public class WebCam
	{
		/// <summary>
		/// Records a video from a webcam.
		/// </summary>
		/// <param name="deviceName">The web cam device name.</param>
		/// <param name="audioDevice">The audio device name to record sound from.</param>
		/// <param name="videoLength">The length of the video to record.</param>
		/// <param name="outputLocation">The save location for the video.</param>
		/// <returns></returns>
		public static async Task RecordVideo(string deviceName, string audioDevice, TimeSpan videoLength, string outputLocation)
		{
			await Task.Factory.StartNew(() =>
			{
				using (var ffmpeg = new Process())
				{
					//The parameters for the program to start
					ffmpeg.StartInfo.Arguments = String.Format("-f dshow -i video=\"{0}\":audio=\"{1}\" \"{2}\"",
						deviceName, audioDevice, outputLocation);
					ffmpeg.StartInfo.CreateNoWindow = true;
					ffmpeg.StartInfo.FileName = "ffmpeg";
					ffmpeg.StartInfo.UseShellExecute = false;
					ffmpeg.StartInfo.RedirectStandardError = true;
					ffmpeg.StartInfo.RedirectStandardInput = true;
					ffmpeg.StartInfo.RedirectStandardOutput = true;
					ffmpeg.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

					//Start the program
					ffmpeg.Start();

					//Start a timer for keeping track of how long things have happened
					var timer = new Stopwatch();
					timer.Start();

					//Wait for the program to startup
					while (timer.Elapsed < TimeSpan.FromSeconds(2))
						Console.WriteLine(ffmpeg.StandardError.ReadLine());
					timer.Restart();

					//Read the output of the program while it records the video
					while (timer.Elapsed < videoLength)
						Console.WriteLine(ffmpeg.StandardError.ReadLine());

					//Send the command to the program to end the recording
					ffmpeg.StandardInput.Write("q");

					//Wait for the program to end
					ffmpeg.WaitForExit(5000);

					//If the program has not ended by now, kill the process
					if (!ffmpeg.HasExited) ffmpeg.Kill();
				}
			});
		}
		/// <summary>
		/// Gets the list of the web cams connected to the computer.
		/// </summary>
		/// <returns></returns>
		public static Task<string[]> GetAllWebcamDevices()
		{
			var returnValue = new TaskCompletionSource<string[]>();

			Task.Factory.StartNew(() =>
			{
				var webcams = new List<string>();

				try
				{
					//Todo: Get a list of all the connected webcams
					
					//Use ffmpeg to get the list of connected webcams

					//You can add a web cam to the list like:
					//webcams.Add("USB Webcam");
				}
				catch (Exception error)
				{
					returnValue.TrySetException(error);
				}
				finally
				{
					returnValue.TrySetResult(webcams.ToArray());
				}
			});

			return returnValue.Task;
		}
	}
}
