using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Diagnostics;
//using System.Linq;


// Use the NxCoreAPI namespace
using NxCoreAPI;

namespace Backtester
{
    public partial class Form1 : Form
    {
        static String FileName;
        static bool InThread = false;
        public static Dictionary<string, string> csvs = new Dictionary<string, string>();
        /*public static List<string> symbols = new List<string> { "FGM", "MSFT", "FB", "GE", "TSLA", "SPWH", "GM", "TM", "AET", "GS", "WYNN","ACN",
            "ADBE", "ADP", "AEE", "AEM", "AEP", "AIV", "CAG", "CCI", "CHD", "CHRW", "CLX", "CMS", "COH", "CPB", "CRUS", "CUBE", "D", "DLR", "DNKN",
            "DRI", "DUK", "DXCM", "EAT", "ED", "EIX", "EQR", "ES", "ETR", "EXC", "FE", "GXP", "HCN", "HCP", "HRL", "HSY", "KORS", "LLY", "LULU", "MAA",
            "MSI", "NEE", "NEM", "NI", "NKE", "NNN", "O", "PCG", "PEG", "PPC", "PPL", "RAI", "SKX", "SO", "SRCL", "T", "TDG", "TSN", "UAA", "UDR", "VTR",
            "VZ", "WEC", "WMT", "WWW", "XEL", "AMT", "TGT", "BA", "CELG", "COST", "DE", "DIS", "HD", "HON", "IBM", "IVV", "KMB", "MA", "MCD", "NFLX",
            "NSC", "NXPI", "PEP", "PM", "PNC", "PRU", "UNH", "UNP", "UPS", "UTX", "MMM", "GILD", "MDT", "MYL", "VRX", "KO", "SBUX", "CAT", "RCL",
            "AAPL", "ALXN", "CMG", "SYY", "WDAY", "LVS", "MGM", "MPEL" };*/
        public static List<string> symbols = new List<string> { "DIS" };
        /*public static List<string> dates = new List<string> { "20170103", "20170104", "20170105", "20170106", "20170109", "20170110", "20170111", "20170112", "20170113", "20170117", "20170118",
                                                                  "20170119", "20170120", "20170123"};*/
        public static List<string> dates = new List<string> { "20170109" };
        //public static List<string> dates = new List<string> { "20170124", "20170125", "20170126", "20170127", "20170130", "20170131"};
        public static List<string> symbolsMissing = new List<string>();

        //for candle generation
        public static List<string> candleGenSymbols = symbols;
        public static List<string> candleGenDates = dates;
        public static List<int> periods = new List<int> { 1, 2, 5 };
        //for backtesting
        public static string backtestSymbol = "TSLA";
        //public static List<string> backtestDates = new List<string> { "20170103", "20170104", "20170106", "20170109", "20170110", "20170111" };
        public static List<string> backtestDates = dates;
        public static string backtestStartTime = "9:45:00:0:AM";
        public static string backtestEndTime = "3:50:00:0:PM";

