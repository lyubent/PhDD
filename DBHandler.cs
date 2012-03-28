using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace WMITest
{
    static class DBHandler
    {
        ///Inserts data to db
        public static bool insertData(string serial, int temp, string loadCycle, string key)
        {
            string sqlStatement = "INSERT INTO `yahoohack`.`HDDInformation` "
                + "(`SerialNumber`, `Temp`, `LoadCycle`, `Key`) " 
                + "VALUES ('" 
                + serial + "','"
                + temp + "','" 
                + loadCycle + "','" 
                + key + "');";
            
            const string connString = "SERVER=yahoohack.db.8221309.hostedresource.com;" +
                                        "DATABASE=yahoohack;" +
                                        "UID=yahoohack;" +
                                        "PASSWORD=Q[nQy6adVdU;";

            MySqlConnection conn = new MySqlConnection(connString);
            MySqlCommand cmd = conn.CreateCommand();

            cmd.CommandText = sqlStatement;

            try { conn.Open(); cmd.ExecuteNonQuery(); }
            catch (Exception err) { Console.WriteLine("Exception Occured: " + err.Message); return false; }
            finally{ conn.Close(); }

            return true;
        }

        
        /// Gets data from db
        public static bool retreiveData(string sqlStatement)
        {
            const string connString = "SERVER=yahoohack.db.8221309.hostedresource.com;" +
                                        "DATABASE=yahoohack;" +
                                        "UID=yahoohack;" +
                                        "PASSWORD=Q[nQy6adVdU;";

            MySqlConnection conn = new MySqlConnection(connString);
            MySqlCommand cmd = conn.CreateCommand();
            MySqlDataReader r = null;

            cmd.CommandText = sqlStatement;

            try
            {
                conn.Open();
                r = cmd.ExecuteReader();

                while (r.Read())
                    Console.WriteLine(r["Username"].ToString());
            }
            catch (Exception err) { Console.WriteLine("Exception Occured: " + err.Message); return false; }
            finally{ conn.Close(); }

            return true;
        }

    }
}
