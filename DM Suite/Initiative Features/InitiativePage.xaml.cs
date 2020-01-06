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
        private Participant CurrentParticipant = new Participant();
        public InitiativePage()
        {
            this.InitializeComponent();
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
            
            if (Sessions.SelectedValue != null)
            {
                CurrentSession.Clear();
                CurrentSessionName = Sessions.SelectedValue.ToString();
                CurrentSession = InitiativeDBHelper.GetSession(CurrentSessionName);
                Flippy.ItemsSource = CurrentSession;
                if (CurrentSession.Count > 2)
                {
                    UpdateTimeline(0);
                }
            }
        }

        private void UpdateTimeline(int index)
        {
            Participant previous = CurrentSession[GetPrevious(index)];
            Participant next = CurrentSession[GetNext(index)];
            CurrentParticipant = CurrentSession[index];
            Previous.Text = previous.Name + " (" + previous.Initiative + ")";
            Current.Text = CurrentParticipant.Name + " (" + CurrentParticipant.Initiative + ")";
            Next.Text = next.Name + " (" + next.Initiative + ")";
            Flippy.SelectedItem = CurrentParticipant;
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

        private void Flippy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //int current = CurrentSession.IndexOf(CurrentParticipant);
            //UpdateTimeline(GetNext(current));
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
