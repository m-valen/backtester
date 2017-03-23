using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.CSharp;


namespace Backtester
{
    class StockVider
    {
        //Returns dictionary of date-ATR pairs for given date raneg
        public static void GetATR(string startDate, string endDate)
        {
            //https://api.stockvider.com/data/NYSE/UNH/ATR?start_date=2015-05-20&end_date=2015-07-20
            string json = string.Empty;
            string url = @"https://api.stockvider.com/data/NYSE/UNH/ATR?start_date=2017-01-04&end_date=2017-01-09&api_key=" + Globals.StockViderAPIKey;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                json = reader.ReadToEnd();
                JObject result = JObject.Parse(json);
                //dynamic results = JsonConvert.DeserializeObject<dynamic>(json);
                var data = (string)result["Dataset"];

                
            }

            //Console.WriteLine(html);

        }
    }
}
