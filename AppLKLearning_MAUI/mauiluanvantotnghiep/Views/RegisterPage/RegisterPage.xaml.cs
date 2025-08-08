using System.Threading.Tasks;

namespace mauiluanvantotnghiep.Views.RegisterPage;

public partial class RegisterPage : ContentPage
{
	public RegisterPage()
	{
		InitializeComponent();
	}


    //chuyển sang trang đăng nhập 
    private async void SignInTapper(object sender, EventArgs e)
    {
        // Xử lý logic chuyển trang
        await Shell.Current.GoToAsync("//SignInPage");  
    }


}