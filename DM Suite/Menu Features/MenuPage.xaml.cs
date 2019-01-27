using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using System;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Pickers;
using DM_Suite.Services.LoggingServices;
using MetroLog;
using System.Threading.Tasks;

namespace DM_Suite.Menu_Features
{
    public sealed partial class MenuPage : Page
    {
        private string newMenuName = ResourceLoader.GetForCurrentView().GetString("Menu_NewName");
        private string newMenuLocation = ResourceLoader.GetForCurrentView().GetString("Menu_NewLocation");

        public MenuPage()
        {
            InitializeComponent();

            currentMenu.Name = newMenuName;
            currentMenu.Location = newMenuLocation;
            unsavedChanges = false;

            OptionsAllCheckBox.IsChecked = true;
            MenuSearchResults.ItemsSource = new List<Menu>(); // Give it a blank list to get the headers to show.
            MenuItemSearchResults.ItemsSource = new List<MenuItem>(); // Give it a blank list to get the headers to show.
            RefreshCurrentMenuInPage();
            
        }

        private Menu currentMenu = new Menu();
        private ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();
        bool unsavedChanges;

        private async void RefreshCurrentMenuInPage()
        {
            if (Menu.IsMenuValid(currentMenu))
            {
                CurrentMenuName.Text = currentMenu.Name;
                CurrentMenuLocation.Text = currentMenu.Location;
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
            List<MenuItem> searchResults = DBHelper.SearchMenuItems(InputMenuItemSearch_Box.Text, GetTypes(), CostMin.Text, CostMax.Text);
            MenuItemSearchResults.ItemsSource = searchResults;
            ResultsCount.Text = resourceLoader.GetString("Heading_Results") + CountResults(searchResults);
            ResultsCount.Visibility = Visibility.Visible;
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
            List<MenuItem> selectedSearchResults = MenuItemSearchResults.SelectedItems.Cast<MenuItem>().ToList();

            if (selectedSearchResults.Count > 0)
            {
                MenuItemSearchResults.SelectedItems.Clear();

                if (currentMenu.DoesMenuContainDuplicates(selectedSearchResults))
                {
                    string errorText = resourceLoader.GetString("Errors_MenuAddToCurrent");
                    MessageDialog errorMessage = new MessageDialog(errorText);
                    await errorMessage.ShowAsync();
                }

                currentMenu.AddMenuItems(selectedSearchResults);
                unsavedChanges = true;
            }

            RefreshCurrentMenuInPage();
        }

        private async void RemoveFromCurrentMenu(object sender, RoutedEventArgs e)
        {
            List<MenuItem> selectedCurrentMenuItems = CurrentMenu.SelectedItems.Cast<MenuItem>().ToList();

            if (selectedCurrentMenuItems.Count > 0)
            {
                currentMenu.RemoveMenuItems(selectedCurrentMenuItems);
                unsavedChanges = true;
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
            if (currentMenu.Name != CurrentMenuName.Text)
            {
                currentMenu.Name = CurrentMenuName.Text;
                unsavedChanges = true;
            }
        }

        private void UpdateCurrentMenuLocation(object sender, RoutedEventArgs e)
        {
            if (currentMenu.Location != CurrentMenuLocation.Text)
            {
                currentMenu.Location = CurrentMenuLocation.Text;
                unsavedChanges = true;
            }
        }

        private async Task<bool> CheckUnsavedChangesAndContinue()
        {
            if (unsavedChanges)
            {
                UnsavedChanges unsavedChangesDialog = new UnsavedChanges();
                await unsavedChangesDialog.ShowAsync();

                if (unsavedChangesDialog.DiscardChanges)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private async void CreateNewMenu(object sender, RoutedEventArgs e)
        {
            bool continueSafely = await CheckUnsavedChangesAndContinue();
            if (continueSafely)
            {
                currentMenu = new Menu
                {
                    Name = newMenuName,
                    Location = newMenuLocation
                };
                unsavedChanges = false;
                RefreshCurrentMenuInPage();
            }
        }

        private async void SaveMenuToDatabase(object sender, RoutedEventArgs e)
        {
            bool successfulSave = DBHelper.AddMenu(currentMenu);
            unsavedChanges = !successfulSave;

            if (successfulSave)
            {
                string messageText = resourceLoader.GetString("Message_DatabaseSaveSuccessful");
                MessageDialog successMessage = new MessageDialog(messageText);
                await successMessage.ShowAsync();
            }
            else
            {
                string messageText = resourceLoader.GetString("Message_DatabaseSaveFailed");
                MessageDialog failureMessage = new MessageDialog(messageText);
                await failureMessage.ShowAsync();
            }

            // TODO: Move to button that deals with loading a menu from the database.
            /* string xml = currentMenu.ExportMenuItemsToXML();

            List<MenuItem> items = new List<MenuItem>();
            items = Menu.ImportMenuItemsFromXML(xml); */
        }

        private async void ImportMenuFromFile(object sender, RoutedEventArgs e)
        {
            bool continueSafely = await CheckUnsavedChangesAndContinue();
            if (continueSafely)
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
                    unsavedChanges = false;
                    RefreshCurrentMenuInPage();
                }
                else
                {
                    LoggingServices.Instance.WriteLine<MenuPage>("Cancelled opening file.", LogLevel.Info);
                }
            }
        }

        private async void ExportMenuToFile(object sender, RoutedEventArgs e)
        {
            if (Menu.IsMenuValid(currentMenu))
            {
                string menuXML = currentMenu.ExportMenuToXML();
                FileHelper.WriteToFile(menuXML, currentMenu.Name);
                unsavedChanges = false;
            }
            else
            {
                MessageDialog errorMessage = new MessageDialog("Error: Cannot export invalid menu.");
                await errorMessage.ShowAsync();
                LoggingServices.Instance.WriteLine<MenuPage>("ExportCurrentMenu failed due to invalid menu.", LogLevel.Error);
            }
        }

        private async void ShowCreateMenuItemDialog(object sender, RoutedEventArgs e)
        {
            CreateMenuItem createDialog = new CreateMenuItem();
            await createDialog.ShowAsync();
        }
    }
}

