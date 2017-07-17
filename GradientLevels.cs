using System;
using System.Collections.Generic;

using System.Text;
using System.Diagnostics;

//using System.Threading.Tasks;

namespace Backtester
{
    public class GradientLevels
    {
        public int furthestFillIndex;
        public int lastFillIndex;
        public int baseLot;
        public int zeroIndex;
        public bool isRezero;

        public int middleIndex = 6;

        public decimal incrementPrice;

        public List<List<object>> Levels = new List<List<object>>();
        public KeyValuePair<decimal, int> priceLevelIndex = new KeyValuePair<decimal, int>();

        public void Initialize(decimal midPrice, decimal _incrementPrice)
        {

            incrementPrice = _incrementPrice;

            Levels.Clear();

            //Levels.Add(new List<object> { midPrice - (7 * incrementPrice), baseLot * 14, "B" });  //Below - 7
            Levels.Add(new List<object> { midPrice - (6 * incrementPrice), baseLot * 10, "B" });  //Below - 6
            Levels.Add(new List<object> { midPrice - (5 * incrementPrice), baseLot * 8, "B" });   //Below - 5
            Levels.Add(new List<object> { midPrice - (4 * incrementPrice), baseLot * 6, "B" });   //Below - 4
            Levels.Add(new List<object> { midPrice - (3 * incrementPrice), baseLot * 4, "B" });   //Below - 3
            Levels.Add(new List<object> { midPrice - (2 * incrementPrice), baseLot * 2, "B" });   //Below - 2
            Levels.Add(new List<object> { midPrice - (1 * incrementPrice), baseLot * 1, "B" });   //Below - 1
            Levels.Add(new List<object> { midPrice - (0 * incrementPrice), baseLot * 0, "M" });   //0 level
            Levels.Add(new List<object> { midPrice + (1 * incrementPrice), baseLot * 1, "S" });   //Above + 1
            Levels.Add(new List<object> { midPrice + (2 * incrementPrice), baseLot * 2, "S" });   //Above + 2
            Levels.Add(new List<object> { midPrice + (3 * incrementPrice), baseLot * 4, "S" });   //Above + 3
            Levels.Add(new List<object> { midPrice + (4 * incrementPrice), baseLot * 6, "S" });   //Above + 4
            Levels.Add(new List<object> { midPrice + (5 * incrementPrice), baseLot * 8, "S" });   //Above + 5
            Levels.Add(new List<object> { midPrice + (6 * incrementPrice), baseLot * 10, "S" });  //Above + 6
                                                                                                  //Levels.Add(new List<object> { midPrice + (7 * incrementPrice), baseLot * 14, "S" });  //Above + 7


        }

