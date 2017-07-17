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
using System.Diagnostics;


namespace Backtester
{
    class Backtest
    {
        public TestResult testResult = new TestResult();

        public TestResult providedResult = new TestResult();

        public BackgroundWorker bw = new BackgroundWorker();

        public bool multiTest = false;

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
        int maxShares = 300;
        string hardStopType;    //PL, Price
        decimal stopIncrements;
        decimal highStop = Convert.ToDecimal(99999.99);
        decimal lowStop = Convert.ToDecimal(0);

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

        string strategy = "Parabolic"; //Parabolic, Toggle
        string rangeType = "Median"; //Mean, Median
        GradientLevels GL = new GradientLevels();
        int fillIndex;
        int lastFillIndex;



        //Auto balance settings needed
        //Hard stop settings needed

        public Backtest(string sym, List<string> _dates, decimal _incrementPrice, string _startTime, string _endTime)
        {

            symbol = sym;
            dates = _dates;

            startTime = _startTime;
            endTime = _endTime;

            incrementPrice = _incrementPrice;
            maxShares = 5000;


        }

        public Backtest(List<List<string>> _symbolParams, Dictionary<string, List<string>> _symbolDates, BackgroundWorker _bw)
        {
            symbolParams = _symbolParams;
            symbolDates = _symbolDates;
            bw = _bw;
        }

        public Backtest(TestResult _providedResult, Dictionary<string, List<string>> _symbolDates, BackgroundWorker _bw)
        {
            multiTest = true;

            //Build symbol params out of provided results

            providedResult = _providedResult;
            symbolDates = _symbolDates;
            bw = _bw;


        }

