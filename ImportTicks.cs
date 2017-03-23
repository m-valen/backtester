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

// Use the NxCoreAPI namespace
using NxCoreAPI;

namespace Backtester
{
    public partial class ImportTicks : Form
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
        //public static List<string> dates = new List<string> {  };
        //public static List<string> dates = new List<string> { "20170124", "20170125", "20170126", "20170127", "20170130", "20170131"};
        public static List<string> symbolsMissing = new List<string>();
        public static List<string> symbolsToImport = new List<string>();
        //for candle generation
        public static List<string> candleGenSymbols = symbols;
        //public static List<string> candleGenDates = dates;
        public static List<int> periods = new List<int> { 1, 2, 5 };
        //for backtesting
        public static string backtestSymbol = "TSLA";
        //public static List<string> backtestDates = new List<string> { "20170103", "20170104", "20170106", "20170109", "20170110", "20170111" };
        //public static List<string> backtestDates = dates;
        public static string backtestStartTime = "9:45:00:0:AM";
        public static string backtestEndTime = "3:50:00:0:PM";

        public static DateTime timeInTapeFile;

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
        public static List<string> importSymbols = new List<string>();
        public static List<string> importDates = new List<string>();
        public static bool importOverwrite = false;
        public static int tapeFileCount = 0;

        public static string ImportStatus = "Before";

        public ImportTicks(List<string> symbols, List<string> dates, bool overwrite = false, int _tapeFileCount = 0)
        {
            InitializeComponent();
            importSymbols = symbols;
            importDates = dates;
            importOverwrite = overwrite;
            tapeFileCount = _tapeFileCount;

            Shown += ImportTicks_Shown;


        }

        private void ImportTicks_Shown(Object sender, EventArgs e)
        {


        }
        private void _ImportTicks(List<string> symbols, List<string> dates, bool overwrite = false, int tapeFileCount = 0)
        {
            label1.Text = "Beginning Import...";
            label1.Refresh();
            ImportStatus = "Processing";
            int currentFiles = 1;

            if (!Directory.Exists("./Data/Ticks")) Directory.CreateDirectory("./Data/Ticks");
            foreach (string date in dates)
            {
                symbolsToImport.Clear();
                symbolsMissing.Clear();
                if (date != null)
                {
                    FileName = date + ".GS.nx2";
                    if (tapeFileCount > 0) label1.Text = "Importing data from " + FileName + "... " + currentFiles + "/" + tapeFileCount;
                    else label1.Text = "Importing data from " + FileName;
                    label1.Refresh();
                }
                else
                    FileName = "";
                foreach (string symbol in symbols)

                {
                    if (!Directory.Exists("./Data/Ticks/" + symbol)) { Directory.CreateDirectory("./Data/Ticks/" + symbol); }
                    string filePath = "./Data/Ticks/" + symbol + "/" + symbol + date + ".csv";
                    if (!File.Exists(filePath)) symbolsMissing.Add(symbol);
                }
                if (!overwrite) symbolsToImport = symbolsMissing;
                else symbolsToImport = symbols;
                // Set in Thread  flag
                InThread = true;
                if (symbolsToImport.Count > 0)
                {

                    // Start a new thread!
                    new Thread(StartProcessTapeThread).Start();

                    // Sleep until the thread exits and resets InThread flag
                    while (InThread)
                        updateTimeLabel(timeInTapeFile);
                    Thread.Sleep(100);

                    //Create new data files

                    foreach (string symbol in symbolsToImport)
                    {
                        string filePath = "./Data/Ticks/" + symbol + "/" + symbol + date + ".csv";

                        if (File.Exists(@filePath)) File.Delete(filePath);
                        File.Create(filePath).Dispose();
                        //File.SetAttributes(filePath, FileAttributes.Normal);
                        File.WriteAllText(filePath, csvs[symbol]);
                    }
                }
            }
            label1.Text = "Completed Import.";
            label1.Refresh();
            ImportStatus = "Completed";
        }
        // Thread function called to start Process Tape
        //----------------------------------------------
        static void StartProcessTapeThread()
        {
            // Use control flags to eliminate OPRA quotes.	  
            NxCore.ProcessTape(FileName,
                               null,
                               (uint)(NxCore.NxCF_EXCLUDE_OPRA),
                               0,
                               OnNxCoreCallback);

            InThread = false;
        }


