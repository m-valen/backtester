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
        public int HardStop;
        //
        public int BuyFills;
        public int SellFills;
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


        public void CalculateTotals()
        {
            List<decimal> maxUnrealizeds = new List<decimal>();
            List<decimal> minUnrealizeds = new List<decimal>();
            List<decimal> wins = new List<decimal>();
            List<decimal> losses = new List<decimal>();

            foreach(SingleResult sr in SingleResults)
            {
                BuyFills += sr.BuyFills;
                SellFills += sr.SellFills;
                IncrementPL += sr.IncrementPL;
                PriceMovePL += sr.PriceMovePL;
                TotalPL += sr.TotalPL;
                maxUnrealizeds.Add(sr.maxUnrealized);
                minUnrealizeds.Add(sr.minUnrealized);
                if (sr.TotalPL > 0) wins.Add(sr.TotalPL);
                else if (sr.TotalPL < 0) losses.Add(sr.TotalPL);
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
            
        }

        public int CompareTo(SymbolResult other)
        {
            return TotalPL.CompareTo(other.TotalPL);
        }

    }
}
