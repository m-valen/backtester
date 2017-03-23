using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;

namespace Backtester
{
    public partial class BacktestForm : Form
    {
        public List<string> backtestDates = new List<string> ();

        public List<List<String>> backtestSymbolParams = new List<List<String>> {
            /*new List<String> { "NXPI", "9:35:00:0:AM", "3:50:00:0:PM", "0.05", "100", "1000", "1000" },
            new List<String> { "AET", "9:35:00:0:AM", "3:50:00:0:PM", "0.17", "100", "1000", "3000" },
            new List<String> {"GE", "9:35:00:0:AM", "3:50:00:0:PM", "0.04", "100", "1200", "3000" },
            new List<String> {"TDG", "9:35:00:0:AM", "3:50:00:0:PM", "0.40", "100", "800", "2000" },
            new List<String> {"ADBE", "9:35:00:0:AM", "3:50:00:0:PM", "0.07", "100", "1000", "3000" },
            new List<String> {"TSN", "9:35:00:0:AM", "3:50:00:0:PM", "0.06", "100", "1000", "3000" },
            new List<String> {"KMB", "9:35:00:0:AM", "3:50:00:0:PM", "0.06", "100", "3000", "3000" },
            new List<String> {"MMM", "9:35:00:0:AM", "3:50:00:0:PM", "0.11", "100", "1000", "3000" },
            new List<String> {"DIS", "9:35:00:0:AM", "3:50:00:0:PM", "0.06", "100", "1000", "3000" },
            new List<String> {"PNC", "9:35:00:0:AM", "3:50:00:0:PM", "0.08", "100", "1000", "3000" },
            new List<String> {"LLY", "9:35:00:0:AM", "3:50:00:0:PM", "0.06", "100", "1000", "3000" }*/


        };
        public 
            Dictionary<string, List<string>> backtestSymbolDates = new Dictionary<string, List<string>> {
            /*{"NXPI", backtestDates },
            {"AET", backtestDates },
            {"GE", backtestDates },
            {"TDG", new List<string> {"20170103", "20170104", "20170105", "20170106", "20170109", "20170110", "20170111", "20170112", "20170113", "20170117", "20170118",
"20170119" } }, //January 20 stock was downgraded
            {"ADBE", backtestDates },
            {"TSN", backtestDates },
            {"KMB", backtestDates },
            {"MMM", backtestDates },
            {"DIS", backtestDates },
            {"PNC", backtestDates },
            {"LLY", backtestDates }*/

        };

        public TestResult testResult = new TestResult();
        public DateTime startDate;
        public DateTime endDate;

        public int numSymbols = 0;

        public bool processCancelled = false;
        public bool closePending = false;

        public BacktestForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Cancel")
            {
                backgroundWorker1.CancelAsync();
                button1.Text = "Run Test";
                button1.Refresh();
                processCancelled = true;
                return;
            }

            backtestSymbolDates.Clear();
            backtestSymbolParams.Clear();

            string[] _symbolLines = textBox2.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            List<string> symbolLines = new List<string>(_symbolLines);
            
            foreach (string line in symbolLines)
            {
                //Separate params
                string[] symbolParams;
                int index = line.IndexOf(",{", 0);
                if (index > 0)
                {
                    string _params = line.Substring(0, index);
                    _params = _params.Trim();
                    symbolParams = _params.Split(',');
                    backtestSymbolParams.Add(new List<string>(symbolParams));
                }
                else
                {
                    continue;
                }
                //Separate exclude dates
                int indexTo = line.IndexOf("}", 0);
                string _excludeDates = line.Substring(index + 2, indexTo - (index + 2));
                string[] symbolExcludeDates = _excludeDates.Split('-');

                //get list of dates to test
                List<DateTime> _symbolTestDates = DataHelper.GetTradingDays(dateTimePicker1.Value, dateTimePicker2.Value);



                startDate = _symbolTestDates[0];
                endDate = _symbolTestDates[_symbolTestDates.Count - 1];
                List<string> symbolTestDates = new List<string>();

                foreach (DateTime d in _symbolTestDates)
                {
                    symbolTestDates.Add(d.ToString("yyyyMMdd"));
                }
                    //Remove exclude dates
                foreach (string d in symbolExcludeDates)
                {
                    if (symbolTestDates.Contains(d)) symbolTestDates.Remove(d);
                }

                backtestSymbolDates.Add(symbolParams[0], symbolTestDates);

                //Check for missing symbol date tick files

                List<string> symbols = new List<string>();

                foreach (string s in symbolParams)
                {
                    symbols.Add(s);
                }


            }

            numSymbols = backtestSymbolParams.Count;

            button1.Text = "Cancel";

            backgroundWorker1.RunWorkerAsync();

           
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text += dateTimePicker5.Value.ToString("yyyy-MM-dd");
            textBox1.Text += "\r\n";
        }

        private void dateTimePicker3_ValueChanged(object sender, EventArgs e)
        {
            dateTimePicker6.Value = dateTimePicker3.Value;
        }

