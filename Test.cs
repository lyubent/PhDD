using System;
using System.Management;
using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using System.Runtime.InteropServices;
using MySql.Data.MySqlClient;

namespace WMITest
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary> 
    class clsMain
    {
        /*static void Main(string[] args)
        {
            while (true)
            {
                getAccessLvl();
                GetWMIStats();
                //doStuff();   //--------------currently dabest
                //doStuff2();
                //writeSpeedTest();  ///YES gibes bytes per sec
                //getStuff();
                //winning();
                //getSMARTAttr();
                //MySqlDataReader result = DBHandler.insertData("SELECT * FROM `yahoohack`.`Users`");

                /*if (result == null) { Console.WriteLine("£!FAFASDFSDF"); }
                Console.WriteLine("________________________________\ndb Data:");
                while (result.Read())
                {
                    Console.WriteLine(result["Username"].ToString());
                }

                Console.ReadLine();
            }
        }*/




        static void getAccessLvl()
        {
            Console.WriteLine(new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator)
                ? "Running as Administrator" : "No admin rights received");
        }





        static void winning()
        {
            var searcher = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM MSStorageDriver_ATAPISmartData");

            foreach (ManagementObject queryObj in searcher.Get())
            {
                Console.WriteLine("-----------------------------------");
                Console.WriteLine("MSStorageDriver_ATAPISmartData instance");
                Console.WriteLine("-----------------------------------");

                var arrVendorSpecific = (byte[])queryObj.GetPropertyValue("VendorSpecific");

                // Create SMART data from 'vendor specific' array
                var d = new SmartData(arrVendorSpecific);
                foreach (var b in d.Attributes)
                {
                    Console.Write("{0} :{1} : ", b.AttributeType, b.Value);
                    foreach (byte vendorByte in b.VendorData)
                    {
                        Console.Write("{0:x} ", vendorByte);
                    }
                    Console.WriteLine();
                }
            }
        }





        static void GetWMIStats()
        {
            long mb = 1048576; //megabyte in # of bytes 1024x1024

            //Connection credentials to the remote computer - not needed if the logged in account has access
            ConnectionOptions oConn = new ConnectionOptions();
            //oConn.Username = "username";
            //oConn.Password = "password";
            System.Management.ManagementScope oMs = new System.Management.ManagementScope("\\\\localhost", oConn);

            //get Fixed disk stats
            System.Management.ObjectQuery oQuery = new System.Management.ObjectQuery("select FreeSpace,Size,Name from Win32_LogicalDisk where DriveType=3");
            ManagementObjectSearcher oSearcher = new ManagementObjectSearcher(oMs, oQuery);
            ManagementObjectCollection oReturnCollection = oSearcher.Get();

            //variables for numerical conversions
            double fs = 0;
            double us = 0;
            double tot = 0;
            double up = 0;
            double fp = 0;

            //for string formating args
            object[] oArgs = new object[2];
            Console.WriteLine("*******************************************");
            Console.WriteLine("Hard Disks");
            Console.WriteLine("*******************************************");


            byte TEMPERATURE_ATTRIBUTE = 194;
            //public List<string> GetDriveTemp()
            //{
            List<string> retval = new List<string>();
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM MSStorageDriver_ATAPISmartData");
                //loop through all the hard disks
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    byte[] arrVendorSpecific = (byte[])queryObj.GetPropertyValue("VendorSpecific");
                    //Find the temperature attribute
                    int tempIndex = Array.IndexOf(arrVendorSpecific, TEMPERATURE_ATTRIBUTE);
                    retval.Add(arrVendorSpecific[tempIndex + 5].ToString());
                    Console.WriteLine(arrVendorSpecific[tempIndex + 5].ToString());
                    Console.WriteLine(arrVendorSpecific[tempIndex].ToString());
                }
            }
            catch (ManagementException err)
            {
                Console.WriteLine("An error occurred while querying for WMI data: " + err.Message);
            }
            //  return retval;



            //loop through found drives and write out info
            foreach (ManagementObject oReturn in oReturnCollection)
            {
                // Disk name
                Console.WriteLine("Name : " + oReturn["Name"].ToString());

                //Free space in MB
                fs = Convert.ToInt64(oReturn["FreeSpace"]) / mb;

                //Used space in MB
                us = (Convert.ToInt64(oReturn["Size"]) - Convert.ToInt64(oReturn["FreeSpace"])) / mb;

                //Total space in MB
                tot = Convert.ToInt64(oReturn["Size"]) / mb;

                //used percentage
                up = us / tot * 100;

                //free percentage
                fp = fs / tot * 100;

                //used space args
                oArgs[0] = (object)us;
                oArgs[1] = (object)up;

                //write out used space stats
                Console.WriteLine("Used: {0:#,###.##} MB ({1:###.##})%", oArgs);

                //free space args
                oArgs[0] = fs;
                oArgs[1] = fp;

                //write out free space stats
                Console.WriteLine("Free: {0:#,###.##} MB ({1:###.##})%", oArgs);
                Console.WriteLine("Size :  {0:#,###.##} MB", tot);
                Console.WriteLine("\n");
            }


            // Get process info including a method to return the user who is running it
            oQuery = new System.Management.ObjectQuery("select * from Win32_Process");
            oSearcher = new ManagementObjectSearcher(oMs, oQuery);
            oReturnCollection = oSearcher.Get();

            Console.WriteLine("____________");
            Console.WriteLine("USING WMI DATA:");

            System.Management.ManagementObjectSearcher ms =
                new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
            foreach (ManagementObject mo in ms.Get())
            {
                System.Console.Write(mo["SerialNumber"]);
            }

            Console.WriteLine("\n_________\n2Using .NET functions");
            ManagementObjectSearcher mosDisks = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
            // Loop through each object (disk) retrieved by WMI
            foreach (ManagementObject moDisk in mosDisks.Get())
            {
                // Add the HDD to the list (use the Model field as the item's caption)
                Console.WriteLine(moDisk["Model"].ToString());
            }

            Console.WriteLine("\n_________\nUsing .NET and S.M.A.R.T. functions");
            ManagementClass driveClass = new ManagementClass("Win32_DiskDrive");
            ManagementObjectCollection drives = driveClass.GetInstances();
            foreach (ManagementObject drive in drives)
            {
                foreach (PropertyData property in drive.Properties)
                {
                    Console.WriteLine("Property: {0}, Value: {1}", property.Name, property.Value);
                }
                Console.WriteLine();
            }

        }




        static void getStuff()
        {
            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\WMI",
                    "SELECT * FROM MSStorageDriver_ATAPISmartData");
                int i = 0;
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    Console.WriteLine("-----------------------------------");
                    Console.WriteLine("MSStorageDriver_ATAPISmartData instance");
                    Console.WriteLine("-----------------------------------");

                    if (queryObj["VendorSpecific"] == null)
                        Console.WriteLine("VendorSpecific: {0}", queryObj["VendorSpecific"]);
                    else
                    {
                        Byte[] arrVendorSpecific = (Byte[])(queryObj["VendorSpecific"]);
                        foreach (Byte arrValue in arrVendorSpecific)
                        {
                            i++;
                            if (i == 310)
                                break;
                            Console.WriteLine("VendorSpecific: {0}", arrValue);
                        }
                    }
                }
            }
            catch (ManagementException e)
            { Console.WriteLine("An error occurred while querying for WMI data: " + e.Message); }

        }




        static void doStuff()
        {
            int i = 0;
            try
            {

                byte TEMPERATURE_ATTRIBUTE = 194;
                
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"\root\WMI", "SELECT * FROM MSStorageDriver_ATAPISmartData");
                //loop through all the hard disks
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    byte[] arrVendorSpecific = (byte[])queryObj.GetPropertyValue("VendorSpecific");

                    int tempIndex = Array.IndexOf(arrVendorSpecific, TEMPERATURE_ATTRIBUTE);
                    Console.WriteLine("HDD TEMP: " + arrVendorSpecific[tempIndex + 5].ToString());

                    Console.WriteLine(arrVendorSpecific[tempIndex].ToString());
                }

            }
            catch (Exception err) { Console.WriteLine(err.Message); }
        }

        static void writeSpeedTest()
        {
            byte[] data = new byte[1024];

            string path = System.IO.Path.GetTempFileName();

            int bytesPerSecond = 0;

            using (System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Create))
            {
                System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

                watch.Start();

                for (int i = 0; i < 1024; i++)
                    fs.Write(data, 0, data.Length);

                fs.Flush();
                watch.Stop();

                bytesPerSecond = (int)((data.Length * 1024) / watch.Elapsed.TotalSeconds);
            }

            Console.WriteLine("BPS: " + bytesPerSecond);
            System.IO.File.Delete(path);
        }



        static void doStuff2()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\WMI",
                    "SELECT * FROM MSStorageDriver_ATAPISmartData");

                byte hddTempPointer = 194;
                int i = 0;
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    if (i == 23) Console.WriteLine("\n\n\n\nKJ");
                    if (queryObj["VendorSpecific"] != null)
                    {
                        byte[] arrVendorSpecific = (byte[])(queryObj["VendorSpecific"]);
                        string temp = arrVendorSpecific[hddTempPointer].ToString();
                        Console.WriteLine(temp);

                        i++;
                    }
                }
            }
            catch (Exception err) { Console.WriteLine(err.Message); }
        }

        //There is a utility called mgmtclassgen that ships with the .NET SDK that
        //will generate managed code for existing WMI classes. It also generates
        // datetime conversion routines like this one.
        //Thanks to Chetan Parmar and dotnet247.com for the help.
        static System.DateTime ToDateTime(string dmtfDate)
        {
            int year = System.DateTime.Now.Year;
            int month = 1;
            int day = 1;
            int hour = 0;
            int minute = 0;
            int second = 0;
            int millisec = 0;
            string dmtf = dmtfDate;
            string tempString = System.String.Empty;

            if (((System.String.Empty == dmtf) || (dmtf == null)))
            {
                return System.DateTime.MinValue;
            }

            if ((dmtf.Length != 25))
            {
                return System.DateTime.MinValue;
            }

            tempString = dmtf.Substring(0, 4);
            if (("****" != tempString))
            {
                year = System.Int32.Parse(tempString);
            }

            tempString = dmtf.Substring(4, 2);

            if (("**" != tempString))
            {
                month = System.Int32.Parse(tempString);
            }

            tempString = dmtf.Substring(6, 2);

            if (("**" != tempString))
            {
                day = System.Int32.Parse(tempString);
            }

            tempString = dmtf.Substring(8, 2);

            if (("**" != tempString))
            {
                hour = System.Int32.Parse(tempString);
            }

            tempString = dmtf.Substring(10, 2);

            if (("**" != tempString))
            {
                minute = System.Int32.Parse(tempString);
            }

            tempString = dmtf.Substring(12, 2);

            if (("**" != tempString))
            {
                second = System.Int32.Parse(tempString);
            }

            tempString = dmtf.Substring(15, 3);

            if (("***" != tempString))
            {
                millisec = System.Int32.Parse(tempString);
            }

            System.DateTime dateRet = new System.DateTime(year, month, day, hour, minute, second, millisec);

            return dateRet;
        }




        /*
         * //loop through each process - I limited it to first 6 so the DOS buffer would not overflow and cut off the disk stats
			int i=0;	
         foreach( ManagementObject oReturn in oReturnCollection ) 
			{
				//if(i==6)
					//break;
				i++;
				Console.WriteLine(oReturn["Name"].ToString().ToLower());
				//arg to send with method invoke to return user and domain - below is link to SDK doc on it
				//http://msdn.microsoft.com/library/default.asp?url=/library/en-us/wmisdk/wmi/getowner_method_in_class_win32_process.asp?frame=true
				string[] o = new String[2];				
				oReturn.InvokeMethod("GetOwner",(object[])o);
				//write out user info that was returned
				Console.WriteLine("User: " + o[1]+ "\\" + o[0]);
				Console.WriteLine("PID: " + oReturn["ProcessId"].ToString());

				//get priority
				if(oReturn["Priority"] != null)
					Console.WriteLine("Priority: " + oReturn["Priority"].ToString());

				//get creation date - need managed code function to convert date - 
				if(oReturn["CreationDate"] != null)
				{
					try
					{
						//get datetime string and convert
						string s = oReturn["CreationDate"].ToString();
						DateTime dc = ToDateTime(s);						
						//write out creation date
						Console.WriteLine("CreationDate: " + dc.AddTicks(-TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Ticks).ToLocalTime().ToString());
					}
					//just in case - I was getting a weird error on some entries
					catch(Exception err)
					{
						Console.WriteLine(err.Message);
					}
				}
				//this is the amount of memory used
				if(oReturn["WorkingSetSize"] != null)
				{
					long mem =  Convert.ToInt64(oReturn["WorkingSetSize"].ToString()) / 1024;
					Console.WriteLine("Mem Usage: {0:#,###.##}Kb",mem);
				}
				Console.WriteLine("");
			}
         */

        static void echo(string data)
        {
            Console.WriteLine(data);
        }

        public enum SmartAttributeType : byte
        {
            ReadErrorRate = 0x01,
            ThroughputPerformance = 0x02,
            SpinUpTime = 0x03,
            StartStopCount = 0x04,
            ReallocatedSectorsCount = 0x05,
            ReadChannelMargin = 0x06,
            SeekErrorRate = 0x07,
            SeekTimePerformance = 0x08,
            PowerOnHoursPOH = 0x09,
            SpinRetryCount = 0x0A,
            CalibrationRetryCount = 0x0B,
            PowerCycleCount = 0x0C,
            SoftReadErrorRate = 0x0D,
            SATADownshiftErrorCount = 0xB7,
            EndtoEnderror = 0xB8,
            HeadStability = 0xB9,
            InducedOpVibrationDetection = 0xBA,
            ReportedUncorrectableErrors = 0xBB,
            CommandTimeout = 0xBC,
            HighFlyWrites = 0xBD,
            AirflowTemperatureWDC = 0xBE,
            TemperatureDifferencefrom100 = 0xBE,
            GSenseErrorRate = 0xBF,
            PoweroffRetractCount = 0xC0,
            LoadCycleCount = 0xC1,
            Temperature = 0xC2,
            HardwareECCRecovered = 0xC3,
            ReallocationEventCount = 0xC4,
            CurrentPendingSectorCount = 0xC5,
            UncorrectableSectorCount = 0xC6,
            UltraDMACRCErrorCount = 0xC7,
            MultiZoneErrorRate = 0xC8,
            WriteErrorRateFujitsu = 0xC8,
            OffTrackSoftReadErrorRate = 0xC9,
            DataAddressMarkerrors = 0xCA,
            RunOutCancel = 0xCB,
            SoftECCCorrection = 0xCC,
            ThermalAsperityRateTAR = 0xCD,
            FlyingHeight = 0xCE,
            SpinHighCurrent = 0xCF,
            SpinBuzz = 0xD0,
            OfflineSeekPerformance = 0xD1,
            VibrationDuringWrite = 0xD3,
            ShockDuringWrite = 0xD4,
            DiskShift = 0xDC,
            GSenseErrorRateAlt = 0xDD,
            LoadedHours = 0xDE,
            LoadUnloadRetryCount = 0xDF,
            LoadFriction = 0xE0,
            LoadUnloadCycleCount = 0xE1,
            LoadInTime = 0xE2,
            TorqueAmplificationCount = 0xE3,
            PowerOffRetractCycle = 0xE4,
            GMRHeadAmplitude = 0xE6,
            DriveTemperature = 0xE7,
            HeadFlyingHours = 0xF0,
            TransferErrorRateFujitsu = 0xF0,
            TotalLBAsWritten = 0xF1,
            TotalLBAsRead = 0xF2,
            ReadErrorRetryRate = 0xFA,
            FreeFallProtection = 0xFE,
        }

        public class SmartData
        {
            readonly Dictionary<SmartAttributeType, SmartAttribute> attributes;
            readonly ushort structureVersion;

            public SmartData(byte[] arrVendorSpecific)
            {
                attributes = new Dictionary<SmartAttributeType, SmartAttribute>();
                for (int offset = 2; offset < arrVendorSpecific.Length; )
                {
                    var a = FromBytes<SmartAttribute>(arrVendorSpecific, ref offset, 12);
                    // Attribute values 0x00, 0xfe, 0xff are invalid
                    if (a.AttributeType != 0x00 && (byte)a.AttributeType != 0xfe && (byte)a.AttributeType != 0xff)
                    {
                        attributes[a.AttributeType] = a;
                    }
                }
                structureVersion = (ushort)(arrVendorSpecific[0] * 256 + arrVendorSpecific[1]);
            }

            public ushort StructureVersion
            {
                get
                {
                    return this.structureVersion;
                }
            }

            public SmartAttribute this[SmartAttributeType v]
            {
                get
                {
                    return this.attributes[v];
                }
            }

            public IEnumerable<SmartAttribute> Attributes
            {
                get
                {
                    return this.attributes.Values;
                }
            }

            static T FromBytes<T>(byte[] bytearray, ref int offset, int count)
            {
                IntPtr ptr = IntPtr.Zero;

                try
                {
                    ptr = Marshal.AllocHGlobal(count);
                    Marshal.Copy(bytearray, offset, ptr, count);
                    offset += count;
                    return (T)Marshal.PtrToStructure(ptr, typeof(T));
                }
                finally
                {
                    if (ptr != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(ptr);
                    }
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SmartAttribute
        {
            public SmartAttributeType AttributeType;
            public ushort Flags;
            public byte Value;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] VendorData;

            public bool Advisory
            {
                get
                {
                    return (Flags & 0x1) == 0x0; // Bit 0 unset?
                }
            }
            public bool FailureImminent
            {
                get
                {
                    return (Flags & 0x1) == 0x1; // Bit 0 set?
                }
            }
            public bool OnlineDataCollection
            {
                get
                {
                    return (Flags & 0x2) == 0x2; // Bit 0 set?
                }
            }

        }



        [StructLayout(LayoutKind.Sequential)]
        public struct Attribute
        {
            public byte AttributeID;
            public ushort Flags;
            public byte Value;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] VendorData;
        }

        static void getSMARTAttr()
        {
            //byte 

            try
            {
                Attribute AtributeInfo;
                ManagementScope Scope = new ManagementScope(String.Format("\\\\{0}\\root\\WMI", "localhost"), null);
                Scope.Connect();
                ObjectQuery Query = new ObjectQuery("SELECT VendorSpecific FROM MSStorageDriver_ATAPISmartData");
                ManagementObjectSearcher Searcher = new ManagementObjectSearcher(Scope, Query);
                byte LoadCycleCount = 0xC1;
                int Delta  = 12;
                foreach (ManagementObject WmiObject in Searcher.Get())
                {
                    byte[] VendorSpecific = (byte[])WmiObject["VendorSpecific"];
                    for (int offset = 2; offset < VendorSpecific.Length; )
                    {
                        if (VendorSpecific[offset] == LoadCycleCount)
                        {

                            IntPtr buffer = IntPtr.Zero;
                            try
                            {
                                buffer = Marshal.AllocHGlobal(Delta);
                                Marshal.Copy(VendorSpecific, offset, buffer, Delta);
                                AtributeInfo = (Attribute)Marshal.PtrToStructure(buffer, typeof(Attribute));
                                Console.WriteLine("AttributeID {0}", AtributeInfo.AttributeID);
                                Console.WriteLine("Flags {0}", AtributeInfo.Flags);
                                Console.WriteLine("Value {0}", AtributeInfo.Value);
                                //Console.WriteLine("Value {0}", BitConverter.ToString(AtributeInfo.VendorData));
                                Console.WriteLine("Data {0}", BitConverter.ToInt32(AtributeInfo.VendorData, 0));
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
                Console.WriteLine(String.Format("Exception {0} Trace {1}",e.Message,e.StackTrace));
            }
            Console.WriteLine("Press Enter to exit");
            Console.Read();
        }
    }
}

