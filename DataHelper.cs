using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Backtester
{
    class DataHelper
    {
        

        public static void GenerateStoreCandles(int Period, List<List<string>> Ticks, string symbol, string date, bool overwrite)
        {
            string csv;
            string filepath = "./Data/Candles/" + symbol + "/" + Period.ToString() + "-minute/" + symbol + date + ".csv";
            
            //Create directories if not exist
            if (!Directory.Exists("./Data/Candles")) Directory.CreateDirectory("./Data/Candles");
            if (!Directory.Exists("./Data/Candles/" + symbol)) Directory.CreateDirectory("./Data/Candles/" + symbol);
            if (!Directory.Exists("./Data/Candles/" + symbol + "/" + Period.ToString() + "-minute")) Directory.CreateDirectory("./Data/Candles/" + symbol + "/" + Period.ToString() + "-minute");

            if (overwrite) { 
                csv = GenerateCandlesFromTicks(Period, Ticks);

                if (File.Exists(@filepath)) File.Delete(@filepath);

                File.Create(filepath).Dispose();
                File.WriteAllText(filepath, csv);
            }
            else
            {
                if (File.Exists(@filepath)) return;
                else
                {
                    csv = GenerateCandlesFromTicks(Period, Ticks);
                    File.Create(filepath).Dispose();
                    File.WriteAllText(filepath, csv);
                }
            }



        }
        public static string GenerateCandlesFromTicks(int Period, List<List<string>> Ticks)
        {

            string candleCSV = "Start Time, Open, High, Low, Close, Volume";

            string startTime = "9:30:00:0:AM";
            string endTime = "4:00:00:0:PM";
            int startMs = GetMsOfDay(startTime);
            int endMs = GetMsOfDay(endTime) - 1;

            int periodInMs = Period * 60000;

            int periodStart = startMs;
            int periodEnd = startMs + periodInMs - 1;

            decimal candleOpen = 0;
            decimal candleHigh = 0;
            decimal candleLow = 0;
            decimal candleClose = 0;
            decimal candleVolume = 0;
            int tickCount = 0;

            //Anomalous tick variables
            List<decimal> recentPriceChanges = new List<decimal>();  //Most recent 50 price changes
            decimal previousTickPrice = 0;
            decimal previousPriceChangeTick = 0;
            decimal priceChange;

            int tickMs = 0;
            decimal tickPrice = 0;
            int tickVolume = 0;
            foreach (List<string> tick in Ticks)
            {
                if (tick[0] == "*")
                {
                    continue;
                }
                tickMs = Convert.ToInt32(tick[0]);
                tickPrice = Convert.ToDecimal(tick[2]);
                tickVolume = Convert.ToInt32(tick[3]);

                if (tickMs >= periodStart && tickMs <= periodEnd  ) //tick in period
                {
                    if (tickCount == 0)
                    {
                        candleOpen = tickPrice; 
                        candleHigh = tickPrice; 
                        candleLow = tickPrice;

                        previousTickPrice = tickPrice;
                        previousPriceChangeTick = tickPrice;
                    }
                    else
                    {
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


                        if (tickPrice > candleHigh) candleHigh = tickPrice;
                        else if (tickPrice < candleLow) candleLow = tickPrice;
                    }
                    candleVolume += tickVolume;
                    candleClose = tickPrice;
                    tickCount++;
                }
                else if (tickMs > periodStart && tickMs > periodEnd) //first tick above period
                {
                    //add previous candle to csv
                    candleCSV += periodStart.ToString() + "," + candleOpen.ToString() + "," + candleHigh.ToString() + "," + candleLow.ToString() + "," +
                        candleClose.ToString() + "," + candleVolume.ToString() + "\n";

                    //set new period
                    periodStart += periodInMs;
                    periodEnd += periodInMs;
                    if (periodStart > endMs) break;
                    while (tickMs > periodEnd) // no ticks occured in this period
                    {
                        candleCSV += "*,*,*,*,*,*\n";
                        periodStart += periodInMs;
                        periodEnd += periodInMs;
                        if (periodStart > endMs) break;
                    }
                    //tick in period, is first tick
                    candleOpen = tickPrice;
                    candleHigh = tickPrice;
                    candleLow = tickPrice;
                    candleClose = tickPrice;
                    candleVolume = tickVolume;
                    tickCount = 1;

                }


                tickPrice = Convert.ToDecimal(tick[2]);
                tickVolume = Convert.ToInt32(tick[3]);


            }


            return candleCSV; 
        }

        public static int GetMsOfDay(string time)
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

        public static List<DateTime> GetTradingDays(DateTime startDate, DateTime endDate)
        {
            /*List<string> _allTradingDays = new List<string>() { "2017-01-03", "2017-01-04", "2017-01-05", "2017-01-06", "2017-01-09", "2017-01-10",
                                                        "2017-01-11", "2017-01-12", "2017-01-13", "2017-01-17", "2017-01-18", "2017-01-19", "2017-01-20",
                                                        "2017-01-23", "2017-01-24", "2017-01-25", "2017-01-26", "2017-01-27", "2017-01-30", "2017-01-31"};  //All trading days available in nxdata   */

            List<string> _allTradingDays = new List<string>() { "2016-11-01","2016-11-02","2016-11-03","2016-11-04","2016-11-07","2016-11-08","2016-11-09","2016-11-10","2016-11-11",
                                                        "2016-11-14","2016-11-15","2016-11-16","2016-11-17","2016-11-18","2016-11-21","2016-11-22","2016-11-23","2016-11-25",
                                                        "2016-11-28","2016-11-29","2016-11-30","2016-12-01","2016-12-02","2016-12-05","2016-12-06","2016-12-07","2016-12-08",
                                                        "2016-12-09","2016-12-12","2016-12-13","2016-12-14","2016-12-15","2016-12-16","2016-12-19","2016-12-20","2016-12-21",
                                                        "2016-12-22","2016-12-23","2016-12-27","2016-12-28","2016-12-29","2016-12-30","2017-01-03", "2017-01-04", "2017-01-05",
                                                        "2017-01-06", "2017-01-09", "2017-01-10","2017-01-11", "2017-01-12", "2017-01-13", "2017-01-17", "2017-01-18", "2017-01-19",
                                                        "2017-01-20","2017-01-23","2017-01-25","2017-01-26","2017-01-30","2017-02-01","2017-02-02","2017-02-03",
                                                        "2017-02-06","2017-02-07","2017-02-08","2017-02-09","2017-02-10","2017-02-13","2017-02-14","2017-02-15","2017-02-16",
                                                        "2017-02-17","2017-02-21","2017-02-22","2017-02-23","2017-02-24","2017-02-27","2017-02-28"
                                                        };  //January 24, January 27 excluded, January 31 excluded

            /*List<string> _allTradingDays = new List<string>() { "2017-01-04", "2017-01-06", "2017-01-09"};  //Shortlist of trading days for home PC*/
            List<DateTime> allTradingDays = new List<DateTime>();

            foreach (string s in _allTradingDays) allTradingDays.Add(Convert.ToDateTime(s));

            List<DateTime> tradingDaysInRange = new List<DateTime>();

            foreach (DateTime d in allTradingDays)
            {
                if (d >= startDate && d <= endDate) tradingDaysInRange.Add(d);
            }



            return tradingDaysInRange;
        }
        
    }
}
