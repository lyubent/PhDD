using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace WMITest
{
    class FileHandler
    {
        // Global variable(s)
        Random random = new Random();
        private string keyGlob;

        /// Constructor
        public FileHandler() { }

        /// Mutators and Accesors
        public string getKey() { return keyGlob; }

        /// Chooses wheter to read from file for a key, or to generate a new key and file
        public void init()
        {
            if (fileExists("key.xml"))
            {
                keyGlob = readKeyFromFile();
            }
            else
            {
                keyGlob = generateKey();
                createKeyFile(keyGlob);
            }

            Console.WriteLine("Your key: " + keyGlob);
        }

        /// Searches for a file in the local directory
        private bool fileExists(string file)
        {
            string[] data;

            Console.WriteLine(Directory.GetCurrentDirectory().ToString());
            data = Directory.GetFiles(Directory.GetCurrentDirectory().ToString(), file);

            /*// displays found file(s)
            foreach (string fileFound in data){Console.WriteLine("Found {0}", fileFound);}*/

            // if file exists data's length will be 1 thus MORETHAN 0 so this will return true
            return !(data.Length == 0);
        }

        /// Writes to a file
        private void createKeyFile(string key)
        {
            string[] data = new string[6];
            data[0] = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>";
            data[1] = "<SystemInfo>";
            data[2] = "  <Details>";
            data[3] = "    <Key>" + key + "</Key>";
            data[4] = "  </Details>";
            data[5] = "</SystemInfo>";

            try
            {
                System.IO.StreamWriter file = new System.IO.StreamWriter("key.xml");
                foreach (string line in data) { file.WriteLine(line); }

                file.Flush(); file.Close();
            }
            catch (Exception err) { Console.WriteLine("Exception generating KEY: " + err.Message); }
        }

        /// Get key from xml file 
        private string readKeyFromFile()
        {
            TextReader tr = new StreamReader("key.xml");
            string keyLine = "";

            try
            {
                for (int i = 0; i < 3; i++) /*Console.WriteLine(*/tr.ReadLine()/*)*/;

                keyLine = tr.ReadLine();

                char[] splitBy = new char[2];
                splitBy[0] = '>';
                splitBy[1] = '<';

                string[] key = keyLine.Split(splitBy);

                return key[2].Trim();
            }
            catch (Exception err) { Console.WriteLine("Exception reading KEY file: " + err.Message); return null; }
        }

        /// Generates a key for a user based on their CPU ticks and a random number
        private string generateKey(){ return DateTime.Now.Ticks.ToString() + random.Next(1000).ToString(); }
    }
}
