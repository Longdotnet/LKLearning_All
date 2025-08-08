using mauiluanvantotnghiep.ViewModels;

namespace mauiluanvantotnghiep.Views.VocabularyPage;

public partial class AllNewWordsPage : ContentPage
{
    public AllNewWordsPage()
    {
        InitializeComponent();
    }

    private void OnPlayClickedUk(object sender, EventArgs e)
    {
        if (sender is ImageButton btn &&
            btn.CommandParameter is string url &&
            !string.IsNullOrWhiteSpace(url))
        {
            mediaPlayerUk.Source = url;
            mediaPlayerUk.Play();
        }
    }

    private void OnPlayClickedUs(object sender, EventArgs e)
    {
        if (sender is ImageButton btn &&
            btn.CommandParameter is string url &&
            !string.IsNullOrWhiteSpace(url))
        {
            mediaPlayerUs.Source = url;
            mediaPlayerUs.Play();
        }
    }
}