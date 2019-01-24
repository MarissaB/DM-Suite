using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Diagnostics;
using Windows.ApplicationModel.Resources;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Windows.UI.Popups;
using System;
using System.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using DM_Suite.Services.LoggingServices;
using MetroLog;

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

            currentMenu.Name = "Untitled New Menu";
            currentMenu.Location = "Test location";

            OptionsAllCheckBox.IsChecked = true;
            SearchResults.ItemsSource = new List<MenuItem>(); // Give it a blank list to get the headers to show.
            RefreshCurrentMenuInPage();
        }

        private Menu currentMenu = new Menu();
        private ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();

        private async void RefreshCurrentMenuInPage()
        {
            if (Menu.IsMenuValid(currentMenu))
            {
                CurrentMenuName.Text = currentMenu.Name;
                CurrentMenu.ItemsSource = null;
                CurrentMenu.ItemsSource = currentMenu.MenuItems;
            }
            else
            {
                LoggingServices.Instance.WriteLine<MenuPage>("RefreshCurrentMenu failed due to invalid menu", LogLevel.Error);
                MessageDialog errorMessage = new MessageDialog("Error: Invalid Menu.");
                await errorMessage.ShowAsync();
            }
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

        private void ExecuteSearch(object sender, RoutedEventArgs e)
        {
            List<MenuItem> searchResults = DBHelper.SearchMenuItems(Input_Box.Text, GetTypes(), CostMin.Text, CostMax.Text);
            SearchResults.ItemsSource = searchResults;
            ResultsCount.Text = resourceLoader.GetString("Heading_Results") + CountResults(searchResults);
            ResultsCount.Visibility = Visibility.Visible;

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
                    LoggingServices.Instance.WriteLine<MenuPage>("Error inserting entries on MenuPage: " + error.ToString(), LogLevel.Error);
                    return;
                }
                db.Close();
            }
            SearchResults.ItemsSource = GetSearchResults();
            Input_Box.Text = string.Empty;
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

            if (selectedSearchResults.Count > 0)
            {
                SearchResults.SelectedItems.Clear();

                if (currentMenu.DoesMenuContainDuplicates(selectedSearchResults))
                {
                    string errorText = resourceLoader.GetString("Errors_MenuAddToCurrent");
                    MessageDialog errorMessage = new MessageDialog(errorText);
                    await errorMessage.ShowAsync();
                }

                currentMenu.AddMenuItems(selectedSearchResults);
            }

            RefreshCurrentMenuInPage();
        }

        private async void RemoveFromCurrentMenu(object sender, RoutedEventArgs e)
        {
            List<MenuItem> selectedCurrentMenuItems = CurrentMenu.SelectedItems.Cast<MenuItem>().ToList();

            if (selectedCurrentMenuItems.Count > 0)
            {
                currentMenu.RemoveMenuItems(selectedCurrentMenuItems);
            }
            else
            {
                string errorText = resourceLoader.GetString("Errors_MenuRemoveFromCurrent");
                MessageDialog errorMessage = new MessageDialog(errorText);
                await errorMessage.ShowAsync();
            }

            RefreshCurrentMenuInPage();
        }

        private void UpdateCurrentMenuName(object sender, RoutedEventArgs e)
        {
            currentMenu.Name = CurrentMenuName.Text;
        }

        private void SaveCurrentMenu(object sender, RoutedEventArgs e)
        {
            string itemsXML = currentMenu.ExportMenuItemsToXML();
        }

        private async void ExportCurrentMenu(object sender, RoutedEventArgs e)
        {
            if (Menu.IsMenuValid(currentMenu))
            {
                string menuXML = currentMenu.ExportMenuToXML();
                FileHelper.WriteToFile(menuXML, currentMenu.Name);
            }
            else
            {
                MessageDialog errorMessage = new MessageDialog("Error: Cannot export invalid menu.");
                await errorMessage.ShowAsync();
                LoggingServices.Instance.WriteLine<MenuPage>("ExportCurrentMenu failed due to invalid menu.", LogLevel.Error);
            }
        }

        private async void OpenMenuFromFile(object sender, RoutedEventArgs e)
        {
            string fileContents = string.Empty;
            FileOpenPicker openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            openPicker.FileTypeFilter.Add(".txt");

            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                fileContents = await FileIO.ReadTextAsync(file);
                currentMenu = Menu.BuildFromXML(fileContents);
                RefreshCurrentMenuInPage();
            }
            else
            {
                LoggingServices.Instance.WriteLine<MenuPage>("Cancelled opening file.", LogLevel.Info);
            }
        }
    }
}

