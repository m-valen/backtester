using System;
using System.Collections.Generic;
using System.Text;

namespace Backtester
{
    public class SymbolRangeResult
    {
        public List<SingleRangeResult> singleRangeResults = new List<SingleRangeResult>();

        //Calculated
        public decimal averageStartHighDiff;
        public decimal averageStartLowDiff;
        public decimal averageStartCloseDiff;
        public decimal averageMaxDiff;

        public decimal greatestHighDiff = 0;
        public decimal greatestLowDiff = 0;
        public decimal greatestCloseDiff = 0;
        


        public List<decimal> highDiffs = new List<decimal>();
        public List<decimal> lowDiffs = new List<decimal>();
        public List<decimal> closeDiffs = new List<decimal>();
        public List<decimal> maxDiffs = new List<decimal>();  //The higher of high or low diff for each day

        public void Calculate()
        {
            foreach(SingleRangeResult srr in singleRangeResults)
            {
                highDiffs.Add(srr.startHighDiff);
                lowDiffs.Add(srr.startLowDiff);
                closeDiffs.Add(Math.Abs(srr.startCloseDiff));
                maxDiffs.Add(Math.Max(srr.startHighDiff, Math.Abs(srr.startLowDiff)));


                if (srr.startHighDiff > greatestHighDiff) greatestHighDiff = srr.startHighDiff;
                if (srr.startLowDiff < greatestLowDiff) greatestLowDiff = srr.startLowDiff;
                if (Math.Abs(srr.startCloseDiff) > Math.Abs(srr.startCloseDiff)) greatestCloseDiff = srr.startCloseDiff;



            }

            averageStartHighDiff = TechnicalAnalysis.Average(highDiffs);
            averageStartLowDiff = TechnicalAnalysis.Average(lowDiffs);
            averageStartCloseDiff = TechnicalAnalysis.Average(closeDiffs);
            averageMaxDiff = TechnicalAnalysis.Average(maxDiffs);
        }

    }
}
