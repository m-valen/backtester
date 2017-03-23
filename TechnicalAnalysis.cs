using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Backtester
{
    class TechnicalAnalysis
    {
        public static decimal TradingRange(string symbol, string date, int startMs, int endMs)
        {
            decimal tradingRange = 0;
            decimal high = 0;
            int highMs = 0;
            decimal low = Convert.ToDecimal(999999999999999.99);
            int lowMs = 0;



            // get ticks
            string filePath = "./Data/Ticks/" + symbol + "/" + symbol + date + ".csv";

            List<List<string>> ticks = FileHelper.getTicks(symbol, date);

            List<decimal> recentPriceChanges = new List<decimal>();  //Most recent 50 price changes
            decimal previousTickPrice = 0;
            decimal previousPriceChangeTick = 0;
            decimal priceChange;

            if (ticks != null)
            {
                foreach (List<string> tick in ticks)
                {
                    if (tick[0] == "*")
                    {
                        continue;
                    }
                    int tickMs = 0;
                    decimal tickPrice = 0;
                    int tickVolume = 0;

                    tickMs = Convert.ToInt32(tick[0]);
                    tickPrice = Convert.ToDecimal(tick[2]);

                    //Anomalous tick catcher
                   if (!(previousTickPrice == 0) && previousPriceChangeTick != 0)
                    {
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
                                if (recentPriceChanges.Count >= 100) {
                                    recentPriceChanges.RemoveAt(0);
                                }
                                recentPriceChanges.Add(priceChange);
                                previousPriceChangeTick = tickPrice;
                            }
                        }
                    }
                    else
                    {
                        previousPriceChangeTick = tickPrice;
                    }
                    
                    if (tickMs >= startMs && tickMs <= endMs)
                    {
                        if (tickPrice > high)
                        {
                            high = tickPrice;
                            highMs = tickMs;
                        }
                        if (tickPrice < low)
                        {
                            low = tickPrice;
                            lowMs = tickMs;
                        }
                    }
                    previousTickPrice = tickPrice;
                }

                tradingRange = high - low;
                Console.WriteLine("High: " + high + " - " + highMs + "..." + "Low : " + low + " - " + lowMs);
                
            }
            else
            {
                Console.WriteLine("Failed to retrieve ticks for - Symbol: " + symbol + ", Date: " + date);
            }

            return tradingRange;
        }

        public static decimal ATR(List<List<string>> candles)
        {
            decimal ATR = 0;

            List<decimal> candleTrueRanges = new List<decimal>();

            decimal trueHigh;
            decimal trueLow;

            decimal candleOpen;
            decimal candleHigh;
            decimal candleLow;
            decimal candleClose;
            decimal previousClose = 0;

            decimal candleTrueRange = 0;

            foreach (List<string> candle in candles)
            {
                trueHigh = 0;
                trueLow = Convert.ToDecimal(9999999999.99);
                if (candle[0] == "*")
                {
                    continue;
                }


                candleOpen = Convert.ToDecimal(candle[1]);
                candleHigh = Convert.ToDecimal(candle[2]);
                candleLow = Convert.ToDecimal(candle[3]);
                candleClose = Convert.ToDecimal(candle[4]);

                if (previousClose == 0)
                {
                    trueHigh = candleHigh;
                    trueLow = candleLow;
                    candleTrueRange = trueHigh - trueLow;
                    previousClose = candleClose;
                    candleTrueRanges.Add(candleTrueRange);
                    continue;
                }
                if (candleHigh > previousClose) trueHigh = candleHigh;
                else trueHigh = previousClose;

                if (candleLow < previousClose) trueLow = candleLow;
                else trueLow = previousClose;

                previousClose = candleClose;

                candleTrueRange = trueHigh - trueLow;
                candleTrueRanges.Add(candleTrueRange);
            }

            ATR = Average(candleTrueRanges);

            return ATR;
        }

        public static decimal Average(List<decimal> _list)
        {
            if (_list.Count == 0)
            {
                return 0;
            }
            decimal sum = 0;
            for (int i = 0; i < _list.Count; i++)
            {
                sum += _list[i];
            }
            decimal averageRange = sum / _list.Count;
            return averageRange;
        }
    }
}
