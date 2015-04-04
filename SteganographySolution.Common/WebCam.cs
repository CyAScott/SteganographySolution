using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Video.FFMPEG;/*AForge.Video.FFMPEG error, I have tried to include into output folder, but is not working*/
using System.Diagnostics;


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
		public async Task RecordVideo(string deviceName, TimeSpan videoLength, string outputLocation)
		{
            
			await Task.Factory.StartNew(() =>
			{
				//Todo: Record a video from a webcam (deviceName) and make sure it has a length equal to videoLength.
                //Use ffmpeg to record the video
               
                   // videoSource.Start();
                 //List all available video sources. (That can be webcams as well as tv cards, etc)
                int with = 640;
                int heigth = 480;
                VideoFileWriter write = new VideoFileWriter(); /* I have integrate the AForge library by writing the using directive,
                                                                * but VideoFileWrite could  not be found becasue it supposed to be in FFMPEG.
                                                                * AForge.Video.FFMPEG.DLL depend upon another dlls.I've copied all that dll 
                                                                * into output folder, but still not working. You will find in Reference the 
                                                                * Nuget Packages Included*/
                write.Open("en.code-bute_test_video.avi", with, height, 25, VideoCodec.MPEG4, 1000000 );

           
			});
		}
		/// <summary>
		/// Gets the list of the web cams connected to the computer.
		/// </summary>
		/// <returns></returns>
		public Task<string[]> GetAllWebcamDevices()
		{
			var returnValue = new TaskCompletionSource<string[]>();
            VideoCaptureDevice camsSource;
			Task.Factory.StartNew(() =>
			{
				var webcams = new List<string>();
                
                FilterInfoCollection camosource = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if(camosource != null)
                    try
                      {
                        //Todo: Get a list of all the connected webcams
		                //Use ffmpeg to get the list of connected webcams
                        //You can add a web cam to the list like:
                        //webcams.Add("USB Webcam");
                       //FilterInfoCollection camssource = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                        if (camsSource.VideoCapabilities.Length > 0)
                            {
                            string resolution = "0;0";
                            for (int i = 0; i < camsSource.VideoCapabilities.Length; i++)
                            {
                                 resolution = camsSource.VideoCapabilities[i].FrameSize.Width.ToString() + ";" + i.ToString();
                            }
                            camsSource.VideoResolution = camsSource.VideoCapabilities[Convert.ToInt32(resolution.Split(';')[1])];

                               
                    }
                    
                    using (var ffmpeg = new Process())
                    {
                        ffmpeg.StartInfo.Arguments = String.Format(
                            "-i \"{0}\" -ac 1 -ar 44100 -acodec pcm_s16le \"{1}\"",
                            mixInThisAudioOrVideoFile.FullName, tempSourceWavLocation);
                        ffmpeg.StartInfo.FileName = "ffmpeg.exe";
                        ffmpeg.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        ffmpeg.StartInfo.RedirectStandardError = true;
                        ffmpeg.StartInfo.RedirectStandardOutput = true;
                        ffmpeg.StartInfo.CreateNoWindow = true;
                        //ffmpeg.StartInfo.Arguments = Argument;

                        ffmpeg.Start();
                        ffmpeg.WaitForExit();

                        ffmpeg.BeginOutputReadLine();
                        ffmpeg.BeginErrorReadLine();

                    }
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
