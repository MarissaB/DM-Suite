using System.Reflection;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace DM_Suite
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomeScreen : Page
    {
        public HomeScreen()
        {
            this.InitializeComponent();
            Heading.Text = ResourceLoader.GetForCurrentView().GetString("Home_Heading") + " v" + Assembly.GetEntryAssembly().GetName().Version;
        }
    }
}
