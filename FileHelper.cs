using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Backtester
{
    class FileHelper
    {
        public static List<List<string>> getTicks(string symbol, string date)
        {
            string filePath = "./Data/Ticks/" + symbol + "/" + symbol + date + ".csv";

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

            return ticks;
        }

        public static List<List<string>> getCandles(string symbol, string date, int period)
        {
            string filePath = "./Data/Candles/" + symbol + "/" + period.ToString() + "-minute/" + symbol + date + ".csv";

            if (!File.Exists(filePath))
            {
                Console.WriteLine("No file found");
                return null;
            }
            List<List<String>> candles = new List<List<String>>();
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
                        candles.Add(new List<String> { values[0], values[1], values[2], values[3], values[4] }); //MsOfDay, Open, High, Low, Close, Volume   4:14:10.285, 62.27, 200 
                    }
                    count = 1;
                }
            }

            return candles;
        }

        public static List<string> getAllSymbols(DateTime? startDate = null, DateTime? endDate = null)
        {
            List<string> symbols = new List<string>{ };
            string[] directories;
            //
            directories = Directory.GetDirectories("./Data/Ticks");
            foreach (string d in directories) { 
                if (startDate == null && Directory.GetFiles(d).Length > 0)
                {
                    symbols.Add(d.Split('\\')[1]);
                }
                else if (startDate != null)
                {
                    string[] files = Directory.GetFiles(d);
                    List<DateTime> dates = new List<DateTime>();
                    //Convert files to dates
                    foreach (string f in files)
                    {
                        int pFrom = f.IndexOf("20");
                        int pTo = f.LastIndexOf(".csv");
                        string _date = f.Substring(pFrom, pTo - pFrom);
                        dates.Add(DateTime.ParseExact(_date, "yyyyMMdd", null));
                    }
                    foreach (DateTime date in dates)
                    {
                        if (date >= startDate && date <= endDate)
                        {
                            symbols.Add(d.Split('\\')[1]);
                            break;
                        }
                    }
                    
                }
            }



            return symbols;
        }

        public static Dictionary<DateTime, List<string>> GetMissingDays(List<string> symbols, List<DateTime> dates)
        {
            Dictionary<DateTime, List<string>> dateSymbolsMissing = new Dictionary<DateTime, List<string>>();
            string filename = "";
            string filepath = "";
            foreach (DateTime _d in dates)
            {
                List<string> symbolsMissing = new List<string>();
                filename = _d.ToString("yyyyMMdd") + ".GS.nx2";
                foreach (string s in symbols)
                {
                    filepath = "./Data/Ticks/" + s + "/" + s + _d.ToString("yyyyMMdd") + ".csv";
                    if (!File.Exists(filepath)) symbolsMissing.Add(s);
                }
                dateSymbolsMissing.Add(_d, symbolsMissing);
            }

            return dateSymbolsMissing;
        }
    }
}
