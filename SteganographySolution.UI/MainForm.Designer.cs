namespace SteganographySolution.UI
{
	partial class ChooseForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.chooseFileRadio = new System.Windows.Forms.RadioButton();
            this.findMovieRadio = new System.Windows.Forms.RadioButton();
            this.recordRadio = new System.Windows.Forms.RadioButton();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.nextBtn = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.Controls.Add(this.chooseFileRadio, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.findMovieRadio, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.recordRadio, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.cancelBtn, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.nextBtn, 2, 4);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(530, 154);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // chooseFileRadio
            // 
            this.chooseFileRadio.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.chooseFileRadio, 3);
            this.chooseFileRadio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chooseFileRadio.Location = new System.Drawing.Point(3, 3);
            this.chooseFileRadio.Name = "chooseFileRadio";
            this.chooseFileRadio.Size = new System.Drawing.Size(524, 24);
            this.chooseFileRadio.TabIndex = 0;
            this.chooseFileRadio.TabStop = true;
            this.chooseFileRadio.Text = "Choose Another File";
            this.chooseFileRadio.UseVisualStyleBackColor = true;
            // 
            // findMovieRadio
            // 
            this.findMovieRadio.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.findMovieRadio, 3);
            this.findMovieRadio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.findMovieRadio.Location = new System.Drawing.Point(3, 33);
            this.findMovieRadio.Name = "findMovieRadio";
            this.findMovieRadio.Size = new System.Drawing.Size(524, 24);
            this.findMovieRadio.TabIndex = 1;
            this.findMovieRadio.TabStop = true;
            this.findMovieRadio.Text = "Find Movie";
            this.findMovieRadio.UseVisualStyleBackColor = true;
            // 
            // recordRadio
            // 
            this.recordRadio.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.recordRadio, 3);
            this.recordRadio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.recordRadio.Location = new System.Drawing.Point(3, 63);
            this.recordRadio.Name = "recordRadio";
            this.recordRadio.Size = new System.Drawing.Size(524, 24);
            this.recordRadio.TabIndex = 2;
            this.recordRadio.TabStop = true;
            this.recordRadio.Text = "Record from WebCam";
            this.recordRadio.UseVisualStyleBackColor = true;
            // 
            // cancelBtn
            // 
            this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cancelBtn.Location = new System.Drawing.Point(333, 127);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(94, 24);
            this.cancelBtn.TabIndex = 3;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            // 
            // nextBtn
            // 
            this.nextBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.nextBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nextBtn.Location = new System.Drawing.Point(433, 127);
            this.nextBtn.Name = "nextBtn";
            this.nextBtn.Size = new System.Drawing.Size(94, 24);
            this.nextBtn.TabIndex = 4;
            this.nextBtn.Text = "Next";
            this.nextBtn.UseVisualStyleBackColor = true;
            // 
            // ChooseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(530, 154);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ChooseForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Choose One";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.Button nextBtn;
        public System.Windows.Forms.RadioButton chooseFileRadio;
        public System.Windows.Forms.RadioButton findMovieRadio;
        public System.Windows.Forms.RadioButton recordRadio;
	}
}

