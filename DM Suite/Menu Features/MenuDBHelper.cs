using DM_Suite.Services.LoggingServices;
using MetroLog;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Resources;

namespace DM_Suite.Menu_Features
{
    public class MenuDBHelper
    {
        public static SqliteConnection DatabaseFile { get; set; }

        public static bool CreateDatabaseTables(string databaseName)
        {
            DatabaseFile = new SqliteConnection("Filename=" + databaseName + ".db");

            bool isSuccessful = false;
            ResourceLoader resourceLoader = ResourceLoader.GetForViewIndependentUse();
            string type1 = resourceLoader.GetString("Menu_Drink/Content").ToUpper();
            string type2 = resourceLoader.GetString("Menu_Food/Content").ToUpper();
            string type3 = resourceLoader.GetString("Menu_Treat/Content").ToUpper();

            using (SqliteConnection db = DatabaseFile)
            {
                db.Open();
                
                string menuItemsTableCommand = "CREATE TABLE IF NOT EXISTS `MENU_ITEMS` ( `ID` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, `NAME` TEXT NOT NULL UNIQUE, `DESCRIPTION` TEXT, `COST` NUMERIC NOT NULL, `TYPE` TEXT NOT NULL DEFAULT '" + type2 + "' CHECK(TYPE IN ( '" + type2 + "' , '" + type1 + "' , '" + type3 + "' )) )";
                SqliteCommand createMenuItemsTable = new SqliteCommand(menuItemsTableCommand, db);

                string menuTableCommand = "CREATE TABLE `MENUS` ( `ID` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, `NAME` TEXT NOT NULL UNIQUE, `LOCATION` TEXT, `CONTENTS` TEXT )";
                SqliteCommand createMenuTable = new SqliteCommand(menuTableCommand, db);
                try
                {
                    createMenuItemsTable.ExecuteReader();
                    createMenuTable.ExecuteReader();
                    isSuccessful = true;
                }
                catch (SqliteException e)
                {
                    LoggingServices.Instance.WriteLine<MenuDBHelper>("Sqlite Database Tables for MENU and MENU_ITEMS could not be created. " + e.Message, LogLevel.Error);
                }
            }

            return isSuccessful;
        }

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

