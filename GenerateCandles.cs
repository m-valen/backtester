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

namespace Backtester
{
    public partial class GenerateCandles : Form
    {
        List<string> symbols;
        List<DateTime> dates;
        List<int> _periods;
        bool overwrite;

        public GenerateCandles(List<string> _symbols, List<DateTime> _dates, List<int> periods, bool _overwrite)
        {
            InitializeComponent();

            symbols = _symbols;
            dates = _dates;
            _periods = periods;
            overwrite = _overwrite;
            
        }

        private void generateCandles(List<string> _symbols, List<DateTime> _dates, List<int> periods, bool _overwrite)
        {
            label1.Text = "Beginning...";
            label1.Refresh();
            foreach (string symbol in _symbols)
            {
                label1.Text = "Generating for: " + symbol;
                label1.Refresh();

                foreach (DateTime d in _dates)
                {
                    string date = d.ToString("yyyyMMdd");
                    string filepath = "./Data/Ticks/" + symbol + "/" + symbol + date + ".csv";
                    //Check if tick file exists and read in csv
                    if (File.Exists(filepath))
                    {
                        List<List<string>> ticks = new List<List<string>>();
                        //Read in csv
                        using (var fs = File.OpenRead(filepath))
                        using (var reader = new StreamReader(fs))
                        {
                            int count = 0;
                            while (!reader.EndOfStream)
                            {
                                var line = reader.ReadLine();
                                var values = line.Split(',');
                                if (!(count == 0))
                                {
                                    ticks.Add(new List<string> { values[0], (values[1]), values[2], values[3] }); //MsOfDay, Time, Price, Size   4:14:10.285, 62.27, 200 
                                }
                                count = 1;
                            }
                        }
                        foreach (int period in periods)
                        {
                            DataHelper.GenerateStoreCandles(period, ticks, symbol, date, _overwrite);
                        }


                    }
                    else
                    {
                        Console.WriteLine("Could not find file: " + filepath);
                    }


                }
                Console.WriteLine("Stored candles for: " + symbol);
            }
            label1.Text = "Completed";
            label1.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            generateCandles(symbols, dates, _periods, overwrite);
        }


    }
}
