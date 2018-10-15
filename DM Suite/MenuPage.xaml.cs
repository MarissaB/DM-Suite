﻿using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Diagnostics;
using Windows.ApplicationModel.Resources;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Windows.UI.Popups;
using System;
using System.Linq;

namespace DM_Suite
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MenuPage : Page
    {
        public MenuPage()
        {
            InitializeComponent();
            OptionsAllCheckBox.IsChecked = true;
            SearchResults.ItemsSource = new List<MenuItem>(); // Give it a blank list to get the headers to show.
            CurrentMenu.ItemsSource = currentMenu; // Give it a blank list to get the headers to show.
        }

        private ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();
        private List<MenuItem> currentMenu = new List<MenuItem>();

        private bool IsSearchValid()
        {
            bool isValid = false;
            bool costMinValid = IsCostValid(CostMin);
            bool costMaxValid = IsCostValid(CostMax);
            bool costRangeValid = false;
            bool typesValid = false;

            if (Convert.ToDecimal(CostMin.Text) < Convert.ToDecimal(CostMax.Text))
            {
                costRangeValid = true;
            }

            if (OptionsAllCheckBox.IsChecked != false)
            {
                typesValid = true;
            }

            if (costMinValid && costMaxValid && typesValid && costRangeValid)
            {
                isValid = true;
            }

            return isValid;
        }

        private bool IsCostValid(TextBox input)
        {
            bool isCostValid = false;

            if (input.Text.Length > 0)
            {
                if (TextBoxRegex.GetIsValid(input))
                {
                    isCostValid = true;
                }
                else
                {
                    isCostValid = false;
                }
            }
            else
            {
                isCostValid = true;
                input.Text = "0";
            }

            return isCostValid;
        }

        private List<string> GetTypes()
        {
            List<string> selectedTypes = new List<string>();

            if (FoodCheckBox.IsChecked != false)
            {
                selectedTypes.Add("FOOD");
            }
            if (DrinkCheckBox.IsChecked != false)
            {
                selectedTypes.Add("DRINK");
            }
            if (TreatCheckBox.IsChecked != false)
            {
                selectedTypes.Add("TREAT");
            }

            return selectedTypes;
        }

        private async void ExecuteSearch(object sender, RoutedEventArgs e)
        {
            if (IsSearchValid())
            {
                List<MenuItem> searchResults = DBHelper.SearchMenuItems(Input_Box.Text, GetTypes(), CostMin.Text, CostMax.Text);
                SearchResults.ItemsSource = searchResults;
                ResultsCount.Text = resourceLoader.GetString("Heading_Results") + CountResults(searchResults);
                ResultsCount.Visibility = Visibility.Visible;
            }
            else
            {
                string errorText = resourceLoader.GetString("Errors_MenuSearch");
                MessageDialog errorMessage = new MessageDialog(errorText);
                await errorMessage.ShowAsync();
            }
        }

        // Method to insert text into the SQLite database
        private void Add_Text(object sender, RoutedEventArgs e)
        {
            using (SqliteConnection db = DBHelper.databaseFile)
            {
                db.Open();
                SqliteCommand insertCommand = new SqliteCommand
                {
                    Connection = db,
                    CommandText = "INSERT INTO MENU_ITEMS VALUES (NULL, @Entry);"
                };
                insertCommand.Parameters.AddWithValue("@NAME", Input_Box.Text);
                try
                {
                    insertCommand.ExecuteReader();
                }
                catch (SqliteException error)
                {
                    Debug.WriteLine("Error inserting entries on MenuPage: " + error.ToString());
                    return;
                }
                db.Close();
            }
            SearchResults.ItemsSource = GetSearchResults();
            Input_Box.Text = string.Empty;
        }

        // Method to grab Text_Entry column from MyTable table in SQLite database
        //  private List<MenuItem> GetSearchResults(string keyword, List<string> types, decimal min, decimal max)
        private List<MenuItem> GetSearchResults()
          {
            List<MenuItem> results = new List<MenuItem>();

            using (SqliteConnection db = new SqliteConnection("Filename=sqliteSample.db"))
            {
                db.Open();
                SqliteCommand selectCommand = new SqliteCommand("SELECT * from MENU_ITEMS", db);
                SqliteDataReader query;
                try
                {
                    query = selectCommand.ExecuteReader();
                }
                catch (SqliteException error)
                {
                    Debug.WriteLine("Error reading entries on MenuPage: " + error.ToString());
                    return results;
                }
                while (query.Read())
                {
                    MenuItem item = new MenuItem(query);
                    results.Add(item);
                }
                db.Close();
            }
            return results;
        }

        private int CountResults(List<MenuItem> menuList)
        {
            int count = menuList.Count;
            return count;
        }

        private void SearchResults_AutoGeneratingColumn(object sender, Microsoft.Toolkit.Uwp.UI.Controls.DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.ToString() == "Cost")
            {
                e.Column.Header = "Cost (gold)";
            }
        }

        #region Checkbox toggle Stuff

        private void SelectAll_Checked(object sender, RoutedEventArgs e)
        {
            FoodCheckBox.IsChecked = DrinkCheckBox.IsChecked = TreatCheckBox.IsChecked = true;
        }

        private void SelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            FoodCheckBox.IsChecked = DrinkCheckBox.IsChecked = TreatCheckBox.IsChecked = false;
        }

        private void SelectAll_Indeterminate(object sender, RoutedEventArgs e)
        {
            // If the SelectAll box is checked (all options are selected), 
            // clicking the box will change it to its indeterminate state.
            // Instead, we want to uncheck all the boxes,
            // so we do this programatically. The indeterminate state should
            // only be set programatically, not by the user.

            if (FoodCheckBox.IsChecked == true &&
                DrinkCheckBox.IsChecked == true &&
                TreatCheckBox.IsChecked == true)
            {
                // This will cause SelectAll_Unchecked to be executed, so
                // we don't need to uncheck the other boxes here.
                OptionsAllCheckBox.IsChecked = false;
            }
        }

        private void SetCheckedState()
        {
            // Controls are null the first time this is called, so we just 
            // need to perform a null check on any one of the controls.
            if (FoodCheckBox != null)
            {
                if (FoodCheckBox.IsChecked == true &&
                    DrinkCheckBox.IsChecked == true &&
                    TreatCheckBox.IsChecked == true)
                {
                    OptionsAllCheckBox.IsChecked = true;
                }
                else if (FoodCheckBox.IsChecked == false &&
                    DrinkCheckBox.IsChecked == false &&
                    TreatCheckBox.IsChecked == false)
                {
                    OptionsAllCheckBox.IsChecked = false;
                }
                else
                {
                    // Set third state (indeterminate) by setting IsChecked to null.
                    OptionsAllCheckBox.IsChecked = null;
                }
            }
        }

        private void Option_Checked(object sender, RoutedEventArgs e)
        {
            SetCheckedState();
        }

        private void Option_Unchecked(object sender, RoutedEventArgs e)
        {
            SetCheckedState();
        }

        #endregion

        private async void AddToCurrentMenu(object sender, RoutedEventArgs e)
        {
            List<MenuItem> selectedSearchResults = SearchResults.SelectedItems.Cast<MenuItem>().ToList();
            bool showError = false;

            if (selectedSearchResults.Count > 0)
            {
                SearchResults.SelectedItems.Clear();
                foreach (MenuItem item in selectedSearchResults)
                {
                    if (!currentMenu.Contains(item))
                    {
                        currentMenu.Add(item);
                    }
                    else
                    {
                        showError = true;
                    }
                }
            }

            if (showError)
            {
                string errorText = resourceLoader.GetString("Errors_MenuAddToCurrent");
                MessageDialog errorMessage = new MessageDialog(errorText);
                await errorMessage.ShowAsync();
            }

            CurrentMenu.ItemsSource = null;
            CurrentMenu.ItemsSource = currentMenu;
        }

        private async void RemoveFromCurrentMenu(object sender, RoutedEventArgs e)
        {
            List<MenuItem> selectedCurrentMenuItems = CurrentMenu.SelectedItems.Cast<MenuItem>().ToList();
            bool showError = false;

            if (selectedCurrentMenuItems.Count > 0)
            {
                foreach (MenuItem item in selectedCurrentMenuItems)
                {
                    currentMenu.Remove(item);
                }
            }
            else
            {
                showError = true;
            }

            if (showError)
            {
                string errorText = resourceLoader.GetString("Errors_MenuRemoveFromCurrent");
                MessageDialog errorMessage = new MessageDialog(errorText);
                await errorMessage.ShowAsync();
            }

            CurrentMenu.ItemsSource = null;
            CurrentMenu.ItemsSource = currentMenu;
        }

    }
}

