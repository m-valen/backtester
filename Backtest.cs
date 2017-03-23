using System;
using System.Collections.Generic;
using System.Net;
using System.Drawing;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Data;


namespace Backtester
{
    class Backtest
    {
        public TestResult testResult = new TestResult();

        public BackgroundWorker bw = new BackgroundWorker();

        //Symbol + date to test on
        public string symbol;
        public List<string> dates;
        public string filePath;

        //backtest settings
        decimal incrementPrice;
        int incrementSize = 100;

        string startTime;
        string endTime;

        int autoBalanceThreshold = 0;
        int hardStopPL = 0;

        public List<List<string>> symbolParams = new List<List<string>>();
        public Dictionary<string, List<string>> symbolDates;

        public static Dictionary<string, List<decimal>> symbolResults = new Dictionary<string, List<decimal>>();

        int symbolBuyFills;
        int symbolSellFills;
        int symbolPosition;
        decimal symbolIncrementPL = 0;
        decimal symbolPriceMovePL = 0;
        decimal symbolTotalPL = 0;

        int totalBuyFills;
        int totalSellFills;
        decimal totalIncrementPL;
        decimal totalPriceMovePL;
        decimal totalTotalPL;



        //Auto balance settings needed
        //Hard stop settings needed

        public Backtest(string sym, List<string> _dates, decimal _incrementPrice, string _startTime, string _endTime)
        {
            
            symbol = sym;
            dates = _dates;

            startTime = _startTime;
            endTime = _endTime;

            incrementPrice = _incrementPrice;

            
        }

        public Backtest(List<List<string>> _symbolParams, Dictionary<string, List<string>> _symbolDates, BackgroundWorker _bw)
        {
            symbolParams = _symbolParams;
            symbolDates = _symbolDates;
            bw = _bw;
        }

        public TestResult Run()
        {
            int symbolCount = 1;
            
            foreach (List<string> symbolParam in symbolParams) {

                SymbolResult symbolResult = new SymbolResult();

                symbol = symbolParam[0];
                dates = symbolDates[symbol];
                startTime = symbolParam[1];
                endTime = symbolParam[2];
                incrementPrice = Convert.ToDecimal(symbolParam[3]);
                incrementSize = Convert.ToInt32(symbolParam[4]);
                autoBalanceThreshold = Convert.ToInt32(symbolParam[5]);
                hardStopPL = Convert.ToInt32(symbolParam[6]);

                symbolResult.Symbol = symbol;
                symbolResult.scenarioNum = symbolCount;
                symbolResult.Dates = dates;
                symbolResult.StartTime = startTime;
                symbolResult.EndTime = endTime;
                symbolResult.IncrementPrice = incrementPrice;
                symbolResult.IncrementSize = incrementSize;
                symbolResult.Autobalance = autoBalanceThreshold;
                symbolResult.HardStop = hardStopPL;

                foreach (string date in dates)
                {
                    if (bw.CancellationPending) return null;
                    bw.ReportProgress(symbolCount, new List<string> { symbol, date});

                    Console.WriteLine("\n{0} - {1}, {2} -> {3} - IncrementPrice: {4}, IncrementSize: {5}, AutoBalance: {6}, Hard Stop PL: {7}\n", symbol, date, startTime, endTime, incrementPrice, incrementSize, autoBalanceThreshold, hardStopPL);
                    SingleResult singleResult = RunSingle(symbol, date, incrementPrice, startTime, endTime, incrementSize, autoBalanceThreshold, hardStopPL);
                    if (singleResult != null) symbolResult.SingleResults.Add(singleResult);
                    //Console.WriteLine("\n----------------------\n");
                
                }
                

                    
                Console.WriteLine("TOTALS - {0}\n-----------------\n", symbol);
                Console.WriteLine("Buy fills: " + symbolBuyFills);
                Console.WriteLine("Sell fills: " + symbolSellFills);
                Console.WriteLine("Price Move PL: " + symbolPriceMovePL);
                Console.WriteLine("Increment PL: " + symbolIncrementPL);
                Console.WriteLine("Total PL: " + symbolTotalPL);
                Console.WriteLine("\n\n");

                //symbolResults.Add(symbol, new List<decimal> {Convert.ToDecimal(symbolBuyFills), Convert.ToDecimal(symbolSellFills), Convert.ToDecimal(symbolPriceMovePL), Convert.ToDecimal(symbolIncrementPL), Convert.ToDecimal(symbolTotalPL) });
                if (symbolResult.SingleResults.Count > 0) { 
                    symbolResult.CalculateTotals();
                }

                if (symbolResult != null) { 
                    testResult.SymbolResults.Add(symbolResult);
                }

                symbolBuyFills = 0;
                symbolSellFills = 0;
                symbolIncrementPL = 0;
                symbolPriceMovePL = 0;
                symbolTotalPL = 0;


                symbolCount++;


            }
            
            if (testResult.SymbolResults.Count > 0) { 
                testResult.CalculateTotals();
            }

            Console.WriteLine("TOTALS\n-----------------\n");
            Console.WriteLine("Buy fills: " + totalBuyFills);
            Console.WriteLine("Sell fills: " + totalSellFills);
            Console.WriteLine("Price Move PL: " + totalPriceMovePL);
            Console.WriteLine("Increment PL: " + totalIncrementPL);
            Console.WriteLine("Total PL: " + totalTotalPL);
            Console.WriteLine("\n\n");


            return testResult;

        }

