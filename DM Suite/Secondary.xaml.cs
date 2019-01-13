using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DM_Suite
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Secondary : Page
    {
        public Secondary()
        {
            InitializeComponent();
            Output.ItemsSource = Grab_Entries();
        }

        // Method to insert text into the SQLite database
        private void Add_Text(object sender, RoutedEventArgs e)
        {
            using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
            {
                db.Open();
                SqliteCommand insertCommand = new SqliteCommand
                {
                    Connection = db,

                    //Use parameterized query to prevent SQL injection attacks
                    CommandText = "INSERT INTO MENU_ITEMS VALUES (NULL, @Entry);"
                };
                insertCommand.Parameters.AddWithValue("@NAME", Input_Box.Text);
                try
                {
                    insertCommand.ExecuteReader();
                }
                catch (SqliteException error)
                {
                    Debug.WriteLine("Menu insert failed. " + error);
                    return;
                }
                db.Close();
            }
            Output.ItemsSource = Grab_Entries();
            Input_Box.Text = string.Empty;
        }

        // Method to grab Text_Entry column from MyTable table in SQLite database
        private List<string> Grab_Entries()
        {
            List<string> entries = new List<string>();
            using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
            {
                db.Open();
                SqliteCommand selectCommand = new SqliteCommand("SELECT NAME from MENU_ITEMS", db);
                SqliteDataReader query;
                try
                {
                    query = selectCommand.ExecuteReader();
                }
                catch (SqliteException error)
                {
                    Debug.WriteLine("Menu item selection query failed. " + error);
                    return entries;
                }
                while (query.Read())
                {
                    entries.Add(query.GetString(0));
                    
                }
                db.Close();
            }
            return entries;
        }
    }
}

