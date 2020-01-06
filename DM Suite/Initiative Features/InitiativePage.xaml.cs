using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System;
using Windows.ApplicationModel.Resources;
using System.Collections.Generic;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace DM_Suite.Initiative_Features
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InitiativePage : Page
    {
        private string CurrentSessionName = ResourceLoader.GetForCurrentView().GetString("Initiative_Default");
        private List<Participant> CurrentSession = new List<Participant>();
        public InitiativePage()
        {
            this.InitializeComponent();
            Sessions.ItemsSource = InitiativeDBHelper.GetSessionsList();
        }

        private void RefreshSessionList_Click(object sender, RoutedEventArgs e)
        {
            Sessions.ItemsSource = InitiativeDBHelper.GetSessionsList();
        }

        private async void ShowAddParticipantItemDialog(object sender, RoutedEventArgs e)
        {
            AddParticipant addDialog = new AddParticipant();
            addDialog.CurrentSession = CurrentSessionName;
            await addDialog.ShowAsync();

            string participantName = addDialog.NameInput;
            int participantInitiative = addDialog.InitiativeInput;
            string participantSession = addDialog.SessionInput;
            bool add = addDialog.Confirm;
            
            if (add)
            {
                Participant participant = new Participant();
                participant.Name = participantName;
                participant.Initiative = participantInitiative;
                participant.Session = participantSession;
                participant.Active = true;
                InitiativeDBHelper.AddParticipant(participant);
                CurrentSession.Add(participant);
                Sessions.ItemsSource = InitiativeDBHelper.GetSessionsList();
            }

        }

        private void Sessions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentSession.Clear();
            CurrentSessionName = Sessions.SelectedValue.ToString();
            CurrentSession = InitiativeDBHelper.GetSession(CurrentSessionName);
        }
    }
}