        public SingleResult RunSingle(string sym, string _date, decimal _incrementPrice, string _startTime, string _endTime, int _incrementSize, int _autoBalanceThreshold, int _hardStopPL)
        {
            

            //Backtest settings

            int startMs = 0;
            int endMs = 0;
        
            //First, be sure file is available

            filePath = "./Data/Ticks/" + symbol + "/" + symbol + _date + ".csv";

            incrementPrice = _incrementPrice;
            incrementSize = _incrementSize;

            startMs = GetMsOfDay(_startTime);
            endMs = GetMsOfDay(_endTime);
      


            if (!File.Exists(filePath))
            {
                Console.WriteLine("No file found");
                return null;
            }
            List<List<String>> ticks = new List<List<String>>();
            //Read in csv
            using (var fs = File.OpenRead(filePath))
            using (var reader = new StreamReader(fs))
            {
                int count = 0;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    if (!(count == 0))
                    {
                        ticks.Add(new List<String> { values[0], (values[1]), values[2], values[3] }); //MsOfDay, Time, Price, Size   4:14:10.285, 62.27, 200 
                    }
                    count = 1;
                }
            }

            int buyFills = 0;
            int sellFills = 0;
            int symbolPosition = 0;
            decimal priceMovePL = 0;
            decimal incrementPL = 0;
            decimal totalPL = 0;
            decimal maxUnrealized = Convert.ToDecimal(-99999.99);
            decimal minUnrealized = Convert.ToDecimal(99999.99);
            int maxLong = 0;
            int maxShort = 0;
            int maxPosition = 0;
            decimal currentBP = 0;
            decimal maxBP = 0;  //(greatest of maxLong, maxShort) + 500 * price


            decimal highPrint = 0;
            decimal lowPrint = Convert.ToDecimal(9999999.99);

            bool isStopped = false;
            string stopTime = "";

            decimal startingPrice = Convert.ToInt32(99999999.00);
            decimal previousPrint = Convert.ToInt32(99999999.00);

            decimal[] orderLevel1 = new decimal[2] { 0, incrementSize }; //level price, level size
            decimal[] orderLevel2 = new decimal[2] { 0, incrementSize };
            decimal[][] closestOrders = new decimal[][] { orderLevel1, orderLevel2 }; 


            int sellFillVolume = 0;
            int buyFillVolume = 0;

            int tickCount = 0;

            //Anomalous tick variables
            List<decimal> recentPriceChanges = new List<decimal>();  //Most recent 50 price changes
            //decimal previousTickPrice = 0;   is called previousprint
            decimal previousPriceChangeTick = 0;
            decimal priceChange;

