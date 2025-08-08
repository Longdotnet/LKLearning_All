using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.ViewModels
{
    public partial class SignUpViewModel : ObservableObject
    {
        // Các thuộc tính đăng ký
        [ObservableProperty]
        private string email;

        // Thuộc tính lỗi định dạng email, hiển thị ngay khi nhập sai
        [ObservableProperty]
        private string emailError;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private string confirmPassword;

        // Thông báo kết quả đăng ký (thành công hoặc lỗi)
        [ObservableProperty]
        private string registrationMessage;

        // Flag báo trạng thái xử lý (ví dụ hiển thị ActivityIndicator)
        [ObservableProperty]
        private bool isBusy;

        // Các tiêu chí kiểm tra định dạng mật khẩu
        [ObservableProperty] private bool isLengthValid;
        [ObservableProperty] private bool isUpperCaseValid;
        [ObservableProperty] private bool isLowerCaseValid;
        [ObservableProperty] private bool isDigitValid;
        [ObservableProperty] private bool isSpecialCharValid;

        public SignUpViewModel()
        {
            // Khởi tạo nếu cần
        }

        /// <summary>
        /// Khi thuộc tính Email thay đổi, kiểm tra ngay định dạng email
        /// </summary>
        /// <param name="value">Giá trị email mới</param>
        partial void OnEmailChanged(string value)
        {
            if (!string.IsNullOrWhiteSpace(value) && !IsValidEmail(value))
            {
                EmailError = "Email không đúng định dạng.";
            }
            else
            {
                EmailError = string.Empty;
            }
        }

        /// <summary>
        /// Khi thuộc tính Password thay đổi, kiểm tra các điều kiện mật khẩu
        /// </summary>
        /// <param name="value">Giá trị password mới</param>
        partial void OnPasswordChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                IsLengthValid = false;
                IsUpperCaseValid = false;
                IsLowerCaseValid = false;
                IsDigitValid = false;
                IsSpecialCharValid = false;
                return;
            }

            IsLengthValid = value.Length >= 8;
            IsUpperCaseValid = Regex.IsMatch(value, @"[A-Z]");
            IsLowerCaseValid = Regex.IsMatch(value, @"[a-z]");
            IsDigitValid = Regex.IsMatch(value, @"\d");
            IsSpecialCharValid = Regex.IsMatch(value, @"[\W_]");
        }

        /// <summary>
        /// Command đăng ký sử dụng API.
        /// Trước khi gửi request, kiểm tra định dạng email và các tiêu chí mật khẩu.
        /// Nếu không thỏa mãn, thông báo lỗi và không tiếp tục.
        /// </summary>
        [RelayCommand]
        private async Task SignUpAsync()
        {
            Console.WriteLine("[DEBUG] Starting RegisterAsync...");
            IsBusy = true;
            RegistrationMessage = string.Empty;

            // Kiểm tra đầu vào cơ bản
            if (string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                RegistrationMessage = "Vui lòng điền đầy đủ thông tin";
                IsBusy = false;
                return;
            }

            // Kiểm tra lỗi định dạng email
            if (!string.IsNullOrEmpty(EmailError))
            {
                RegistrationMessage = "Email không đúng định dạng. Vui lòng kiểm tra lại.";
                IsBusy = false;
                Email = string.Empty;
                return;
            }

            // Kiểm tra các điều kiện mật khẩu
            if (!IsLengthValid || !IsUpperCaseValid || !IsLowerCaseValid || !IsDigitValid || !IsSpecialCharValid)
            {
                RegistrationMessage = "Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt.";
                IsBusy = false;
                Password = string.Empty;
                ConfirmPassword = string.Empty;
                return;
            }

            // Kiểm tra mật khẩu và xác nhận không trùng nhau
            if (Password != ConfirmPassword)
            {
                RegistrationMessage = "Mật khẩu và xác nhận không khớp";
                IsBusy = false;
                Password = string.Empty;
                ConfirmPassword = string.Empty;
                return;
            }

            try
            {
                // Thiết lập HttpClientHandler bỏ qua chứng chỉ (dành cho DEV với HTTPS self-signed)
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
                };

                using (var httpClient = new HttpClient(handler))
                {
                    // Tạo payload đăng ký theo định dạng JSON của API
                    var payload = new
                    {
                        email = Email,
                        password = Password,
                        confirmPassword = ConfirmPassword
                    };

                    string json = JsonSerializer.Serialize(payload);
                    Console.WriteLine("[DEBUG] Register payload: " + json);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // Đường dẫn API đăng ký
                    var apiUrl = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyKhachHang/DangKy";
                    Console.WriteLine("[DEBUG] Sending POST request to: " + apiUrl);

                    HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);
                    Console.WriteLine($"[DEBUG] Response Status Code: {response.StatusCode}");

                    if (response.IsSuccessStatusCode)
                    {
                        RegistrationMessage = "Đăng Ký Thành Công";
                        Console.WriteLine("[DEBUG] Registration Success");

                        // Chờ 2 giây trước khi chuyển giao diện
                        await Task.Delay(2000);
                        await Shell.Current.GoToAsync("//SignInPage");
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("[DEBUG] Registration failed: " + errorContent);

                        // Nếu lỗi chứa thông báo email đã tồn tại
                        if (errorContent.Contains("Email đã tồn tại"))
                        {
                            RegistrationMessage = "Email đã tồn tại trong hệ thống. Vui lòng chọn email khác.";
                        }
                        else
                        {
                            RegistrationMessage = $"Đăng ký thất bại: {errorContent}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                RegistrationMessage = $"Có lỗi xảy ra: {ex.Message}";
                Console.WriteLine("[DEBUG] Exception in RegisterAsync: " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Phương thức kiểm tra định dạng email bằng Regex.
        /// </summary>
        /// <param name="email">Chuỗi email cần kiểm tra</param>
        /// <returns>true nếu đúng định dạng, false nếu không.</returns>
        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
        }
    }
}