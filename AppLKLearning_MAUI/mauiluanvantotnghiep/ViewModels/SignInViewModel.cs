using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mauiluanvantotnghiep.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.ViewModels
{
    public partial class SignInViewModel : ObservableObject
    {
        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private bool isPassword = true; // Mặc định ẩn mật khẩu

        [ObservableProperty]
        private string eyeIconSource = "eye_closed.png"; // Biểu tượng ban đầu

        [ObservableProperty]
        private string emailError; // Thông báo lỗi email

        [ObservableProperty]
        private bool isEmailErrorVisible; // Kiểm soát hiển thị lỗi email

        [ObservableProperty]
        private string passwordError; // Thông báo lỗi password

        [ObservableProperty]
        private bool isPasswordErrorVisible; // Kiểm soát hiển thị lỗi password

        [ObservableProperty]
        private string generalError; // Thông báo lỗi chung

        private bool isEmailValid; // Trạng thái email hợp lệ

        public SignInViewModel()
        {
            // Khởi tạo các lệnh bằng RelayCommand
        }

       
        partial void OnEmailChanged(string value)
        {
            ValidateEmail(); // Chỉ kiểm tra email khi email thay đổi
        }

        partial void OnPasswordChanged(string value)
        {
            if (isEmailValid)
            {
                ValidatePassword(); // Chỉ kiểm tra password nếu email hợp lệ
            }
        }

        private void ValidateEmail()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                EmailError = "Email không được để trống.";
                IsEmailErrorVisible = true;
                isEmailValid = false;
                GeneralError = "Vui lòng điền đầy đủ và đúng các trường.";
            }
            else
            {
                string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                if (!Regex.IsMatch(Email, emailPattern))
                {
                    EmailError = "Vui lòng nhập đúng định dạng Email.";
                    IsEmailErrorVisible = true;
                    isEmailValid = false;
                    GeneralError = "Vui lòng điền đầy đủ và đúng các trường.";
                }
                else
                {
                    EmailError = null;
                    IsEmailErrorVisible = false;
                    isEmailValid = true;
                    // Kiểm tra password nếu đã có giá trị
                    if (!string.IsNullOrWhiteSpace(Password))
                    {
                        ValidatePassword();
                    }
                }
            }
        }

        private void ValidatePassword()
        {
            if (isEmailValid)
            {
                if (string.IsNullOrWhiteSpace(Password))
                {
                    PasswordError = "Mật khẩu không được để trống.";
                    IsPasswordErrorVisible = true;
                    GeneralError = "Vui lòng điền đầy đủ và đúng các trường.";
                }
                else
                {
                    PasswordError = null;
                    IsPasswordErrorVisible = false;
                    GeneralError = null; // Xóa lỗi chung nếu tất cả hợp lệ
                }
            }
        }

        [RelayCommand]
        private void TogglePasswordVisibility()
        {
            IsPassword = !IsPassword;
            EyeIconSource = IsPassword ? "eye_closed.png" : "eye.png";
        }

        [RelayCommand]
        private async Task SignInAsync()
        {
            ValidateEmail(); // Kiểm tra email trước
            if (!isEmailValid || !string.IsNullOrWhiteSpace(PasswordError))
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", GeneralError ?? "Vui lòng sửa các lỗi trước khi đăng nhập.", "OK");
                return;
            }

            try
            {
                var apiUrl = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyKhachHang/DangNhap";
                var loginData = new { email = Email, password = Password };

                var json = JsonSerializer.Serialize(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true
                };

                using var client = new HttpClient(handler);
                var response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var resultJson = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var loginResponse = JsonSerializer.Deserialize<LoginResponse>(resultJson, options);

                    if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                    {
                        await SecureStorage.SetAsync("auth_token", loginResponse.Token);
                        MessagingCenter.Send(this, "UnfocusEntries");
                       
                        await Shell.Current.GoToAsync("//CoursePage");
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Lỗi", "Đăng nhập không thành công, vui lòng thử lại.", "OK");
                    }
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    await Application.Current.MainPage.DisplayAlert("Lỗi", errorMsg, "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", ex.Message, "OK");
            }
        }

        [RelayCommand]
        private async Task TestAsync()
        {
            await Shell.Current.DisplayAlert("Test", "SignUpCommand vẫn hoạt động", "OK");
        }

        [RelayCommand]
        private async Task NavigateToSignUpAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("//RegisterPage");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Nav Error", ex.Message, "OK");
                Debug.WriteLine("Nav Error: " + ex);
            }
        }

        [RelayCommand]
        private async Task NavigateToForgotPasswordAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("//ForgotPasswordPage");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Nav Error", ex.Message, "OK");
                Debug.WriteLine("Nav Error: " + ex);
            }
        }
    }
}