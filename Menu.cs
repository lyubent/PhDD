using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using MySql.Data.MySqlClient;
using System.Threading;
using System.ComponentModel;

namespace WMITest
{
    class Menu
    {

        public Menu() {}

        public static void Main(string[] args)
        {
            Menu menu = new Menu();
            Console.Title = " PhDD";
            
            Thread hddMonitor = new Thread(new ThreadStart(menu.Run));
            hddMonitor.Start();
            
            while (true)
            {
                Console.Write("PhDD >");
                Console.ReadLine();
            }
        }

        public void Run()
        {

            FileHandler file = new FileHandler();
            HDDHandler hdd = new HDDHandler();
            
            while (true)
            {
                try
                {
                    //initialise key
                    file.init();

                    List<string> smartData;
                    //update SMART data every (X)mins
                    Console.WriteLine("Pary");
                    smartData = hdd.getAttributes();
                    Console.WriteLine("Pary2");

                    hdd.displayData(smartData);
                    
                    int temp;
                    bool conversion = int.TryParse(smartData[1].ToString(), out temp);

                    if (conversion)
                    {
                        DBHandler.insertData(smartData[0].Trim(),
                            temp, smartData[2], file.getKey());
                    }
                    else { Console.WriteLine("Conversion failed, HDD vendor specs unrecognised."); }

                    // serial       Temp            Load/Unload     KEY
                    //smartData[0], smartData[1], smartData[2], file.getKey();

                    /// !<--- debug ---> debug --->
                    hdd.displayData(smartData);
                }
                catch (Exception err) { Console.WriteLine("Exception in main thread: " + err.StackTrace); }
            
                Console.Write("Updated HDD Stats at" + DateTime.Now);
                Thread.Sleep(10000*6*10);
            }
        }
    }
}
