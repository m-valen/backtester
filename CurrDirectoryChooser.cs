using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Backtester
{
    public partial class CurrDirectoryChooser : Form
    {
        public CurrDirectoryChooser()
        {
            InitializeComponent();
        }

        private void CurrDirectoryChooser_Load(object sender, EventArgs e)
        {
            textBox1.Text = Directory.GetCurrentDirectory();   
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //Set the current directory.
                Directory.SetCurrentDirectory(textBox1.Text);
                Properties.Settings.Default.WorkingDirectory = textBox1.Text;
                Properties.Settings.Default.Save();

            }
            catch (DirectoryNotFoundException f)
            {
                MessageBox.Show("Could not set to provided directory");
            }
        }
    }
}
