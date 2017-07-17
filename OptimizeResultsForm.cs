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

        public List<string> displayGroups = new List<string>();

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

        public void PrepareReport(TestResult _testResult, Dictionary<string, int> _optimizeShow, List<string> _displayGroups, BackgroundWorker _bw)
        {
            testResult = _testResult;
            optimizeShow = _optimizeShow;
            displayGroups = _displayGroups;
            bw = _bw;

            //Sort results 
            if (optimizeShow.ContainsKey("TotalPL"))
            {
                testResult.SymbolResults.Sort(); //Sort ascending
                testResult.SymbolResults.Reverse();  //Sort descending
                resultsToShow = optimizeShow["TotalPL"];
            }
            else if (optimizeShow.ContainsKey("ProfitMargin"))
            {
                testResult.SymbolResults.Sort(delegate (SymbolResult x, SymbolResult y)
                {
                    return x.ProfitMargin.CompareTo(y.ProfitMargin);
                });

                testResult.SymbolResults.Reverse();
                resultsToShow = optimizeShow["ProfitMargin"];
            }

            else
            {
                testResult.SymbolResults.Sort(delegate (SymbolResult x, SymbolResult y)
                {
                    return x.varianceStopCappedNormalized.CompareTo(y.varianceStopCappedNormalized);
                });

                //testResult.SymbolResults.Reverse();
                resultsToShow = optimizeShow["VarianceNormalized"];
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


            int _symbolSheetRowCount = 0;
            int _symbolSheetColCount = 0;
            int _dailySheetRowCount = 0;
            int _dailySheetColCount = 0;

            /*
            if (displayGroups.Contains("Position-BP"))
            {
                _dailySheetColCount += 4;
                _symbolSheetColCount += 1;
            }
            if (displayGroups.Contains("Fills"))
            {
                _dailySheetColCount += 2;
                _symbolSheetColCount += 4;
            }
            if (displayGroups.Contains("PL"))
            {
                _dailySheetColCount += 6;
                _symbolSheetColCount += 11;
            }
            if (displayGroups.Contains("Range"))
            {
                _dailySheetColCount += 7;
                _symbolSheetColCount += 7;
            }

            _symbolSheetRowCount = testResult.SymbolResults.Count + 1;
            _dailySheetRowCount = (testResult.SymbolResults[0].SingleResults.Count * testResult.SymbolResults.Count) + 1;

            int ssrc = _symbolSheetRowCount;
            int sscc = _symbolSheetColCount;

            int dsrc = _dailySheetRowCount;
            int dscc = _dailySheetColCount;
            */

            List<List<object>> symbolSheetList = new List<List<object>>();
            List<List<object>> dailySheetList = new List<List<object>>();

            //object[,] symbolSheetArray = new object[ssrc, _symbolSheetColCount] { };
            //object[,] dailySheetArray = new object[,] { };

            int wsColumn = 0;



            //cell format: [row, column]
            dailySheetList.Add(new List<object>());

            dailySheetList[0].Add("Symbol"); wsColumn++;
            dailySheetList[0].Add("Scenario"); wsColumn++;
            dailySheetList[0].Add("Date"); wsColumn++;


            if (displayGroups.Contains("Position-BP")) {
                dailySheetList[0].Add("Buying Power"); wsColumn++;
                dailySheetList[0].Add("Max Long"); wsColumn++;
                dailySheetList[0].Add("Max Short"); wsColumn++;
                dailySheetList[0].Add("Final Position"); wsColumn++;
            }
            
            if (displayGroups.Contains("Fills"))
            {
                dailySheetList[0].Add("Buy Fills"); wsColumn++;
                dailySheetList[0].Add("Sell Fills"); wsColumn++;
                dailySheetList[0].Add("Complete Fills"); wsColumn++;
            }

            if (displayGroups.Contains("PL"))
            {
                dailySheetList[0].Add("Increment PL"); wsColumn++;
                dailySheetList[0].Add("Price Move PL"); wsColumn++;
                dailySheetList[0].Add("Total PL"); wsColumn++;
                dailySheetList[0].Add("Max Unrealized"); wsColumn++;
                dailySheetList[0].Add("Min Unrealized"); wsColumn++;
                dailySheetList[0].Add("Stop Time"); wsColumn++;
            }
            

            if (displayGroups.Contains("Range"))
            {
                dailySheetList[0].Add("Start Price"); wsColumn++;
                dailySheetList[0].Add("Final Price"); wsColumn++;
                dailySheetList[0].Add("High"); wsColumn++;
                dailySheetList[0].Add("Low"); wsColumn++;
                dailySheetList[0].Add("Start High Diff"); wsColumn++;
                dailySheetList[0].Add("Start Low Diff"); wsColumn++;
                dailySheetList[0].Add("Start Close Diff"); wsColumn++;
            }

            int symbolSheetColumn = 0;

            symbolSheetList.Add(new List<object>());

            symbolSheetList[0].Add("Symbol"); symbolSheetColumn++;
            symbolSheetList[0].Add("Scenario"); symbolSheetColumn++;

            if (displayGroups.Contains("Position-BP"))
            {
                symbolSheetList[0].Add("Buying Power"); symbolSheetColumn++;
            }

            if (displayGroups.Contains("Fills"))
            {
                symbolSheetList[0].Add("Buy Fills"); symbolSheetColumn++;
                symbolSheetList[0].Add("Avg Buy Fills"); symbolSheetColumn++;
                symbolSheetList[0].Add("Sell Fills"); symbolSheetColumn++;
                symbolSheetList[0].Add("Avg Sell Fills"); symbolSheetColumn++;
                symbolSheetList[0].Add("Complete Fills"); symbolSheetColumn++;
                symbolSheetList[0].Add("Avg Complete Fills"); symbolSheetColumn++;
            }

            if (displayGroups.Contains("PL"))
            {
                symbolSheetList[0].Add("Increment PL"); symbolSheetColumn++;
                symbolSheetList[0].Add("Price Move PL"); symbolSheetColumn++;
                symbolSheetList[0].Add("Total PL"); symbolSheetColumn++;
                symbolSheetList[0].Add("Profit Margin (%)"); symbolSheetColumn++;
                symbolSheetList[0].Add("Avg Max Unrealized"); symbolSheetColumn++;
                symbolSheetList[0].Add("Avg Min Unrealized"); symbolSheetColumn++;
                symbolSheetList[0].Add("# of Wins"); symbolSheetColumn++;
                symbolSheetList[0].Add("Avg Win"); symbolSheetColumn++;
                symbolSheetList[0].Add("# of Losses"); symbolSheetColumn++;
                symbolSheetList[0].Add("Avg Loss"); symbolSheetColumn++;
                symbolSheetList[0].Add("Yield"); symbolSheetColumn++;
            }
            if (displayGroups.Contains("AvgWin-Analysis"))
            {
                symbolSheetList[0].Add("Highest Max Unrealized"); wsColumn++;
                symbolSheetList[0].Add("Days Above Average Win"); wsColumn++;
                symbolSheetList[0].Add("Days Gave Back Average Win"); wsColumn++;
                symbolSheetList[0].Add("Median Max Unrealized"); wsColumn++;
                symbolSheetList[0].Add("Median Win"); wsColumn++;
                
            }
            if (displayGroups.Contains("Range"))
            {
                symbolSheetList[0].Add("Step Fill Ratio"); symbolSheetColumn++;
                symbolSheetList[0].Add("Median Step Fill Ratio"); symbolSheetColumn++;
                symbolSheetList[0].Add("Mean Variance"); symbolSheetColumn++;
                symbolSheetList[0].Add("Median Variance"); symbolSheetColumn++;
                symbolSheetList[0].Add("Median Variance Stop Capped"); symbolSheetColumn++;
                symbolSheetList[0].Add("Mean Variance Normalized"); symbolSheetColumn++;
                symbolSheetList[0].Add("Median Variance Normalized"); symbolSheetColumn++;
                symbolSheetList[0].Add("Median Variance Stop Capped Normalized"); symbolSheetColumn++;
                //symbolSheetList[0].Add("Variance Squares Normalized"); symbolSheetColumn++;
                //symbolSheetList[0].Add("Variance Squares"); symbolSheetColumn++;
                //symbolSheetList[0].Add("Average High Diff"); symbolSheetColumn++;
                //symbolSheetList[0].Add("Greatest High Diff"); symbolSheetColumn++;
                //symbolSheetList[0].Add("Average Low Diff"); symbolSheetColumn++;
                //symbolSheetList[0].Add("Greatest Low Diff"); symbolSheetColumn++;
                symbolSheetList[0].Add("Average Max Diff"); symbolSheetColumn++;
                symbolSheetList[0].Add("Median Max Diff"); symbolSheetColumn++;
                symbolSheetList[0].Add("Average Close Diff"); symbolSheetColumn++;
                symbolSheetList[0].Add("Greatest Close Diff"); symbolSheetColumn++;
                symbolSheetList[0].Add("Average Max Range Steps"); symbolSheetColumn++;
                symbolSheetList[0].Add("Median Max Range Steps"); symbolSheetColumn++;
                symbolSheetList[0].Add("# Stops - 4 increments"); symbolSheetColumn++;
                symbolSheetList[0].Add("# Stops - 5 increments"); symbolSheetColumn++;
                symbolSheetList[0].Add("# Stops - 6 increments"); symbolSheetColumn++;
                symbolSheetList[0].Add("# Stops - 7 increments"); symbolSheetColumn++;

            }
            

            symbolSheetColumn++;


            //symbolParamSheet.Cells[1, 1] = "Symbol";
            symbolSheetList[0].Add("Start Time"); symbolSheetColumn++;
            symbolSheetList[0].Add("End Time"); symbolSheetColumn++;
            symbolSheetList[0].Add("Increment Price"); symbolSheetColumn++;
            symbolSheetList[0].Add("Increment Size"); symbolSheetColumn++;
            symbolSheetList[0].Add("Autobalance"); symbolSheetColumn++;
            symbolSheetList[0].Add("Hard Stop PL"); symbolSheetColumn++;



            int symbolRow = 1;
            int dailyRow = 1;

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

                    symbolSheetList.Add(new List<object>());

                    symbolSheetColumn = 0;



                    symbolSheetList[symbolRow].Add(symbolResult.Symbol); symbolSheetColumn++;
                    symbolSheetList[symbolRow].Add(symbolResult.scenarioNum); symbolSheetColumn++;

                    if (displayGroups.Contains("Position-BP"))
                    {
                        symbolSheetList[symbolRow].Add(symbolResult.BuyingPower); symbolSheetColumn++;
                    }

                    if (displayGroups.Contains("Fills"))
                    {
                        symbolSheetList[symbolRow].Add(symbolResult.BuyFills); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.averageBuyFills); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.SellFills); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.averageSellFills); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.CompleteFills); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.averageCompleteFills); symbolSheetColumn++;
                    }

                    if (displayGroups.Contains("PL"))
                    {
                        symbolSheetList[symbolRow].Add(symbolResult.IncrementPL); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.PriceMovePL); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.TotalPL); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.ProfitMargin); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.AvgMaxUnrealized); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.AvgMinUnrealized); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.NumWins); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.AvgWin); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.NumLosses); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.AvgLoss); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.yield); symbolSheetColumn++;
                    }
                    if (displayGroups.Contains("AvgWin-Analysis"))
                    {
                        symbolSheetList[symbolRow].Add(symbolResult.highestMaxUnrealized); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.daysAboveAverageWin); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.daysGaveBackAverageWin); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.medianMaxUnrealized); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.medianWin); symbolSheetColumn++;
                    }
                    if (displayGroups.Contains("Range"))
                    {
                        symbolSheetList[symbolRow].Add(symbolResult.stepFillRatio); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.medianStepFillRatio); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.variance); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.medianVariance); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.varianceStopCapped); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.varianceNormalized); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.medianVarianceNormalized); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.varianceStopCappedNormalized); symbolSheetColumn++;
                        //symbolSheetList[symbolRow].Add(symbolResult.varianceSquaresNormalized); symbolSheetColumn++;
                        //symbolSheetList[symbolRow].Add(symbolResult.varianceSquares); symbolSheetColumn++;
                        //symbolSheetList[symbolRow].Add(symbolResult.averageStartHighDiff); symbolSheetColumn++;
                        //symbolSheetList[symbolRow].Add(symbolResult.greatestHighDiff); symbolSheetColumn++;
                        //symbolSheetList[symbolRow].Add(symbolResult.averageStartLowDiff); symbolSheetColumn++;
                        //symbolSheetList[symbolRow].Add(symbolResult.greatestLowDiff); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.averageMaxDiff); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.medianMaxDiff); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.averageStartCloseDiff); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.greatestCloseDiff); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.averageMaxRangeSteps); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.medianMaxRangeSteps); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.numPriceStops[0]); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.numPriceStops[1]); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.numPriceStops[2]); symbolSheetColumn++;
                        symbolSheetList[symbolRow].Add(symbolResult.numPriceStops[3]); symbolSheetColumn++;
                    }
                    

                    symbolSheetColumn++;

                    symbolSheetList[symbolRow].Add(symbolResult.StartTime); symbolSheetColumn++;
                    symbolSheetList[symbolRow].Add(symbolResult.EndTime); symbolSheetColumn++;
                    symbolSheetList[symbolRow].Add(symbolResult.IncrementPrice); symbolSheetColumn++;
                    symbolSheetList[symbolRow].Add(symbolResult.IncrementSize); symbolSheetColumn++;
                    symbolSheetList[symbolRow].Add(symbolResult.Autobalance); symbolSheetColumn++;
                    symbolSheetList[symbolRow].Add(symbolResult.HardStop); symbolSheetColumn++;



                    foreach (SingleResult sr in symbolResult.SingleResults)
                    {
                        wsColumn = 0;

                        dailySheetList.Add(new List<object>());

                        dailySheetList[dailyRow].Add(symbolResult.Symbol); wsColumn++;
                        dailySheetList[dailyRow].Add(symbolResult.scenarioNum); wsColumn++;
                        dailySheetList[dailyRow].Add(sr.Date); wsColumn++;

                        
                        if (displayGroups.Contains("Position-BP"))
                        {
                            dailySheetList[dailyRow].Add(sr.MaxBuyingPower); wsColumn++;
                            dailySheetList[dailyRow].Add(sr.MaxLong); wsColumn++;
                            dailySheetList[dailyRow].Add(sr.MaxShort); wsColumn++;
                            dailySheetList[dailyRow].Add(sr.Position); wsColumn++;
                        }

                        if (displayGroups.Contains("Fills"))
                        {
                            dailySheetList[dailyRow].Add(sr.BuyFills); wsColumn++;
                            dailySheetList[dailyRow].Add(sr.SellFills); wsColumn++;
                            dailySheetList[dailyRow].Add(sr.CompleteFills); wsColumn++;
                        }

                        if (displayGroups.Contains("PL"))
                        {
                            dailySheetList[dailyRow].Add(sr.IncrementPL); wsColumn++;
                            dailySheetList[dailyRow].Add(sr.PriceMovePL); wsColumn++;
                            dailySheetList[dailyRow].Add(sr.TotalPL); wsColumn++;
                            dailySheetList[dailyRow].Add(sr.maxUnrealized); wsColumn++;
                            dailySheetList[dailyRow].Add(sr.minUnrealized); wsColumn++;
                            dailySheetList[dailyRow].Add(sr.StopTime); wsColumn++;
                        }

                        if (displayGroups.Contains("Range"))
                        {
                            dailySheetList[dailyRow].Add(sr.StartingPrice); wsColumn++;
                            dailySheetList[dailyRow].Add(sr.FinalPrint); wsColumn++;
                            dailySheetList[dailyRow].Add(sr.HighPrint); wsColumn++;
                            dailySheetList[dailyRow].Add(sr.LowPrint); wsColumn++;
                            dailySheetList[dailyRow].Add(sr.startHighDiff); wsColumn++;
                            dailySheetList[dailyRow].Add(sr.startLowDiff); wsColumn++;
                            dailySheetList[dailyRow].Add(sr.startCloseDiff); wsColumn++;
                        }
                        
                        dailyRow++;
                    }

                    
                    symbolRow++;
                    scenarioCount++;
                }
            }

            var symbolSheetArray = new object[symbolSheetList.Count, symbolSheetList[0].Count];
            for (int i = 0; i < symbolSheetList.Count; i++)
            {
                for (int j = 0; j < symbolSheetList[i].Count; j++)
                {
                    if (symbolSheetList[i].Count != symbolSheetList[0].Count)
                        throw new InvalidOperationException("The list cannot contain elements (lists) of different sizes.");
                    symbolSheetArray[i, j] = symbolSheetList[i][j];
                }
            }

            var dailySheetArray = new object[dailySheetList.Count, dailySheetList[0].Count];
            for (int i = 0; i < dailySheetList.Count; i++)
            {
                for (int j = 0; j < dailySheetList[i].Count; j++)
                {
                    if (dailySheetList[i].Count != dailySheetList[0].Count)
                        throw new InvalidOperationException("The list cannot contain elements (lists) of different sizes.");
                    dailySheetArray[i, j] = dailySheetList[i][j];
                }
            }

            //object[][] symbolSheetArray = symbolSheetList.Select(a => a.ToArray()).ToArray();
            //object[][] dailySheetArray = dailySheetList.ToArray();

            //int symbolSheetRowCount = symbolSheetArray.GetLength(0);
            //int symbolSheetColCount = symbolSheetArray.GetLength(1);

            //int dailySheetRowCount = dailySheetArray.GetLength(0);
            //int dailySheetColCount = dailySheetArray.GetLength(1);

            var symbolSheetStartCell = symbolSheet.Cells[1, 1];

            int count1 = symbolSheetArray.GetLength(0);
            int count2 = symbolSheetArray.GetLength(1);

            var symbolSheetEndCell = symbolSheet.Cells[symbolSheetArray.GetLength(0), symbolSheetArray.GetLength(1)];
            var writeRange = (Range)symbolSheet.Range[symbolSheetStartCell, symbolSheetEndCell];
            writeRange.Value = symbolSheetArray;


            var dailySheetStartCell = ws.Cells[1, 1];
            var dailySheetEndCell = ws.Cells[dailySheetArray.GetLength(0), dailySheetArray.GetLength(1)];
            writeRange = (Range)ws.Range[dailySheetStartCell, dailySheetEndCell];
            writeRange.Value = dailySheetArray;


            ws.Range["G2", "S" + dailyRow].NumberFormat = "#.00";
            symbolSheet.Range["E2", "S" + symbolRow].NumberFormat = "#.00";


            symbolSheet.Activate();
            symbolSheet.Application.ActiveWindow.SplitRow = 1;
            symbolSheet.Application.ActiveWindow.WindowState = Microsoft.Office.Interop.Excel.XlWindowState.xlNormal;
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
