using Windows.UI.Xaml.Controls;

namespace DM_Suite.Menu_Features
{
    public sealed partial class EditMenuItem : ContentDialog
    {
        public EditMenuItem()
        {
            InitializeComponent();
        }

        public string NameInput { get; set; }
        public string DescriptionInput { get; set; }
        public string CostInput { get; set; }
        public string TypeInput { get; set; }
        public bool MadeChanges { get; private set; }

        private void ContentDialog_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Name.Text = NameInput;
            Description.Text = DescriptionInput;
            Cost.Text = CostInput;
            foreach (RadioButton radioButton in TypeSelections.Children)
            {
                if (radioButton.Content.ToString() == TypeInput)
                {
                    radioButton.IsChecked = true;
                }
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            DescriptionInput = Description.Text;
            CostInput = Cost.Text;
            foreach (RadioButton radioButton in TypeSelections.Children)
            {
                if ((bool)radioButton.IsChecked)
                {
                    TypeInput = radioButton.Content.ToString();
                    MadeChanges = true;
                }
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            MadeChanges = false;
        }

        
    }
}
