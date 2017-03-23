using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Backtester
{
    public partial class ProgressWindow : Form
    {
        public static string allala = "HI";

        public string LabelText
        {
            get
            {
                return this.label1.Text;
            }
            set
            {
                this.label1.Text = value;
            }
        }


        public ProgressWindow()
        {
            InitializeComponent();
        }

        private void ProgressWindow_Load(object sender, EventArgs e)
        {

        }

        public void ChangeLabel(string s)
        {
            label1.Text = s;
        }
    }
}