            foreach (List<String> tick in ticks)
            {
                //Skip manually selected ticks
                if (tick[0] == "*")
                {
                    continue;
                }
                //Time Constraint

                int tickMs = Convert.ToInt32(tick[0]);
                if (tickMs >= startMs && tickMs <= endMs && !isStopped)
                {
                    tickCount++;
                    decimal tickPrice = Convert.ToDecimal(tick[2]);
                    int tickVolume = Convert.ToInt32(tick[3]);
                    if (tickCount == 1)
                    {
                        startingPrice = tickPrice;
                        previousPrint = startingPrice;
                        previousPriceChangeTick = startingPrice;
                        closestOrders[0][0] = startingPrice - incrementPrice;
                        closestOrders[1][0] = startingPrice + incrementPrice;
                    }

                    //Anomalous tick catcher
                    priceChange = Math.Abs(tickPrice - previousPriceChangeTick);
                    if (priceChange >= Convert.ToDecimal(0.01))  //Valid price change 
                    {
                        if (recentPriceChanges.Count < 10) //Count first 10 price changes as valid
                        {
                            recentPriceChanges.Add(priceChange);
                            previousPriceChangeTick = tickPrice;
                        }
                        if (priceChange > (TechnicalAnalysis.Average(recentPriceChanges) * 10) && priceChange > (tickPrice * Convert.ToDecimal(0.008))) //Anomalous Print
                        {
                            continue; //Don't process anomalous tick
                        }
                        else  //Valid price change
                        {
                            if (recentPriceChanges.Count >= 100)
                            {
                                recentPriceChanges.RemoveAt(0);
                            }
                            recentPriceChanges.Add(priceChange);
                            previousPriceChangeTick = tickPrice;
                        }
                    }
                    

                    while (tickPrice < closestOrders[0][0])
                    {
                        buyFillVolume += tickVolume;
                        if (buyFillVolume >= closestOrders[0][1]) //Complete fill
                        {
                            buyFills++;
                            closestOrders[0][0] -= incrementPrice;
                            closestOrders[1][0] -= incrementPrice;
                            tickVolume -= Convert.ToInt32(closestOrders[0][1]);
                            sellFillVolume = 0;
                            buyFillVolume = 0;
                            if (symbolPosition > 0) priceMovePL -= symbolPosition * incrementPrice;
                            else if (symbolPosition < 0)
                            {
                                priceMovePL -= (symbolPosition + incrementSize) * incrementPrice;
                                incrementPL += incrementSize * incrementPrice;
                            }
                            symbolPosition += Convert.ToInt32(closestOrders[0][1]);
                            if (symbolPosition >= autoBalanceThreshold)
                            {
                                closestOrders[1][1] = incrementSize * 2;
                            }
                            else if (symbolPosition <= -(autoBalanceThreshold))
                            {
                                closestOrders[0][1] = incrementSize * 2;
                            }
                            else
                            {
                                closestOrders[0][1] = incrementSize;
                                closestOrders[1][1] = incrementSize;
                            }
                        }
                        else
                        {
                            buyFillVolume += tickVolume;
                            break;
                        }
                    }
                    while (tickPrice > closestOrders[1][0])
                    {
                        sellFillVolume += tickVolume;
                        if (sellFillVolume >= closestOrders[1][1]) //Complete fill
                        {
                            sellFills++;
                            closestOrders[0][0] += incrementPrice;
                            closestOrders[1][0] += incrementPrice;
                            tickVolume -= Convert.ToInt32(closestOrders[1][1]);
                            sellFillVolume = 0;
                            buyFillVolume = 0;
                            if (symbolPosition < 0) priceMovePL += symbolPosition * incrementPrice;
                            else if (symbolPosition > 0)
                            {
                                priceMovePL += (symbolPosition - incrementSize) * incrementPrice;
                                incrementPL += incrementSize * incrementPrice;
                            }
                            symbolPosition -= Convert.ToInt32(closestOrders[1][1]);

                            if (symbolPosition >= autoBalanceThreshold)
                            {
                                closestOrders[1][1] = incrementSize * 2;
                            }
                            else if (symbolPosition <= -(autoBalanceThreshold))
                            {
                                closestOrders[0][1] = incrementSize * 2;
                            }
                            else
                            {
                                closestOrders[0][1] = incrementSize;
                                closestOrders[1][1] = incrementSize;
                            }
                        }
                        else
                        {
                            buyFillVolume += tickVolume;
                            break;
                        }
                    }

                    //update totalPL, and min/max unrealized values
                    totalPL = incrementPL + priceMovePL;

                    decimal tickAdjustedPL;
                    decimal _lastFill = closestOrders[0][0] + incrementPrice;
                    decimal _difference = tickPrice - _lastFill;
                    decimal tickAdjustedPriceMovePL = priceMovePL + (symbolPosition * _difference);
                    tickAdjustedPL = tickAdjustedPriceMovePL + incrementPL;

                    //if (totalPL > maxUnrealized) maxUnrealized = totalPL;
                    //if (totalPL < minUnrealized) minUnrealized = totalPL;

                    if (tickAdjustedPL > maxUnrealized) maxUnrealized = tickAdjustedPL;
                    if (tickAdjustedPL < minUnrealized) minUnrealized = tickAdjustedPL;

                    //check if high print
                    if (tickPrice > highPrint) highPrint = tickPrice;
                    if (tickPrice < lowPrint) lowPrint = tickPrice;

                    //Hard stop
                    //if (-(totalPL) > hardStopPL)
                    if (-(tickAdjustedPL) > hardStopPL)
                    {
                        isStopped = true;
                        stopTime = tick[1];

                    }

                    //Maxshort, long, BP

                    if (symbolPosition > maxLong) maxLong = symbolPosition;
                    if (symbolPosition < maxShort) maxShort = symbolPosition;

                    if (maxLong > Math.Abs(maxShort)) maxPosition = maxLong;
                    else maxPosition = Math.Abs(maxShort);

                    currentBP = (Math.Abs(symbolPosition) + 500) * tickPrice;

                    if (currentBP > maxBP) maxBP = currentBP;

                    tickCount++;

                    previousPrint = tickPrice;
                }
            }