        public static List<List<String>> backtestSymbolParams = new List<List<String>> {
            /*new List<String> { "NXPI", "9:35:00:0:AM", "3:50:00:0:PM", "0.05", "100", "1000", "1000" },
            new List<String> { "AET", "9:35:00:0:AM", "3:50:00:0:PM", "0.17", "100", "1000", "3000" },
            new List<String> {"GE", "9:35:00:0:AM", "3:50:00:0:PM", "0.04", "100", "1200", "3000" },
            new List<String> {"TDG", "9:35:00:0:AM", "3:50:00:0:PM", "0.40", "100", "800", "2000" },
            new List<String> {"ADBE", "9:35:00:0:AM", "3:50:00:0:PM", "0.07", "100", "1000", "3000" },
            new List<String> {"TSN", "9:35:00:0:AM", "3:50:00:0:PM", "0.06", "100", "1000", "3000" },
            new List<String> {"KMB", "9:35:00:0:AM", "3:50:00:0:PM", "0.06", "100", "3000", "3000" },
            new List<String> {"MMM", "9:35:00:0:AM", "3:50:00:0:PM", "0.11", "100", "1000", "3000" },*/
            new List<String> {"DIS", "9:35:00:0:AM", "3:50:00:0:PM", "0.06", "100", "1000", "3000" },
            new List<String> {"PNC", "9:35:00:0:AM", "3:50:00:0:PM", "0.08", "100", "1000", "3000" },
            new List<String> {"LLY", "9:35:00:0:AM", "3:50:00:0:PM", "0.06", "100", "1000", "3000" }


        };
        public static Dictionary<string, List<string>> backtestSymbolDates = new Dictionary<string, List<string>> {
            /*{"NXPI", backtestDates },
            {"AET", backtestDates },
            {"GE", backtestDates },
            {"TDG", new List<string> {"20170103", "20170104", "20170105", "20170106", "20170109", "20170110", "20170111", "20170112", "20170113", "20170117", "20170118",
"20170119" } }, //January 20 stock was downgraded
            {"ADBE", backtestDates },
            {"TSN", backtestDates },
            {"KMB", backtestDates },
            {"MMM", backtestDates },*/
            {"DIS", backtestDates },
            {"PNC", backtestDates },
            {"LLY", backtestDates }

        };



        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form dm = new DataManagement();
            dm.Show();
        }



        

        private void button2_Click(object sender, EventArgs e)
        {
            BacktestForm bt = new BacktestForm();
            bt.Show();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            StockVider.GetATR("20170104", "20170109");
        }

       

        private void button5_Click(object sender, EventArgs e)
        {
            List<KeyValuePair<string, decimal>> ratios = new List<KeyValuePair<string, decimal>>();
            Dictionary<string, decimal> ratiosSorted = new Dictionary<string, decimal>();
            
            foreach (string symbol in symbols)
            {
                List<decimal> dailyRanges = new List<decimal>();
                List<List<string>> candles = new List<List<string>>();
                decimal ratio = 999999999999999;
                foreach (string date in dates)
                {
                    //decimal tradingRange = TechnicalAnalysis.TradingRange(symbol, date, DataHelper.GetMsOfDay("9:35:00:0:AM"), DataHelper.GetMsOfDay("4:00:00:0:PM"));
                    //dailyRanges.Add(tradingRange);

                    //Console.WriteLine(date + " - " + tradingRange.ToString());
                    //Find average true range of candles
                    candles = FileHelper.getCandles(symbol, date, 1);

                }
                decimal averageDailyRange = TechnicalAnalysis.Average(dailyRanges);
                decimal candleATR = TechnicalAnalysis.ATR(candles);


                if (averageDailyRange > 0 && candleATR > 0) { 
                ratio = averageDailyRange / candleATR; // Lower the better

                ratios.Insert(0, new KeyValuePair<string, decimal>(symbol, ratio));
                }

                //ratios2.Add(symbol, Convert.ToDouble(ratio));

                //Console.WriteLine(symbol + " - " + ratio)
                //Console.WriteLine(symbol + " - " + averageRange);
                Console.WriteLine(symbol + " - " + ratio.ToString("#.###") + ": Candle ATR - " + candleATR.ToString("#.###") + ", Average Daily Range - " + averageDailyRange.ToString("#.##"));
            }

            //var sort_by_value = from r in ratios2 orderby r.Value select r
            ratios.Sort(delegate (KeyValuePair<string, decimal> firstPair, KeyValuePair<string, decimal> secondPair)
            {
                return firstPair.Value.CompareTo(secondPair.Value);
            });

            foreach (KeyValuePair<string, decimal> pair in ratios)
            {
                ratiosSorted.Add(pair.Key, pair.Value);
                Console.WriteLine(pair.Key + " - " + pair.Value.ToString());
            }



        }

        private void button4_Click(object sender, EventArgs e)
        {
            OptimizerForm of = new OptimizerForm();
            of.Show();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            CurrDirectoryChooser cdc = new CurrDirectoryChooser();
            cdc.Show();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Directory.SetCurrentDirectory(Properties.Settings.Default.WorkingDirectory);
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            RangeFinderForm rff = new Backtester.RangeFinderForm();
            rff.Show();
        }
    }
}

