using mauiluanvantotnghiep.Models;
using Plugin.Firebase.CloudMessaging;

namespace mauiluanvantotnghiep.Views.SignInPage;

public partial class SignInPage : ContentPage
{

    public SignInPage()
	{
		InitializeComponent();
	}
    private async void OnCounterClicked(object sender, EventArgs e)
    {
        await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
        var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
        Console.WriteLine($"FCM token: {token}");
        
    }





}