using Windows.UI.Xaml.Controls;

namespace DM_Suite
{
    public sealed partial class UnsavedChanges : ContentDialog
    {
        public UnsavedChanges()
        {
            InitializeComponent();
        }

        public bool DiscardChanges { get; private set; }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            DiscardChanges = true;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            DiscardChanges = false;
        }
    }
}