        // The NXCore Callback Function	
        //-----------------------------
        static unsafe int OnNxCoreCallback(IntPtr pSys, IntPtr pMsg)
        {

            // Alias structure pointers to the pointers passed in.
            NxCoreSystem* pNxCoreSys = (NxCoreSystem*)pSys;
            NxCoreMessage* pNxCoreMsg = (NxCoreMessage*)pMsg;

            // Do something based on the message type
            switch (pNxCoreMsg->MessageType)
            {
                // NxCore Status Message
                case NxCore.NxMSG_STATUS:
                    OnNxCoreStatus(pNxCoreSys, pNxCoreMsg);
                    break;

                // NxCore Trade Message
                case NxCore.NxMSG_TRADE:
                    //if (pNxCoreMsg->coreHeader.nxExgTimestamp.Hour > 8 && pNxCoreMsg->coreHeader.nxExgTimestamp.Minute > 35) return 1;
                    OnNxCoreTrade(pNxCoreSys, pNxCoreMsg);
                    break;

                case NxCore.NxMSG_SYMBOLSPIN:
                    OnNxCoreSymbolSpin(pNxCoreSys, pNxCoreMsg);
                    break;

                    // NxCore Level1 Quote Message
                    //case NxCore.NxMSG_EXGQUOTE:
                    //  OnNxCoreExgQuote(pNxCoreSys, pNxCoreMsg);
                    //  break;

                    // NxCore Level2 Quote Message
                    //case NxCore.NxMSG_MMQUOTE:
                    // OnNxCoreMMQuote(pNxCoreSys, pNxCoreMsg);
            }

            // Continue running the tape
            return (int)NxCore.NxCALLBACKRETURN_CONTINUE;
        }


        // OnNxCoreStatus: Function to handle NxCore Status messages
        //----------------------------------------------------------
        static unsafe void OnNxCoreStatus(NxCoreSystem* pNxCoreSys, NxCoreMessage* pNxCoreMsg)
        {

            // If a Minute has elapsed print the NxCore system time.
            // NOTE: The last time sent is 24:00:00. Don't print that
            // time as DateTime() will fail...24:00:00 is not actually
            // a real time value.
            if ((pNxCoreSys->ClockUpdateInterval >= NxCore.NxCLOCK_MINUTE) &&
                (pNxCoreSys->nxTime.Hour < 24))
            {
                DateTime thisTime = new DateTime(pNxCoreSys->nxDate.Year,
                                                 pNxCoreSys->nxDate.Month,
                                                 pNxCoreSys->nxDate.Day,
                                                 pNxCoreSys->nxTime.Hour,
                                                 pNxCoreSys->nxTime.Minute,
                                                 pNxCoreSys->nxTime.Second);

                timeInTapeFile = thisTime;

            }

            // Print the specific NxCore status message
            switch (pNxCoreSys->Status)
            {
                case NxCore.NxCORESTATUS_COMPLETE:
                    Console.WriteLine("NxCore Complete Message.");
                    break;

                case NxCore.NxCORESTATUS_INITIALIZING:
                    Console.WriteLine("NxCore Initialize Message.");
                    break;

                case NxCore.NxCORESTATUS_SYNCHRONIZING:
                    Console.WriteLine("NxCore Synchronizing Message.");
                    break;

                case NxCore.NxCORESTATUS_WAITFORCOREACCESS:
                    Console.WriteLine("NxCore Wait For Access.");
                    break;

                case NxCore.NxCORESTATUS_RESTARTING_TAPE:
                    Console.WriteLine("NxCore Restart Tape Message.");
                    break;

                case NxCore.NxCORESTATUS_ERROR:
                    Console.WriteLine("NxCore Error.");
                    break;

                case NxCore.NxCORESTATUS_RUNNING:
                    break;
            }
        }

