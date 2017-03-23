using System;
using System.Collections.Generic;
using System.Text;

namespace Backtester
{
    public class SingleResult
    {
        public string Date;
        public string StopTime = null;
        public decimal StopPrice;
        public decimal StartingPrice;
        public decimal FinalPrint;
        public int BuyFills;
        public int SellFills;
        public int Position;

        //ToDo
        public int MaxShort;
        public int MaxLong;
        public decimal MaxBuyingPower;
        //

        public decimal IncrementPL;
        public decimal PriceMovePL;
        public decimal TotalPL;

        public decimal maxUnrealized;
        public decimal minUnrealized;

        public decimal HighPrint;
        public decimal LowPrint;
    }
}
