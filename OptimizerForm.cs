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
    public partial class OptimizerForm : Form
    {

        public List<List<String>> backtestSymbolParams = new List<List<String>>
        {
            /*new List<String> { "NXPI", "9:35:00:0:AM", "3:50:00:0:PM", "0.05", "100", "1000", "1000" },
            new List<String> { "AET", "9:35:00:0:AM", "3:50:00:0:PM", "0.17", "100", "1000", "3000" },
            new List<String> {"GE", "9:35:00:0:AM", "3:50:00:0:PM", "0.04", "100", "1200", "3000" }*/


        };
        public Dictionary<string, List<string>> backtestSymbolDates = new Dictionary<string, List<string>>
        {
            /*{"NXPI", backtestDates } */

        };

        public Dictionary<string, int> optimizeShow = new Dictionary<string, int>();

        public TestResult testResult = new TestResult();
        public DateTime startDate;
        public DateTime endDate;

        public static OptimizeResultsForm orf;

        public bool processCancelled = false;
        public bool closePending = false;


        public int numScenarios = 0;

        public OptimizerForm()
        {
            InitializeComponent();
        }

        //Time fields

        private void button1_Click(object sender, EventArgs e)
        {
            string startTime = dateTimePicker6.Value.ToString("h:mm:ss:ff:tt");
            string endTime = dateTimePicker7.Value.ToString("h:mm:ss:ff:tt");

            if (textBox3.Text == "")
            {
                textBox3.Text = startTime + "-" + endTime;
            }
            else textBox3.Text += "," + startTime + "-" + endTime;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox3.Text = "";
        }

        //Increment price fields

        private void button4_Click(object sender, EventArgs e)
        {
            decimal startIncrement = numericUpDown1.Value;
            decimal endIncrement = numericUpDown2.Value;
            decimal step = numericUpDown3.Value;

            for (decimal i = startIncrement; i <= endIncrement; i += step)
            {
                if (textBox4.Text == "") textBox4.Text = i.ToString();
                else textBox4.Text += "," + i.ToString();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            decimal price = numericUpDown4.Value;
            if (textBox4.Text == "") textBox4.Text = price.ToString();
            else textBox4.Text += "," + price.ToString();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            textBox4.Text = "";
        }

        //Increment Size fields

        private void button7_Click(object sender, EventArgs e)
        {
            int incrementSize = Convert.ToInt32(numericUpDown5.Value);
            if (textBox5.Text == "") textBox5.Text = incrementSize.ToString();
            else textBox5.Text += "," + incrementSize.ToString();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            textBox5.Text = "";
        }

        //Auto balance fields

        private void button11_Click(object sender, EventArgs e)
        {
            decimal startBalance = numericUpDown6.Value;
            decimal endBalance = numericUpDown7.Value;
            decimal step = numericUpDown8.Value;

            for (decimal i = startBalance; i <= endBalance; i += step)
            {
                if (textBox6.Text == "") textBox6.Text = i.ToString();
                else textBox6.Text += "," + i.ToString();
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            decimal balance = numericUpDown9.Value;
            if (textBox6.Text == "") textBox6.Text = balance.ToString();
            else textBox6.Text += "," + balance.ToString();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            textBox6.Text = "";
        }

        //Hard Stop fields

        private void button14_Click(object sender, EventArgs e)
        {
            decimal startStop = numericUpDown13.Value;
            decimal endStop = numericUpDown12.Value;
            decimal step = numericUpDown11.Value;

            for (decimal i = startStop; i <= endStop; i += step)
            {
                if (textBox7.Text == "") textBox7.Text = i.ToString();
                else textBox7.Text += "," + i.ToString();
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            decimal stop = numericUpDown10.Value;
            if (textBox7.Text == "") textBox7.Text = stop.ToString();
            else textBox7.Text += "," + stop.ToString();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            textBox7.Text = "";
        }

        private void OptimizerForm_Load(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text += dateTimePicker5.Value.ToString("yyyy-MM-dd");
            textBox1.Text += "\r\n";
        }

        private void button16_Click(object sender, EventArgs e)
        {
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
            textBox6.Text = "";
            textBox7.Text = "";
        }

        private void button15_Click(object sender, EventArgs e)
        {
            backtestSymbolDates.Clear();
            backtestSymbolParams.Clear();
            optimizeShow.Clear();

            if (button15.Text == "Cancel")
            {
                processCancelled = true;
                if (backgroundWorker1.IsBusy) backgroundWorker1.CancelAsync();
                else if (backgroundWorker2.IsBusy) backgroundWorker2.CancelAsync();

                button15.Text = "Run Test";
                return;
            }
            processCancelled = false;
            startDate = dateTimePicker1.Value;
            endDate = dateTimePicker2.Value;

            //Get OptimizeShow values
            if (radioButton1.Checked)
            {
                optimizeShow.Add("TotalPL", Convert.ToInt32(numericUpDown14.Value));
            }
            if (radioButton2.Checked)
            {
                optimizeShow.Add("ProfitMargin", Convert.ToInt32(numericUpDown14.Value));
            }

            //Get test dates
            List<DateTime> _symbolTestDates = DataHelper.GetTradingDays(dateTimePicker1.Value, dateTimePicker2.Value);
            List<string> _testDates = new List<string>();

            foreach (DateTime d in _symbolTestDates)
            {
                _testDates.Add(d.ToString("yyyyMMdd"));
            }


            //Get exclude dates
            string globalExcludeDatesText = textBox1.Text.Replace("-", "");
            string[] globalExcludeDates = globalExcludeDatesText.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            List<string> excludeDates = new List<string>(globalExcludeDates);

            //Combine, remove duplicates and exclude dates for full list of test dates

            List<string> testDates = new List<string>();
            foreach (string _d in _testDates)
            {
                if (!(excludeDates.Contains(_d))) testDates.Add(_d);
            }
            //Get symbol, add to symbolDates
            string symbol = textBox2.Text.Trim();

            backtestSymbolDates.Add(symbol, testDates);

            

            //Make a list for every param
            string[] startEndTimes = textBox3.Text.Split(',');
            string[] incrementPrices = textBox4.Text.Split(',');
            string[] incrementSizes = textBox5.Text.Split(',');
            string[] autoBalances = textBox6.Text.Split(',');
            string[] hardStops = textBox7.Text.Split(',');

            //Add every combo to symbol test params

            for (int a = 0; a < startEndTimes.Length; a++)
            {
                for (int b = 0; b < incrementPrices.Length; b++)
                {
                    for (int c = 0; c < incrementSizes.Length; c++)
                    {
                        for (int d = 0; d < autoBalances.Length; d++)
                        {
                            for (int f = 0; f < hardStops.Length; f++)
                            {
                                backtestSymbolParams.Add( new List<string> { symbol, startEndTimes[a].Split('-')[0], startEndTimes[a].Split('-')[1], incrementPrices[b],
                                    incrementSizes[c], autoBalances[d], hardStops[f] });
                            }
                        }
                    }
                }
            }

            numScenarios = backtestSymbolParams.Count;
            button15.Text = "Cancel";
            

            backgroundWorker1.RunWorkerAsync();

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Backtest backtest = new Backtest(backtestSymbolParams, backtestSymbolDates, sender as BackgroundWorker);

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


            label15.Text = "Testing: " + currSymbol + " ... " + symbolNumber.ToString() + " / " + numScenarios.ToString() + " ... " + currDate;
            label15.Refresh();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (closePending) this.Close();
            testResult = (TestResult)e.Result;
            if (testResult == null)  //Process cancelled
            {
                label15.Text = "Process Cancelled. Ready for a new test";
                label15.Refresh();
                processCancelled = false;   //Reset process cancelled flag
                return;
            }
            
            testResult.startDate = startDate;
            testResult.endDate = endDate;
            label15.Text = "Backtest Completed on " + numScenarios + " scenarios...Preparing Report...";
            label15.Refresh();

            backgroundWorker2.RunWorkerAsync();

            


        }

        //Save test
        private void button18_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            if (!(Directory.Exists("./Data/Optimizers/Saved"))) Directory.CreateDirectory("./Data/Optimizers/Saved");
            saveFileDialog1.InitialDirectory = Directory.GetCurrentDirectory() + "\\Data\\Optimizers\\Saved";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName))
                {
                    sw.Write("<formData>");
                    sw.WriteLine("<startDate>" + dateTimePicker1.Value + "</startDate>");
                    sw.WriteLine("<endDate>" + dateTimePicker2.Value + "</endDate>");
                    sw.WriteLine("<excludeDates>" + textBox1.Text + "</excludeDates>");
                    sw.Write("<symbolTextBox>" + textBox2.Text + "</symbolTextBox>");
                    sw.Write("<timeTextBox>" + textBox3.Text + "</timeTextBox>");
                    sw.Write("<incrementPriceTextBox>" + textBox4.Text + "</incrementPriceTextBox>");
                    sw.Write("<incrementSizeTextBox>" + textBox5.Text + "</incrementSizeTextBox>");
                    sw.Write("<autobalanceTextBox>" + textBox6.Text + "</autobalanceTextBox>");
                    sw.Write("<hardStopTextBox>" + textBox7.Text + "</hardStopTextBox>");
                    sw.Write("<totalPLRadioButton>" + radioButton1.Checked + "</totalPLRadioButton>");
                    sw.Write("<showCount>" + numericUpDown14.Value + "</showCount>");
                    sw.Write("<profitMarginRadioButton>" + radioButton2.Checked + "</profitMarginRadioButton>");
                    sw.WriteLine("</formData>");
                }

            }
            else
            {
                return;
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            if (!(Directory.Exists("./Data/Optimizers/Saved"))) Directory.CreateDirectory("./Data/Optimizers/Saved");
            openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory() + "\\Data\\Optimizers\\Saved";
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

                string excludeDates = xmlDoc.SelectSingleNode("//excludeDates").InnerText;
                textBox1.Text = excludeDates;

                string symbolTextBox = xmlDoc.SelectSingleNode("//symbolTextBox").InnerText;
                textBox2.Text = symbolTextBox;

                string timeTextBox = xmlDoc.SelectSingleNode("//timeTextBox").InnerText;
                textBox3.Text = timeTextBox;

                string incrementPriceTextBox = xmlDoc.SelectSingleNode("//incrementPriceTextBox").InnerText;
                textBox4.Text = incrementPriceTextBox;

                string incrementSizeTextBox = xmlDoc.SelectSingleNode("//incrementSizeTextBox").InnerText;
                textBox5.Text = incrementSizeTextBox;

                string autobalanceTextBox = xmlDoc.SelectSingleNode("//autobalanceTextBox").InnerText;
                textBox6.Text = autobalanceTextBox;

                string hardStopTextBox = xmlDoc.SelectSingleNode("//hardStopTextBox").InnerText;
                textBox7.Text = hardStopTextBox;

                string totalPLCheckBox = xmlDoc.SelectSingleNode("//totalPLRadioButton").InnerText;
                radioButton1.Checked = Convert.ToBoolean(totalPLCheckBox);

                string showCount = xmlDoc.SelectSingleNode("//showCount").InnerText;
                numericUpDown14.Value = Convert.ToDecimal(showCount);

                string profitMarginCheckBox = xmlDoc.SelectSingleNode("//profitMarginRadioButton").InnerText;
                radioButton2.Checked = Convert.ToBoolean(profitMarginCheckBox);

            }
            else { return; }
        }

        private void groupBox7_Enter(object sender, EventArgs e)
        {

        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            orf = new OptimizeResultsForm();
            orf.PrepareReport(testResult, optimizeShow, sender as BackgroundWorker);
           
        }

        private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            label15.Text = "Preparing report for scenario... " + e.ProgressPercentage + " / " + e.UserState.ToString();
            label15.Refresh();
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (closePending) this.Close();
            if (processCancelled)
            {
                processCancelled = false;
                label15.Text = "Processed cancelled. Ready for new test";
                label15.Refresh();
                return;
            }
            
            orf.Show();

            //button15.Enabled = true;

            button15.Text = "Run Test";

            label15.Text = "Ready";
            label15.Refresh();
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
            else if (backgroundWorker2.IsBusy)
            {
                closePending = true;
                backgroundWorker2.CancelAsync();
                e.Cancel = true;
                this.Enabled = false;   // or this.Hide()
                return;
            }
            base.OnFormClosing(e);
        }
    }
}
