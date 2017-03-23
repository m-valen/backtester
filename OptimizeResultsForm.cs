using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

using Microsoft.Office.Interop.Excel;

namespace Backtester
{
    public partial class OptimizeResultsForm : Form
    {

        public TestResult testResult = new TestResult();
        public Dictionary<string, int> optimizeShow = new Dictionary<string, int>();
        public int resultsToShow = 0;

        public BackgroundWorker bw = new BackgroundWorker();

        public string textReport = "";

        public Microsoft.Office.Interop.Excel.Application xlApp;
        public Workbook wb;

        public bool processCancelled = false;
        public bool closePending = false;
        public OptimizeResultsForm()
        {
            InitializeComponent();
           
        }

        public void PrepareReport(TestResult _testResult, Dictionary<string, int> _optimizeShow, BackgroundWorker _bw)
        {
            testResult = _testResult;
            optimizeShow = _optimizeShow;
            bw = _bw;

            //Sort results 
            if (optimizeShow.ContainsKey("TotalPL"))
            {
                testResult.SymbolResults.Sort(); //Sort ascending
                testResult.SymbolResults.Reverse();  //Sort descending
                resultsToShow = optimizeShow["TotalPL"];
            }
            else
            {
                testResult.SymbolResults.Sort(delegate (SymbolResult x, SymbolResult y)
                {
                    return x.ProfitMargin.CompareTo(y.ProfitMargin);
                });

                testResult.SymbolResults.Reverse();
                resultsToShow = optimizeShow["ProfitMargin"];
            }

            textReport += "Test for " + testResult.SymbolResults.Count + " scenarios, between " + testResult.startDate.ToString("yyyy-MM-dd") + " and "
                                + testResult.endDate.ToString("yyyy-MM-dd") + "\r\n";
            textReport += "-------------------------------------------------------\r\n\n";

            int scenarioCount = 1;

            foreach (SymbolResult sr in testResult.SymbolResults)
            {
                if (bw.CancellationPending) return;
                if (scenarioCount <= resultsToShow)
                {
                    //bw.ReportProgress(scenarioCount, resultsToShow.ToString());
                    textReport += sr.Symbol + " : " + sr.StartTime + " - " + sr.EndTime + ", " + "Increment Price: " + sr.IncrementPrice + ", Increment Size: " + sr.IncrementSize + ", Autobalance: " +
                    sr.Autobalance + ", Hard Stop: " + sr.HardStop + "\r\n";
                    textReport += "-------------------------------------------------------\r\n";

                    /*foreach (SingleResult single in sr.SingleResults)
                    {
                        textReport += sr.Symbol + " - " + single.Date + "\r\n";
                        if (single.StopTime != null)
                        {
                            textReport += "Stopped Out at : " + single.StopTime + "\r\n";
                        }
                        textReport += "Required Buying Power: " + (single.HighPrint * sr.Autobalance * 2).ToString("#.##") + "\r\n";
                        textReport += "Buy Fills: " + single.BuyFills + "\r\n";
                        textReport += "Sell Fills: " + single.SellFills + "\r\n";
                        textReport += "Start Price: " + single.StartingPrice + "\r\n";
                        textReport += "Final Price: " + single.FinalPrint + "\r\n";
                        textReport += "Final Position: " + single.Position + "\r\n";
                        textReport += "Increment PL: " + single.IncrementPL + "\r\n";
                        textReport += "Price Move PL: " + single.PriceMovePL + "\r\n";
                        textReport += "Total PL: " + single.TotalPL + "\r\n";
                        textReport += "Max Unrealized: " + single.maxUnrealized + "\r\n";
                        textReport += "Min Unrealized: " + single.minUnrealized + "\r\n";
                        textReport += "-------------------------------------------------------\r\n\n";

                    }
                    //textReport += "Totals for: " + sr.Symbol + "\r\n";
                    textReport += "-------------------------------------------------------\r\n";*/
                    textReport += "Buying Power: " + sr.BuyingPower + "\r\n";
                    textReport += "Buy Fills: " + sr.BuyFills + "\r\n";
                    textReport += "Sell Fills: " + sr.SellFills + "\r\n";
                    textReport += "Increment PL: " + sr.IncrementPL + "\r\n";
                    textReport += "Price Move PL: " + sr.PriceMovePL + "\r\n";
                    textReport += "Total PL: " + sr.TotalPL + "\r\n";
                    textReport += "Profit Margin (%): " + sr.ProfitMargin.ToString("#.##") + "\r\n\n";
                    textReport += "Avg Max Unrealized: " + sr.AvgMaxUnrealized.ToString("#.##") + "\r\n";
                    textReport += "Avg Min Unrealized: " + sr.AvgMinUnrealized.ToString("#.##") + "\r\n";
                    textReport += "# Of Wins: " + sr.NumWins + "\r\n";
                    textReport += "Avg Win: " + sr.AvgWin.ToString("#.##") + "\r\n";
                    textReport += "# Of Losses: " + sr.NumLosses + "\r\n";
                    textReport += "Avg Loss: " + sr.AvgLoss.ToString("#.##") + "\r\n";
                    textReport += "-------------------------------------------------------\r\n\n";

                    scenarioCount++;
                }

                //Test result totals
                /*textReport += "Totals\r\n";
                textReport += "-------------------------------------------------------\r\n";
                textReport += "Buy Fills: " + testResult.BuyFills + "\r\n";
                textReport += "Sell Fills: " + testResult.SellFills + "\r\n";
                textReport += "Increment PL: " + testResult.IncrementPL + "\r\n";
                textReport += "Price Move PL: " + testResult.PriceMovePL + "\r\n";
                textReport += "Total PL: " + testResult.TotalPL + "\r\n";
                textReport += "Profit Margin (%): " + testResult.ProfitMargin.ToString("#.##") + "\r\n";*/
            }
            

        }
        private void OptimizeResultsForm_Load(object sender, EventArgs e)
        {
            textBox1.Text = textReport;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            if (!(Directory.Exists("./Data/Optimizers/Results"))) Directory.CreateDirectory("./Data/Optimizers/Results");
            saveFileDialog1.InitialDirectory = Directory.GetCurrentDirectory() + "\\Data\\Optimizers\\Results";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName))
                {
                    sw.Write(textBox1.Text);
                }

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Cancel")
            {
                backgroundWorker1.CancelAsync();
            }
            else if (button1.Text == "Save as Excel") {
                button1.Text = "Cancel";
                backgroundWorker1.RunWorkerAsync();
            }
            
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            xlApp = new Microsoft.Office.Interop.Excel.Application();
            wb = xlApp.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);

            Worksheet ws = (Worksheet)wb.Worksheets.Add();
            ws.Name = "Daily Tests";

            Worksheet symbolSheet = (Worksheet)wb.Worksheets.Add(ws);
            symbolSheet.Name = "Symbol Totals";



            if (!(Directory.Exists("./Data/Optimizers/Results"))) Directory.CreateDirectory("./Data/Optimizers/Results");
            saveFileDialog2.InitialDirectory = Directory.GetCurrentDirectory() + "\\Data\\Optimizers\\Results";
            saveFileDialog2.Filter = "Excel |*.xlsx";





            //cell format: [row, column]

            ws.Cells[1, 1] = "Symbol";
            ws.Cells[1, 2] = "Scenario";
            ws.Cells[1, 3] = "Date";
            ws.Cells[1, 4] = "Max Long";
            ws.Cells[1, 5] = "Max Short";
            ws.Cells[1, 6] = "Buying Power";
            ws.Cells[1, 7] = "Buy Fills";
            ws.Cells[1, 8] = "Sell Fills";
            ws.Cells[1, 9] = "Start Price";
            ws.Cells[1, 10] = "Final Price";
            ws.Cells[1, 11] = "Final Position";
            ws.Cells[1, 12] = "Increment PL";
            ws.Cells[1, 13] = "Price Move PL";
            ws.Cells[1, 14] = "Total PL";
            ws.Cells[1, 15] = "Max Unrealized";
            ws.Cells[1, 16] = "Min Unrealized";
            ws.Cells[1, 17] = "Stop Time";



            symbolSheet.Cells[1, 1] = "Symbol";
            symbolSheet.Cells[1, 2] = "Scenario";
            symbolSheet.Cells[1, 3] = "Buying Power";
            symbolSheet.Cells[1, 4] = "Buy Fills";
            symbolSheet.Cells[1, 5] = "Sell Fills";
            symbolSheet.Cells[1, 6] = "Increment PL";
            symbolSheet.Cells[1, 7] = "Price Move PL";
            symbolSheet.Cells[1, 8] = "Total PL";
            symbolSheet.Cells[1, 9] = "Profit Margin (%)";
            symbolSheet.Cells[1, 10] = "Avg Max Unrealized";
            symbolSheet.Cells[1, 11] = "Avg Min Unrealized";
            symbolSheet.Cells[1, 12] = "# Of Wins";
            symbolSheet.Cells[1, 13] = "Avg Win";
            symbolSheet.Cells[1, 14] = "# Of Losses";
            symbolSheet.Cells[1, 15] = "Avg Loss";



            //symbolParamSheet.Cells[1, 1] = "Symbol";
            symbolSheet.Cells[1, 17] = "Start Time";
            symbolSheet.Cells[1, 18] = "End Time";
            symbolSheet.Cells[1, 19] = "Increment Price";
            symbolSheet.Cells[1, 20] = "Increment Size";
            symbolSheet.Cells[1, 21] = "Autobalance";
            symbolSheet.Cells[1, 22] = "Hard Stop PL";



            int symbolRow = 2;
            int dailyRow = 2;

            int scenarioCount = 1;

            int totalResults = testResult.SymbolResults.Count;
            int denominator;
            if (totalResults < resultsToShow) denominator = totalResults;
            else denominator = resultsToShow;

            foreach (SymbolResult symbolResult in testResult.SymbolResults)
            {

                if (backgroundWorker1.CancellationPending)
                {
                    processCancelled = true;
                    wb.Close();
                    xlApp.Quit();
                    return;
                }


                if (scenarioCount <= resultsToShow)
                {
                    backgroundWorker1.ReportProgress(scenarioCount, denominator);

                    

                    symbolSheet.Cells[symbolRow, 1] = symbolResult.Symbol;
                    symbolSheet.Cells[symbolRow, 2] = symbolResult.scenarioNum;
                    symbolSheet.Cells[symbolRow, 3] = symbolResult.BuyingPower;
                    symbolSheet.Cells[symbolRow, 4] = symbolResult.BuyFills;
                    symbolSheet.Cells[symbolRow, 5] = symbolResult.SellFills;
                    symbolSheet.Cells[symbolRow, 6] = symbolResult.IncrementPL;
                    symbolSheet.Cells[symbolRow, 7] = symbolResult.PriceMovePL;
                    symbolSheet.Cells[symbolRow, 8] = symbolResult.TotalPL;
                    symbolSheet.Cells[symbolRow, 9] = symbolResult.ProfitMargin;
                    symbolSheet.Cells[symbolRow, 10] = symbolResult.AvgMaxUnrealized;
                    symbolSheet.Cells[symbolRow, 11] = symbolResult.AvgMinUnrealized;
                    symbolSheet.Cells[symbolRow, 12] = symbolResult.NumWins;
                    symbolSheet.Cells[symbolRow, 13] = symbolResult.AvgWin;
                    symbolSheet.Cells[symbolRow, 14] = symbolResult.NumLosses;
                    symbolSheet.Cells[symbolRow, 15] = symbolResult.AvgLoss;

                    symbolSheet.Cells[symbolRow, 17] = symbolResult.StartTime;
                    symbolSheet.Cells[symbolRow, 18] = symbolResult.EndTime;
                    symbolSheet.Cells[symbolRow, 19] = symbolResult.IncrementPrice;
                    symbolSheet.Cells[symbolRow, 20] = symbolResult.IncrementSize;
                    symbolSheet.Cells[symbolRow, 21] = symbolResult.Autobalance;
                    symbolSheet.Cells[symbolRow, 22] = symbolResult.HardStop;



                    foreach (SingleResult sr in symbolResult.SingleResults)
                    {
                        ws.Cells[dailyRow, 1] = symbolResult.Symbol;
                        ws.Cells[dailyRow, 2] = symbolResult.scenarioNum;
                        ws.Cells[dailyRow, 3] = sr.Date;
                        ws.Cells[dailyRow, 4] = sr.MaxLong;
                        ws.Cells[dailyRow, 5] = sr.MaxShort;
                        ws.Cells[dailyRow, 6] = sr.MaxBuyingPower;
                        ws.Cells[dailyRow, 7] = sr.BuyFills;
                        ws.Cells[dailyRow, 8] = sr.SellFills;
                        ws.Cells[dailyRow, 9] = sr.StartingPrice;
                        ws.Cells[dailyRow, 10] = sr.FinalPrint;
                        ws.Cells[dailyRow, 11] = sr.Position;
                        ws.Cells[dailyRow, 12] = sr.IncrementPL;
                        ws.Cells[dailyRow, 13] = sr.PriceMovePL;
                        ws.Cells[dailyRow, 14] = sr.TotalPL;
                        ws.Cells[dailyRow, 15] = sr.maxUnrealized;
                        ws.Cells[dailyRow, 16] = sr.minUnrealized;
                        ws.Cells[dailyRow, 17] = sr.StopTime;

                        dailyRow++;
                    }

                    symbolRow++;
                    scenarioCount++;
                }
            }

            ws.Range["G2", "N" + dailyRow].NumberFormat = "#.00";
            symbolSheet.Range["E2", "N" + symbolRow].NumberFormat = "#.00";


            symbolSheet.Activate();
            symbolSheet.Application.ActiveWindow.SplitRow = 1;
            symbolSheet.Application.ActiveWindow.FreezePanes = true;

            ws.Activate();
            ws.Application.ActiveWindow.SplitRow = 1;
            ws.Application.ActiveWindow.FreezePanes = true;

            ws.Columns.AutoFit();
            symbolSheet.Columns.AutoFit();


            
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (closePending)
            {
                this.Close();
            }
            if (processCancelled)
            {
                label1.Text = "Excel Report Cancelled.";
                label1.Refresh();
                button1.Text = "Save as Excel";
                button1.Refresh();
                processCancelled = false;
                return;
            }
            //No cancel
            button1.Text = "Save as Excel";
            if (saveFileDialog2.ShowDialog() == DialogResult.OK)
            {
                wb.SaveAs(saveFileDialog2.FileName);
            }
            else {
                wb.Close();
                xlApp.Quit();
                return;
            }

            wb.Close();
            xlApp.Quit();

            label1.Text = "Excel Report Finished.";
            label1.Refresh();
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            label1.Text = "Preparing Excel report for scenario... " + e.ProgressPercentage + " / " + e.UserState.ToString();
            label1.Refresh();
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
            base.OnFormClosing(e);
        }
    }
}
