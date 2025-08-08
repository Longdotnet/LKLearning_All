namespace mauiluanvantotnghiep.Views.WelcomePage;

public partial class WelcomePage : ContentPage
{
	public WelcomePage()
	{
		InitializeComponent();


        // Ban ??u ?n 2 Frame ph?
        subFrame.IsVisible = false;
        subFrame2.IsVisible = false;
        subFrame3.IsVisible = false;
        subframe4.IsVisible = false;

        // Th?c hi?n animation khi trang load
        RunEntranceAnimation();


    }


    private async void RunEntranceAnimation()
    {
        // Thi?t l?p ban ??u cho mainFrame: thu phóng to và ?n ?i
        await Task.WhenAll
        (
            mainFrame.ScaleTo(10, 0),
            mainFrame.FadeTo(0, 0)
        );

        // Animation cho mainFrame t? to v? kích th??c ban ??u và hi?n th? d?n
        await Task.WhenAll
        (
            mainFrame.ScaleTo(1, 1000, Easing.SinIn),
            mainFrame.FadeTo(0.7, 1000, Easing.SinOut)
        );

        // Hi?n th? l?n l??t 2 Frame ph? v?i hi?u ?ng fade
        subFrame.IsVisible = true;
        subFrame2.IsVisible = true;
        subFrame3.IsVisible = true;
        subframe4.IsVisible = true;


        await Task.WhenAll
        (
            subFrame.FadeTo(0.7, 300, Easing.SinIn),
            subFrame2.FadeTo(0.7, 300, Easing.SinIn),
             subFrame3.FadeTo(0.7, 300, Easing.SinIn),
            subframe4.FadeTo(0.7, 300, Easing.SinIn)

        );
    }

    private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
    {
        // Khi ng??i dùng ch?m vào bi?u t??ng "?", ?n d?n các Frame ph?
        await Task.WhenAll
        (
            subFrame.FadeTo(0, 300, Easing.SinOut),
            subFrame2.FadeTo(0, 300, Easing.SinOut),
            subFrame3.FadeTo(0, 300, Easing.SinInOut),
            subframe4.FadeTo(0, 300, Easing.SinInOut)
        );

        // Sau ?ó ?n d?n mainFrame v?i hi?u ?ng fade và scale
        await Task.WhenAll
        (
            mainFrame.FadeTo(0, 1000, Easing.SinIn),
            mainFrame.ScaleTo(10, 1000, Easing.SinOut)
        );

        // Ví d? ?i?u h??ng ??n trang MainPage sau animation
        await Shell.Current.GoToAsync("//SignInPage");
    }
}