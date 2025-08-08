using mauiluanvantotnghiep.ViewModels.VocabularyViewModel;
using CommunityToolkit.Maui.Views;

namespace mauiluanvantotnghiep.Views.VocabularyPage
{
    [QueryProperty(nameof(VocabularyId), "vocabularyId")]
    public partial class VocabularyDetailPage : ContentPage
    {
        private int _vocabularyId;
        private VocabularyDetailViewModel _viewModel;

        public int VocabularyId
        {
            get => _vocabularyId;
            set
            {
                _vocabularyId = value;
                if (_vocabularyId > 0)
                {
                    _viewModel = new VocabularyDetailViewModel(_vocabularyId);
                    BindingContext = _viewModel;
                }
            }
        }

        public VocabularyDetailPage()
        {
            InitializeComponent();
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }

        private void OnTabClicked(object sender, EventArgs e)
        {
            if (sender is not Button button) return;

            // Reset all tab styles
            ResetTabStyles();

            // Apply selected style
            button.Style = (Style)Resources["ActiveTab"];

            // Show/hide content based on tab
            string tab = button.CommandParameter?.ToString();
            ShowTabContent(tab);

            // Update ViewModel's selected tab if property exists
            if (_viewModel != null)
            {
                _viewModel.SelectedTab = tab;
            }
        }

        private void ResetTabStyles()
        {
            var inactiveStyle = (Style)Resources["InactiveTab"];
            TabMeanings.Style = inactiveStyle;
            TabExamples.Style = inactiveStyle;
            TabAdditional.Style = inactiveStyle;
        }

        private void ShowTabContent(string tab)
        {
            // Hide all content
            MeaningsView.IsVisible = false;
            ExamplesView.IsVisible = false;
            AdditionalView.IsVisible = false;

            // Show selected content
            switch (tab)
            {
                case "Meanings":
                    MeaningsView.IsVisible = true;
                    break;
                case "Examples":
                    ExamplesView.IsVisible = true;
                    break;
                case "Additional":
                    AdditionalView.IsVisible = true;
                    break;
            }
        }

        private void OnPlayClickedItemLisstUk(object sender, EventArgs e)
        {
            PlayAudio(sender, mediaPlayerlistUk);
        }

        private void OnPlayClickedItemLisstUs(object sender, EventArgs e)
        {
            PlayAudio(sender, mediaPlayerlistUs);
        }

        private void PlayAudio(object sender, MediaElement mediaPlayer)
        {
            if (sender is ImageButton btn &&
                btn.CommandParameter is string url &&
                !string.IsNullOrWhiteSpace(url))
            {
                try
                {
                    mediaPlayer.Source = url;
                    mediaPlayer.Play();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[PlayAudio] Error: {ex.Message}");
                }
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            
            // Stop any playing audio
            mediaPlayerlistUk?.Stop();
            mediaPlayerlistUs?.Stop();
            
            // Cleanup ViewModel if it implements IDisposable
            if (_viewModel is IDisposable disposable)
                disposable.Dispose();
        }
    }
}