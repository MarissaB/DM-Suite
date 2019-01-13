using DM_Suite.Services.LoggingServices;
using MetroLog;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Linq;

namespace DM_Suite
{
    public class DBHelper
    {
        public static SqliteConnection databaseFile = new SqliteConnection("Filename=sqliteSample.db");

        public static List<MenuItem> SearchMenuItems(string keyword, List<string> types, string min, string max)
        {
            List<MenuItem> results = new List<MenuItem>();
            foreach (string type in types)
            {
                using (SqliteConnection db = databaseFile)
                {
                    db.Open();
                    SqliteCommand selectCommand = new SqliteCommand("select * from MENU_ITEMS where "
                                                                    + "TYPE = @type and "
                                                                    + "(NAME like @name OR "
                                                                    + "DESCRIPTION like @name) and "
                                                                    + "COST between @min and @max", db);
                    selectCommand.Parameters.AddWithValue("@type", type);
                    selectCommand.Parameters.AddWithValue("@name", "%" + keyword + "%");
                    selectCommand.Parameters.AddWithValue("@min", min);
                    selectCommand.Parameters.AddWithValue("@max", max);
                    SqliteDataReader query;
                    try
                    {
                        query = selectCommand.ExecuteReader();
                    }
                    catch (SqliteException error)
                    {
                        LoggingServices.Instance.WriteLine<DBHelper>("Error creating new MenuItems entries: " + error.ToString(), LogLevel.Error);
                        return results;
                    }
                    while (query.Read())
                    {
                        MenuItem item = new MenuItem(query);
                        results.Add(item);
                    }
                    db.Close();
                }
            }
            results = results.OrderBy(item => item.Name).ToList();
            return results;
        }
    }
}
