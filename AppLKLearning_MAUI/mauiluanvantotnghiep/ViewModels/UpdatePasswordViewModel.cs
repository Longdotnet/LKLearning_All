using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.ViewModels
{
    public partial class UpdatePasswordViewModel : ObservableObject
    {
        // 1. Các trường nhập liệu
        [ObservableProperty] private string currentPassword;
        [ObservableProperty] private string newPassword;
        [ObservableProperty] private string confirmPassword;
        [ObservableProperty] private string updateMessage;
        [ObservableProperty] private bool isBusy;

        // 2. Các tiêu chí kiểm tra định dạng mật khẩu
        [ObservableProperty] private bool isLengthValid;
        [ObservableProperty] private bool isUpperCaseValid;
        [ObservableProperty] private bool isLowerCaseValid;
        [ObservableProperty] private bool isDigitValid;
        [ObservableProperty] private bool isSpecialCharValid;

        private readonly HttpClient _httpClient;

        public UpdatePasswordViewModel()
        {
            // Handler bỏ qua SSL cho môi trường DEV
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    (req, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);

            // Đảm bảo nút được cập nhật trạng thái lần đầu
            UpdatePasswordCommand.NotifyCanExecuteChanged();
        }

        // 3. Khi NewPassword thay đổi: cập nhật các tiêu chí và trạng thái nút
        partial void OnNewPasswordChanged(string value)
        {
            // Cập nhật các tiêu chí
            IsLengthValid = !string.IsNullOrEmpty(value) && value.Length >= 8;
            IsUpperCaseValid = !string.IsNullOrEmpty(value) && value.Any(char.IsUpper);
            IsLowerCaseValid = !string.IsNullOrEmpty(value) && value.Any(char.IsLower);
            IsDigitValid = !string.IsNullOrEmpty(value) && value.Any(char.IsDigit);
            string special = "!@#$%^&*()_+-=[]{}|;':\",.<>/?";
            IsSpecialCharValid = !string.IsNullOrEmpty(value) && value.Any(ch => special.Contains(ch));

            // Thông báo nút "Cập nhật" cần đánh giá lại
            UpdatePasswordCommand.NotifyCanExecuteChanged();
        }

        // 4. Khi ConfirmPassword thay đổi: thông báo nút đánh giá lại
        partial void OnConfirmPasswordChanged(string value)
        {
            UpdatePasswordCommand.NotifyCanExecuteChanged();
        }

        // 5. Logic CanExecute cho lệnh UpdatePassword
        private bool CanUpdatePassword()
        {
            return !IsBusy
                && !string.IsNullOrWhiteSpace(CurrentPassword)
                && !string.IsNullOrWhiteSpace(NewPassword)
                && !string.IsNullOrWhiteSpace(ConfirmPassword)
                && NewPassword == ConfirmPassword
                && IsLengthValid
                && IsUpperCaseValid
                && IsLowerCaseValid
                && IsDigitValid
                && IsSpecialCharValid;
        }

        // 6. Lệnh cập nhật mật khẩu với CanExecute đã chỉ định
        [RelayCommand(CanExecute = nameof(CanUpdatePassword))]
        public async Task UpdatePasswordAsync()
        {
            try
            {
                IsBusy = true;
                UpdateMessage = string.Empty;
                UpdatePasswordCommand.NotifyCanExecuteChanged();

                // Lấy token
                string token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await Shell.Current.DisplayAlert("Lỗi", "Không tìm thấy token đăng nhập", "OK");
                    return;
                }
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                // Chuẩn bị payload
                var payload = new
                {
                    currentPassword = CurrentPassword,
                    newPassword = NewPassword,
                    confirmPassword = ConfirmPassword
                };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Gọi POST lên endpoint ChangePassword
                var response = await _httpClient.PostAsync(
                    $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyKhachHang/ChangePassword",
                    content);

                // Debug
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Status={(int)response.StatusCode}");
                var respBody = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Body={respBody}");

                if (response.IsSuccessStatusCode)
                {
                    UpdateMessage = "Cập nhật mật khẩu thành công.";
                    await Shell.Current.DisplayAlert("Thành công", UpdateMessage, "OK");
                    await Task.Delay(2000);
                    await Shell.Current.GoToAsync("//UserProfilePage");
                }
                else
                {
                    UpdateMessage = $"Cập nhật thất bại ({(int)response.StatusCode}): {respBody}";
                }
            }
            catch (Exception ex)
            {
                UpdateMessage = $"Đã xảy ra lỗi: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
                UpdatePasswordCommand.NotifyCanExecuteChanged();
            }
        }

        // 7. Lệnh điều hướng
        [RelayCommand]
        public async Task NavigateToUserProfilePageAsync()
            => await Shell.Current.GoToAsync("//UserProfilePage");
    }
}