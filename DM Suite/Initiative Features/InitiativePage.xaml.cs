using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace DM_Suite.Initiative_Features
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InitiativePage : Page
    {
        public InitiativePage()
        {
            this.InitializeComponent();
            Sessions.ItemsSource = InitiativeDBHelper.GetSessionsList();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Sessions.ItemsSource = InitiativeDBHelper.GetSessionsList();
        }
    }
}
