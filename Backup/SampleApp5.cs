////////////////////////////////////////////////////
//// SampleApp5.cs                              ////
//// Sample App for C# NxCore API Wrapper       ////
//// Author: Jeffrey Donovan                    ////
//// Date: 12-22-09                             ////
////////////////////////////////////////////////////
//// Demonstrates:                              ////
//// Starting ProcessTape from a thread         ////
//// Handling NxCore Trade and Quote Messages   ////
////////////////////////////////////////////////////
//// To Read as Written:                        ////
//// Tab Size: 4  Indent Size: 2, Keep Tabs     ////
////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

// Use the NxCoreAPI namespace
using NxCoreAPI;


namespace SampleApp5
{ 

  class Program
  {
	static String FileName;
	static bool InThread = false;


	// Main Entry Point for app.	
	//--------------------------
	static void Main(string[] args)
	{
	  Console.WriteLine("NxCore CSharp SampleApp5 Start.");

	  // If a tape filename was passed in command line argument,
	  // assign it to FileName	  
	  if (args.Length > 0)
		FileName = args[0];
	  else
		FileName = "";

	  // Set in Thread  flag
	  InThread = true;

	  // Start a new thread!
	  new Thread(StartProcessTapeThread).Start();

	  // Sleep until the thread exits and resets InThread flag
	  while (InThread)
		Thread.Sleep(100);

	  Console.WriteLine("NxCore CSharp SampleApp5 Stop.");
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
						 Program.OnNxCoreCallback);

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
		  OnNxCoreTrade(pNxCoreSys, pNxCoreMsg);
		  break;

		// NxCore Level1 Quote Message
		case NxCore.NxMSG_EXGQUOTE:
		  OnNxCoreExgQuote(pNxCoreSys, pNxCoreMsg);
		  break;

		// NxCore Level2 Quote Message
		case NxCore.NxMSG_MMQUOTE:
		  OnNxCoreMMQuote(pNxCoreSys, pNxCoreMsg);
		  break;
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

		Console.WriteLine("NxCore Time: {0:T}", thisTime);
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
	
	
	// OnNxCoreTrade: Function to handle NxCore Trade messages.	
	//--------------------------------------------------------------
	static unsafe void OnNxCoreTrade(NxCoreSystem* pNxCoreSys, NxCoreMessage* pNxCoreMsg)
	{

	  // Get the symbol for category message
	  String Symbol = new String(&pNxCoreMsg->coreHeader.pnxStringSymbol->String);

	  // Assign a pointer to the Trade data
	  NxCoreTrade* Trade = &pNxCoreMsg->coreData.Trade;

	  // Get the price and net change
	  double Price = NxCore.PriceToDouble(Trade->Price,Trade->PriceType);
	  double NetChange = NxCore.PriceToDouble(Trade->NetChange, Trade->PriceType);

	  // Write out Symbol, Time, Price, NetChg, Size, Reporting Exg
	  Console.WriteLine("Trade for Symbol: {0:S}, Time: {1:d}:{2:d}:{3:d}  Price: {4:f}  NetChg: {5:f}  Size: {6:d}  Exchg: {7:d} ",
						Symbol,
						pNxCoreMsg->coreHeader.nxExgTimestamp.Hour,pNxCoreMsg->coreHeader.nxExgTimestamp.Minute,pNxCoreMsg->coreHeader.nxExgTimestamp.Second,
						Price,NetChange,Trade->Size,
						pNxCoreMsg->coreHeader.ReportingExg);

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
	
  }
}
