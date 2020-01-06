using Microsoft.Toolkit.Uwp.UI.Extensions;
using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

namespace DM_Suite.Initiative_Features
{
    public sealed partial class AddParticipant : ContentDialog
    {
        public AddParticipant()
        {
            InitializeComponent();
            IsPrimaryButtonEnabled = false;
            if (CurrentSession != null)
            {
                SessionBox.IsEnabled = false;
                SessionBox.Text = CurrentSession;
                AddToNewSession.IsChecked = false;
            }
            else
            {
                SessionBox.IsEnabled = true;
                AddToNewSession.IsEnabled = false;
                AddToNewSession.IsChecked = true;
            }
        }

        public string NameInput { get; private set; }
        public int InitiativeInput { get; private set; }
        public string CurrentSession { get; set; }
        public string SessionInput { get; set; }
        public bool Confirm { get; private set; }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            NameInput = NameBox.Text;
            SessionInput = SessionBox.Text;
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
            SessionBox.IsEnabled = AddToNewSession.IsChecked.Value;
        }
    }
}
