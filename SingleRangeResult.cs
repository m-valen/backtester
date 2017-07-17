using System;
using System.Collections.Generic;
using System.Text;

namespace Backtester
{
    public class SingleRangeResult
    {
        public string startTime;
        public string endTime;
        public string date;
        public decimal startPrice;
        public decimal endPrice;
        public decimal high;
        public decimal low;

        //Calculated

        public decimal startHighDiff;
        public decimal startLowDiff;
        public decimal startCloseDiff;

        public void Calculate()
        {
            startHighDiff = high - startPrice;  //always positive
            startLowDiff = low - startPrice;  //always negative
            startCloseDiff = endPrice - startPrice;  //positive for gain, negative for loss
        }
    }
}
