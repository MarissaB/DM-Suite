using DM_Suite.Services.LoggingServices;
using MetroLog;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;

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
    }
}
