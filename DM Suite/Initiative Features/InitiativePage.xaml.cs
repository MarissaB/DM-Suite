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
        private List<string> SessionList = new List<string>();
        private string CurrentSessionName = ResourceLoader.GetForCurrentView().GetString("Initiative_Default");
        private List<Participant> CurrentSession = new List<Participant>();
        private Participant CurrentParticipant = new Participant();
        
        public InitiativePage()
        {
            this.InitializeComponent();
            SessionList = InitiativeDBHelper.GetSessionsList();
            PreviousParticipant.IsEnabled = false;
            NextParticipant.IsEnabled = false;
        }

        private async void ShowAddParticipantItemDialog(object sender, RoutedEventArgs e)
        {
            AddParticipant addDialog = new AddParticipant
            {
                CurrentSession = CurrentSessionName
            };
            await addDialog.ShowAsync();

            string participantName = addDialog.NameInput;
            int participantInitiative = addDialog.InitiativeInput;
            string participantSession = addDialog.SessionInput;
            bool add = addDialog.Confirm;

            if (add)
            {
                Participant participant = new Participant
                {
                    Name = participantName,
                    Initiative = participantInitiative,
                    Session = participantSession,
                    Active = true
                };
                InitiativeDBHelper.AddParticipant(participant);
                SessionList = InitiativeDBHelper.GetSessionsList(); // Selected item will be set to 'null'
                Sessions.ItemsSource = SessionList;
                Sessions.SelectedIndex = GetCurrentSessionIndex(); // Set the selected session to match the current one
                RefreshSessionView();
            }

        }

        private void Sessions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Sessions.SelectedValue != null) // Selecting a new session
            {
                if (Sessions.SelectedValue.ToString() != CurrentSessionName)
                {
                    // Picking a different session from what was already there, so start fresh
                    CurrentSession.Clear();
                    CurrentSessionName = Sessions.SelectedValue.ToString();
                    CurrentSession = InitiativeDBHelper.GetSession(CurrentSessionName);
                    InitiativeFlipper.ItemsSource = CurrentSession;
                    UpdateTimeline(0);
                }
            }
        }

        private void UpdateTimeline(int index)
        {
            PreviousParticipant.IsEnabled = true;
            NextParticipant.IsEnabled = true;
            Participant previous = CurrentSession[GetPrevious(index)];
            Participant next = CurrentSession[GetNext(index)];
            CurrentParticipant = CurrentSession[index];
            Previous.Text = previous.Name + " (" + previous.Initiative + ")";
            Current.Text = CurrentParticipant.Name + " (" + CurrentParticipant.Initiative + ")";
            Next.Text = next.Name + " (" + next.Initiative + ")";
            InitiativeFlipper.SelectedItem = CurrentParticipant;
        }

        private void RefreshSessionView()
        {
            CurrentSession = InitiativeDBHelper.GetSession(CurrentSessionName);
            InitiativeFlipper.ItemsSource = CurrentSession;
            int index = GetCurrentParticipantIndex();
            if (index >= 0)
            {
                UpdateTimeline(index);
                InitiativeFlipper.SelectedIndex = index;
            }
        }

        private int GetCurrentParticipantIndex()
        {
            int index = -1;
            foreach (Participant participant in CurrentSession)
            {
                if (participant.Name == CurrentParticipant.Name && participant.Initiative == participant.Initiative)
                {
                    index = CurrentSession.IndexOf(participant);
                    CurrentParticipant = participant;
                }
            }

            return index;
        }

        private int GetCurrentSessionIndex()
        {
            int index = -1;
            foreach (string sessionName in SessionList)
            {
                if (sessionName == CurrentSessionName)
                {
                    index = SessionList.IndexOf(sessionName);
                }
            }
            return index;
            
        }

        private int GetPrevious(int current)
        {
            if (current - 1 < 0)
            {
                return CurrentSession.Count - 1; // Last in the list just finished their turn, so initiative is restarting
            }
            else
            {
                return current - 1;
            }
        }

        private int GetNext(int current)
        {
            if (current + 1 > CurrentSession.Count - 1)
            {
                return 0; // Last in the list is on their turn, so the initiative is restarting next
            }
            else
            {
                return current + 1;
            }
        }

        private void PreviousParticipant_Click(object sender, RoutedEventArgs e)
        {
            int current = CurrentSession.IndexOf(CurrentParticipant);
            UpdateTimeline(GetPrevious(current));
        }

        private void NextParticipant_Click(object sender, RoutedEventArgs e)
        {
            int current = CurrentSession.IndexOf(CurrentParticipant);
            UpdateTimeline(GetNext(current));
        }
    }
}
