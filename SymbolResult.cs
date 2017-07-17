using System;
using System.Collections.Generic;
using System.Text;

namespace Backtester
{
    public class SymbolResult : IComparable<SymbolResult>
    {
        public List<SingleResult> SingleResults = new List<SingleResult>();
        public int scenarioNum = 0;
        public string Symbol;
        public List<string> Dates;
        public string StartTime;
        public string EndTime;
        public decimal IncrementPrice;
        public int IncrementSize;
        public int Autobalance;
        public string HardStopType;
        public decimal HardStopIncrements;
        public int HardStop;
        //
        public int BuyFills;
        public int SellFills;
        public int CompleteFills;
        public decimal averageCompleteFills;
        public decimal averageBuyFills;
        public decimal averageSellFills;
        public decimal IncrementPL;
        public decimal PriceMovePL;
        public decimal TotalPL;
        public decimal ProfitMargin;
        public decimal AvgMaxUnrealized;
        public decimal AvgMinUnrealized;
        public decimal AvgWin;
        public int NumWins;
        public decimal AvgLoss;
        public int NumLosses;
        public decimal BuyingPower;

        public decimal yield;

        //Calculated range stuff
        public decimal averageStartHighDiff;
        public decimal averageStartLowDiff;
        public decimal averageStartCloseDiff;
        public decimal averageMaxDiff;
        public decimal averageMaxRangeSteps;
        public decimal medianStartHighDiff;
        public decimal medianStartLowDiff;
        public decimal medianStartCloseDiff;
        public decimal medianMaxDiff;
        public decimal medianMaxRangeSteps;
        public decimal medianStepFillRatio;
        public decimal stepFillRatio;  //Need to calculate this
        public decimal variance;  //mean
        public decimal medianVariance;
        public decimal varianceStopCapped;  //median
        public decimal varianceNormalized;  //mean
        public decimal medianVarianceNormalized;
        public decimal varianceStopCappedNormalized;  //median
        public decimal varianceSquares;
        public decimal varianceSquaresNormalized;

        public List<int> numPriceStops = new List<int> { 0, 0, 0, 0 }; //4, 5, 6, 7


        public decimal greatestHighDiff = 0;
        public decimal greatestLowDiff = 0;
        public decimal greatestCloseDiff = 0;



        public List<decimal> highDiffs = new List<decimal>();
        public List<decimal> lowDiffs = new List<decimal>();
        public List<decimal> closeDiffs = new List<decimal>();
        public List<decimal> maxDiffs = new List<decimal>();  //The higher of high or low diff for each day

        public List<decimal> _buyFills = new List<decimal>();
        public List<decimal> _sellFills = new List<decimal>();

        //Average Win/Median Module

        public decimal highestMaxUnrealized;
        public int daysAboveAverageWin;
        public int daysGaveBackAverageWin;
        public decimal medianWin;
        public decimal medianMaxUnrealized;

        public List<decimal> highestUnrealizeds = new List<decimal>();


