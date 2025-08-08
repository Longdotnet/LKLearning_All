using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.ViewModels
{
    public partial class ForgotPasswordViewModel : ObservableObject
    {
        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string otp;

        [ObservableProperty]
        private string newPassword;

        [ObservableProperty]
        private string confirmPassword;

        [ObservableProperty]
        private string emailError;

        [ObservableProperty]
        private bool isEmailErrorVisible;

        [ObservableProperty]
        private string otpError;

        [ObservableProperty]
        private bool isOtpErrorVisible;

        [ObservableProperty]
        private string passwordError;

        [ObservableProperty]
        private bool isPasswordErrorVisible;

        [ObservableProperty]
        private string confirmPasswordError;

        [ObservableProperty]
        private bool isConfirmPasswordErrorVisible;

        [ObservableProperty]
        private string generalMessage;

        [ObservableProperty]
        private bool isOtpSectionVisible = false;

        [ObservableProperty]
        private bool isPasswordResetSectionVisible = false;

        [ObservableProperty]
        private bool isSendOtpButtonEnabled = true;

        [ObservableProperty]
        private bool isVerifyOtpButtonEnabled = false;

        [ObservableProperty]
        private bool isResetPasswordButtonEnabled = false;

        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private string sendOtpButtonText = "Gửi OTP";

        [ObservableProperty]
        private bool isNewPassword = true;

        [ObservableProperty]
        private bool isConfirmPasswordField = true;

        [ObservableProperty]
        private string newPasswordEyeIconSource = "eye_closed.png";

        [ObservableProperty]
        private string confirmPasswordEyeIconSource = "eye_closed.png";

        // Password validation properties
        [ObservableProperty]
        private bool isLengthValid;

        [ObservableProperty]
        private bool isUpperCaseValid;

        [ObservableProperty]
        private bool isLowerCaseValid;

        [ObservableProperty]
        private bool isDigitValid;

        [ObservableProperty]
        private bool isSpecialCharValid;

        private bool isEmailValid = false;
        private bool isOtpVerified = false;

        public ForgotPasswordViewModel()
        {
            // Khởi tạo
        }

        partial void OnEmailChanged(string value)
        {
            ValidateEmail();
        }

        partial void OnOtpChanged(string value)
        {
            ValidateOtp();
        }

        partial void OnNewPasswordChanged(string value)
        {
            ValidateNewPassword();
            ValidateConfirmPassword();
        }

        partial void OnConfirmPasswordChanged(string value)
        {
            ValidateConfirmPassword();
        }

        private void ValidateEmail()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                EmailError = "Email không được để trống.";
                IsEmailErrorVisible = true;
                isEmailValid = false;
                IsSendOtpButtonEnabled = false;
            }
            else
            {
                string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                if (!Regex.IsMatch(Email, emailPattern))
                {
                    EmailError = "Vui lòng nhập đúng định dạng Email.";
                    IsEmailErrorVisible = true;
                    isEmailValid = false;
                    IsSendOtpButtonEnabled = false;
                }
                else
                {
                    EmailError = null;
                    IsEmailErrorVisible = false;
                    isEmailValid = true;
                    IsSendOtpButtonEnabled = true;
                }
            }
        }

        private void ValidateOtp()
        {
            if (string.IsNullOrWhiteSpace(Otp))
            {
                OtpError = "OTP không được để trống.";
                IsOtpErrorVisible = true;
                IsVerifyOtpButtonEnabled = false;
            }
            else if (Otp.Length != 6 || !Regex.IsMatch(Otp, @"^\d{6}$"))
            {
                OtpError = "OTP phải là 6 chữ số.";
                IsOtpErrorVisible = true;
                IsVerifyOtpButtonEnabled = false;
            }
            else
            {
                OtpError = null;
                IsOtpErrorVisible = false;
                IsVerifyOtpButtonEnabled = true;
            }
        }

        private void ValidateNewPassword()
        {
            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                IsLengthValid = false;
                IsUpperCaseValid = false;
                IsLowerCaseValid = false;
                IsDigitValid = false;
                IsSpecialCharValid = false;
                PasswordError = "Mật khẩu không được để trống.";
                IsPasswordErrorVisible = true;
                UpdateResetPasswordButtonState();
                return;
            }

            IsLengthValid = NewPassword.Length >= 8;
            IsUpperCaseValid = Regex.IsMatch(NewPassword, @"[A-Z]");
            IsLowerCaseValid = Regex.IsMatch(NewPassword, @"[a-z]");
            IsDigitValid = Regex.IsMatch(NewPassword, @"\d");
            IsSpecialCharValid = Regex.IsMatch(NewPassword, @"[\W_]");

            if (!IsLengthValid || !IsUpperCaseValid || !IsLowerCaseValid || !IsDigitValid || !IsSpecialCharValid)
            {
                PasswordError = "Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt.";
                IsPasswordErrorVisible = true;
            }
            else
            {
                PasswordError = null;
                IsPasswordErrorVisible = false;
            }

            UpdateResetPasswordButtonState();
        }

        private void ValidateConfirmPassword()
        {
            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ConfirmPasswordError = "Xác nhận mật khẩu không được để trống.";
                IsConfirmPasswordErrorVisible = true;
            }
            else if (NewPassword != ConfirmPassword)
            {
                ConfirmPasswordError = "Mật khẩu xác nhận không khớp.";
                IsConfirmPasswordErrorVisible = true;
            }
            else
            {
                ConfirmPasswordError = null;
                IsConfirmPasswordErrorVisible = false;
            }

            UpdateResetPasswordButtonState();
        }

        private void UpdateResetPasswordButtonState()
        {
            IsResetPasswordButtonEnabled = isOtpVerified &&
                                         !string.IsNullOrWhiteSpace(NewPassword) &&
                                         !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                                         IsLengthValid && IsUpperCaseValid && IsLowerCaseValid && 
                                         IsDigitValid && IsSpecialCharValid &&
                                         NewPassword == ConfirmPassword &&
                                         string.IsNullOrEmpty(PasswordError) &&
                                         string.IsNullOrEmpty(ConfirmPasswordError);
        }

        [RelayCommand]
        private void ToggleNewPasswordVisibility()
        {
            IsNewPassword = !IsNewPassword;
            NewPasswordEyeIconSource = IsNewPassword ? "eye_closed.png" : "eye.png";
        }

        [RelayCommand]
        private void ToggleConfirmPasswordVisibility()
        {
            IsConfirmPasswordField = !IsConfirmPasswordField;
            ConfirmPasswordEyeIconSource = IsConfirmPasswordField ? "eye_closed.png" : "eye.png";
        }

        [RelayCommand]
        private async Task SendOtpAsync()
        {
            if (!isEmailValid || IsBusy)
                return;

            IsBusy = true;
            SendOtpButtonText = "Đang gửi...";
            IsSendOtpButtonEnabled = false;

            try
            {
                var apiUrl = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyKhachHang/ForgotPassword";
                var requestData = new { email = Email, method = "Email" };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true
                };

                using var client = new HttpClient(handler);
                var response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var resultText = await response.Content.ReadAsStringAsync();
                    GeneralMessage = "OTP đã được gửi qua Email. Vui lòng kiểm tra hộp thư của bạn.";
                    IsOtpSectionVisible = true;
                    SendOtpButtonText = "Gửi lại OTP";
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    GeneralMessage = $"Lỗi: {errorMsg}";
                }
            }
            catch (Exception ex)
            {
                GeneralMessage = $"Đã xảy ra lỗi: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
                IsSendOtpButtonEnabled = true;
            }
        }

        [RelayCommand]
        private async Task VerifyOtpAsync()
        {
            if (!IsVerifyOtpButtonEnabled || IsBusy)
                return;

            IsBusy = true;

            try
            {
                var apiUrl = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyKhachHang/VerifyOtp";
                var requestData = new { email = Email, otp = Otp };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true
                };

                using var client = new HttpClient(handler);
                var response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var resultText = await response.Content.ReadAsStringAsync();
                    GeneralMessage = "Xác minh OTP thành công. Vui lòng nhập mật khẩu mới.";
                    isOtpVerified = true;
                    IsPasswordResetSectionVisible = true;
                    IsVerifyOtpButtonEnabled = false; // Disable verify button after success
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    GeneralMessage = $"Lỗi xác minh OTP: {errorMsg}";
                }
            }
            catch (Exception ex)
            {
                GeneralMessage = $"Đã xảy ra lỗi: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ResetPasswordAsync()
        {
            if (!IsResetPasswordButtonEnabled || IsBusy)
                return;

            IsBusy = true;

            try
            {
                var apiUrl = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyKhachHang/ResetPassword";
                var requestData = new 
                { 
                    email = Email, 
                    otp = Otp,
                    newPassword = NewPassword,
                    confirmPassword = ConfirmPassword
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true
                };

                using var client = new HttpClient(handler);
                var response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var resultText = await response.Content.ReadAsStringAsync();
                    await Application.Current.MainPage.DisplayAlert("Thành công", 
                        "Đặt lại mật khẩu thành công. Bạn có thể đăng nhập với mật khẩu mới.", "OK");
                    
                    // Chuyển về trang đăng nhập
                    await Shell.Current.GoToAsync("//SignInPage");
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    GeneralMessage = $"Lỗi đặt lại mật khẩu: {errorMsg}";
                }
            }
            catch (Exception ex)
            {
                GeneralMessage = $"Đã xảy ra lỗi: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task BackToSignInAsync()
        {
            await Shell.Current.GoToAsync("//SignInPage");
        }
    }
}
