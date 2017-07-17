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
using System.Runtime.InteropServices;

// Use the NxCoreAPI namespace
using NxCoreAPI;

namespace Backtester
{
    class TickImport
    {
        public static Dictionary<string, string> csvs = new Dictionary<string, string>();

        static bool InThread = false;
        public static bool killThread = false;
        static String FileName;

        public static List<string> importSymbols = new List<string>();
        public static List<string> importDates = new List<string>();
        public static bool importOverwrite = false;
        public static int tapeFileCount = 0;

        public static string ImportStatus = "Before";
        public static string ImportStatusDetails = "";

        public static DateTime timeInTapeFile;

        public static List<string> symbolsToImport = new List<string>();

        public delegate void reportProgress(string status, string details, DateTime timeInTapeFile);
        public event reportProgress NotifyOfProgress;

        public static bool tapeFileComplete = false;



        public TickImport(List<string> symbols, List<string> dates)
        {
            importSymbols = symbols;
            importDates = dates;
        }

        private void ImportTicks_Shown(Object sender, EventArgs e)
        {


        }
        public void RunImport(BackgroundWorker bw, bool overwrite = false, int tapeFileCount = 0 )
        {
            killThread = false;
            //importSymbols = symbols;
            List<string> symbolsMissing = new List<string>();
            bw.ReportProgress(1, new List<string> { "Processing", "", "" });

            int currentFiles = 1;

            if (!Directory.Exists("./Data/Ticks")) Directory.CreateDirectory("./Data/Ticks");
            foreach (string date in importDates)
            {
                //List<string> __importSymbols = importSymbols;
                symbolsMissing.Clear();
                //importSymbols = __importSymbols;
                if (date != null)
                {
                    FileName = date + ".GS.nx2";
                    if (tapeFileCount > 0)
                    {
                       bw.ReportProgress(1, new List<string> { "Processing", "Importing data from " + FileName + "... " + currentFiles + "/" + tapeFileCount , ""});
                    }
                    else
                    {
                        bw.ReportProgress(1, new List<string> { "Processing", "Importing data from " + FileName , ""});
                    }
                }
                else
                    FileName = "";
                foreach (string symbol in importSymbols)

                {
                    if (!Directory.Exists("./Data/Ticks/" + symbol)) { Directory.CreateDirectory("./Data/Ticks/" + symbol); }
                    string filePath = "./Data/Ticks/" + symbol + "/" + symbol + date + ".csv";
                    if (!File.Exists(filePath)) symbolsMissing.Add(symbol);
                }
                if (!overwrite) symbolsToImport = symbolsMissing;
                else symbolsToImport = importSymbols;
                // Set in Thread  flag
                InThread = true;
                if (symbolsToImport.Count > 0)
                {

                    // Start a new thread!
                    Thread pt = new Thread(() => StartProcessTapeThread());
                    pt.Start();
                    //new Thread(StartProcessTapeThread).Start();

                    // Sleep until the thread exits and resets InThread flag
                    while (InThread) { 
                        updateTimeLabel(timeInTapeFile);
                        bw.ReportProgress(1, new List<string> { "Processing", "Importing data from " + FileName + "... " + currentFiles + "/" + tapeFileCount, timeInTapeFile.ToString("HH:mm") });
                        if (bw.CancellationPending)
                        {
                            killThread = true;
                        }
                    Thread.Sleep(100);
                    }

                    //Create new data files

                    foreach (string _symbol in symbolsToImport)
                    {
                        string symbol = _symbol.Trim();
                        string filePath = "./Data/Ticks/" + symbol + "/" + symbol + date + ".csv";

                        if (tapeFileComplete) { 
                        if (File.Exists(@filePath)) File.Delete(filePath);
                            File.Create(filePath).Dispose();
                            //File.SetAttributes(filePath, FileAttributes.Normal);
                            File.WriteAllText(filePath, csvs[symbol]);
                        }
                    }
                }
                if (!bw.CancellationPending) currentFiles++;
            }
            bw.ReportProgress(1, new List<string>{ "Completed", FileName + "... " + currentFiles + "/" + tapeFileCount, timeInTapeFile.ToString("HH:mm") });

        }