            using (SqliteConnection db = DatabaseFile)
            {
                db.Open();
                insertCommand.Connection = db;

                SqliteDataReader query;
                try
                {
                    LoggingServices.Instance.WriteLine<MenuDBHelper>("Attempting insert...\t\t" + insertCommand.CommandText, LogLevel.Info);
                    query = insertCommand.ExecuteReader();
                    isSuccessful = true;
                    LoggingServices.Instance.WriteLine<MenuDBHelper>("Successfully saved menu item " + menuItem.Name, LogLevel.Info);
                }
                catch (SqliteException error)
                {
                    LoggingServices.Instance.WriteLine<MenuDBHelper>("Failed insert...\t\t" + error.Message, LogLevel.Error);
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

            using (SqliteConnection db = DatabaseFile)
            {
                db.Open();
                updateCommand.Connection = db;

                SqliteDataReader query;
                try
                {
                    LoggingServices.Instance.WriteLine<MenuDBHelper>("Attempting update...\t\t" + updateCommand.CommandText, LogLevel.Info);
                    query = updateCommand.ExecuteReader();
                    isSuccessful = true;
                    LoggingServices.Instance.WriteLine<MenuDBHelper>("Successfully updated menu " + menuItem.Name, LogLevel.Info);
                }
                catch (SqliteException error)
                {
                    LoggingServices.Instance.WriteLine<MenuDBHelper>("Failed update...\t\t" + error.Message, LogLevel.Error);
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

            using (SqliteConnection db = DatabaseFile)
            {
                db.Open();
                deleteCommand.Connection = db;

                SqliteDataReader query;
                try
                {
                    LoggingServices.Instance.WriteLine<MenuDBHelper>("Attempting delete...\t\t" + deleteCommand.CommandText, LogLevel.Info);
                    query = deleteCommand.ExecuteReader();
                    isSuccessful = true;
                    LoggingServices.Instance.WriteLine<MenuDBHelper>("Successfully deleted menu item " + menuItem.Name, LogLevel.Info);
                }
                catch (SqliteException error)
                {
                    LoggingServices.Instance.WriteLine<MenuDBHelper>("Failed delete...\t\t" + error.Message, LogLevel.Error);
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

            using (SqliteConnection db = DatabaseFile)
            {
                db.Open();
                selectCommand.Connection = db;

                SqliteDataReader query;
                try
                {
                    LoggingServices.Instance.WriteLine<MenuDBHelper>("Attempting search...\t\t" + selectCommand.CommandText, LogLevel.Info);
                    query = selectCommand.ExecuteReader();
                    LoggingServices.Instance.WriteLine<MenuDBHelper>("Successfully found menu item " + menuItem.Name, LogLevel.Info);
                    while (query.Read())
                    {
                        resultItem = new MenuItem(query);
                    }
                }
                catch (SqliteException error)
                {
                    LoggingServices.Instance.WriteLine<MenuDBHelper>("Failed search...\t\t" + error.Message, LogLevel.Error);
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
                using (SqliteConnection db = DatabaseFile)
                {
                    db.Open();
                    selectCommand.Connection = db;
                    selectCommand.Parameters.AddWithValue("@type", type);
                    string logger2 = ", Type == " + type;
                    
                    SqliteDataReader query;
                    try
                    {
                        LoggingServices.Instance.WriteLine<MenuDBHelper>("Attempting query...\t\t" + selectCommand.CommandText, LogLevel.Info);
                        LoggingServices.Instance.WriteLine<MenuDBHelper>(logger + logger2, LogLevel.Info);
                        query = selectCommand.ExecuteReader();
                    }
                    catch (SqliteException error)
                    {
                        LoggingServices.Instance.WriteLine<MenuDBHelper>("Error fetching database entries: " + error.Message, LogLevel.Error);
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
            LoggingServices.Instance.WriteLine<MenuDBHelper>("Found results: " + results.Count, LogLevel.Info);
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

            using (SqliteConnection db = DatabaseFile)
            {
                db.Open();
                insertCommand.Connection = db;

                SqliteDataReader query;
                try
                {
                    LoggingServices.Instance.WriteLine<MenuDBHelper>("Attempting insert...\t\t" + insertCommand.CommandText, LogLevel.Info);
                    query = insertCommand.ExecuteReader();
                    isSuccessful = true;
                    LoggingServices.Instance.WriteLine<MenuDBHelper>("Successfully saved menu " + menu.Name, LogLevel.Info);
                }
                catch (SqliteException error)
                {
                    LoggingServices.Instance.WriteLine<MenuDBHelper>("Failed insert...\t\t" + error.Message, LogLevel.Error);
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

            using (SqliteConnection db = DatabaseFile)
            {
                db.Open();
                updateCommand.Connection = db;

                SqliteDataReader query;
                try
                {
                    LoggingServices.Instance.WriteLine<MenuDBHelper>("Attempting update...\t\t" + updateCommand.CommandText, LogLevel.Info);
                    query = updateCommand.ExecuteReader();
                    isSuccessful = true;
                    LoggingServices.Instance.WriteLine<MenuDBHelper>("Successfully updated menu " + menu.Name, LogLevel.Info);
                }
                catch (SqliteException error)
                {
                    LoggingServices.Instance.WriteLine<MenuDBHelper>("Failed update...\t\t" + error.Message, LogLevel.Error);
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

            using (SqliteConnection db = DatabaseFile)
            {
                db.Open();
                deleteCommand.Connection = db;

                SqliteDataReader query;
                try
                {
                    LoggingServices.Instance.WriteLine<MenuDBHelper>("Attempting delete...\t\t" + deleteCommand.CommandText, LogLevel.Info);
                    query = deleteCommand.ExecuteReader();
                    isSuccessful = true;
                    LoggingServices.Instance.WriteLine<MenuDBHelper>("Successfully deleted menu " + menu.Name, LogLevel.Info);
                }
                catch (SqliteException error)
                {
                    LoggingServices.Instance.WriteLine<MenuDBHelper>("Failed delete...\t\t" + error.Message, LogLevel.Error);
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
            
            using (SqliteConnection db = DatabaseFile)
            {
                db.Open();
                selectCommand.Connection = db;

                SqliteDataReader query;
                try
                {
                    LoggingServices.Instance.WriteLine<MenuDBHelper>("Attempting query...\t\t" + selectCommand.CommandText, LogLevel.Info);
                    LoggingServices.Instance.WriteLine<MenuDBHelper>(logger, LogLevel.Info);
                    query = selectCommand.ExecuteReader();
                }
                catch (SqliteException error)
                {
                    LoggingServices.Instance.WriteLine<MenuDBHelper>("Error fetching database entries: " + error.Message, LogLevel.Error);
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
            LoggingServices.Instance.WriteLine<MenuDBHelper>("Found results: " + results.Count, LogLevel.Info);
            return results;
        }
    }
}
