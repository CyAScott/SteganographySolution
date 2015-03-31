using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SteganographySolution.Common
{
	public class WebCam
	{
		/// <summary>
		/// Records a video from a webcam.
		/// </summary>
		/// <param name="deviceName">The web cam device name.</param>
		/// <param name="videoLength">The length of the video to record.</param>
		/// <param name="outputLocation">The save location for the video.</param>
		/// <returns></returns>
		public static async Task RecordVideo(string deviceName, TimeSpan videoLength, string outputLocation)
		{
			await Task.Factory.StartNew(() =>
			{
				//Todo: Record a video from a webcam (deviceName) and make sure it has a length equal to videoLength.

				//Use ffmpeg to record the video
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