        // Thread function called to start Process Tape
        //----------------------------------------------
        static void StartProcessTapeThread()
        {
            tapeFileComplete = false;
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

            if (killThread)
            {
                return (int)NxCore.NxCALLBACKRETURN_STOP;
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
                if (killThread == true)
                {
                    //Figure out how to do this!!!
                    //Thread.CurrentThread.Interrupt();

                    //Thread.CurrentThread.Abort();
                }
            }

            // Print the specific NxCore status message
            switch (pNxCoreSys->Status)
            {
                case NxCore.NxCORESTATUS_COMPLETE:
                    Console.WriteLine("NxCore Complete Message.");
                    tapeFileComplete = true;
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
                    
                    MessageBox.Show("NxCore Error");
                    MessageBox.Show(pNxCoreSys->StatusData.ToString());
                    //MessageBox.Show(pNxCoreSys->StatusDisplay);
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

                //Check that price is not tradethruexempt in the extended trade conditions - for future imports
                //int tradeCondition = pNxCoreMsg->coreData.Trade.ExtTradeConditions[]

                var tradeCondition = pNxCoreMsg->coreData.Trade.TradeCondition;
                byte* extendedTradeConditions = pNxCoreMsg->coreData.Trade.ExtTradeConditions;
                

                int len = 0;

                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        string aaa = extendedTradeConditions[i].ToString();
                        if (aaa == "255") break;
                        len++;
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }

                byte[] _extendedTradeConditions = new byte[len];
                Marshal.Copy((IntPtr)extendedTradeConditions, _extendedTradeConditions, 0, len);


                Debug.WriteLine(tradeCondition);

                foreach(int condition in _extendedTradeConditions)
                {
                    Debug.WriteLine(condition);
                }
                Debug.WriteLine("");
                Debug.WriteLine("------------------------");
                Debug.WriteLine("");

                //string extCondition = extendedTradeConditions[0].ToString();

                

                //Debug.WriteLine(extCondition);

                int a = 1;

                /*if (tradeCondition == 108) //TradeThruExempt
                {
                    //Get BBO
                    double bestBidPrice = pNxCoreMsg->coreData.ExgQuote.BestBidPrice;
                    double bestAskPrice = pNxCoreMsg->coreData.ExgQuote.BestAskPrice;

                    Console.WriteLine("Trade for Symbol: {0:S}, Time: {1:d}:{2:d}:{3:d}  Price: {4:f}    BBO:  {5}  {6}   TradeCondition:  {7}", Symbol,
                        pNxCoreMsg->coreHeader.nxExgTimestamp.Hour, pNxCoreMsg->coreHeader.nxExgTimestamp.Minute, pNxCoreMsg->coreHeader.nxExgTimestamp.Second,
                                  Price, bestBidPrice, bestAskPrice, tradeCondition);
                }*/

                if (len == 0) { 

                var newLine = string.Format("{0},{1},{2},{3},{4}", MsOfDay, TimeOfTrade, Price, Volume, tradeCondition.ToString());
                csvs[Symbol.Remove(0, 1)] += newLine + "\n";
                }

                if (len == 1)
                {

                    var newLine = string.Format("{0},{1},{2},{3},{4},{5}", MsOfDay, TimeOfTrade, Price, Volume, tradeCondition.ToString(), _extendedTradeConditions[0].ToString());
                    csvs[Symbol.Remove(0, 1)] += newLine + "\n";
                }

                if (len == 2)
                {

                    var newLine = string.Format("{0},{1},{2},{3},{4},{5},{6}", MsOfDay, TimeOfTrade, Price, Volume, tradeCondition.ToString(), _extendedTradeConditions[0].ToString(),
                        _extendedTradeConditions[1].ToString());
                    csvs[Symbol.Remove(0, 1)] += newLine + "\n";
                }

                if (len == 3)
                {

                    var newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7}", MsOfDay, TimeOfTrade, Price, Volume, tradeCondition.ToString(), _extendedTradeConditions[0].ToString(),
                        _extendedTradeConditions[1].ToString(),_extendedTradeConditions[2].ToString());
                    csvs[Symbol.Remove(0, 1)] += newLine + "\n";
                }

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



        private void updateTimeLabel(DateTime t)
        {
            timeInTapeFile = t;
        }
    }
}
