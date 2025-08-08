using CommunityToolkit.Maui.Views;

namespace mauiluanvantotnghiep.Views.VideoPopupPage;

public partial class VideoFullScreenPopup : Popup
{
    public VideoFullScreenPopup(WebViewSource source)
    {
        InitializeComponent();
        FullScreenWebView.Source = source;
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        Close();
    }
}
