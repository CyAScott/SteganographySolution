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

        public static async void Decrypt()
        {
            using(var dialog = new OpenFileDialog())
            {
                // TODO: set filter value
                if (dialog.ShowDialog() != DialogResult.OK) return;
                var file = new FileInfo(dialog.FileName);

                using (var keyForm = new KeyForm())
                {
                    var key = keyForm.key;

                    using (var spinForm = new SpinnerForm())
                    {
                        var tempFile = Path.GetTempPath();
                        var saveFile = new SaveFileDialog();
                        saveFile.Title = "Choose save location";
                        FileStream tStream = new FileStream(tempFile, FileMode.Open);
                        spinForm.outTextBoxSet("Starting Extraction...\n");
                        var extractResult = EmbbedFile.ExtractFileFromMediaFile(file, tempFile);
                        extractResult.Wait();

                        if (extractResult.IsCompleted)
                        {
                            spinForm.outTextBoxSet("Extraction Complete.\n");
                        }

                        spinForm.outTextBoxSet("Starting Decryption...");

                        saveFile.ShowDialog();
                        FileStream saveStream = new FileStream(saveFile.FileName, FileMode.Open);
                        await Encryption.DecryptStream(tStream, saveStream, key);

                        spinForm.outTextBoxSet("Decryption Complete.\n");
                
                    }
                }
            }
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
                        Encrypt();
                    }
                    else if (chooseOption.findMovieRadio.Checked)
                    {
                        FindMovie(file, time);
                    }
                    else if (chooseOption.recordRadio.Checked)
                    {
                        RecordFromWebCam(file, time);
                    }
                }
            }
        }

        public static async void FindMovie(FileInfo fileIn, TimeSpan tSpan)
        {
            using (var chooseFile = new OpenFileDialog())
            {
                chooseFile.Filter = "";
                // TODO: Filter video file types (filter string)
                if (chooseFile.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var chosenFile = new FileInfo(chooseFile.FileName);
                var chosenFileLength = chosenFile.GetMediaFileLenth();

                if (chosenFileLength.Result >= tSpan)
                {
                    using (var spinWait = new SpinnerForm())
                    {
                        spinWait.Show();
                        var tempFile = Path.GetTempFileName();
                        var key = Encryption.GenerateKey();
                        var saveFile = new SaveFileDialog();
                        saveFile.Title = "Select save location.";
                        FileStream inStream = new FileStream(chooseFile.FileName, FileMode.Open);
                        FileStream outStream = new FileStream(tempFile, FileMode.Open);

                        spinWait.outTextBoxSet("Starting Encryption...\n");
                        await Encryption.EncryptStream(inStream, outStream, key);
          
                        spinWait.outTextBoxSet("File Encryption Complete.\n");
                        
                        saveFile.ShowDialog();
                        spinWait.outTextBoxSet("Starting Embed...\n");
                        await EmbbedFile.EmbedStreamIntoFlacFile(outStream, (int) outStream.Length, 
                            chosenFile, saveFile.FileName);
                      
                        spinWait.outTextBoxSet("File Embed Complete.\n");
                      
                        inStream.Close();
                        outStream.Close();
                    }
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
                var temp = Path.GetTempFileName();

                if (result.AudioDevices.Length == 0)
                    throw new ArgumentException("No audio devices found.");

				if (result.WebCams.Length == 0)
					throw new ArgumentException("No web cams found.");

				using (var dialog = new ChooseWebCamForm(result))
				{
					if (dialog.ShowDialog() != DialogResult.OK) return;

					await WebCam.RecordVideo(
						dialog.webCamComboBox.SelectedItem as string, 
						dialog.audioDeivceComboBox.SelectedItem as string,
						tSpan, temp);
				}

                using (var spinWait = new SpinnerForm())
                {
                    var tempFile = Path.GetTempFileName();
                    var key = Encryption.GenerateKey();
                    var saveFile = new SaveFileDialog();
                    saveFile.Title = "Select save location.";
                    FileInfo recordedInfo = new FileInfo(temp);
                    FileStream inStream = new FileStream(temp, FileMode.Open);
                    FileStream outStream = new FileStream(tempFile, FileMode.Open);

                    await Encryption.EncryptStream(inStream, outStream, key);
                    spinWait.outTextBoxSet("File Encryption Complete.");
                    
                    saveFile.ShowDialog();

                    await EmbbedFile.EmbedStreamIntoFlacFile(outStream, (int)outStream.Length,
                        recordedInfo, saveFile.FileName);
                    spinWait.outTextBoxSet("File Embedded Successfully.");
                
                    inStream.Close();
                    outStream.Close();
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
	}
}
