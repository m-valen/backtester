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
using System.Xml;


// Use the NxCoreAPI namespace
using NxCoreAPI;

namespace Backtester
{
    public partial class DataManagement : Form
    {
        private bool closePending;
        private bool importCancelled;
        //private bool importCancelled = false;
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
        //public static List<string> dates = new List<string> {  };
        //public static List<string> dates = new List<string> { "20170124", "20170125", "20170126", "20170127", "20170130", "20170131"};
        public static List<string> symbolsMissing = new List<string>();
        public static List<string> symbolsToImport = new List<string>();
        //for candle generation
        public static List<string> candleGenSymbols = symbols;
        //public static List<string> candleGenDates = dates;

        //for backtesting
        public static string backtestSymbol = "TSLA";
        //public static List<string> backtestDates = new List<string> { "20170103", "20170104", "20170106", "20170109", "20170110", "20170111" };
        //public static List<string> backtestDates = dates;
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
        public static Dictionary<string, List<string>> backtestSymbolDates = new Dictionary<string, List<string>>
        {
            /*{"NXPI", backtestDates },
            {"AET", backtestDates },
            {"GE", backtestDates },
            {"TDG", new List<string> {"20170103", "20170104", "20170105", "20170106", "20170109", "20170110", "20170111", "20170112", "20170113", "20170117", "20170118",
"20170119" } }, //January 20 stock was downgraded
            {"ADBE", backtestDates },
            {"TSN", backtestDates },
            {"KMB", backtestDates },
            {"MMM", backtestDates },*/
            //{"DIS", backtestDates },
            //{"PNC", backtestDates },
            //{"LLY", backtestDates }

        };

        public static DateTime date1 = DateTime.MinValue;
        public static DateTime date2 = DateTime.MaxValue;

        public List<string> distinctSymbols = new List<string>();
        public List<string> dates = new List<string>();
        public bool tickOverwrite = false;
        public int tapeFileCount = 0;

        public static string ImportStatus = "Before";
        public static string ImportStatusDetails = "";

        //TickImport ti = new TickImport();
        CandleGeneration cg = new CandleGeneration();

        public bool candleOverwrite = false;
        public static List<int> periods = new List<int> { 1, 2, 5 };
        public static List<DateTime> candleDates = new List<DateTime>();


        public DataManagement()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Cancel")
            {
                importCancelled = true;
                backgroundWorker1.CancelAsync();
                button1.Text = "Run Process";
                return;
            }

            // If a tape filename was passed in command line argument,
            // assign it to FileName	  
            /*if (args.Length > 0)
                NxCore.ProcessTape(args[0],
                                   null, 0, 0,
                                   Program.OnNxCoreCallback);*/
            //Read in symbols
            var _symbols = textBox1.Text;
            _symbols = _symbols.Replace("\n", String.Empty);
            _symbols = _symbols.Replace("\r", String.Empty);
            string[] __symbols = _symbols.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            distinctSymbols.Clear();

            //Remove duplicates
            //List<string> distinctSymbols = new List<string>();
            foreach (string _symbol in __symbols)
            {
                string symbol = _symbol.Trim();
                if (!(distinctSymbols.Contains(symbol)))
                {

                    distinctSymbols.Add(symbol);
                }
            }

            //read in dates
            if (radioButton2.Checked)
            {
                date1 = monthCalendar1.SelectionStart;
                date2 = monthCalendar2.SelectionEnd;
            }
            else if (radioButton1.Checked)
            {
                date1 = DateTime.MinValue;
                date2 = DateTime.MaxValue;
            }
            List<DateTime> _dates = DataHelper.GetTradingDays(date1, date2);
            candleDates = DataHelper.GetTradingDays(date1, date2);