        public TestResult Run()
        {
            int symbolCount = 1;

            //foreach scenario in testresult

            if (multiTest) {

                foreach (SymbolResult syr in providedResult.SymbolResults)
                {
                    SymbolResult symbolResult = new SymbolResult();   //This is the new result

                    //Adjust parameters to current range

                    symbol = syr.Symbol;
                    dates = syr.Dates;
                    startTime = syr.StartTime;
                    endTime = syr.EndTime;
                    if (rangeType == "Median") incrementPrice = Math.Round(syr.medianMaxDiff / 5, 2);
                    if (rangeType == "Mean") incrementPrice = Math.Round(syr.averageMaxDiff / 5, 2);
                    incrementSize = syr.IncrementSize;
                    autoBalanceThreshold = syr.Autobalance;
                    hardStopType = "IncrementSteps";
                    stopIncrements = Convert.ToDecimal(7.1);
                    hardStopPL = Convert.ToInt32(incrementPrice * 100 * 21);

                    maxShares = 9999999;

                    symbolResult.Symbol = symbol;
                    symbolResult.scenarioNum = symbolCount;
                    symbolResult.Dates = dates;
                    symbolResult.StartTime = startTime;
                    symbolResult.EndTime = endTime;
                    symbolResult.IncrementPrice = incrementPrice;
                    symbolResult.IncrementSize = incrementSize;
                    symbolResult.Autobalance = autoBalanceThreshold;
                    symbolResult.HardStop = hardStopPL;


                    foreach (SingleResult sr in syr.SingleResults)
                    {
                        if (bw.CancellationPending) return null;
                        bw.ReportProgress(symbolCount, new List<string> { symbol, sr.Date });

                        Console.WriteLine("\n{0} - {1}, {2} -> {3} - IncrementPrice: {4}, IncrementSize: {5}, AutoBalance: {6}, Hard Stop PL: {7}\n", symbol, sr.Date, startTime, endTime, incrementPrice, incrementSize, autoBalanceThreshold, hardStopPL);
                        SingleResult singleResult = RunSingle(symbol, sr.Date, incrementPrice, startTime, endTime, incrementSize, autoBalanceThreshold, hardStopPL);
                        if (singleResult != null)
                        {
                            //Preserve old variance results



                            singleResult.startHighDiff = sr.startHighDiff;
                            singleResult.startLowDiff = sr.startLowDiff;
                            singleResult.startCloseDiff = sr.startCloseDiff;
                            //Store new symbol result, save old result in another object
                            symbolResult.SingleResults.Add(singleResult);

                        }
                    }
                    Console.WriteLine("TOTALS - {0}\n-----------------\n", symbol);
                    Console.WriteLine("Buy fills: " + symbolBuyFills);
                    Console.WriteLine("Sell fills: " + symbolSellFills);
                    Console.WriteLine("Price Move PL: " + symbolPriceMovePL);
                    Console.WriteLine("Increment PL: " + symbolIncrementPL);
                    Console.WriteLine("Total PL: " + symbolTotalPL);
                    Console.WriteLine("\n\n");

                    //symbolResults.Add(symbol, new List<decimal> {Convert.ToDecimal(symbolBuyFills), Convert.ToDecimal(symbolSellFills), Convert.ToDecimal(symbolPriceMovePL), Convert.ToDecimal(symbolIncrementPL), Convert.ToDecimal(symbolTotalPL) });
                    if (symbolResult.SingleResults.Count > 0)
                    {
                        symbolResult.CalculateTotals();
                    }

                    if (symbolResult != null)
                    {
                        testResult.SymbolResults.Add(symbolResult);
                    }

                    symbolBuyFills = 0;
                    symbolSellFills = 0;
                    symbolIncrementPL = 0;
                    symbolPriceMovePL = 0;
                    symbolTotalPL = 0;


                    symbolCount++;
                }

            }

            else {
                foreach (List<string> symbolParam in symbolParams) {

                    SymbolResult symbolResult = new SymbolResult();

                    symbol = symbolParam[0];
                    dates = symbolDates[symbol];
                    startTime = symbolParam[1];
                    endTime = symbolParam[2];
                    incrementPrice = Convert.ToDecimal(symbolParam[3]);
                    incrementSize = Convert.ToInt32(symbolParam[4]);
                    autoBalanceThreshold = Convert.ToInt32(symbolParam[5]);
                    hardStopType = Convert.ToString(symbolParam[6]);

                    if (hardStopType == "PL")
                    {
                        hardStopPL = Convert.ToInt32(symbolParam[7]);
                    }

                    else if (hardStopType == "IncrementSteps")
                    {
                        stopIncrements = Convert.ToDecimal(symbolParam[7]);
                    }

                    maxShares = 5000;


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
                        bw.ReportProgress(symbolCount, new List<string> { symbol, date });

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

            Debug.WriteLine(_date + " - " + sym + "\n");
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
                        ticks.Add(new List<String> { values[0], (values[1]), values[2], values[3], values[4] }); //MsOfDay, Time, Price, Size, Trade Condition   4:14:10.285, 62.27, 200 
                    }
                    count = 1;
                }
            }

            List<int> acceptableTradeConditions = new List<int> { 0, 1, 9, 11, 51, 62, 66, 95, 115 };

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

            if (strategy == "Parabolic")
            {
                GL = new GradientLevels();

                GL.lastFillIndex = GL.middleIndex;
                GL.furthestFillIndex = GL.middleIndex;
                GL.baseLot = incrementSize;
                GL.zeroIndex = GL.middleIndex;
                //fillIndex = GL.middleIndex;
                lastFillIndex = GL.middleIndex;
                fillIndex = lastFillIndex;
            }

