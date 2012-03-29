using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using MySql.Data.MySqlClient;
using System.Threading;
using System.ComponentModel;
using System.Security.Principal;

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
            if (Menu.getAccessLvl()) { hddMonitor.Start(); }

            bool exit = false;
            string input = "";
            while (!exit)
            {
                Console.Write("PhDD >");

                input = Console.ReadLine();

                switch (input.ToLower())
                {
                    case "exit":
                        {
                            hddMonitor.Abort();
                            exit = true;
                            break;
                        }
                    case "help": { menu.displayMenuHelp(); break; }
                    case "clear": { Console.Clear(); break; }
                }
            }
        }

        /// background worker thread for checking the hdd stats every 10 mins
        public void Run()
        {

            FileHandler file = new FileHandler();
            HDDHandler hdd = new HDDHandler();
            
            //initialise key
            file.init();

            while (true)
            {
                List<string> smartData = new List<string>();
                try
                {
                    //update SMART data every 10 mins
                    smartData = hdd.getAttributes();
                    
                    int temp;
                    bool conversion = int.TryParse(smartData[1].ToString(), out temp);

                    if (conversion)
                    {
                        // 1)model, 2)Temp, 3)Load/Unload, 4)KEY
                        DBHandler.insertData(smartData[0].Trim(),
                            temp, smartData[2], file.getKey());
                    }
                    else { Console.WriteLine("[-]\tConversion failed, HDD vendor specs unrecognised. [-]"); }

                }
                catch (Exception err) { Console.WriteLine("[-]\tException in main thread: "
                    + err.StackTrace + " [-]"); }

                Console.WriteLine("\n__________________________________________________________________"
                    + "\n\tUpdated HDD Stats at {0}", DateTime.Now); 
                hdd.displayData(smartData);

                Thread.Sleep(10000*6*10);
            }
        }


        /// Menu
        public void displayMenuHelp()
        {
            Console.WriteLine("\n\tCommand line options:\n");

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write("\tclear");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("\t-Cleans the screen");

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write("\thelp");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("\t-Displayes this guide");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("\texit");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("\t-Shuts down PhDD");

            Console.WriteLine("\n\t*commands are not case sensitive.\n");
        }

        ///checks permissions level
        static bool getAccessLvl()
        {
            bool admin = false;

            if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                admin = true;
            }
            else
                Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine(new WindowsPrincipal(
                WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator)
                ? "Running as Administrator" : "No admin rights received, restart application as administartor!");
            Console.WriteLine("\n\n");
            Console.ForegroundColor = ConsoleColor.Gray;

            return admin;
        }
    }
}
