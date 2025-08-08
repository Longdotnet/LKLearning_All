
using mauiluanvantotnghiep.ViewModels;
using Microsoft.Maui.Controls;

namespace mauiluanvantotnghiep.Views.VocabularyPage;

public partial class SearchVocabulariesPage : ContentPage
{
	public SearchVocabulariesPage()
	{
		InitializeComponent();
	}

    // Sự kiện khi nhấn nút Play bên mỗi item
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

    //sự kiện khi nhấp vào nút play trong listNew
    private void OnPlayClickedItemLisstUk(object sender, EventArgs e)
    {
        if (sender is ImageButton btn &&
            btn.CommandParameter is string url &&
            !string.IsNullOrWhiteSpace(url))
        {
            // Gán URL vào MediaElement và phát
            mediaPlayerlistUk.Source = url;
            mediaPlayerlistUk.Play();
        }
    }


    //sự kiện khi nhấp vào nút play trong listNew
    private void OnPlayClickedItemLisstUs(object sender, EventArgs e)
    {
        if (sender is ImageButton btn &&
            btn.CommandParameter is string url &&
            !string.IsNullOrWhiteSpace(url))
        {
            // Gán URL vào MediaElement và phát
            mediaPlayerlistUs.Source = url;
            mediaPlayerlistUs.Play();
        }
    }







}