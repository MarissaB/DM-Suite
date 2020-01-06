using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace DM_Suite.Initiative_Features
{
    public sealed partial class AddParticipant : ContentDialog
    {
        public AddParticipant()
        {
            InitializeComponent();
        }

        public string NameInput { get; private set; }
        public int InitiativeInput { get; private set; }
        public string CurrentSession { get; set; }
        public string SessionInput { get; set; }
        public bool Confirm { get; private set; }

        private List<string> SessionsList { get; set; }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            IsPrimaryButtonEnabled = false;
            SessionsList = InitiativeDBHelper.GetSessionsList();
            SessionSelection.ItemsSource = SessionsList;
            if (SessionsList.Contains(CurrentSession))
            {
                AddToNewSession.IsChecked = false;
                ToggleAddingToNew(false);
            }
            else
            {
                ToggleAddingToNew(true);
                AddToNewSession.IsEnabled = false;
                AddToNewSession.IsChecked = true;
            }
            
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            NameInput = NameBox.Text;
            if (AddToNewSession.IsChecked.Value)
            {
                SessionInput = SessionBox.Text;
            }
            else
            {
                SessionInput = SessionSelection.SelectedValue.ToString();
            }
            Confirm = true;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Confirm = false;
        }

        private void Initiative_TextChanged(object sender, TextChangedEventArgs e)
        {
            IsPrimaryButtonEnabled = IsInitiativeValid();
        }

        private bool IsInitiativeValid()
        {
            try
            {
                InitiativeInput = Convert.ToInt32(Initiative.Text);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void AddToNewSession_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ToggleAddingToNew(AddToNewSession.IsChecked.Value);
        }

        private void ToggleAddingToNew(bool addingNew)
        {
            if (addingNew)
            {
                SessionBox.IsEnabled = true;
                SessionBox.Visibility = Visibility.Visible;
                SessionBox.Text = CurrentSession;

                SessionSelection.IsEnabled = false;
                SessionSelection.Visibility = Visibility.Collapsed;
            }
            else
            {
                SessionBox.IsEnabled = false;
                SessionBox.Visibility = Visibility.Collapsed;
                SessionBox.Text = CurrentSession;

                SessionSelection.IsEnabled = true;
                SessionSelection.Visibility = Visibility.Visible;
                SessionSelection.SelectedValue = CurrentSession;
            }
        }
    }
}
