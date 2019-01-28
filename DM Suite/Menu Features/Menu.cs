using DM_Suite.Services.LoggingServices;
using MetroLog;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace DM_Suite.Menu_Features
{
    public class Menu
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public List<MenuItem> MenuItems { get; } = new List<MenuItem>();

        public Menu() { }

        public Menu(SqliteDataReader query)
        {
            if (query.FieldCount == 4)
            {
                try
                {
                    Name = query.GetValue(1).ToString();
                    Location = query.GetValue(2).ToString();
                    MenuItems = ImportMenuItemsFromXML(query.GetValue(3).ToString());
                }
                catch (Exception error)
                {
                    LoggingServices.Instance.WriteLine<Menu>("Error mapping Menu from query: " + error.Message, LogLevel.Info);
                }
            }
            else
            {
                LoggingServices.Instance.WriteLine<Menu>("Invalid query result for mapping Menu. Expected FieldCount of 4, found FieldCount of " + query.FieldCount, LogLevel.Info);
            }
        }

        public void AddMenuItems(List<MenuItem> itemsToAdd)
        {
            foreach (MenuItem item in itemsToAdd)
            {
                if (!MenuItems.Contains(item))
                {
                    MenuItems.Add(item);
                }
            }
        }

        public void RemoveMenuItems(List<MenuItem> itemsToRemove)
        {
            foreach (MenuItem item in itemsToRemove)
            {
                if (MenuItems.Contains(item))
                {
                    MenuItems.Remove(item);
                }
            }
        }

        public bool DoesMenuContainDuplicates(List<MenuItem> itemsToCheck)
        {
            bool containsDuplicates = false;
            if (MenuItems.Count > 0)
            {
                foreach (MenuItem item in itemsToCheck)
                {
                    if (MenuItems.Contains(item))
                    {
                        containsDuplicates = true;
                    }
                }
            }

            return containsDuplicates;
        }

        public string ExportMenuToXML()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Menu), new Type[] { typeof(MenuItem) });
            string xml = "";

            using (StringWriter stringWriter = new StringWriter())
            {
                using (XmlTextWriter writer = new XmlTextWriter(stringWriter) { Formatting = Formatting.Indented })
                {
                    serializer.Serialize(writer, this);
                    xml = stringWriter.ToString();
                }
            }
            return xml;
        }

        public string ExportMenuItemsToXML()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<MenuItem>));
            string xml = "";

            using (StringWriter stringWriter = new StringWriter())
            {
                using (XmlTextWriter writer = new XmlTextWriter(stringWriter) { Formatting = Formatting.Indented })
                {
                    serializer.Serialize(writer, MenuItems);
                    xml = stringWriter.ToString();
                }
            }
            return xml;
        }

        public static Menu BuildFromXML(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Menu));
            Menu menu = new Menu();

            using (TextReader reader = new StringReader(xml))
            {
                try
                {
                    menu = (Menu)serializer.Deserialize(reader);
                }
                catch (Exception ex)
                {
                    LoggingServices.Instance.WriteLine<Menu>("Building menu list from XML failed! " + ex.Message, LogLevel.Info);
                }
            }
            return menu;
        }

        public static List<MenuItem> ImportMenuItemsFromXML(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<MenuItem>));
            List<MenuItem> items = new List<MenuItem>();

            using (TextReader reader = new StringReader(xml))
            {
                try
                {
                    items = (List<MenuItem>)serializer.Deserialize(reader);
                }
                catch (Exception ex)
                {
                    LoggingServices.Instance.WriteLine<Menu>("Building menu item list from XML failed! " + ex.Message, LogLevel.Info);
                }
            }

            return items;
        }

        public static bool IsMenuValid(Menu menu)
        {
            bool isValid = false;

            // Check for an empty name and null list of menu items
            if (!string.IsNullOrEmpty(menu.Name) && menu.MenuItems != null)
            {
                isValid = true;
            }

            return isValid;
        }
    }
}
