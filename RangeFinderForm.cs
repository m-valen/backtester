using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;

using Microsoft.Office.Interop.Excel;

namespace Backtester
{
    public partial class RangeFinderForm : Form
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

        public bool processCancelled = false;
        public bool closePending = false;

        public string symbol;
        public List<string> dates;
        public int numScenarios = 0;

        public List<SymbolRangeResult> symbolRangeResults = new List<SymbolRangeResult>();

        public Microsoft.Office.Interop.Excel.Application xlApp;
        public Workbook wb;



        public RangeFinderForm()
        {
            InitializeComponent();
        }

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

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text += dateTimePicker5.Value.ToString("yyyy-MM-dd");
            textBox1.Text += "\r\n";
        }

        private void button15_Click(object sender, EventArgs e)
        {
            button15.Text = "Running";

            backtestSymbolDates = new Dictionary<string, List<string>>();
            backtestSymbolDates.Clear();

            backtestSymbolParams = new List<List<String>>();
            backtestSymbolParams.Clear();

            startDate = dateTimePicker1.Value;
            endDate = dateTimePicker2.Value;

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

            for (int a = 0; a < startEndTimes.Length; a++)
            {
                backtestSymbolParams.Add(new List<string> { symbol, startEndTimes[a].Split('-')[0], startEndTimes[a].Split('-')[1]  });
            }

            Run();


            button15.Text = "Run Test";
        }

        public void Run()
        {
            foreach (List<string> symbolParam in backtestSymbolParams)
            {
                SymbolRangeResult symbolRangeResult = new SymbolRangeResult();

                symbol = symbolParam[0];
                dates = backtestSymbolDates[symbol];

                List<decimal> singleResult = new List<decimal>();

                int startMs = DataHelper.GetMsOfDay(symbolParam[1]);
                int endMs = DataHelper.GetMsOfDay(symbolParam[2]);

                foreach (string date in dates)
                {
                    singleResult = TechnicalAnalysis.TradingRange(symbol, date, startMs, endMs);
                    SingleRangeResult srr = new SingleRangeResult();
                    srr.startTime = symbolParam[1];
                    srr.endTime = symbolParam[2];

                    srr.startPrice = singleResult[0];
                    srr.endPrice = singleResult[1];
                    srr.high = singleResult[2];
                    srr.low = singleResult[3];
                    srr.date = date;

                    srr.Calculate();

                    symbolRangeResult.singleRangeResults.Add(srr);
                }
                symbolRangeResult.Calculate();
                symbolRangeResults.Add(symbolRangeResult);
            }

            GenerateExcel();
            symbolRangeResults.Clear();
        }

        public void GenerateExcel()
        {
            xlApp = new Microsoft.Office.Interop.Excel.Application();
            wb = xlApp.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);

            Worksheet ws = (Worksheet)wb.Worksheets.Add();
            ws.Name = "Daily Tests";

            Worksheet symbolSheet = (Worksheet)wb.Worksheets.Add(ws);
            symbolSheet.Name = "Symbol Totals";



            if (!(Directory.Exists("./Data/RangeFinders/Results"))) Directory.CreateDirectory("./Data/RangeFinders/Results");
            saveFileDialog2.InitialDirectory = Directory.GetCurrentDirectory() + "\\Data\\RangeFinders\\Results";
            saveFileDialog2.Filter = "Excel |*.xlsx";


            //cell format: [row, column]

            ws.Cells[1, 1] = "Symbol";
            ws.Cells[1, 2] = "Scenario";
            ws.Cells[1, 3] = "Date";
            ws.Cells[1, 4] = "Start Price";
            ws.Cells[1, 5] = "Final Price";
            ws.Cells[1, 6] = "High";
            ws.Cells[1, 7] = "Low";
            ws.Cells[1, 8] = "Start High Diff";
            ws.Cells[1, 9] = "Start Low Diff";
            ws.Cells[1, 10] = "Start Close Diff";





            symbolSheet.Cells[1, 1] = "Symbol";
            symbolSheet.Cells[1, 2] = "Scenario";
            symbolSheet.Cells[1, 3] = "Average High Diff";
            symbolSheet.Cells[1, 4] = "Greatest High Diff";
            symbolSheet.Cells[1, 5] = "Average Low Diff";
            symbolSheet.Cells[1, 6] = "Greatest Low Diff";
            symbolSheet.Cells[1, 7] = "Average Max Diff";
            symbolSheet.Cells[1, 8] = "Average Close Diff";
            symbolSheet.Cells[1, 9] = "Greatest Close Diff";



            //symbolParamSheet.Cells[1, 1] = "Symbol";
            symbolSheet.Cells[1, 11] = "Start Time";
            symbolSheet.Cells[1, 12] = "End Time";


            int symbolRow = 2;
            int dailyRow = 2;

            int scenarioCount = 1;

            //int totalResults = testResult.SymbolResults.Count;
            //int denominator;
            //if (totalResults < resultsToShow) denominator = totalResults;
            //else denominator = resultsToShow;

            foreach (SymbolRangeResult symbolRangeResult in symbolRangeResults)
            {

                /*if (backgroundWorker1.CancellationPending)
                {
                    processCancelled = true;
                    wb.Close();
                    xlApp.Quit();
                    return;
                }
                */

                //if (scenarioCount <= resultsToShow)
                //{
                // backgroundWorker1.ReportProgress(scenarioCount, denominator);



                symbolSheet.Cells[symbolRow, 1] = symbol;
                symbolSheet.Cells[symbolRow, 2] = scenarioCount;
                symbolSheet.Cells[symbolRow, 3] = symbolRangeResult.averageStartHighDiff;
                symbolSheet.Cells[symbolRow, 4] = symbolRangeResult.greatestHighDiff;
                symbolSheet.Cells[symbolRow, 5] = symbolRangeResult.averageStartLowDiff;
                symbolSheet.Cells[symbolRow, 6] = symbolRangeResult.greatestLowDiff;
                symbolSheet.Cells[symbolRow, 7] = symbolRangeResult.averageMaxDiff;
                symbolSheet.Cells[symbolRow, 8] = symbolRangeResult.averageStartCloseDiff;
                symbolSheet.Cells[symbolRow, 9] = symbolRangeResult.greatestCloseDiff;

                symbolSheet.Cells[symbolRow, 11] = symbolRangeResult.singleRangeResults[0].startTime;
                symbolSheet.Cells[symbolRow, 12] = symbolRangeResult.singleRangeResults[0].endTime;



                foreach (SingleRangeResult srr in symbolRangeResult.singleRangeResults)
                    {
                        ws.Cells[dailyRow, 1] = symbol;
                        ws.Cells[dailyRow, 2] = scenarioCount;
                        ws.Cells[dailyRow, 3] = srr.date;
                        ws.Cells[dailyRow, 4] = srr.startPrice;
                        ws.Cells[dailyRow, 5] = srr.endPrice;
                        ws.Cells[dailyRow, 6] = srr.high;
                        ws.Cells[dailyRow, 7] = srr.low;
                        ws.Cells[dailyRow, 8] = srr.startHighDiff;
                        ws.Cells[dailyRow, 9] = srr.startLowDiff;
                        ws.Cells[dailyRow, 10] = srr.startCloseDiff;

                    dailyRow++;
                    }

                    symbolRow++;
                    scenarioCount++;
               // }
            }

            ws.Range["G2", "N" + dailyRow].NumberFormat = "#.00";
            symbolSheet.Range["E2", "P" + symbolRow].NumberFormat = "#.00";


            symbolSheet.Activate();
            symbolSheet.Application.ActiveWindow.SplitRow = 1;
            symbolSheet.Application.ActiveWindow.FreezePanes = true;

            ws.Activate();
            ws.Application.ActiveWindow.SplitRow = 1;
            ws.Application.ActiveWindow.FreezePanes = true;

            ws.Columns.AutoFit();
            symbolSheet.Columns.AutoFit();

            if (saveFileDialog2.ShowDialog() == DialogResult.OK)
            {
                wb.SaveAs(saveFileDialog2.FileName);
            }
            else
            {
                wb.Close();
                xlApp.Quit();
                return;
            }

            wb.Close();
            xlApp.Quit();
        }

        private void button18_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            if (!(Directory.Exists("./Data/RangeFinders/Saved"))) Directory.CreateDirectory("./Data/RangeFinders/Saved");
            saveFileDialog1.InitialDirectory = Directory.GetCurrentDirectory() + "\\Data\\RangeFinders\\Saved";

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
            if (!(Directory.Exists("./Data/RangeFinders/Saved"))) Directory.CreateDirectory("./Data/RangeFinders/Saved");
            openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory() + "\\Data\\RangeFinders\\Saved";
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

            }
            else { return; }
        }
    }
}
