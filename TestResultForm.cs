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
    public partial class TestResultForm : Form
    {
        public TestResult testResult = new TestResult();

        public TestResultForm(TestResult _testResult)
        {
            InitializeComponent();

            testResult = _testResult;

            textBox1.Text += "Test for " + testResult.SymbolResults.Count + " symbols, between " + testResult.startDate.ToString("yyyy-MM-dd") + " and "
                                + testResult.endDate.ToString("yyyy-MM-dd") + "\r\n";
            textBox1.Text += "-------------------------------------------------------\r\n\n";

            foreach (SymbolResult sr in testResult.SymbolResults)
            {
                
                textBox1.Text += sr.Symbol + " : " + sr.StartTime + " - " + sr.EndTime + ", " + "Increment Price: " + sr.IncrementPrice + ", Increment Size: " + sr.IncrementSize + ", Autobalance: " + 
                    sr.Autobalance + ", Hard Stop: " + sr.HardStop + "\r\n";
                textBox1.Text += "-------------------------------------------------------\r\n";

                foreach (SingleResult single in sr.SingleResults)
                {
                    textBox1.Text += sr.Symbol + " - " + single.Date + "\r\n";
                    if (single.StopTime != null)
                    {
                        textBox1.Text += "Stopped Out at : " + single.StopTime + "\r\n";
                    }
                    textBox1.Text += "Max Long: " + single.MaxLong + "\r\n";
                    textBox1.Text += "Max Short: " + single.MaxShort + "\r\n";
                    textBox1.Text += "Buying Power: " + single.MaxBuyingPower.ToString("#.##") + "\r\n";
                    textBox1.Text += "Buy Fills: " + single.BuyFills + "\r\n";
                    textBox1.Text += "Sell Fills: " + single.SellFills + "\r\n";
                    textBox1.Text += "Start Price: " + single.StartingPrice + "\r\n";
                    textBox1.Text += "Final Price: " + single.FinalPrint + "\r\n";
                    textBox1.Text += "Final Position: " + single.Position + "\r\n";
                    textBox1.Text += "Increment PL: " + single.IncrementPL + "\r\n";
                    textBox1.Text += "Price Move PL: " + single.PriceMovePL + "\r\n";
                    textBox1.Text += "Total PL: " + single.TotalPL + "\r\n";
                    textBox1.Text += "Max Unrealized: " + single.maxUnrealized + "\r\n";
                    textBox1.Text += "Min Unrealized: " + single.minUnrealized + "\r\n";
                    textBox1.Text += "-------------------------------------------------------\r\n\n";

                }
                textBox1.Text += "Totals for: " + sr.Symbol + "\r\n";
                textBox1.Text += "-------------------------------------------------------\r\n";
                textBox1.Text += "Buying Power: " + sr.BuyingPower.ToString("#.##") + "\r\n";
                textBox1.Text += "Buy Fills: " + sr.BuyFills + "\r\n";
                textBox1.Text += "Sell Fills: " + sr.SellFills + "\r\n";

                textBox1.Text += "Increment PL: " + sr.IncrementPL + "\r\n";
                textBox1.Text += "Price Move PL: " + sr.PriceMovePL + "\r\n";
                textBox1.Text += "Total PL: " + sr.TotalPL + "\r\n";
                textBox1.Text += "Profit Margin (%): " + sr.ProfitMargin.ToString("#.##") + "\r\n\n";
                textBox1.Text += "Avg Max Unrealized: " + sr.AvgMaxUnrealized.ToString("#.##") + "\r\n";
                textBox1.Text += "Avg Min Unrealized: " + sr.AvgMinUnrealized.ToString("#.##") + "\r\n";
                textBox1.Text += "Avg Win: " + sr.AvgWin.ToString("#.##") + "\r\n";
                textBox1.Text += "Avg Loss: " + sr.AvgLoss.ToString("#.##") + "\r\n";
                textBox1.Text += "-------------------------------------------------------\r\n\n";

            }

            //Test result totals
            textBox1.Text += "Totals\r\n";
            textBox1.Text += "-------------------------------------------------------\r\n";
            textBox1.Text += "Buy Fills: " + testResult.BuyFills + "\r\n";
            textBox1.Text += "Sell Fills: " + testResult.SellFills + "\r\n";
            textBox1.Text += "Increment PL: " + testResult.IncrementPL + "\r\n";
            textBox1.Text += "Price Move PL: " + testResult.PriceMovePL + "\r\n";
            textBox1.Text += "Total PL: " + testResult.TotalPL + "\r\n";
            textBox1.Text += "Profit Margin (%): " + testResult.ProfitMargin.ToString("#.##") +  "\r\n";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            if (!(Directory.Exists("./Data/Tests/Results"))) Directory.CreateDirectory("./Data/Tests/Results");
            saveFileDialog1.InitialDirectory = Directory.GetCurrentDirectory() + "\\Data\\Tests\\Results";

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
            Microsoft.Office.Interop.Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();
            Workbook wb = xlApp.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);

            Worksheet symbolParamSheet = (Worksheet)wb.Worksheets.Add();
            symbolParamSheet.Name = "Symbol Parameters";

            Worksheet symbolSheet = (Worksheet)wb.Worksheets.Add(symbolParamSheet);
            symbolSheet.Name = "Symbol Totals";

            Worksheet ws = (Worksheet)wb.Worksheets.Add(symbolSheet);
            ws.Name = "Daily Tests";

            if (!(Directory.Exists("./Data/Tests/Results"))) Directory.CreateDirectory("./Data/Tests/Results");
            saveFileDialog2.InitialDirectory = Directory.GetCurrentDirectory() + "\\Data\\Tests\\Results";
            saveFileDialog2.Filter = "Excel |*.xlsx";

            

            

            //cell format: [row, column]

            ws.Cells[1, 1] = "Symbol";
            ws.Cells[1, 2] = "Date";
            ws.Cells[1, 3] = "Max Long";
            ws.Cells[1, 4] = "Max Short";
            ws.Cells[1, 5] = "Buying Power";
            ws.Cells[1, 6] = "Buy Fills";
            ws.Cells[1, 7] = "Sell Fills";
            ws.Cells[1, 8] = "Start Price";
            ws.Cells[1, 9] = "Final Price";
            ws.Cells[1, 10] = "Final Position";
            ws.Cells[1, 11] = "Increment PL";
            ws.Cells[1, 12] = "Price Move PL";
            ws.Cells[1, 13] = "Total PL";
            ws.Cells[1, 14] = "Max Unrealized";
            ws.Cells[1, 15] = "Min Unrealized";
            ws.Cells[1, 16] = "Stop Time";

            

            symbolSheet.Cells[1, 1] = "Symbol";
            symbolSheet.Cells[1, 2] = "Buy Fills";
            symbolSheet.Cells[1, 3] = "Sell Fills";
            symbolSheet.Cells[1, 4] = "Buying Power";
            symbolSheet.Cells[1, 5] = "Increment PL";
            symbolSheet.Cells[1, 6] = "Price Move PL";
            symbolSheet.Cells[1, 7] = "Total PL";
            symbolSheet.Cells[1, 8] = "Profit Margin (%)";
            symbolSheet.Cells[1, 9] = "Avg Max Unrealized";
            symbolSheet.Cells[1, 10] = "Avg Min Unrealized";
            symbolSheet.Cells[1, 11] = "# Of Wins";
            symbolSheet.Cells[1, 12] = "Avg Win";
            symbolSheet.Cells[1, 13] = "# Of Losses";
            symbolSheet.Cells[1, 14] = "Avg Loss";

            

            symbolParamSheet.Cells[1, 1] = "Symbol";
            symbolParamSheet.Cells[1, 2] = "Start Time";
            symbolParamSheet.Cells[1, 3] = "End Time";
            symbolParamSheet.Cells[1, 4] = "Increment Price";
            symbolParamSheet.Cells[1, 5] = "Increment Size";
            symbolParamSheet.Cells[1, 6] = "Autobalance";
            symbolParamSheet.Cells[1, 7] = "Hard Stop PL";

            

            int symbolRow = 2;
            int dailyRow = 2;

            foreach (SymbolResult symbolResult in testResult.SymbolResults)
            {
                symbolSheet.Cells[symbolRow, 1] = symbolResult.Symbol;
                symbolSheet.Cells[symbolRow, 2] = symbolResult.BuyFills;
                symbolSheet.Cells[symbolRow, 3] = symbolResult.SellFills;
                symbolSheet.Cells[symbolRow, 4] = symbolResult.BuyingPower;
                symbolSheet.Cells[symbolRow, 5] = symbolResult.IncrementPL;
                symbolSheet.Cells[symbolRow, 6] = symbolResult.PriceMovePL;
                symbolSheet.Cells[symbolRow, 7] = symbolResult.TotalPL;
                symbolSheet.Cells[symbolRow, 8] = symbolResult.ProfitMargin;
                symbolSheet.Cells[symbolRow, 9] = symbolResult.AvgMaxUnrealized;
                symbolSheet.Cells[symbolRow, 10] = symbolResult.AvgMinUnrealized;
                symbolSheet.Cells[symbolRow, 11] = symbolResult.NumWins;
                symbolSheet.Cells[symbolRow, 12] = symbolResult.AvgWin;
                symbolSheet.Cells[symbolRow, 13] = symbolResult.NumLosses;
                symbolSheet.Cells[symbolRow, 14] = symbolResult.AvgLoss;

                symbolParamSheet.Cells[symbolRow, 1] = symbolResult.Symbol;
                symbolParamSheet.Cells[symbolRow, 2] = symbolResult.StartTime;
                symbolParamSheet.Cells[symbolRow, 3] = symbolResult.EndTime;
                symbolParamSheet.Cells[symbolRow, 4] = symbolResult.IncrementPrice;
                symbolParamSheet.Cells[symbolRow, 5] = symbolResult.IncrementSize;
                symbolParamSheet.Cells[symbolRow, 6] = symbolResult.Autobalance;
                symbolParamSheet.Cells[symbolRow, 7] = symbolResult.HardStop;


                
                foreach (SingleResult sr in symbolResult.SingleResults)
                {
                    ws.Cells[dailyRow, 1] = symbolResult.Symbol;
                    ws.Cells[dailyRow, 2] = sr.Date;
                    ws.Cells[dailyRow, 3] = sr.MaxLong;
                    ws.Cells[dailyRow, 4] = sr.MaxShort;
                    ws.Cells[dailyRow, 5] = sr.MaxBuyingPower;
                    ws.Cells[dailyRow, 6] = sr.BuyFills;
                    ws.Cells[dailyRow, 7] = sr.SellFills;
                    ws.Cells[dailyRow, 8] = sr.StartingPrice;
                    ws.Cells[dailyRow, 9] = sr.FinalPrint;
                    ws.Cells[dailyRow, 10] = sr.Position;
                    ws.Cells[dailyRow, 11] = sr.IncrementPL;
                    ws.Cells[dailyRow, 12] = sr.PriceMovePL;
                    ws.Cells[dailyRow, 13] = sr.TotalPL;
                    ws.Cells[dailyRow, 14] = sr.maxUnrealized;
                    ws.Cells[dailyRow, 15] = sr.minUnrealized;
                    ws.Cells[dailyRow, 16] = sr.StopTime;

                    dailyRow++;
                }

                symbolRow++;
            }

            ws.Range["F2", "M" + dailyRow].NumberFormat = "#.00";
            symbolSheet.Range["D2", "M" + symbolRow].NumberFormat = "#.00";

            symbolSheet.Activate();
            symbolSheet.Application.ActiveWindow.SplitRow = 1;
            symbolSheet.Application.ActiveWindow.FreezePanes = true;

            ws.Activate();
            ws.Application.ActiveWindow.SplitRow = 1;
            ws.Application.ActiveWindow.FreezePanes = true;

            symbolParamSheet.Activate();
            symbolParamSheet.Application.ActiveWindow.SplitRow = 1;
            symbolParamSheet.Application.ActiveWindow.FreezePanes = true;

            ws.Columns.AutoFit();
            symbolSheet.Columns.AutoFit();
            symbolParamSheet.Columns.AutoFit();


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




            /*Workbook workbook = new Workbook();

            //Worksheet of single results

            Worksheet dailyResults = new Worksheet("Daily Results");
            Worksheet symbolTotals = new Worksheet("Symbol Totals");
            Worksheet symbolParams = new Worksheet("Symbol Parameters");

            dailyResults.Cells[0, 0] = new Cell("Symbol");
            dailyResults.Cells[0, 1] = new Cell("0, 1");
            dailyResults.Cells[1, 0] = new Cell("1, 0");

            workbook.Worksheets.Add(dailyResults);
            workbook.Save("./Data/Tests/Results/Test.xlsx");*/



        }
    }
}