        public void CalculateTotals()
        {
            List<decimal> maxUnrealizeds = new List<decimal>();
            List<decimal> minUnrealizeds = new List<decimal>();
            List<decimal> wins = new List<decimal>();
            List<decimal> losses = new List<decimal>();
            List<decimal> _completeFills = new List<decimal>();


            foreach (SingleResult sr in SingleResults)
            {
                BuyFills += sr.BuyFills;
                _buyFills.Add(sr.BuyFills);
                SellFills += sr.SellFills;
                _sellFills.Add(sr.SellFills);
                IncrementPL += sr.IncrementPL;
                PriceMovePL += sr.PriceMovePL;
                TotalPL += sr.TotalPL;
                maxUnrealizeds.Add(sr.maxUnrealized);
                minUnrealizeds.Add(sr.minUnrealized);
                if (sr.TotalPL > 0) wins.Add(sr.TotalPL);
                else if (sr.TotalPL < 0) losses.Add(sr.TotalPL);

                //Range stuff
                highDiffs.Add(sr.startHighDiff);
                lowDiffs.Add(sr.startLowDiff);
                closeDiffs.Add(Math.Abs(sr.startCloseDiff));
                decimal maxDiff = Math.Max(sr.startHighDiff, Math.Abs(sr.startLowDiff));
                maxDiffs.Add(maxDiff);

                if (maxDiff >= (IncrementPrice * 7)) {
                    numPriceStops[0]++; numPriceStops[1]++; numPriceStops[2]++; numPriceStops[3]++;
                }
                else if (maxDiff >= (IncrementPrice * 6))
                {
                    numPriceStops[0]++; numPriceStops[1]++; numPriceStops[2]++;
                }
                else if (maxDiff >= (IncrementPrice * 5))
                {
                    numPriceStops[0]++; numPriceStops[1]++;
                }
                else if (maxDiff >= (IncrementPrice * 4))
                {
                    numPriceStops[0]++;
                }


                if (sr.startHighDiff > greatestHighDiff) greatestHighDiff = sr.startHighDiff;
                if (sr.startLowDiff < greatestLowDiff) greatestLowDiff = sr.startLowDiff;
                if (Math.Abs(sr.startCloseDiff) > Math.Abs(greatestCloseDiff)) greatestCloseDiff = sr.startCloseDiff;


                CompleteFills += sr.CompleteFills;

                highestUnrealizeds.Add(sr.maxUnrealized);

            }
            if (IncrementPL > 0) ProfitMargin = (TotalPL / IncrementPL) * 100;
            else ProfitMargin = Convert.ToDecimal(-9999.99);
            AvgMaxUnrealized = TechnicalAnalysis.Average(maxUnrealizeds);
            AvgMinUnrealized = TechnicalAnalysis.Average(minUnrealizeds);
            AvgWin = TechnicalAnalysis.Average(wins);
            AvgLoss = TechnicalAnalysis.Average(losses);
            NumWins = wins.Count;
            NumLosses = losses.Count;

            //Buying Power
            decimal buyingPower = 0;
            foreach (SingleResult sr in SingleResults)
            {
                if (sr.MaxBuyingPower > buyingPower) buyingPower = sr.MaxBuyingPower;
            }
            BuyingPower = buyingPower;

            yield = (IncrementPL / buyingPower) * 100;

            averageStartHighDiff = TechnicalAnalysis.Average(highDiffs);
            averageStartLowDiff = TechnicalAnalysis.Average(lowDiffs);
            averageStartCloseDiff = TechnicalAnalysis.Average(closeDiffs);
            averageMaxDiff = TechnicalAnalysis.Average(maxDiffs);
            averageMaxRangeSteps = averageMaxDiff / IncrementPrice;

            averageBuyFills = TechnicalAnalysis.Average(_buyFills);
            averageSellFills = TechnicalAnalysis.Average(_sellFills);

            averageCompleteFills = Convert.ToDecimal(CompleteFills) / Convert.ToDecimal(SingleResults.Count);

            stepFillRatio = averageCompleteFills / (((averageMaxRangeSteps * averageMaxRangeSteps) - averageMaxRangeSteps) / 2);

            //Medians

            medianMaxDiff = TechnicalAnalysis.Median(maxDiffs);
            medianMaxRangeSteps = medianMaxDiff / IncrementPrice;
            if (medianMaxRangeSteps > 1)
            {
                medianStepFillRatio = averageCompleteFills / (((medianMaxRangeSteps * medianMaxRangeSteps) - medianMaxRangeSteps) / 2);
            }
            else
            {
                medianStepFillRatio = 0;
            }

            //Calculate variance

            decimal sumSquareds = 0;
            decimal sumDiffsMean = 0;
            decimal sumDiffsMedian = 0;
            decimal sumDiffsCapped = 0;

            foreach (SingleResult sr in SingleResults)
            {
                sumSquareds += Convert.ToDecimal(Math.Pow(Convert.ToDouble(Math.Max(Math.Abs(sr.startHighDiff * 100), Math.Abs(sr.startLowDiff * 100)) - (averageMaxDiff * 100)), 2));
                sumDiffsMean += Convert.ToDecimal(Math.Abs(Math.Max(Math.Abs(sr.startHighDiff), Math.Abs(sr.startLowDiff)) - averageMaxDiff));
                sumDiffsMedian += Convert.ToDecimal(Math.Abs(Math.Max(Math.Abs(sr.startHighDiff), Math.Abs(sr.startLowDiff)) - medianMaxDiff));
                sumDiffsCapped += Convert.ToDecimal(Math.Abs(Math.Min(Math.Abs(Math.Max(Math.Abs(sr.startHighDiff), Math.Abs(sr.startLowDiff))), (IncrementPrice * Convert.ToDecimal(7.1))) - medianMaxDiff));
            }

            if (SingleResults.Count > 1) { 
            varianceSquares = sumSquareds / (SingleResults.Count - 1) / 100;
            }
            varianceSquaresNormalized = varianceSquares / (averageMaxDiff * 100 * averageMaxDiff / 100);

            variance = sumDiffsMean / SingleResults.Count;

            varianceNormalized = variance / medianMaxDiff;

            varianceStopCapped = sumDiffsCapped / SingleResults.Count;

            varianceStopCappedNormalized = varianceStopCapped / medianMaxDiff;

            medianVariance = sumDiffsMedian / SingleResults.Count;

            medianVarianceNormalized = medianVariance / medianMaxDiff;

            //Average/Median Win Module
            /*
             *  public decimal highestUnrealized;
                public int daysAboveAverageWin;
                public int daysGaveBackAverageWin;
                public decimal medianWin;
             *
             * */

            foreach (SingleResult sr in SingleResults)
            {
                if (sr.maxUnrealized > highestMaxUnrealized) highestMaxUnrealized = sr.maxUnrealized;
                if (sr.TotalPL > AvgWin)
                {
                    daysAboveAverageWin++;            
                }
                if (sr.maxUnrealized > AvgWin && sr.TotalPL < AvgWin) daysGaveBackAverageWin++;
            }
            if (wins.Count >= 1)
            {
                medianWin = TechnicalAnalysis.Median(wins);
            }
            else medianWin = 0;
            if (maxUnrealizeds.Count >= 1)
            {
                medianMaxUnrealized = TechnicalAnalysis.Median(maxUnrealizeds);
            }


        }

        public int CompareTo(SymbolResult other)
        {
            return TotalPL.CompareTo(other.TotalPL);
        }

    }
}
