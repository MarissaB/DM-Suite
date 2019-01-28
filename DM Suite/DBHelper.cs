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

        public static bool AddMenuItem(MenuItem menuItem)
        {
            bool isSuccessful = false;
            string commandText = "Insert into MENU_ITEMS (NAME, DESCRIPTION, COST, TYPE) VALUES(@name, @description, @cost, @type)";
            SqliteCommand insertCommand = new SqliteCommand();
            insertCommand.Parameters.AddWithValue("@name", menuItem.Name);
            insertCommand.Parameters.AddWithValue("@description", menuItem.Description);
            insertCommand.Parameters.AddWithValue("@cost", menuItem.Cost);
            insertCommand.Parameters.AddWithValue("@type", menuItem.Type.ToUpper());
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
                    LoggingServices.Instance.WriteLine<DBHelper>("Successfully saved menu item " + menuItem.Name, LogLevel.Info);
                }
                catch (SqliteException error)
                {
                    LoggingServices.Instance.WriteLine<DBHelper>("Failed insert...\t\t" + error.Message, LogLevel.Error);
                }
                db.Close();
            }

            return isSuccessful;
        }

        public static bool UpdateMenuItem(MenuItem menuItem)
        {
            bool isSuccessful = false;
            string commandText = "Update MENU_ITEMS SET Description = @description, Cost = @cost, Type = @type WHERE Name = @name";
            SqliteCommand updateCommand = new SqliteCommand();
            updateCommand.Parameters.AddWithValue("@name", menuItem.Name);
            updateCommand.Parameters.AddWithValue("@description", menuItem.Description);
            updateCommand.Parameters.AddWithValue("@cost", menuItem.Cost);
            updateCommand.Parameters.AddWithValue("@type", menuItem.Type.ToUpper());
            updateCommand.CommandText = commandText;

            using (SqliteConnection db = databaseFile)
            {
                db.Open();
                updateCommand.Connection = db;

                SqliteDataReader query;
                try
                {
                    LoggingServices.Instance.WriteLine<DBHelper>("Attempting update...\t\t" + updateCommand.CommandText, LogLevel.Info);
                    query = updateCommand.ExecuteReader();
                    isSuccessful = true;
                    LoggingServices.Instance.WriteLine<DBHelper>("Successfully updated menu " + menuItem.Name, LogLevel.Info);
                }
                catch (SqliteException error)
                {
                    LoggingServices.Instance.WriteLine<DBHelper>("Failed update...\t\t" + error.Message, LogLevel.Error);
                }
                db.Close();
            }

            return isSuccessful;
        }

        public static bool DeleteMenuItem(MenuItem menuItem)
        {
            bool isSuccessful = false;
            string commandText = "Delete from MENU_ITEMS where Name = @name";
            SqliteCommand deleteCommand = new SqliteCommand();
            deleteCommand.Parameters.AddWithValue("@name", menuItem.Name);
            deleteCommand.CommandText = commandText;

            using (SqliteConnection db = databaseFile)
            {
                db.Open();
                deleteCommand.Connection = db;

                SqliteDataReader query;
                try
                {
                    LoggingServices.Instance.WriteLine<DBHelper>("Attempting delete...\t\t" + deleteCommand.CommandText, LogLevel.Info);
                    query = deleteCommand.ExecuteReader();
                    isSuccessful = true;
                    LoggingServices.Instance.WriteLine<DBHelper>("Successfully deleted menu item " + menuItem.Name, LogLevel.Info);
                }
                catch (SqliteException error)
                {
                    LoggingServices.Instance.WriteLine<DBHelper>("Failed delete...\t\t" + error.Message, LogLevel.Error);
                }
                db.Close();
            }

            return isSuccessful;
        }

        public static MenuItem SearchMenuItem(MenuItem menuItem)
        {
            MenuItem resultItem = new MenuItem();
            string commandText = "Select * from MENU_ITEMS where Name = @name LIMIT 1";
            SqliteCommand selectCommand = new SqliteCommand();
            selectCommand.Parameters.AddWithValue("@name", menuItem.Name);
            selectCommand.CommandText = commandText;

            using (SqliteConnection db = databaseFile)
            {
                db.Open();
                selectCommand.Connection = db;

                SqliteDataReader query;
                try
                {
                    LoggingServices.Instance.WriteLine<DBHelper>("Attempting search...\t\t" + selectCommand.CommandText, LogLevel.Info);
                    query = selectCommand.ExecuteReader();
                    LoggingServices.Instance.WriteLine<DBHelper>("Successfully found menu item " + menuItem.Name, LogLevel.Info);
                    while (query.Read())
                    {
                        resultItem = new MenuItem(query);
                    }
                }
                catch (SqliteException error)
                {
                    LoggingServices.Instance.WriteLine<DBHelper>("Failed search...\t\t" + error.Message, LogLevel.Error);
                }
                
                db.Close();
            }

            return resultItem;
        }

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
                        LoggingServices.Instance.WriteLine<DBHelper>("Error fetching database entries: " + error.Message, LogLevel.Error);
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
                    LoggingServices.Instance.WriteLine<DBHelper>("Successfully saved menu " + menu.Name, LogLevel.Info);
                }
                catch (SqliteException error)
                {
                    LoggingServices.Instance.WriteLine<DBHelper>("Failed insert...\t\t" + error.Message, LogLevel.Error);
                }
                db.Close();
            }

            return isSuccessful;
        }

        public static bool UpdateMenu(Menu menu)
        {
            bool isSuccessful = false;
            string commandText = "Update MENUS SET Location = @location, Contents = @contents WHERE Name = @name";
            SqliteCommand updateCommand = new SqliteCommand();
            updateCommand.Parameters.AddWithValue("@name", menu.Name);
            updateCommand.Parameters.AddWithValue("@location", menu.Location);
            updateCommand.Parameters.AddWithValue("@contents", menu.ExportMenuItemsToXML());
            updateCommand.CommandText = commandText;

            using (SqliteConnection db = databaseFile)
            {
                db.Open();
                updateCommand.Connection = db;

                SqliteDataReader query;
                try
                {
                    LoggingServices.Instance.WriteLine<DBHelper>("Attempting update...\t\t" + updateCommand.CommandText, LogLevel.Info);
                    query = updateCommand.ExecuteReader();
                    isSuccessful = true;
                    LoggingServices.Instance.WriteLine<DBHelper>("Successfully updated menu " + menu.Name, LogLevel.Info);
                }
                catch (SqliteException error)
                {
                    LoggingServices.Instance.WriteLine<DBHelper>("Failed update...\t\t" + error.Message, LogLevel.Error);
                }
                db.Close();
            }

            return isSuccessful;
        }

        public static bool DeleteMenu(Menu menu)
        {
            bool isSuccessful = false;
            string commandText = "Delete from MENUS where Name = @name";
            SqliteCommand deleteCommand = new SqliteCommand();
            deleteCommand.Parameters.AddWithValue("@name", menu.Name);
            deleteCommand.CommandText = commandText;

            using (SqliteConnection db = databaseFile)
            {
                db.Open();
                deleteCommand.Connection = db;

                SqliteDataReader query;
                try
                {
                    LoggingServices.Instance.WriteLine<DBHelper>("Attempting delete...\t\t" + deleteCommand.CommandText, LogLevel.Info);
                    query = deleteCommand.ExecuteReader();
                    isSuccessful = true;
                    LoggingServices.Instance.WriteLine<DBHelper>("Successfully deleted menu " + menu.Name, LogLevel.Info);
                }
                catch (SqliteException error)
                {
                    LoggingServices.Instance.WriteLine<DBHelper>("Failed delete...\t\t" + error.Message, LogLevel.Error);
                }
                db.Close();
            }

            return isSuccessful;
        }

        public static List<Menu> SearchMenus(string name, string location)
        {
            string commandText = "Select * from MENUS where NAME like @name AND LOCATION like @location";
            SqliteCommand selectCommand = new SqliteCommand();
            selectCommand.Parameters.AddWithValue("@name", "%" + name + "%");
            selectCommand.Parameters.AddWithValue("@location", "%" + location + "%");
            string logger = "Input values:\t\tName == " + name + ", Location == " + location;

            selectCommand.CommandText = commandText;
            List<Menu> results = new List<Menu>();
            
            using (SqliteConnection db = databaseFile)
            {
                db.Open();
                selectCommand.Connection = db;

                SqliteDataReader query;
                try
                {
                    LoggingServices.Instance.WriteLine<DBHelper>("Attempting query...\t\t" + selectCommand.CommandText, LogLevel.Info);
                    LoggingServices.Instance.WriteLine<DBHelper>(logger, LogLevel.Info);
                    query = selectCommand.ExecuteReader();
                }
                catch (SqliteException error)
                {
                    LoggingServices.Instance.WriteLine<DBHelper>("Error fetching database entries: " + error.Message, LogLevel.Error);
                    return results;
                }
                while (query.Read())
                {
                    Menu item = new Menu(query);
                    results.Add(item);
                }
                db.Close();
            }
            selectCommand.Parameters.RemoveAt(selectCommand.Parameters.Count - 1);
            selectCommand.Connection = null;
            
            results = results.OrderBy(item => item.Name).ToList();
            LoggingServices.Instance.WriteLine<DBHelper>("Found results: " + results.Count, LogLevel.Info);
            return results;
        }
    }
}
