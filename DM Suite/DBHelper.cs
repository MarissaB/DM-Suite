using DM_Suite.Services.LoggingServices;
using MetroLog;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using DM_Suite.Menu_Features;

namespace DM_Suite
{
    public class DBHelper
    {
        public static SqliteConnection databaseFile = new SqliteConnection("Filename=sqliteSample.db");

        public static List<MenuItem> SearchMenuItems(string keyword, List<string> types, string min, string max)
        {
            string commandText = "Select * from MENU_ITEMS where TYPE = @type and (NAME like @name OR DESCRIPTION like @name)";
            SqliteCommand selectCommand = new SqliteCommand();
            selectCommand.Parameters.AddWithValue("@name", "%" + keyword + "%");
            string logger = "Input values:\t\tKeyword == " + keyword;

            try
            {
                decimal minimum = Convert.ToDecimal(min);
                decimal maximum = Convert.ToDecimal(max);
                if (minimum < maximum)
                {
                    commandText += " and COST between @min and @max";
                    selectCommand.Parameters.AddWithValue("@min", min);
                    selectCommand.Parameters.AddWithValue("@max", max);
                    logger += ", Min == " + min;
                    logger += ", Max == " + max;
                }
            }
            catch
            { /* No input for cost or invalid values */ }            

            selectCommand.CommandText = commandText;
            List<MenuItem> results = new List<MenuItem>();
            foreach (string type in types)
            {
                using (SqliteConnection db = databaseFile)
                {
                    db.Open();
                    selectCommand.Connection = db;
                    selectCommand.Parameters.AddWithValue("@type", type);
                    string logger2 = ", Type == " + type;
                    
                    SqliteDataReader query;
                    try
                    {
                        LoggingServices.Instance.WriteLine<DBHelper>("Attempting query...\t\t" + selectCommand.CommandText, LogLevel.Info);
                        LoggingServices.Instance.WriteLine<DBHelper>(logger + logger2, LogLevel.Info);
                        query = selectCommand.ExecuteReader();
                    }
                    catch (SqliteException error)
                    {
                        LoggingServices.Instance.WriteLine<DBHelper>("Error fetching database entries: " + error.ToString(), LogLevel.Error);
                        return results;
                    }
                    while (query.Read())
                    {
                        MenuItem item = new MenuItem(query);
                        results.Add(item);
                    }
                    db.Close();
                }
                selectCommand.Parameters.RemoveAt(selectCommand.Parameters.Count - 1);
                selectCommand.Connection = null;
            }
            results = results.OrderBy(item => item.Name).ToList();
            LoggingServices.Instance.WriteLine<DBHelper>("Found results: " + results.Count, LogLevel.Info);
            return results;
        }

        public static bool AddMenu(Menu menu)
        {
            bool isSuccessful = false;
            string commandText = "Insert into MENUS (NAME, LOCATION, CONTENTS) VALUES(@name, @location, @contents)";
            SqliteCommand insertCommand = new SqliteCommand();
            insertCommand.Parameters.AddWithValue("@name", menu.Name);
            insertCommand.Parameters.AddWithValue("@location", menu.Location);
            insertCommand.Parameters.AddWithValue("@contents", menu.ExportMenuItemsToXML());
            insertCommand.CommandText = commandText;

            using (SqliteConnection db = databaseFile)
            {
                db.Open();
                insertCommand.Connection = db;

                SqliteDataReader query;
                try
                {
                    LoggingServices.Instance.WriteLine<DBHelper>("Attempting insert...\t\t" + insertCommand.CommandText, LogLevel.Info);
                    query = insertCommand.ExecuteReader();
                    isSuccessful = true;
                }
                catch (SqliteException error)
                {
                    LoggingServices.Instance.WriteLine<DBHelper>("Failed insert...\t\t" + error.ToString(), LogLevel.Error);
                }
                db.Close();
            }

            return isSuccessful;
        }

    }
}
