using System;
using System.Collections.Generic;
using System.Text;

namespace Backtester
{
    public class TestResult
    {
        public List<SymbolResult> SymbolResults = new List<SymbolResult>();
        public DateTime startDate;
        public DateTime endDate;
        public int BuyFills;
        public int SellFills;
        public decimal IncrementPL;
        public decimal PriceMovePL;
        public decimal TotalPL;
        public decimal ProfitMargin;


        public void CalculateTotals()
        {
             foreach(SymbolResult sr in SymbolResults)
            {
                BuyFills += sr.BuyFills;
                SellFills += sr.SellFills;
                IncrementPL += sr.IncrementPL;
                PriceMovePL += sr.PriceMovePL;
                TotalPL += sr.TotalPL;
            }
            if (IncrementPL > 0) ProfitMargin = (TotalPL / IncrementPL) * 100;
            else ProfitMargin = Convert.ToDecimal(-9999.99);
        }
    }
}
