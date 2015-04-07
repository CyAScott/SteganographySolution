using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using SteganographySolution.Common;

namespace SteganographySolution.UI
{
	public static class Program
	{
		#region UI Threaded Methods
		private static Form form;
		public static TForm New<TForm>()
			where TForm : new()
		{
			return form.InvokeRequired ? (TForm)form.Invoke(new Func<TForm>(() => new TForm())) : new TForm();
		}
		public static DialogResult GetFormDialogResults<TForm>(this TForm This)
			where TForm : Form, new()
		{
			return form.InvokeRequired ? (DialogResult)form.Invoke(new Func<DialogResult>(This.ShowDialog)) : This.ShowDialog();
		}
		public static DialogResult GetDialogResults<TDialog>(this TDialog This)
			where TDialog : CommonDialog, new()
		{
			return form.InvokeRequired ? (DialogResult)form.Invoke(new Func<DialogResult>(This.ShowDialog)) : This.ShowDialog();
		}
		public static void UIDispose(this IDisposable dispose)
		{
			if (form.InvokeRequired)
				form.Invoke(new Action(dispose.Dispose));
			else
				dispose.Dispose();
		}
		public static void ShowForm<TForm>(this TForm This)
			where TForm : Form, new()
		{
			if (form.InvokeRequired)
				form.Invoke(new Action(This.Show));
			else
				This.Show();
		}
		public static void SetTitleAndFilter(this OpenFileDialog dialog, string filter, string title)
		{
			if (form.InvokeRequired)
				form.Invoke(new Action(() =>
				{
					dialog.Filter = filter;
					dialog.Title = title;
				}));
			else
			{
				dialog.Filter = filter;
				dialog.Title = title;
			}
		}
		public static void SetTitleAndFilter(this SaveFileDialog dialog, string filter, string title)
		{
			if (form.InvokeRequired)
				form.Invoke(new Action(() =>
				{
					dialog.Filter = filter;
					dialog.Title = title;
				}));
			else
			{
				dialog.Filter = filter;
				dialog.Title = title;
			}
		}
		[STAThread]
		public static void Main()
		{
			using (form = new Form())
			{
				form.FormBorderStyle = FormBorderStyle.None;
				form.ShowInTaskbar = false;
				form.Opacity = 0;
				Start();
				Application.Run(form);
			}
		}
		#endregion

