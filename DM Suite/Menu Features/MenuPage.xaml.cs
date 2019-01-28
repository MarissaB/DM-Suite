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
            MenuSearchResults.ItemsSource = DBHelper.SearchMenus(string.Empty, string.Empty);
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
                string errorText = resourceLoader.GetString("Errors_MenuInvalid");
                MessageDialog errorMessage = new MessageDialog(errorText);
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
            if (((Button)sender).Name == "MenuItemSearch")
            {
                SearchMenuItems();
            }

            if (((Button)sender).Name == "MenuSearch")
            {
                SearchMenus();
            }
        }

        private void SearchResults_AutoGeneratingColumn(object sender, Microsoft.Toolkit.Uwp.UI.Controls.DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.ToString() == "Cost")
            {
                e.Column.Header = "Cost (gold)";
            }

            if (e.Column.Header.ToString() == "MenuItems")
            {
                e.Cancel = true;
            }
        }

        private void ClearSearch(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Name == "MenuItemReset")
            {
                InputMenuItemSearch_Box.Text = string.Empty;
                MenuItemSearchResults.ItemsSource = new List<MenuItem>();
                MenuItemResultsCount.Visibility = Visibility.Collapsed;
            }

            if (((Button)sender).Name == "MenuReset")
            {
                InputMenuNameSearch_Box.Text = string.Empty;
                InputMenuLocationSearch_Box.Text = string.Empty;
                MenuSearchResults.ItemsSource = new List<Menu>();
                MenuResultsCount.Visibility = Visibility.Collapsed;
            }
        }

        private void SearchMenuItems()
        {
            List<MenuItem> searchResults = DBHelper.SearchMenuItems(InputMenuItemSearch_Box.Text, GetTypes(), CostMin.Text, CostMax.Text);
            MenuItemSearchResults.ItemsSource = searchResults;
            MenuItemResultsCount.Text = resourceLoader.GetString("Heading_Results") + searchResults.Count;
            MenuItemResultsCount.Visibility = Visibility.Visible;
        }

        private void SearchMenus()
        {
            List<Menu> searchResults = DBHelper.SearchMenus(InputMenuNameSearch_Box.Text, InputMenuLocationSearch_Box.Text);
            MenuSearchResults.ItemsSource = searchResults;
            MenuResultsCount.Text = resourceLoader.GetString("Heading_Results") + searchResults.Count;
            MenuResultsCount.Visibility = Visibility.Visible;
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

        private async Task<bool> ConfirmDeletion()
        {
            ConfirmDelete confirmDeleteDialog = new ConfirmDelete();
            await confirmDeleteDialog.ShowAsync();

            if (confirmDeleteDialog.DeleteRecord)
            {
                return true;
            }
            else
            {
                return false;
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
            if (Menu.IsMenuValid(currentMenu))
            {
                bool successfulSave;
                int menuCount = DBHelper.SearchMenus(currentMenu.Name, string.Empty).Count;
                if (menuCount == 0) // Menu with this name doesn't exist yet, so we add it
                {
                    successfulSave = DBHelper.AddMenu(currentMenu);
                    unsavedChanges = !successfulSave;
                }
                else // Menu with this name exists, so we update it instead of adding since names are unique
                {
                    successfulSave = DBHelper.UpdateMenu(currentMenu);
                    unsavedChanges = !successfulSave;
                }
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
            }
            else
            {
                string messageText = resourceLoader.GetString("Errors_MenuInvalid");
                MessageDialog failureMessage = new MessageDialog(messageText);
                await failureMessage.ShowAsync();
            }
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
                string messageText = resourceLoader.GetString("Errors_MenuInvalid");
                MessageDialog failureMessage = new MessageDialog(messageText);
                await failureMessage.ShowAsync();
            }
        }

        private async void ShowCreateMenuItemDialog(object sender, RoutedEventArgs e)
        {
            CreateMenuItem createDialog = new CreateMenuItem();
            await createDialog.ShowAsync();

            string itemName = createDialog.NameInput;
            string itemDescription = createDialog.DescriptionInput;
            string itemCost = createDialog.CostInput;
            string itemType = createDialog.TypeInput.ToUpper();

            if (MenuItem.IsMenuItemValid(itemName, itemCost, itemType))
            {
                MenuItem newItem = new MenuItem(itemName, itemDescription, itemCost, itemType);
                bool successfulSave = false;

                if(string.IsNullOrEmpty(DBHelper.SearchMenuItem(newItem).Name)) // check if item already exists in database
                { 
                    successfulSave = DBHelper.AddMenuItem(newItem);
                }
                
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
            }
            else
            {
                string messageText = resourceLoader.GetString("Errors_MenuItemInvalid");
                MessageDialog failureMessage = new MessageDialog(messageText);
                await failureMessage.ShowAsync();
            }

        }

        private async void MenuSearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MenuSearchResults.SelectedItem != null)
            {
                MenuDelete.IsEnabled = true;
                MenuDeleteIcon.Opacity = 1;
                bool continueSafely = await CheckUnsavedChangesAndContinue();
                if (continueSafely)
                {
                    currentMenu = (Menu)MenuSearchResults.SelectedItem;
                    unsavedChanges = false;
                    RefreshCurrentMenuInPage();
                }
                else
                {
                    MenuSearchResults.SelectedItem = null;
                }
            }
            else
            {
                MenuDelete.IsEnabled = false;
                MenuDeleteIcon.Opacity = 0.6;
            }
        }

        private async void DeleteSelectedMenu(object sender, RoutedEventArgs e)
        {
            bool continueSafely = await ConfirmDeletion();
            if (continueSafely)
            {
                bool isSuccessful = DBHelper.DeleteMenu((Menu)MenuSearchResults.SelectedItem);
                if (isSuccessful)
                {
                    string messageText = resourceLoader.GetString("Message_DatabaseDeleteSuccessful");
                    MessageDialog successMessage = new MessageDialog(messageText);
                    await successMessage.ShowAsync();
                }
                else
                {
                    string messageText = resourceLoader.GetString("Message_DatabaseDeleteFailed");
                    MessageDialog failureMessage = new MessageDialog(messageText);
                    await failureMessage.ShowAsync();
                }

                SearchMenus();
            }
        }

        private void MenuItemSearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MenuItemSearchResults.SelectedItems.Count > 0)
            {
                MenuItemDelete.IsEnabled = true;
                MenuItemDeleteIcon.Opacity = 1;
            }
            else
            {
                MenuItemDelete.IsEnabled = false;
                MenuItemDeleteIcon.Opacity = 0.6;
            }

            if (MenuItemSearchResults.SelectedItems.Count == 1)
            {
                MenuItemEdit.IsEnabled = true;
                MenuItemEditIcon.Opacity = 1;
            }
            else
            {
                MenuItemEdit.IsEnabled = false;
                MenuItemEditIcon.Opacity = 0.6;
            }
        }

        private async void DeleteSelectedMenuItems(object sender, RoutedEventArgs e)
        {
            bool continueSafely = await ConfirmDeletion();
            if (continueSafely)
            {
                int successes = 0;
                foreach (object selected in MenuItemSearchResults.SelectedItems)
                {
                    bool isSuccessful = DBHelper.DeleteMenuItem((MenuItem)selected);
                    if (isSuccessful)
                    {
                        successes++;
                    }
                }

                if (successes == MenuItemSearchResults.SelectedItems.Count)
                {
                    string messageText = resourceLoader.GetString("Message_DatabaseDeleteSuccessful");
                    MessageDialog successMessage = new MessageDialog(messageText);
                    await successMessage.ShowAsync();
                }
                else
                {
                    string messageText = resourceLoader.GetString("Message_DatabaseDeleteFailed");
                    MessageDialog failureMessage = new MessageDialog(messageText);
                    await failureMessage.ShowAsync();
                }

                SearchMenuItems();            
            }
        }

        private async void EditMenuItem(object sender, RoutedEventArgs e)
        {
            MenuItem editingItem = (MenuItem)MenuItemSearchResults.SelectedItem;
            EditMenuItem editDialog = new EditMenuItem
            {
                NameInput = editingItem.Name,
                CostInput = editingItem.Cost.ToString(),
                DescriptionInput = editingItem.Description,
                TypeInput = editingItem.Type
            };
            await editDialog.ShowAsync();

            if (editDialog.MadeChanges)
            {
                if (MenuItem.IsMenuItemValid(editDialog.NameInput, editDialog.CostInput, editDialog.TypeInput))
                {
                    editingItem.Cost = Convert.ToDecimal(editDialog.CostInput);
                    editingItem.Description = editDialog.DescriptionInput;
                    editingItem.Type = editDialog.TypeInput;

                    bool successfulUpdate = DBHelper.UpdateMenuItem(editingItem);

                    if (successfulUpdate)
                    {
                        string messageText = resourceLoader.GetString("Message_DatabaseUpdateSuccessful");
                        MessageDialog successMessage = new MessageDialog(messageText);
                        await successMessage.ShowAsync();
                        SearchMenuItems();
                    }
                    else
                    {
                        string messageText = resourceLoader.GetString("Message_DatabaseUpdateFailed");
                        MessageDialog failureMessage = new MessageDialog(messageText);
                        await failureMessage.ShowAsync();
                    }
                }
                else
                {
                    string messageText = resourceLoader.GetString("Errors_MenuItemInvalid");
                    MessageDialog failureMessage = new MessageDialog(messageText);
                    await failureMessage.ShowAsync();
                }
            }

        }
    }
}

