using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace DM_Suite
{
    public sealed partial class ConfirmDelete : ContentDialog
    {
        public ConfirmDelete()
        {
            InitializeComponent();
        }

        public bool DeleteRecord { get; private set; }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            DeleteRecord = true;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            DeleteRecord = false;
        }
    }
}