        static unsafe void OnNxCoreSymbolSpin(NxCoreSystem* pNxCoreSys, NxCoreMessage* pNxCoreMsg)
        {
            String Symbol = new string(&pNxCoreMsg->coreHeader.pnxStringSymbol->String);
            if (symbolsToImport.Contains(Symbol.Remove(0, 1)))
            {
                pNxCoreMsg->coreHeader.pnxStringSymbol->UserData1 = 1;
                Console.WriteLine("SET");
                csvs[Symbol.Remove(0, 1)] = "MsOfDay, Time,Price,Size\n";
            }

        }

        // OnNxCoreTrade: Function to handle NxCore Trade messages.	
        //--------------------------------------------------------------
        static unsafe void OnNxCoreTrade(NxCoreSystem* pNxCoreSys, NxCoreMessage* pNxCoreMsg)
        {

            // Get the symbol for category message
            String Symbol = new String(&pNxCoreMsg->coreHeader.pnxStringSymbol->String);
            if (pNxCoreMsg->coreHeader.pnxStringSymbol->UserData1 == 1)
            {
                //if (Symbol.ToString() == "eCSIQ") { 

                // Assign a pointer to the Trade data
                NxCoreTrade* Trade = &pNxCoreMsg->coreData.Trade;

                // Get the price and net change
                double Price = NxCore.PriceToDouble(Trade->Price, Trade->PriceType);
                double NetChange = NxCore.PriceToDouble(Trade->NetChange, Trade->PriceType);
                int Hour = pNxCoreMsg->coreHeader.nxExgTimestamp.Hour;
                int Minute = pNxCoreMsg->coreHeader.nxExgTimestamp.Minute;
                int Second = pNxCoreMsg->coreHeader.nxExgTimestamp.Second;
                int Millisecond = pNxCoreMsg->coreHeader.nxExgTimestamp.Millisecond;
                int MsOfDay = Convert.ToInt32(pNxCoreMsg->coreHeader.nxExgTimestamp.MsOfDay);
                string TimeOfTrade = Hour.ToString() + ":" + Minute.ToString() + ":" + Second.ToString() + ":" + Millisecond.ToString();
                string Volume = pNxCoreMsg->coreData.Trade.Size.ToString();

                //Check that price is within BBO or not tradethruexempt
                int tradeCondition = pNxCoreMsg->coreData.Trade.TradeCondition;
                //
                //var extendedTradeCondition = pNxCoreMsg->coreData.Trade.ExtTradeConditions;

                if (tradeCondition == 108) //TradeThruExempt
                {
                    //Get BBO
                    double bestBidPrice = pNxCoreMsg->coreData.ExgQuote.BestBidPrice;
                    double bestAskPrice = pNxCoreMsg->coreData.ExgQuote.BestAskPrice;

                    Console.WriteLine("Trade for Symbol: {0:S}, Time: {1:d}:{2:d}:{3:d}  Price: {4:f}    BBO:  {5}  {6}   TradeCondition:  {7}", Symbol,
                        pNxCoreMsg->coreHeader.nxExgTimestamp.Hour, pNxCoreMsg->coreHeader.nxExgTimestamp.Minute, pNxCoreMsg->coreHeader.nxExgTimestamp.Second,
                                  Price, bestBidPrice, bestAskPrice, tradeCondition);
                }

                var newLine = string.Format("{0},{1},{2},{3}", MsOfDay, TimeOfTrade, Price, Volume);
                csvs[Symbol.Remove(0, 1)] += newLine + "\n";

                // Write out Symbol, Time, Price, NetChg, Size, Reporting Exg
                /*Console.WriteLine("Trade for Symbol: {0:S}, Time: {1:d}:{2:d}:{3:d}  Price: {4:f}  NetChg: {5:f}  Size: {6:d}  Exchg: {7:d} ",
                                  Symbol,
                                  pNxCoreMsg->coreHeader.nxExgTimestamp.Hour, pNxCoreMsg->coreHeader.nxExgTimestamp.Minute, pNxCoreMsg->coreHeader.nxExgTimestamp.Second,
                                  Price, NetChange, Trade->Size,
                                  pNxCoreMsg->coreHeader.ReportingExg);*/
            }
        }


