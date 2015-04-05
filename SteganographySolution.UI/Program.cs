using System;
using System.IO;
using System.Windows.Forms;
using SteganographySolution.Common;

namespace SteganographySolution.UI
{
	public static class Program
	{
		[STAThread]
		public static void Main()
		{
            if (MessageBox.Show("Would you like to Encrypt, click yes else cick no to Decrypt. ",
                "Encrypt or Decrypt",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
                Encrypt();
            else
                Decrypt();
		}

        public static void Decrypt()
        {

        }

        public static void Encrypt()
        {
            using (var dialog = new OpenFileDialog())
            {
                if (dialog.ShowDialog() != DialogResult.OK) return;

                var file = new FileInfo(dialog.FileName);

                var time = file.EstimateAudioTrackLength();

                MessageBox.Show("Estimated needs to be greater than or equal to: " + time);

                using (var chooseOption = new ChooseForm())
                {
                    if (chooseOption.ShowDialog() != DialogResult.OK) return;

                    if (chooseOption.chooseFileRadio.Checked)
                    {
                        // Make this a method 1 argument = file info type (file opened)
                        Encrypt();
                    }
                    else if (chooseOption.findMovieRadio.Checked)
                    {
                        FindMovie(file, time);
                    }
                    else if (chooseOption.recordRadio.Checked)
                    {
                        // make method
                        RecordFromWebCam(file, time);
                    }
                    else
                    {

                    }
                    
                }
            }
        }

        public static void FindMovie(FileInfo fileIn, TimeSpan tSpan)
        {
            // make method 
            using (var chooseFile = new OpenFileDialog())
            {
                // TODO: Filter video file types (filter string)
                if (chooseFile.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                var chosenFile = new FileInfo(chooseFile.FileName);

                // TODO: read video length 
                if (true)
                {
                    GenerateKey(fileIn, chosenFile, tSpan);
                }
                else
                {
                    FindMovie(fileIn, tSpan);
                }
            }
        }

        public static async void RecordFromWebCam(FileInfo fileIn, TimeSpan tSpan)
        {
            try{

                var result = await WebCam.GetAllWebcamDevices();

                if (result.AudioDevices.Length == 0)
                    throw new ArgumentException("No audio devices found.");

				if (result.WebCams.Length == 0)
					throw new ArgumentException("No web cams found.");

				using (var dialog = new ChooseWebCamForm(result))
				{
					if (dialog.ShowDialog() != DialogResult.OK) return;

					var temp = Path.GetTempFileName();

					await WebCam.RecordVideo(
						dialog.webCamComboBox.SelectedItem as string, 
						dialog.audioDeivceComboBox.SelectedItem as string,
						tSpan, temp);

					GenerateKey(fileIn, new FileInfo(temp), tSpan);
				}
            }
            catch(Exception error){
                if(MessageBox.Show(error.Message, "Error Occurred", 
                    MessageBoxButtons.RetryCancel,
                    MessageBoxIcon.Error) == DialogResult.Retry)
                {
                    RecordFromWebCam(fileIn, tSpan);   
                }
            }
        }

        public static void GenerateKey(FileInfo fileIn, FileInfo video, 
            TimeSpan videoLength)
        {
            var key = Encryption.GenerateKey();



        }   
	}
}
