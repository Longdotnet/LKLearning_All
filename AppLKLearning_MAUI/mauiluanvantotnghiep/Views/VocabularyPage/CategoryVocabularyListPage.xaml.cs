using CommunityToolkit.Maui.Views;

namespace mauiluanvantotnghiep.Views.VocabularyPage
{
    public partial class CategoryVocabularyListPage : ContentPage
    {
        public CategoryVocabularyListPage()
        {
            InitializeComponent();
        }

        // UK Audio Event Handler
        private void OnPlayClickedUk(object sender, EventArgs e)
        {
            if (sender is ImageButton btn &&
                btn.CommandParameter is string url &&
                !string.IsNullOrWhiteSpace(url))
            {
                // Gán URL vào MediaElement và phát
                mediaPlayerUk.Source = url;
                mediaPlayerUk.Play();
            }
        }

        // US Audio Event Handler
        private void OnPlayClickedUs(object sender, EventArgs e)
        {
            if (sender is ImageButton btn &&
                btn.CommandParameter is string url &&
                !string.IsNullOrWhiteSpace(url))
            {
                // Gán URL vào MediaElement và phát
                mediaPlayerUs.Source = url;
                mediaPlayerUs.Play();
            }
        }
    }
}