        public int ReCalculate(int fillIndex)   //0 = no error, 1 = index error
        {
            int newFurthestFillIndex;
            isRezero = false;


            if (furthestFillIndex >= middleIndex && fillIndex > furthestFillIndex)  //new high fill
            {
                furthestFillIndex = fillIndex;
                //Update levels below price

                if (fillIndex == middleIndex + 1)  //only update 8
                {
                    Levels[middleIndex + 1][1] = 0; Levels[middleIndex + 1][2] = "M";
                    Levels[middleIndex][1] = baseLot; Levels[middleIndex][2] = "B";
                }

                else if (fillIndex == middleIndex + 2) //update 8, 9
                {
                    Levels[middleIndex + 2][1] = 0; Levels[middleIndex + 2][2] = "M";
                    Levels[middleIndex + 1][1] = baseLot; Levels[middleIndex + 1][2] = "B";
                    Levels[middleIndex][1] = baseLot * 2; Levels[middleIndex][2] = "B";
                }
                else if (fillIndex == middleIndex + 3)
                {
                    Levels[middleIndex + 3][1] = 0; Levels[middleIndex + 3][2] = "M";
                    Levels[middleIndex + 2][1] = baseLot * 2; Levels[middleIndex + 2][2] = "B";
                    Levels[middleIndex + 1][1] = baseLot * 2; Levels[middleIndex + 1][2] = "B";
                    Levels[middleIndex][1] = baseLot * 3; Levels[middleIndex][2] = "B";    // 0
                }
                else if (fillIndex == middleIndex + 4)
                {
                    Levels[middleIndex + 4][1] = 0; Levels[middleIndex + 4][2] = "M";
                    Levels[middleIndex + 3][1] = baseLot * 3; Levels[middleIndex + 3][2] = "B";
                    Levels[middleIndex + 2][1] = baseLot * 3; Levels[middleIndex + 2][2] = "B";
                    Levels[middleIndex + 1][1] = baseLot * 3; Levels[middleIndex + 1][2] = "B";
                    Levels[middleIndex][1] = baseLot * 4; Levels[middleIndex][2] = "B";    // 0
                }
                else if (fillIndex == middleIndex + 5)
                {
                    Levels[middleIndex + 5][1] = 0; Levels[middleIndex + 5][2] = "M";
                    Levels[middleIndex + 4][1] = baseLot * 4; Levels[middleIndex + 4][2] = "B";
                    Levels[middleIndex + 3][1] = baseLot * 4; Levels[middleIndex + 3][2] = "B";
                    Levels[middleIndex + 2][1] = baseLot * 4; Levels[middleIndex + 2][2] = "B";
                    Levels[middleIndex + 1][1] = baseLot * 9; Levels[middleIndex + 1][2] = "B";    //new 0
                    Levels[middleIndex][1] = baseLot * 1; Levels[middleIndex][2] = "B";

                    zeroIndex = middleIndex + 1;
                }
                else if (fillIndex == middleIndex + 6)
                {
                    Levels[middleIndex + 6][1] = 0; Levels[middleIndex + 6][2] = "M";
                    Levels[middleIndex + 5][1] = baseLot * 5; Levels[middleIndex + 5][2] = "B";
                    Levels[middleIndex + 4][1] = baseLot * 5; Levels[middleIndex + 4][2] = "B";
                    Levels[middleIndex + 3][1] = baseLot * 5; Levels[middleIndex + 3][2] = "B";
                    Levels[middleIndex + 2][1] = baseLot * 16; Levels[middleIndex + 2][2] = "B";   //new 0
                    Levels[middleIndex + 1][1] = baseLot * 1; Levels[middleIndex + 1][2] = "B";

                    zeroIndex = middleIndex + 2;
                }
                /*
                else if (fillIndex == 14)
                {
                    Levels[14][1] = 0; Levels[14][2] = "M";
                    Levels[13][1] = baseLot * 6; Levels[13][2] = "B";
                    Levels[12][1] = baseLot * 6; Levels[12][2] = "B";
                    Levels[11][1] = baseLot * 6; Levels[11][2] = "B";
                    Levels[10][1] = baseLot * 27; Levels[10][2] = "B";   //new 0
                    Levels[9][1] = baseLot * 1; Levels[9][2] = "B";

                    zeroIndex = 10;
                }
                */

            }

            else if (furthestFillIndex <= middleIndex && fillIndex < furthestFillIndex) //new low fill
            {

                furthestFillIndex = fillIndex;

                if (fillIndex == middleIndex - 1)  //only update 8
                {
                    Levels[middleIndex - 1][1] = 0; Levels[middleIndex - 1][2] = "M";
                    Levels[middleIndex][1] = baseLot; Levels[middleIndex][2] = "S";
                }
                else if (fillIndex == middleIndex - 2)  //only update 8
                {
                    Levels[middleIndex - 2][1] = 0; Levels[middleIndex - 2][2] = "M";
                    Levels[middleIndex - 1][1] = baseLot; Levels[middleIndex - 1][2] = "S";
                    Levels[middleIndex][1] = baseLot * 2; Levels[middleIndex][2] = "S";
                }
                else if (fillIndex == middleIndex - 3)  //only update 8
                {
                    Levels[middleIndex - 3][1] = 0; Levels[middleIndex - 3][2] = "M";
                    Levels[middleIndex - 2][1] = baseLot * 2; Levels[middleIndex - 2][2] = "S";
                    Levels[middleIndex - 1][1] = baseLot * 2; Levels[middleIndex - 1][2] = "S";
                    Levels[middleIndex][1] = baseLot * 3; Levels[middleIndex][2] = "S";
                }
                else if (fillIndex == middleIndex - 4)  //only update 8
                {
                    Levels[middleIndex - 4][1] = 0; Levels[middleIndex - 4][2] = "M";
                    Levels[middleIndex - 3][1] = baseLot * 3; Levels[middleIndex - 3][2] = "S";
                    Levels[middleIndex - 2][1] = baseLot * 3; Levels[middleIndex - 2][2] = "S";
                    Levels[middleIndex - 1][1] = baseLot * 3; Levels[middleIndex - 1][2] = "S";
                    Levels[middleIndex][1] = baseLot * 4; Levels[middleIndex][2] = "S";
                }
                else if (fillIndex == middleIndex - 5)  //only update 8
                {
                    Levels[middleIndex - 5][1] = 0; Levels[middleIndex - 5][2] = "M";
                    Levels[middleIndex - 4][1] = baseLot * 4; Levels[middleIndex - 4][2] = "S";
                    Levels[middleIndex - 3][1] = baseLot * 4; Levels[middleIndex - 3][2] = "S";
                    Levels[middleIndex - 2][1] = baseLot * 4; Levels[middleIndex - 2][2] = "S";
                    Levels[middleIndex - 1][1] = baseLot * 9; Levels[middleIndex - 1][2] = "S";   // new 0
                    Levels[middleIndex][1] = baseLot * 1; Levels[middleIndex][2] = "S";

                    zeroIndex = middleIndex - 1;
                }
                else if (fillIndex == middleIndex - 6)  //only update 8
                {
                    Levels[middleIndex - 6][1] = 0; Levels[middleIndex - 6][2] = "M";
                    Levels[middleIndex - 5][1] = baseLot * 5; Levels[middleIndex - 5][2] = "S";
                    Levels[middleIndex - 4][1] = baseLot * 5; Levels[middleIndex - 4][2] = "S";
                    Levels[middleIndex - 3][1] = baseLot * 5; Levels[middleIndex - 3][2] = "S";
                    Levels[middleIndex - 2][1] = baseLot * 16; Levels[middleIndex - 2][2] = "S";    // new 0
                    Levels[middleIndex - 1][1] = baseLot * 1; Levels[middleIndex - 1][2] = "S";

                    zeroIndex = middleIndex - 2;
                }
                /*
                else if (fillIndex == 0)  //only update 8
                {
                    Levels[0][1] = 0; Levels[0][2] = "M";
                    Levels[1][1] = baseLot * 6; Levels[1][2] = "S";
                    Levels[2][1] = baseLot * 6; Levels[2][2] = "S";
                    Levels[3][1] = baseLot * 6; Levels[3][2] = "S";
                    Levels[4][1] = baseLot * 27; Levels[4][2] = "S";   // new 0
                    Levels[5][1] = baseLot * 1; Levels[5][2] = "S";

                    zeroIndex = 4;
                }
                */
            }

            else  //not a new high or low fill
            {

                if (fillIndex < lastFillIndex)  //Filled Down
                {
                    //Check if 0
                    if (fillIndex == zeroIndex)
                    {
                        isRezero = true;
                        reZero(Convert.ToDecimal(Levels[fillIndex][0]));
                    }

                    //If not 0
                    else
                    {
                        Levels[fillIndex + 1][1] = Levels[fillIndex][1]; Levels[fillIndex + 1][2] = "S";
                        Levels[fillIndex][1] = 0; Levels[fillIndex][2] = "M";
                    }


                }

                else if (fillIndex > lastFillIndex) //Filled Up
                {
                    //Check if 0
                    if (fillIndex == zeroIndex)
                    {
                        isRezero = true;
                        reZero(Convert.ToDecimal(Levels[fillIndex][0]));
                    }

                    //If not 0
                    else
                    {
                        Levels[fillIndex - 1][1] = Levels[fillIndex][1]; Levels[fillIndex - 1][2] = "B";
                        Levels[fillIndex][1] = 0; Levels[fillIndex][2] = "M";
                    }
                }

                else
                {
                    Debug.WriteLine("Fill Error");

                    //Return something suggesting error, shut down

                    return 1;

                }
            }

            lastFillIndex = fillIndex;

            return 0;

        }

        public void reZero(decimal midPrice)
        {
            Debug.WriteLine("rezeroing");

            Levels.Clear();

            Initialize(midPrice, incrementPrice);

            furthestFillIndex = middleIndex;

            zeroIndex = middleIndex;


        }




    }
}
