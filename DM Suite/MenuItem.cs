using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM_Suite
{
    public class MenuItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public string Type { get; set; }

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
    }
}