        // OnNxCoreQuote: Function to handle NxCore ExgQuote messages.	
        //--------------------------------------------------------------
        static unsafe void OnNxCoreExgQuote(NxCoreSystem* pNxCoreSys, NxCoreMessage* pNxCoreMsg)
        {

            // Get the symbol for category message
            String Symbol = new String(&pNxCoreMsg->coreHeader.pnxStringSymbol->String);

            // Assign a pointer to the ExgQuote data
            NxCoreExgQuote* Quote = &pNxCoreMsg->coreData.ExgQuote;

            // Get bid and ask price
            double Bid = NxCore.PriceToDouble(Quote->coreQuote.BidPrice, Quote->coreQuote.PriceType);
            double Ask = NxCore.PriceToDouble(Quote->coreQuote.AskPrice, Quote->coreQuote.PriceType);

            // Write out Symbol, Time, Bid, Ask, Bidsize, Asksize, Reporting Exg
            Console.WriteLine("ExgQuote for Symbol: {0:S}, Time: {1:d}:{2:d}:{3:d}  Bid: {4:f}  Ask: {5:f}  BidSize: {6:d}  AskSise: {7:d}  Exchg: {8:d} ",
                              Symbol,
                              pNxCoreMsg->coreHeader.nxExgTimestamp.Hour, pNxCoreMsg->coreHeader.nxExgTimestamp.Minute, pNxCoreMsg->coreHeader.nxExgTimestamp.Second,
                              Bid, Ask, Quote->coreQuote.BidSize, Quote->coreQuote.AskSize,
                              pNxCoreMsg->coreHeader.ReportingExg);

        }


        // OnNxCoreMMQuote: Function to handle NxCore MMQuote messages.	
        //--------------------------------------------------------------
        static unsafe void OnNxCoreMMQuote(NxCoreSystem* pNxCoreSys, NxCoreMessage* pNxCoreMsg)
        {

            // Get the symbol for category message
            String Symbol = new String(&pNxCoreMsg->coreHeader.pnxStringSymbol->String);

            // Assign a pointer to the MMQuote data
            NxCoreMMQuote* Quote = &pNxCoreMsg->coreData.MMQuote;

            if (Quote->pnxStringMarketMaker == null) return;

            // Get the market maker string
            String MarketMaker = new String(&Quote->pnxStringMarketMaker->String);

            // Get bid and ask price
            double Bid = NxCore.PriceToDouble(Quote->coreQuote.BidPrice, Quote->coreQuote.PriceType);
            double Ask = NxCore.PriceToDouble(Quote->coreQuote.AskPrice, Quote->coreQuote.PriceType);

            // Write out Symbol, MarketMaker, Time, Bid, Ask, Bidsize, Asksize, Reporting Exg
            Console.WriteLine("MMQuote for Symbol: {0:S}, MarketMaker: {1:S}  Time: {2:d}:{3:d}:{4:d}  Bid: {5:f}  Ask: {6:f}  BidSize: {7:d}  AskSise: {8:d}  Exchg: {9:d} ",
                              Symbol, MarketMaker,
                              pNxCoreMsg->coreHeader.nxExgTimestamp.Hour, pNxCoreMsg->coreHeader.nxExgTimestamp.Minute, pNxCoreMsg->coreHeader.nxExgTimestamp.Second,
                              Bid, Ask, Quote->coreQuote.BidSize, Quote->coreQuote.AskSize,
                              pNxCoreMsg->coreHeader.ReportingExg);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Begin Import")
            {
                button1.Text = "Cancel Import";
                button1.Refresh();
                _ImportTicks(importSymbols, importDates, importOverwrite, tapeFileCount);
                //Generate Candles

            }
            if (button1.Text == "Cancel Import")
            {
                //Environment.Exit(0);
            }

        }

        private void updateTimeLabel(DateTime t)
        {
            label2.Text = t.Hour + ":" + t.Minute;
            label2.Refresh();
        }
    }
}