		public static async Task Start()
		{
			if (MessageBox.Show("If you would like to embed a file into a video click yes " +
				"else cick no to extract a file from a video.",
				"Embed or Extract",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question) == DialogResult.Yes)
				await Encrypt();
			else
				await Decrypt();
		}
		public static void End()
		{
			form.UIDispose();
		}
        public static async Task Decrypt()
        {
	        var dialog = New<OpenFileDialog>();
	        try
	        {
				dialog.SetTitleAndFilter("Video File (*.avi)|*.avi", "Open Video File");
		        if (dialog.GetDialogResults() != DialogResult.OK) return;

		        var file = new FileInfo(dialog.FileName);

		        var keyForm = New<KeyForm>();
		        try
		        {
			        if (keyForm.GetFormDialogResults() != DialogResult.OK) return;

			        var key = keyForm.key;

			        var saveFile = New<SaveFileDialog>();
			        try
					{
						saveFile.SetTitleAndFilter("All Files (*.*)|*.*", "Export File Location");
				        if (saveFile.GetDialogResults() != DialogResult.OK) return;

				        var spinForm = New<SpinnerForm>();
				        try
				        {
					        spinForm.ShowForm();

					        var tempFile = Path.GetTempFileName();

					        spinForm.outTextBoxSet("Starting Extraction...\n");
					        await file.ExtractFileFromMediaFile(tempFile);

					        spinForm.outTextBoxSet("Starting Decryption...");
					        using (var tStream = new FileStream(tempFile, FileMode.Open))
					        using (var saveStream = new FileStream(saveFile.FileName, FileMode.CreateNew))
								await tStream.DecryptStream(saveStream, key);

					        spinForm.outTextBoxSet("Decryption Complete.\n");
				        }
				        finally
						{
							spinForm.UIDispose();
				        }

						End();
					}
			        finally
			        {
				        saveFile.UIDispose();
			        }
		        }
		        finally
		        {
			        keyForm.UIDispose();
		        }
	        }
	        finally
	        {
		        dialog.UIDispose();
	        }
        }
        public static async Task Encrypt()
        {
	        var dialog = New<OpenFileDialog>();
	        try
	        {
				dialog.SetTitleAndFilter("All Files (*.*)|*.*", "File to Embed");
		        if (dialog.GetDialogResults() != DialogResult.OK) return;

		        var file = new FileInfo(dialog.FileName);

		        var time = file.EstimateAudioTrackLength();

		        MessageBox.Show("Estimated length of the video needs to be greater than or equal to: " + time, "Video Length", MessageBoxButtons.OK, MessageBoxIcon.Information);

		        var chooseOption = New<ChooseForm>();
		        try
		        {
			        if (chooseOption.GetFormDialogResults() != DialogResult.OK) return;

			        if (chooseOption.chooseFileRadio.Checked)
				        await Encrypt();
			        else if (chooseOption.findMovieRadio.Checked)
				        await FindMovie(file, time);
			        else if (chooseOption.recordRadio.Checked)
				        await RecordFromWebCam(file, time);
		        }
		        finally
		        {
			        chooseOption.UIDispose();
		        }
	        }
	        finally
	        {
		        dialog.UIDispose();
	        }
        }
        public static async Task FindMovie(FileInfo fileIn, TimeSpan tSpan)
        {
	        var chooseFile = New<OpenFileDialog>();

	        try
	        {
				chooseFile.SetTitleAndFilter("Video Files (*.mp4)|*.mp4", "Video File to Embed File In");
		        if (chooseFile.GetDialogResults() != DialogResult.OK) return;

		        var chosenFile = new FileInfo(chooseFile.FileName);
		        var chosenFileLength = chosenFile.GetMediaFileLenth();

		        if (chosenFileLength.Result >= tSpan)
			        await ExportVideo(fileIn, chosenFile);
				else
		        {
			        MessageBox.Show("The video was not long enough.", "Video Length", MessageBoxButtons.OK, MessageBoxIcon.Error);
					await FindMovie(fileIn, tSpan);
		        }
	        }
	        finally
	        {
		        chooseFile.UIDispose();
	        }
        }
        public static async Task RecordFromWebCam(FileInfo fileIn, TimeSpan tSpan)
		{
	        try
	        {
		        var result = await WebCam.GetAllWebcamDevices();
		        var temp = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".avi");

		        if (result.AudioDevices.Length == 0)
			        throw new ArgumentException("No audio devices found.");

		        if (result.WebCams.Length == 0)
			        throw new ArgumentException("No web cams found.");

		        var ChooseWebCamDialog = (ChooseWebCamForm)form.Invoke(new Func<ChooseWebCamForm>(() => new ChooseWebCamForm(result)));
				
				try
		        {
			        if (ChooseWebCamDialog.GetFormDialogResults() != DialogResult.OK) return;

					await WebCam.RecordVideo(
				        ChooseWebCamDialog.webCamComboBox.SelectedItem as string,
				        ChooseWebCamDialog.audioDeivceComboBox.SelectedItem as string,
				        tSpan, temp);
		        }
		        finally
		        {
			        ChooseWebCamDialog.UIDispose();
		        }

				await ExportVideo(fileIn, new FileInfo(temp));
	        }
	        catch (Exception error)
	        {
		        if (MessageBox.Show(error.Message, "Error Occurred",
			        MessageBoxButtons.RetryCancel,
			        MessageBoxIcon.Error) == DialogResult.Retry)
			        RecordFromWebCam(fileIn, tSpan);
	        }
        }
		public static async Task ExportVideo(FileInfo fileIn, FileInfo videoFile)
		{
			var key = Encryption.GenerateKey();

			var spinWait = New<SpinnerForm>();

			try
			{
				spinWait.ShowForm();

				var encryptedFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

				spinWait.outTextBoxSet("The key is: " + key.ConvertToBase64());

				spinWait.outTextBoxSet("Encrypting the file...\n");

				using (var inStream = fileIn.OpenRead())
				using (var encryptedStream = File.Create(encryptedFile))
					await Encryption.EncryptStream(inStream, encryptedStream, key);

				Debug.WriteLine(Convert.ToBase64String(File.ReadAllBytes(encryptedFile)));

				spinWait.outTextBoxSet("Embedding the encrypted file into an audio file...\n");
				var flacFile = Path.Combine(Path.GetTempPath(),
					Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".flac");
				using (var encryptedStream = new FileStream(encryptedFile, FileMode.Open))
					await encryptedStream.EmbedStreamIntoFlacFile((int)(new FileInfo(encryptedFile)).Length, videoFile, flacFile);

				spinWait.outTextBoxSet("Merging the audio file with the video file...\n");

				var saveFile = New<SaveFileDialog>();

				try
				{
					saveFile.SetTitleAndFilter("Video Files (*.avi)|*.avi", "Export Video");
					if (saveFile.GetDialogResults() != DialogResult.OK) return;

					await videoFile.ReplaceAudioTrackWithFlacFile(new FileInfo(flacFile), saveFile.FileName);

				}
				finally
				{
					saveFile.UIDispose();
				}

				spinWait.outTextBoxSet("File Embed Complete.\n");

				form.Invoke(new Action<string>(Clipboard.SetText), key.ConvertToBase64());

				MessageBox.Show("Your encryption key was put in the clipboard.", "File Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			finally
			{
				spinWait.UIDispose();
			}

			End();
		}
	}
}
