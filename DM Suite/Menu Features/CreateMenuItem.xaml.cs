using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

namespace DM_Suite.Menu_Features
{
    public sealed partial class CreateMenuItem : ContentDialog
    {
        public CreateMenuItem()
        {
            InitializeComponent();
        }

        private ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();
        public string NameInput { get; private set; }
        public string DescriptionInput { get; private set; }
        public string CostInput { get; private set; }
        public string TypeInput { get; private set; }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            NameInput = Name.Text;
            DescriptionInput = Description.Text;
            CostInput = Cost.Text;
            foreach (RadioButton radioButton in TypeSelections.Children)
            {
                if ((bool)radioButton.IsChecked)
                {
                    TypeInput = radioButton.Content.ToString();
                }
            }
            
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        
    }
}