            foreach (List<String> tick in ticks)
            {
                //Skip manually selected ticks
                if (tick[0] == "*")
                {
                    continue;
                }
                //Time Constraint
                int tradeCondition = Convert.ToInt32(tick[4]);
                if (!(acceptableTradeConditions.Contains(tradeCondition)))
                {
                    continue;   //Don't process trade condition
                }


                int tickMs = Convert.ToInt32(tick[0]);
                if (tickMs >= startMs && tickMs <= endMs && !isStopped)
                {
                    tickCount++;
                    decimal tickPrice = Convert.ToDecimal(tick[2]);
                    int tickVolume = Convert.ToInt32(tick[3]);

                    if (_date == "20170421")
                    {
                        int a = 1;
                    }

                    if (tickCount == 1)
                    {
                        startingPrice = tickPrice;
                        if (hardStopType == "IncrementSteps")
                        {
                            setStops(startingPrice, incrementPrice, stopIncrements);
                        }

                        previousPrint = startingPrice;
                        previousPriceChangeTick = startingPrice;

                        if (strategy == "Toggle") {

                            closestOrders[0][0] = startingPrice - incrementPrice;
                            closestOrders[1][0] = startingPrice + incrementPrice;
                        }

                        else if (strategy == "Parabolic")
                        {

                            GL.Initialize(startingPrice, incrementPrice);

                            closestOrders[0][0] = Convert.ToDecimal(GL.Levels[GL.middleIndex - 1][0]);
                            closestOrders[1][0] = Convert.ToDecimal(GL.Levels[GL.middleIndex + 1][0]);
                        }
                    }





                    //Anomalous tick catcher + trade condition exclusion
                    priceChange = Math.Abs(tickPrice - previousPriceChangeTick);

                    if (priceChange >= Convert.ToDecimal(0.01))  //Valid price change 
                    {

                        if (recentPriceChanges.Count < 10) //Count first 10 price changes as valid
                        {
                            recentPriceChanges.Add(priceChange);
                            previousPriceChangeTick = tickPrice;
                        }
                        else if (priceChange > (TechnicalAnalysis.Average(recentPriceChanges) * 10) && priceChange > (tickPrice * Convert.ToDecimal(0.002))) //Anomalous Print
                        {
                            continue; //Don't process anomalous tick
                        }
                        //Trade condition catcher
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


                    if (hardStopType == "IncrementSteps")
                    {
                        if (tickPrice >= highStop)
                        {
                            isStopped = true;
                            stopTime = tick[1];
                        }
                        else if (tickPrice <= lowStop)
                        {
                            isStopped = true;
                            stopTime = tick[1];
                        }
                    }


                    if (strategy == "Toggle") {

                        while (tickPrice < closestOrders[0][0]) // Buy Fill
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
                                if (symbolPosition >= autoBalanceThreshold)   // too long
                                {
                                    closestOrders[1][1] = incrementSize * 2;
                                }
                                else if (symbolPosition <= -(autoBalanceThreshold))  //too short
                                {
                                    closestOrders[0][1] = incrementSize * 2;
                                }
                                else
                                {
                                    closestOrders[0][1] = incrementSize;
                                    closestOrders[1][1] = incrementSize;
                                }
                                if (symbolPosition >= maxShares)
                                {
                                    closestOrders[0][1] = 0;
                                }
                                else if (symbolPosition <= -(maxShares))
                                {
                                    closestOrders[1][1] = 0;
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
                                if (symbolPosition >= maxShares)
                                {
                                    closestOrders[0][1] = 0;
                                }
                                else if (symbolPosition <= -(maxShares))
                                {
                                    closestOrders[1][1] = 0;
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
                    }

                    if (strategy == "Parabolic")
                    {

                        while (tickPrice < closestOrders[0][0]) // Buy Fill
                        {
                            buyFillVolume += tickVolume;
                            if (buyFillVolume >= closestOrders[0][1]) //Complete fill
                            {
                                buyFills++;
                                //Update metrics
                                if (symbolPosition > 0) //Long
                                {
                                    totalPL -= Math.Abs(symbolPosition) * incrementPrice;
                                }
                                else if (symbolPosition < 0)  //Short
                                {
                                    totalPL += Math.Abs(symbolPosition) * incrementPrice;
                                }

                                symbolPosition += Convert.ToInt32(closestOrders[0][1]);  //Update new symbol position



                                //Update order levels
                                if (fillIndex > 0) fillIndex = --lastFillIndex;

                                Console.WriteLine(symbolPosition + " shares at index " + fillIndex + " for Total PL: " + totalPL);
                                if (_date == "20170228")
                                {
                                    int a = 1;
                                }
                                int reCalc = GL.ReCalculate(fillIndex);

                                if (reCalc == 1)
                                {
                                    Debug.WriteLine("Fill Error");
                                }

                                tickVolume -= Convert.ToInt32(closestOrders[0][1]);
                                sellFillVolume = 0;
                                buyFillVolume = 0;

                                if (fillIndex != 0) {
                                    closestOrders[0][0] = Convert.ToDecimal(GL.Levels[fillIndex - 1][0]);
                                    closestOrders[0][1] = Convert.ToDecimal(GL.Levels[fillIndex - 1][1]);
                                }
                                else
                                {
                                    closestOrders[0][0] = Convert.ToDecimal(0);
                                    closestOrders[0][1] = Convert.ToDecimal(0);
                                }
                                closestOrders[1][0] = Convert.ToDecimal(GL.Levels[fillIndex + 1][0]);
                                closestOrders[1][1] = Convert.ToDecimal(GL.Levels[fillIndex + 1][1]);


                                //Update last filled index

                                if (GL.isRezero)
                                {
                                    if (_date == "20170301") {
                                        int a = 1;
                                    }
                                    lastFillIndex = GL.middleIndex;
                                    fillIndex = lastFillIndex;
                                    setStops(Convert.ToDecimal(GL.Levels[lastFillIndex][0]), incrementPrice, stopIncrements);
                                    closestOrders[0][0] = Convert.ToDecimal(GL.Levels[fillIndex - 1][0]);
                                    closestOrders[0][1] = Convert.ToDecimal(GL.Levels[fillIndex - 1][1]);
                                    closestOrders[1][0] = Convert.ToDecimal(GL.Levels[fillIndex + 1][0]);
                                    closestOrders[1][1] = Convert.ToDecimal(GL.Levels[fillIndex + 1][1]);
                                }
                                else
                                {
                                    lastFillIndex = fillIndex;
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

                                //Update metrics
                                if (symbolPosition > 0) //Long
                                {
                                    totalPL += Math.Abs(symbolPosition) * incrementPrice;
                                }
                                else if (symbolPosition < 0)  //Short
                                {
                                    totalPL -= Math.Abs(symbolPosition) * incrementPrice;
                                }

                                symbolPosition -= Convert.ToInt32(closestOrders[1][1]);  //Update symbol position

                                //Get new order levels

                                if (fillIndex < GL.middleIndex * 2) fillIndex = ++lastFillIndex;

                                Console.WriteLine(symbolPosition + " shares at index " + fillIndex + " for Total PL: " + totalPL);
                                if (_date == "20170228")
                                {
                                    int a = 1;
                                }
                                int reCalc = GL.ReCalculate(fillIndex);

                                if (reCalc == 1)
                                {
                                    Debug.WriteLine("Fill Error");
                                }

                                tickVolume -= Convert.ToInt32(closestOrders[1][1]);
                                sellFillVolume = 0;
                                buyFillVolume = 0;

                                closestOrders[0][0] = Convert.ToDecimal(GL.Levels[fillIndex - 1][0]);
                                closestOrders[0][1] = Convert.ToDecimal(GL.Levels[fillIndex - 1][1]);

                                if (fillIndex != GL.middleIndex * 2)
                                {
                                    closestOrders[1][0] = Convert.ToDecimal(GL.Levels[fillIndex + 1][0]);
                                    closestOrders[1][1] = Convert.ToDecimal(GL.Levels[fillIndex + 1][1]);
                                }
                                else
                                {
                                    closestOrders[1][0] = Convert.ToDecimal(99999);
                                    closestOrders[1][1] = Convert.ToDecimal(0);
                                }


                                //Update last filled index

                                if (GL.isRezero)
                                {

                                    lastFillIndex = GL.middleIndex;;
                                    fillIndex = lastFillIndex;
                                    setStops(Convert.ToDecimal(GL.Levels[lastFillIndex][0]), incrementPrice, stopIncrements);
                                    closestOrders[0][0] = Convert.ToDecimal(GL.Levels[fillIndex - 1][0]);
                                    closestOrders[0][1] = Convert.ToDecimal(GL.Levels[fillIndex - 1][1]);
                                    closestOrders[1][0] = Convert.ToDecimal(GL.Levels[fillIndex + 1][0]);
                                    closestOrders[1][1] = Convert.ToDecimal(GL.Levels[fillIndex + 1][1]);
                                }
                                else
                                {
                                    lastFillIndex = fillIndex;
                                }
                            }
                            else
                            {
                                sellFillVolume += tickVolume;
                                break;
                            }
                        }


                    }



                    decimal tickAdjustedPL = 0;
                    decimal _lastFill = Convert.ToDecimal(GL.Levels[lastFillIndex][0]);
                    decimal _difference = tickPrice - _lastFill;

                    if (strategy == "Toggle") {    //set tick adjusted PL as price move difference + increment PL
                        decimal tickAdjustedPriceMovePL = priceMovePL + (symbolPosition * _difference);
                        tickAdjustedPL = tickAdjustedPriceMovePL + incrementPL;
                    }

                    else if (strategy == "Parabolic")
                    {
                        tickAdjustedPL = totalPL + (symbolPosition * _difference);

                    }

                    //if (totalPL > maxUnrealized) maxUnrealized = totalPL;
                    //if (totalPL < minUnrealized) minUnrealized = totalPL;

                    if (tickAdjustedPL > maxUnrealized)
                    {
                        maxUnrealized = tickAdjustedPL;
                        if (_date == "20170228") {
                            Console.WriteLine("Total PL: " + totalPL + ", " + "Max Unrealized: " + maxUnrealized);
                            Console.WriteLine("Tick Price: " + tickPrice + ", " + "Last Fill: " + _lastFill);
                            Console.WriteLine("High Stop: " + highStop + ", Low Stop: " + lowStop);
                        }
                    }
                    if (tickAdjustedPL < minUnrealized)
                    {
                        minUnrealized = tickAdjustedPL;
                        if (_date == "20170228")
                        {
                            Console.WriteLine("Total PL: " + totalPL + ", " + "Min Unrealized: " + minUnrealized);
                            Console.WriteLine("Tick Price: " + tickPrice + ", " + "Last Fill: " + _lastFill);
                            Console.WriteLine("High Stop: " + highStop + ", Low Stop: " + lowStop);
                        }
                    }

                    //check if high print
                    if (tickPrice > highPrint) highPrint = tickPrice;
                    if (tickPrice < lowPrint) lowPrint = tickPrice;

                    //Hard stop
                    //if (-(totalPL) > hardStopPL)
                    if (hardStopType == "PL" && -(tickAdjustedPL) > hardStopPL)
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
            }  // End of tick processing

            //Adjust priceMovePL for difference between final print and last fill

            decimal lastFill = 0;
            if (strategy == "Toggle") lastFill = closestOrders[0][0] + incrementPrice;
            else if (strategy == "Parabolic") lastFill = Convert.ToDecimal(GL.Levels[lastFillIndex][0]);
            decimal finalPrint = previousPrint;
            decimal difference = finalPrint - lastFill;

            SingleResult singleResult = new SingleResult();

            if (strategy == "Toggle") {
                priceMovePL += symbolPosition * difference;
                totalPL = priceMovePL + incrementPL;

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

            }

            else if (strategy == "Parabolic")
            {
                totalPL = totalPL + symbolPosition * difference;

                singleResult.Date = _date;
                singleResult.StartingPrice = startingPrice;
                singleResult.FinalPrint = finalPrint;
                singleResult.BuyFills = buyFills;
                singleResult.SellFills = sellFills;
                singleResult.Position = symbolPosition;
                singleResult.PriceMovePL = 0;
                singleResult.IncrementPL = 0;
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
                symbolIncrementPL += 0;
                symbolPriceMovePL += 0;
                symbolTotalPL += totalPL;




                totalBuyFills += buyFills;
                totalSellFills += sellFills;
                totalIncrementPL += 0;
                totalPriceMovePL += 0;
                totalTotalPL += totalPL;

            }

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


            singleResult.Calculate();
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
        private void setStops(decimal _midPrice, decimal _incrementPrice, decimal _stopIncrements)
        {
            highStop = Math.Round(_midPrice + (_incrementPrice * _stopIncrements), 2);
            lowStop = Math.Round(_midPrice - (_incrementPrice * _stopIncrements), 2);
        }

    }

}