            //Adjust priceMovePL for difference between final print and last fill
            decimal lastFill = closestOrders[0][0] + incrementPrice;
            decimal finalPrint = previousPrint;
            decimal difference = finalPrint - lastFill;
            priceMovePL += symbolPosition * difference;
            totalPL = priceMovePL + incrementPL;


            SingleResult singleResult = new SingleResult();
            singleResult.Date = _date;
            singleResult.StartingPrice = startingPrice;
            singleResult.FinalPrint = finalPrint;
            singleResult.BuyFills = buyFills;
            singleResult.SellFills = sellFills;
            singleResult.Position = symbolPosition;
            singleResult.PriceMovePL = priceMovePL;
            singleResult.IncrementPL = incrementPL;
            singleResult.TotalPL = totalPL;
            singleResult.maxUnrealized = maxUnrealized;
            singleResult.minUnrealized = minUnrealized;
            singleResult.HighPrint = highPrint;
            singleResult.LowPrint = lowPrint;
            singleResult.MaxLong = maxLong;
            singleResult.MaxShort = maxShort;
            singleResult.MaxBuyingPower = maxBP;

            if (isStopped)
            {
                singleResult.StopTime = stopTime;
            }



            symbolBuyFills += buyFills;
            symbolSellFills += sellFills;
            symbolIncrementPL += incrementPL;
            symbolPriceMovePL += priceMovePL;
            symbolTotalPL += totalPL;

            


            totalBuyFills += buyFills;
            totalSellFills += sellFills;
            totalIncrementPL += incrementPL;
            totalPriceMovePL += priceMovePL;
            totalTotalPL += totalPL;

            if (isStopped)
            {
                Console.WriteLine("Stopped Out at {0}", stopTime);
                singleResult.StopTime = stopTime;
            }
            /*Console.WriteLine("Start Price: " + startingPrice);
            Console.WriteLine("Final Price " + finalPrint);
            Console.WriteLine("Buy fills: " + buyFills);
            Console.WriteLine("Sell fills: " + sellFills);
            Console.WriteLine("Final position: " + symbolPosition);
            Console.WriteLine("Price Move PL: " + priceMovePL);
            Console.WriteLine("Increment PL: " + incrementPL);
            Console.WriteLine("Total PL: " + totalPL);*/

            return singleResult;
        }
        private int GetMsOfDay(string time)
        {
            var timeSplit = time.Split(':');
            int hour = Convert.ToInt32(timeSplit[0]);
            int minute = Convert.ToInt32(timeSplit[1]);
            int second = Convert.ToInt32(timeSplit[2]);
            string meridian = timeSplit[4];
            int timeInMs = 0;

            if (hour == 12) hour = 0;

            if (meridian == "AM") timeInMs = (hour * 3600000) + (minute * 60000) + (second * 1000);
            else if (meridian == "PM") timeInMs = ((hour + 12) * 3600000) + (minute * 60000) + (second * 1000);

            return timeInMs;
        }
    }

}
