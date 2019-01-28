using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DM_Suite.Menu_Features
{
    public class MenuItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public string Type { get; set; }

        public MenuItem() { }

        public MenuItem(string name, string description, string cost, string type)
        {
            Name = name;
            Description = description;
            Cost = Convert.ToDecimal(cost);
            Type = type;
        }

        public MenuItem(SqliteDataReader query)
        {
            if (query.FieldCount == 5)
            {
                try
                {
                    Name = query.GetValue(1).ToString();
                    Description = query.GetValue(2).ToString();
                    Cost = Cost = Convert.ToDecimal(query.GetValue(3));
                    Type = query.GetValue(4).ToString();
                }
                catch (Exception error)
                {
                    Debug.WriteLine("Error mapping MenuItem from query: " + error.ToString());
                }
            }
            else
            {
                Debug.WriteLine("Invalid query result for mapping MenuItem. Expected FieldCount of 5, found FieldCount of " + query.FieldCount);
            }
        }

        public static bool IsMenuItemValid(string nameInput, string costInput, string typeInput)
        {
            List<string> types = new List<string>
            {
                "Food",
                "Drink",
                "Treat"
            };

            // Check for an empty name and invalid type
            if (!string.IsNullOrEmpty(nameInput) && types.Contains(typeInput))
            {
                try
                {
                    decimal cost = Convert.ToDecimal(costInput);
                    return true;
                }
                catch { return false; }
            }
            else { return false; }
        }
    }
}