            if (checkBox1.Checked)
            {
                Dictionary<DateTime, List<string>> dateSymbolsMissing = new Dictionary<DateTime, List<string>>();
                string filename = "";
                string filepath = "";
                foreach (DateTime _d in _dates)
                {
                    List<string> symbolsMissing = new List<string>();
                    filename = _d.ToString("yyyyMMdd") + ".GS.nx2";
                    foreach (string symbol in distinctSymbols)
                    {
                        filepath = "./Data/Ticks/" + symbol + "/" + symbol + _d.ToString("yyyyMMdd") + ".csv";
                        if (!File.Exists(filepath)) symbolsMissing.Add(symbol);
                    }
                    dateSymbolsMissing.Add(_d, symbolsMissing);
                }
                tapeFileCount = 0;

                foreach (KeyValuePair<DateTime, List<string>> dateSymbol in dateSymbolsMissing)
                {
                    if (dateSymbol.Value.Count > 0) tapeFileCount++;
                }
                if (!checkBox2.Checked)
                {
                    DialogResult result = MessageBox.Show(tapeFileCount + " tape files need to be processed. Continue with import?", "Confirmation",
                                                MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        button1.Text = "Cancel";
                        //button1.Enabled = false;
                        dates.Clear();
                        foreach (DateTime d in _dates)
                        {
                            dates.Add(d.ToString("yyyyMMdd"));
                        }
                        //Form it = new ImportTicks(distinctSymbols, dates, false, tapeFileCount);
                        //it.Show();
                        tickOverwrite = false;

                    }
                    else return;
                }
                else if (checkBox2.Checked)
                {
                    tapeFileCount = _dates.Count;
                    DialogResult result = MessageBox.Show(tapeFileCount + " tape files need to be processed. Continue with import?", "Confirmation",
                                                MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        button1.Text = "Cancel";
                        //button1.Enabled = false;
                        //List<string> dates = new List<string>();
                        foreach (DateTime d in _dates)
                        {
                            dates.Add(d.ToString("yyyyMMdd"));
                        }
                        //Form it = new ImportTicks(distinctSymbols, dates, true, tapeFileCount);
                        //it.Show();
                        tickOverwrite = true;

                        //TickImport ti = new TickImport();
                        //ti.NotifyOfProgress += new TickImport.reportProgress(tickImport_OnProgressReport);





                    }
                    else return;
                }

            }
            else if (checkBox3.Checked)
            {
                
                if (button1.Text == "Cancel")
                {
                    importCancelled = true;
                    backgroundWorker1.CancelAsync();
                    button1.Text = "Run Process";
                    return;
                }
                
                candleOverwrite = checkBox4.Checked;
                button1.Text = "Cancel";


                //Form gs = new GenerateCandles(distinctSymbols, _dates, periods, checkBox4.Checked);
                //gs.Show();
            }

            backgroundWorker1.RunWorkerAsync();

        }



        private void tickImport_OnProgressReport(string status, string details, DateTime timeInTapeFile)
        {
            Console.WriteLine(status, details, timeInTapeFile);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            List<string> allSymbols = new List<string>();
            if (checkBox5.Checked) {
                allSymbols = FileHelper.getAllSymbols(monthCalendar1.SelectionStart, monthCalendar2.SelectionEnd);
            }
            else
            {
                allSymbols = FileHelper.getAllSymbols();
            }
            string symbols = String.Join(",", allSymbols.ToArray());

            textBox1.Text = "";

            if (allSymbols.Count > 0)
            {
                textBox1.Text = symbols;
            }
            else textBox1.Text = "No symbols found.";
        }



        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                monthCalendar1.Enabled = false;
                monthCalendar2.Enabled = false;
            }
            else if (radioButton2.Checked)
            {
                monthCalendar1.Enabled = true;
                monthCalendar2.Enabled = true;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                radioButton2.Checked = true;
                radioButton1.Enabled = false;
                radioButton2.Enabled = false;
                checkBox2.Enabled = true;
            }
            else
            {
                radioButton1.Enabled = true;
                radioButton2.Enabled = true;
                checkBox2.Checked = false;
                checkBox2.Enabled = false;
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                checkBox4.Enabled = true;
                if (checkBox1.Checked)
                {
                    checkBox4.Checked = true;
                    checkBox4.Enabled = false;
                }
            }
            else
            {
                checkBox4.Enabled = false;
                checkBox4.Checked = false;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                checkBox3.Checked = true;
                checkBox4.Checked = true;
                checkBox3.Enabled = false;
                checkBox4.Enabled = false;
            }
            else
            {
                checkBox3.Enabled = true;
                checkBox4.Enabled = true;
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (checkBox1.Checked)
            {
                TickImport ti = new TickImport(distinctSymbols, dates);
                ti.RunImport(sender as BackgroundWorker, tickOverwrite, tapeFileCount);
            }
            if (checkBox3.Checked && !importCancelled)
            {
                cg.generateCandles(distinctSymbols, candleDates, periods, checkBox4.Checked, sender as BackgroundWorker);
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            List<string> report = e.UserState as List<string>;
            if (report[1] != "") {
                label1.Text = report[1];
            }
            if (!(report[2] == ""))
            {
                label2.Text = report[2];
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button1.Text = "Run Process";
            button1.Enabled = true;
            if (closePending) this.Close();
            

            if (!closePending && !importCancelled)
                MessageBox.Show("Process complete");

            else if (importCancelled) MessageBox.Show("Process cancelled");
            closePending = false;
            importCancelled = false;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            if (!(Directory.Exists("./Data/Symbols"))) Directory.CreateDirectory("./Data/Symbols");
            saveFileDialog1.InitialDirectory = Directory.GetCurrentDirectory() + "\\Data\\Symbols";


            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName))
                {
                    sw.Write("<formData>");
                    sw.WriteLine("<list>" + textBox1.Text + "</list>");
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
            if (!(Directory.Exists("./Data/Symbols"))) Directory.CreateDirectory("./Data/Symbols");
            openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory() + "\\Data\\Symbols";
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                string file = openFileDialog1.FileName;
                string xml = File.ReadAllText(file);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xml);

                //Set global settings
                string list = xmlDoc.SelectSingleNode("//list").InnerText;
                textBox1.Text = list;
            }
            else
            {

            }
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