        private void dateTimePicker4_ValueChanged(object sender, EventArgs e)
        {
            dateTimePicker7.Value = dateTimePicker4.Value;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            List<string> symbolParams = new List<string>();
            symbolParams.Add(textBox3.Text.ToUpper());   //Symbol
            symbolParams.Add(dateTimePicker6.Value.ToString("h:mm:ss:ff:tt"));
            symbolParams.Add(dateTimePicker7.Value.ToString("h:mm:ss:ff:tt"));
            symbolParams.Add(numericUpDown1.Value.ToString());
            symbolParams.Add(numericUpDown2.Value.ToString());
            symbolParams.Add(numericUpDown3.Value.ToString());
            symbolParams.Add(numericUpDown4.Value.ToString());

            List<DateTime> symbolDates = new List<DateTime>();
            List<string> symbolExcludeDates = new List<string>();


            symbolDates = DataHelper.GetTradingDays(dateTimePicker1.Value, dateTimePicker2.Value);

            List<string> symbolStringDates = new List<string>();

            foreach (DateTime d in symbolDates)
            {
                symbolStringDates.Add(d.ToString("yyyyMMdd"));
            }

            //Get global exclude dates
            string globalExcludeDatesText = textBox1.Text.Replace("-", "");
            string[] globalExcludeDates = globalExcludeDatesText.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            //Get symbol exclude dates
            string excludeDatesText = textBox4.Text.Replace("-", "");
            string[] _excludeDates = excludeDatesText.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            List<string> excludeDates = new List<string>(_excludeDates); 

            //Add global exclude dates to symbol exclude dates
            foreach (string d in globalExcludeDates)
            {
                if (!(excludeDates.Contains(d))) excludeDates.Add(d);
            }

            //write params, exclude dates to textbox
            foreach(string p in symbolParams)
            {
                textBox2.Text += p + ",";
            }
            textBox2.Text += "{";
            foreach (string d in excludeDates)
            {
                textBox2.Text += d + "-";
            }
            if (excludeDates.Count > 0) { 
                textBox2.Text = textBox2.Text.Remove(textBox2.Text.LastIndexOf("-"), 1);
            }
            textBox2.Text += "}\r\n";


        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox4.Text += dateTimePicker8.Value.ToString("yyyy-MM-dd");
            textBox4.Text += "\r\n";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            textBox3.Text = "";
            dateTimePicker6.Value = dateTimePicker3.Value;
            dateTimePicker7.Value = dateTimePicker4.Value;
            numericUpDown1.Value = Convert.ToDecimal(0.01);
            numericUpDown2.Value = 100;
            numericUpDown3.Value = 1000;
            numericUpDown4.Value = 3000;
            textBox4.Text = "";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            if (!(Directory.Exists("./Data/Tests/Saved"))) Directory.CreateDirectory("./Data/Tests/Saved");
            saveFileDialog1.InitialDirectory = Directory.GetCurrentDirectory() + "\\Data\\Tests\\Saved";


            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName))
                {
                    sw.Write("<formData>");
                    sw.WriteLine("<startDate>" + dateTimePicker1.Value + "</startDate>");
                    sw.WriteLine("<startTime>" + dateTimePicker3.Value + "</startTime>");
                    sw.WriteLine("<endDate>" + dateTimePicker2.Value + "</endDate>");
                    sw.WriteLine("<endTime>" + dateTimePicker4.Value + "</endTime>");
                    sw.WriteLine("<excludeDates>" + textBox1.Text + "</excludeDates>");
                    sw.Write("<symbolTextBox>" + textBox2.Text + "</symbolTextBox>");
                    sw.WriteLine("</formData>");
                }
                    
            }
            else
            {
                return;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (!(Directory.Exists("./Data/Tests/Saved"))) Directory.CreateDirectory("./Data/Tests/Saved");
            openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory() + "\\Data\\Tests\\Saved";
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                string file = openFileDialog1.FileName;
                string xml = File.ReadAllText(file);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xml);

                //Set global settings
                string startDate = xmlDoc.SelectSingleNode("//startDate").InnerText;
                dateTimePicker1.Value = DateTime.Parse(startDate);

                string endDate = xmlDoc.SelectSingleNode("//endDate").InnerText;
                dateTimePicker2.Value = DateTime.Parse(endDate);

                string startTime = xmlDoc.SelectSingleNode("//startTime").InnerText;
                dateTimePicker3.Value = DateTime.Parse(startTime);

                string endTime = xmlDoc.SelectSingleNode("//endTime").InnerText;
                dateTimePicker4.Value = DateTime.Parse(endTime);

                string excludeDates = xmlDoc.SelectSingleNode("//excludeDates").InnerText;
                textBox1.Text = excludeDates;

                string symbolTextBox = xmlDoc.SelectSingleNode("//symbolTextBox").InnerText;
                textBox2.Text = symbolTextBox;

            }
            else
            {

            }
        }

        private void BacktestForm_Load(object sender, EventArgs e)
        {

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Backtest backtest = new Backtest(backtestSymbolParams, backtestSymbolDates, sender as BackgroundWorker);
            List<String> backtestSymbols = new List<String>();
            List<String> _backtestDates = new List<String>();

            TestResult testResult = new TestResult();

            testResult = backtest.Run();

            e.Result = testResult;
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int symbolNumber = e.ProgressPercentage;
            List<string> report = e.UserState as List<string>; // 0 = symbol, 1 = date
            string currSymbol = report[0];
            string currDate = report[1];


            label14.Text = "Testing: " + currSymbol + " ... " + symbolNumber.ToString() + " / " + numSymbols.ToString() + " ... " + currDate;
            label14.Refresh(); 
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (closePending) this.Close();
            testResult = (TestResult)e.Result;
            if (testResult == null)
            {
                label14.Text = "Process cancelled. Ready for a new test";
                label14.Refresh();
                processCancelled = false;
                return;
            }

            testResult.startDate = startDate;
            testResult.endDate = endDate;
            label14.Text = "Backtest Completed on " + numSymbols + " symbols";
            label14.Refresh();

            button1.Text = "Run Test";

            TestResultForm trf = new TestResultForm(testResult);
            trf.Show();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (backgroundWorker1.IsBusy)
            {
                closePending = true;
                backgroundWorker1.CancelAsync();
                e.Cancel = true;
                this.Enabled = false;   // or this.Hide()
                return;
            }
        }
    }
}
