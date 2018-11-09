using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace DM_Suite
{
    public class Menu
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public List<MenuItem> MenuItems { get; } = new List<MenuItem>();

        public Menu() { }

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
                    xml = stringWriter.ToString(); // Your XML
                }
            }
            return xml;
        }
    }
}
