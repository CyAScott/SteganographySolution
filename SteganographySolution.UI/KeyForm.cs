using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SteganographySolution.Common;

namespace SteganographySolution.UI
{
    public partial class KeyForm : Form
    {
        public byte[] key { get; set; }

        public KeyForm()
        {
            InitializeComponent();
        }

        private void nextBtn_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] temp = Encryption.ConvertFromBase64(this.keyText.Text);
                byte[] genKey = Encryption.GenerateKey();

                if (temp.Length != genKey.Length)
                {
                    MessageBox.Show("Invalid key, check the key length and try again.");
                    return;
                }
                
                this.key = temp;
            }
            catch (Exception) 
            {
            }
            
        }


    }
}
