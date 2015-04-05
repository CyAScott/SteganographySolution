using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
﻿using System.Windows.Forms;
using SteganographySolution.Common;

namespace SteganographySolution.UI
{
    public partial class ChooseWebCamForm : Form
    {
        public ChooseWebCamForm()
        {
            InitializeComponent();
        }

		public ChooseWebCamForm(MediaDevices devices)
		{
			InitializeComponent();

			audioDeivceComboBox.Items.AddRange(devices.AudioDevices);
			webCamComboBox.Items.AddRange(devices.WebCams);

			audioDeivceComboBox.SelectedIndex = 0;
			webCamComboBox.SelectedIndex = 0;
		}
    }
}
