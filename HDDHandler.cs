using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Management;

namespace WMITest
{
    class HDDHandler
    {
        public HDDHandler()
        { }

        // Struct for required SMART data bytes 
        [StructLayout(LayoutKind.Sequential)] 
        public struct Attribute
        {
            public byte AttributeID;
            public ushort Flags;
            public byte Value;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] VendorData;
        }

        /// Get data from HDD S.M.A.R.T. based on byte Attribute
        private string getSMARTAttr(byte Attr)
        {
            string Value = "null";

            try 
            {
                Attribute AtributeInfo;
                ManagementScope Scope = new ManagementScope(String.Format("\\\\{0}\\root\\WMI", "localhost"), null);
                Scope.Connect();
                ObjectQuery Query = new ObjectQuery("SELECT VendorSpecific FROM MSStorageDriver_ATAPISmartData");
                ManagementObjectSearcher Searcher = new ManagementObjectSearcher(Scope, Query);

                byte SMARTAttribute = Attr; //0xC1
                int Delta = 12;
                
                foreach (ManagementObject WmiObject in Searcher.Get())
                {
                    byte[] VendorSpecific = (byte[])WmiObject["VendorSpecific"];
                    for (int offset = 2; offset < VendorSpecific.Length; )
                    {
                        if (VendorSpecific[offset] == SMARTAttribute)
                        {

                            IntPtr buffer = IntPtr.Zero;
                            try
                            {
                                buffer = Marshal.AllocHGlobal(Delta);
                                Marshal.Copy(VendorSpecific, offset, buffer, Delta);
                                AtributeInfo = (Attribute)Marshal.PtrToStructure(buffer, typeof(Attribute));
                                //Console.WriteLine("AttributeID {0}", AtributeInfo.AttributeID);
                                //Console.WriteLine("Flags {0}", AtributeInfo.Flags);
                                //Console.WriteLine("Value {0}", AtributeInfo.Value);
                                //Console.WriteLine("Value {0}", BitConverter.ToString(AtributeInfo.VendorData));
                                //Console.WriteLine("Data {0}", BitConverter.ToInt32(AtributeInfo.VendorData, 0));

                                Value = BitConverter.ToInt32(AtributeInfo.VendorData, 0).ToString();
                            }
                            finally
                            {
                                if (buffer != IntPtr.Zero)
                                {
                                    Marshal.FreeHGlobal(buffer);
                                }
                            }
                        }
                        offset += Delta;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("[-]\tException {0} Trace {1} + \n[-]", e.Message, e.StackTrace));
            }

            return Value;
        }

        // Uses WMI to get the HDD's serial number
        private string getSerial()
        {
            string serial = "";
            ManagementObjectSearcher ms =
                new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
            
            foreach (ManagementObject mo in ms.Get())
            {
                serial =  mo["Model"].ToString();
            }

            return serial;
        }

        // Returns a collection of the required attributes
        public List<string> getAttributes()
        {
            List<string> data = new List<string>();

            try
            {
                data.Add(getSerial());          // Serial#
                data.Add(getTemp());   // Temp
                data.Add(getSMARTAttr(0xC1).ToString());   // Load/Unload cycle count
                return data;
            }
            catch (Exception err) { Console.WriteLine("[-]\tException getting SMART " 
                + err.Message + " [-]"); return data; }
        }

        static string getTemp()
        {
            try
            {
                string temp = "0";
                byte TEMPERATURE_ATTRIBUTE = 194;

                ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"\root\WMI", "SELECT * FROM MSStorageDriver_ATAPISmartData");
                //loop through all the hard disks
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    byte[] arrVendorSpecific = (byte[])queryObj.GetPropertyValue("VendorSpecific");

                    int tempIndex = Array.IndexOf(arrVendorSpecific, TEMPERATURE_ATTRIBUTE);
                    temp =  arrVendorSpecific[tempIndex + 5].ToString();
                }
                return temp;
            }
            catch (Exception err) { Console.WriteLine(err.Message); return "0"; }
        }

        public void displayData(List<string> data)
        {
            Console.WriteLine("\tModel: " + data[0].Trim());
            Console.Write("\tHDD Temp: "); formatTempColour(data[1].Trim());
            Console.Write("\tHDD Load/Unload Cycles: "); formatCycleColour(data[2].Trim());

            Console.Write("__________________________________________________________________\nPhDD >");
        }

        private void formatTempColour(string data)
        {
            int temp;
            int.TryParse(data, out temp);

            if (temp < 33)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(temp);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else if (temp > 33 && temp < 43)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(temp);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else if (temp > 43)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(temp);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        private void formatCycleColour(string data)
        {
            int loadCycleCount;
            int.TryParse(data, out loadCycleCount);

            if (loadCycleCount < 150000)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(loadCycleCount);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else if (loadCycleCount > 150000 && loadCycleCount < 300000)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(loadCycleCount);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else if (loadCycleCount > 300000)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(loadCycleCount);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
    }
}
