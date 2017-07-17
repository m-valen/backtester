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
        public int CompleteFills;
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

        public decimal startHighDiff;
        public decimal startLowDiff;
        public decimal startCloseDiff;

        public void Calculate()
        {
            startHighDiff = HighPrint - StartingPrice;  //always positive
            startLowDiff = LowPrint - StartingPrice;  //always negative
            startCloseDiff = FinalPrint - StartingPrice;  //positive for gain, negative for loss
            CompleteFills = Math.Min(BuyFills, SellFills);
        }

    }
}
