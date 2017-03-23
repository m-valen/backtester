////////////////////////////////////////////////////
//// SampleApp5.cs                              ////
//// Sample App for C# NxCore API Wrapper       ////
//// Author: Jeffrey Donovan                    ////
//// Date: 12-22-09                             ////
////////////////////////////////////////////////////
//// Demonstrates:                              ////
//// Starting ProcessTape from a thread         ////
//// Handling NxCore Trade and Quote Messages   ////
////////////////////////////////////////////////////
//// To Read as Written:                        ////
//// Tab Size: 4  Indent Size: 2, Keep Tabs     ////
////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Windows.Forms;

// Use the NxCoreAPI namespace
using NxCoreAPI;


namespace Backtester
{

    class Program
    {
        // Main Entry Point for app.	
        //--------------------------
        [STAThread]

        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

        }


    }
}
