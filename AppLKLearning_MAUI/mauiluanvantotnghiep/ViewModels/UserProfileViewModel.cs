using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mauiluanvantotnghiep.Models;
using mauiluanvantotnghiep.ViewModels.AppConfig;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.ViewModels
{
    public partial class UserProfileViewModel : ObservableObject
    {
        // ─────────────────────────────────────────────────────────────
        // 1) Các thuộc tính bind lên UI    
        [ObservableProperty] private string fullname;
        [ObservableProperty] private string email;
        [ObservableProperty] private int age;
        [ObservableProperty] private string phone;
        [ObservableProperty] private string gender;
        [ObservableProperty] private DateTime dateOfBirth;
        [ObservableProperty] private DateTime lastLoginDate;
        [ObservableProperty] private string avatarUrl;


        // New properties for avatar selection
        [ObservableProperty] private ObservableCollection<Avatar> avatars;
        [ObservableProperty] private int selectedAvatarId;

        [ObservableProperty]
        private ObservableCollection<string> genderOptions
            = new ObservableCollection<string> { "Nam", "Nữ" };

        // ─────────────────────────────────────────────────────────────
        // 2) Validation / error
        [ObservableProperty] private string emailError;
        [ObservableProperty] private bool isEmailErrorVisible;
        [ObservableProperty] private string generalError;

        // ─────────────────────────────────────────────────────────────
        // 3) Flags cho chế độ chỉnh sửa (read-only / editable)
        [ObservableProperty] private bool isFullnameEditable;
        [ObservableProperty] private bool isEmailEditable;
        [ObservableProperty] private bool isPhoneEditable;
        [ObservableProperty] private bool isGenderEditable;
        [ObservableProperty] private bool isDateOfBirthEditable;
        [ObservableProperty] private bool isAvatarUrlEditable;


        private readonly HttpClient _httpClient;


        // Event to trigger popup display
        public event Action<ObservableCollection<Avatar>> ShowAvatarPopup;

        // ─────────────────────────────────────────────────────────────
        // 4) Commands để toggle editable


        public IRelayCommand ToggleFullnameEditableCommand { get; }
        public IRelayCommand ToggleEmailEditableCommand { get; }
        public IRelayCommand TogglePhoneEditableCommand { get; }
        public IRelayCommand ToggleGenderEditableCommand { get; }
        public IRelayCommand ToggleDateOfBirthEditableCommand { get; }


        public UserProfileViewModel()
        {
            // Khởi tạo HttpClient (bỏ qua SSL dev self-signed)
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    (req, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);

            // Mặc định read-only
            IsFullnameEditable = false;
            IsEmailEditable = false;
            IsPhoneEditable = false;
            IsGenderEditable = false;
            IsDateOfBirthEditable = false;
            isAvatarUrlEditable = false;

            // Toggle commands
            ToggleFullnameEditableCommand = new RelayCommand(() =>
                IsFullnameEditable = true);
            ToggleEmailEditableCommand = new RelayCommand(() =>
                IsEmailEditable = true);
            TogglePhoneEditableCommand = new RelayCommand(() =>
                IsPhoneEditable = true);
            ToggleGenderEditableCommand = new RelayCommand(() =>
                IsGenderEditable = true);
            ToggleDateOfBirthEditableCommand = new RelayCommand(() =>
                IsDateOfBirthEditable = true);


            // Lần đầu load data
            FetchUserDataCommand.Execute(null);
        }


        // New method to fetch avatars
        private async Task FetchAvatarsAsync()
        {
            try
            {
                string token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await Shell.Current.DisplayAlert("Lỗi", "Không tìm thấy token đăng nhập", "OK");
                    return;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"{AppConfig.AppConfig.BaseUrl}/api/QuanLyKhachHang/LayDanhSachAvatar");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var avatarList = JsonSerializer.Deserialize<List<Avatar>>(json, options);
                    Avatars = new ObservableCollection<Avatar>(avatarList);
                }
                else
                {
                    await Shell.Current.DisplayAlert("Lỗi", "Không thể lấy danh sách avatar", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Đã xảy ra lỗi: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task ToggleAvatarUrlEditableAsync()
        {
            await FetchAvatarsAsync();
            if (Avatars != null && Avatars.Count > 0)
            {
                ShowAvatarPopup?.Invoke(Avatars);
            }
            else
            {
                await Shell.Current.DisplayAlert("Thông báo", "Không có avatar để chọn", "OK");
            }
        }

        // ─────────────────────────────────────────────────────────────
        // 5) Tính Age tự động khi DateOfBirth thay đổi
        partial void OnDateOfBirthChanged(DateTime value)
            => Age = CalculateAge(value);

        private int CalculateAge(DateTime birthDate)
        {
            if (birthDate == DateTime.MinValue) return 0;
            var today = DateTime.Today;
            var a = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-a)) a--;
            return a;
        }

        // ─────────────────────────────────────────────────────────────
        // 6) Validate email và format phone ngay khi người dùng nhập
        partial void OnEmailChanged(string value) => ValidateEmail();

        // *** THÊM: Format phone number khi thay đổi ***
        partial void OnPhoneChanged(string value) => FormatPhoneNumber();

        private void FormatPhoneNumber()
        {
            if (string.IsNullOrWhiteSpace(Phone))
                return;

            // Remove all spaces and special characters except +
            string cleanPhone = Regex.Replace(Phone, @"[^\d+]", "");
            
            // If already has +84, keep it
            if (cleanPhone.StartsWith("+84"))
                return;
            
            // If starts with 84, add +
            if (cleanPhone.StartsWith("84") && cleanPhone.Length >= 10)
            {
                Phone = "+" + cleanPhone;
                return;
            }
            
            // If starts with 0, replace with +84
            if (cleanPhone.StartsWith("0") && cleanPhone.Length >= 10)
            {
                Phone = "+84" + cleanPhone.Substring(1);
                return;
            }
            
            // If just numbers without 0 or 84, add +84
            if (cleanPhone.Length >= 9 && !cleanPhone.StartsWith("0") && !cleanPhone.StartsWith("84"))
            {
                Phone = "+84" + cleanPhone;
            }
        }

        private void ValidateEmail()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                EmailError = "Email không được để trống.";
                IsEmailErrorVisible = true;
                GeneralError = "Vui lòng sửa các lỗi trước khi lưu.";
                return;
            }

            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(Email, pattern))
            {
                EmailError = "Email không đúng định dạng.";
                IsEmailErrorVisible = true;
                GeneralError = "Vui lòng sửa các lỗi trước khi lưu.";
                return;
            }

            // Clear error
            EmailError = string.Empty;
            IsEmailErrorVisible = false;
            GeneralError = string.Empty;
        }

        // ─────────────────────────────────────────────────────────────
        // 7) Command tải thông tin người dùng
        [RelayCommand]
        public async Task FetchUserDataAsync()
        {
            try
            {
                // Lấy token
                string token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await Shell.Current.DisplayAlert("Lỗi",
                        "Không tìm thấy token đăng nhập", "OK");
                    return;
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                // Gọi API
                var resp = await _httpClient.GetAsync(
                    $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyKhachHang/LayThongTinUser");
                if (!resp.IsSuccessStatusCode)
                {
                    var err = await resp.Content.ReadAsStringAsync();
                    await Shell.Current.DisplayAlert("Lỗi",
                        $"Lấy thông tin thất bại: {err}", "OK");
                    return;
                }

                var json = await resp.Content.ReadAsStringAsync();
                var opts = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var profile = JsonSerializer.Deserialize<UserProfile>(json, opts);
                if (profile == null)
                {
                    await Shell.Current.DisplayAlert("Lỗi",
                        "Dữ liệu người dùng không hợp lệ", "OK");
                    return;
                }

                // Gán an toàn
                Fullname = profile.Fullname ?? string.Empty;
                Email = profile.Email;
                Phone = profile.Phone ?? string.Empty;
                AvatarUrl = profile.AvatarUrl ?? string.Empty;

                // *** THÊM: Khởi tạo SelectedAvatarId từ dữ liệu hiện tại ***
                // Nếu có AvatarId trong profile, sử dụng nó, ngược lại giữ giá trị hiện tại
                if (profile.AvatarId.HasValue && profile.AvatarId.Value > 0)
                {
                    SelectedAvatarId = profile.AvatarId.Value;
                }
                else if (SelectedAvatarId == 0)
                {
                    // Nếu không có avatar từ server và chưa chọn avatar nào, set default
                    SelectedAvatarId = 1; // hoặc ID avatar mặc định
                }

                // Gender null-safe
                if (profile.Gender.HasValue)
                    Gender = profile.Gender.Value ? "Nam" : "Nữ";
                else
                    Gender = string.Empty;

                // DateOfBirth null-safe → triggers OnDateOfBirthChanged
                DateOfBirth = profile.DateOfBirth ?? DateTime.MinValue;

                // *** Thêm dòng này để gán LastLoginDate ***
                LastLoginDate = profile.LastLoginDate ?? DateTime.MinValue;

                // Age từ server nếu có, ngược lại đã được tính ở OnDateOfBirthChanged
                Age = profile.Age ?? Age;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi",
                    $"Đã xảy ra lỗi: {ex.Message}", "OK");
            }
        }

        // ─────────────────────────────────────────────────────────────
        // 8) Command cập nhật thông tin người dùng
        [RelayCommand]
        public async Task SaveUserDataAsync()
        {
            // Validate cuối cùng
            ValidateEmail();
            
            // *** THÊM: Format phone number trước khi lưu ***
            FormatPhoneNumber();
            
            if (!string.IsNullOrEmpty(EmailError))
            {
                await Shell.Current.DisplayAlert("Lỗi",
                    GeneralError ?? "Có lỗi dữ liệu", "OK");
                return;
            }

            try
            {
                string token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await Shell.Current.DisplayAlert("Lỗi",
                        "Không tìm thấy token đăng nhập", "OK");
                    return;
                }

                // *** THÊM: Ensure phone format trước khi gửi API ***
                string formattedPhone = FormatPhoneForAPI(Phone);

                var payload = new
                {
                    fullname = Fullname,
                    email = Email,
                    age = Age,
                    phone = formattedPhone, // Sử dụng phone đã format
                    gender = (Gender == "Nam"),
                    dateOfBirth = DateOfBirth.ToString("yyyy-MM-dd"),
                    avatarId = SelectedAvatarId > 0 ? SelectedAvatarId : (int?)null
                };

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var body = JsonSerializer.Serialize(payload);
                var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");

                var resp = await _httpClient.PutAsync(
                    $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyKhachHang/CapNhatThongTinUser",
                    content);

                if (resp.IsSuccessStatusCode)
                {
                    await Shell.Current.DisplayAlert("Thành công",
                        "Cập nhật thành công", "OK");
                }
                else
                {
                    var err = await resp.Content.ReadAsStringAsync();
                    if (err.Contains("Email đã tồn tại"))
                    {
                        EmailError = "Email này đã được sử dụng!";
                        IsEmailErrorVisible = true;
                        await Shell.Current.DisplayAlert("Lỗi",
                            "Email đã tồn tại, vui lòng chọn email khác.", "OK");
                        return;
                    }
                    await Shell.Current.DisplayAlert("Lỗi",
                        "Cập nhật thất bại", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi",
                    $"Đã xảy ra lỗi: {ex.Message}", "OK");
            }
            finally
            {
                // Reset read-only
                IsFullnameEditable = false;
                IsEmailEditable = false;
                IsPhoneEditable = false;
                IsGenderEditable = false;
                IsDateOfBirthEditable = false;
            }
        }

        // *** THÊM: Helper method để format phone cho API - Lưu dạng 84 không có dấu + ***
        private string FormatPhoneForAPI(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;

            // Remove all spaces and special characters except +
            string cleanPhone = Regex.Replace(phone, @"[^\d+]", "");
            
            // If has +84, remove + and return 84xxxxxxxx
            if (cleanPhone.StartsWith("+84"))
                return cleanPhone.Substring(1); // Bỏ dấu +, trả về 84xxxxxxxx
            
            // If starts with 84, return as is
            if (cleanPhone.StartsWith("84") && cleanPhone.Length >= 10)
                return cleanPhone; // Trả về 84xxxxxxxx
            
            // If starts with 0, replace with 84
            if (cleanPhone.StartsWith("0") && cleanPhone.Length >= 10)
                return "84" + cleanPhone.Substring(1); // Trả về 84xxxxxxxx
            
            // If just numbers, add 84
            if (cleanPhone.Length >= 9)
                return "84" + cleanPhone; // Trả về 84xxxxxxxx
            
            return phone; // Return original if can't format
        }

        // ─────────────────────────────────────────────────────────────
        // 9) Command Xóa thông tin người dùng 
        [RelayCommand]
        public async Task DeleteUserDataAsync()
        {
            Console.WriteLine("[DEBUG] Starting DeleteUserDataAsync...");

            // Hiển thị hộp thoại xác nhận xóa tài khoản
            bool confirmed = await Shell.Current.DisplayAlert(
                "Xác nhận xóa tài khoản",
                "Bạn có chắc chắn muốn xóa tài khoản? Mọi dữ liệu sẽ bị mất vĩnh viễn.",
                "Xóa",
                "Hủy"
            );

            if (!confirmed)
            {
                Console.WriteLine("[DEBUG] Delete canceled by user.");
                return;
            }

            try
            {
                // Lấy token từ SecureStorage
                string token = await SecureStorage.GetAsync("auth_token");
                Console.WriteLine("[DEBUG] Retrieved token: " + token);
                if (string.IsNullOrEmpty(token))
                {
                    await Shell.Current.DisplayAlert("Lỗi", "Không tìm thấy token đăng nhập", "OK");
                    return;
                }

                // Cấu hình HttpClientHandler để bỏ qua SSL certificate (chỉ dùng cho DEV)
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
                };

                using (var httpClient = new HttpClient(handler))
                {
                    // Set Authorization header với token
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    Console.WriteLine("[DEBUG] Sending DELETE request to API.");

                    // Gọi API xóa tài khoản (đường dẫn có thể điều chỉnh theo cấu hình server)
                    var response = await httpClient.DeleteAsync($"{AppConfig.AppConfig.BaseUrl}/api/QuanLyKhachHang/XoaTaiKhoan");
                    Console.WriteLine($"[DEBUG] Response Status Code: {response.StatusCode}");

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("[DEBUG] Account deleted successfully.");
                        await Shell.Current.DisplayAlert("Thành công", "Tài khoản của bạn đã được xóa", "OK");

                        // Xóa token đăng nhập khỏi SecureStorage để bảo đảm không đăng nhập trái phép
                        SecureStorage.Remove("auth_token");

                        // Chuyển hướng về trang đăng nhập
                        await Shell.Current.GoToAsync("//SignInPage");
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("[DEBUG] Error Response: " + errorContent);
                        await Shell.Current.DisplayAlert("Lỗi", $"Không thể xóa tài khoản: {errorContent}", "OK");
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine("[DEBUG] HttpRequestException: " + httpEx.Message);
                await Shell.Current.DisplayAlert("Lỗi kết nối", $"Không thể kết nối đến server: {httpEx.Message}", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[DEBUG] Exception in DeleteUserDataAsync: " + ex.Message);
                await Shell.Current.DisplayAlert("Lỗi", $"Đã xảy ra lỗi: {ex.Message}", "OK");
            }
        }


        // ─────────────────────────────────────────────────────────────
        // 10) Command cập nhật password
        [RelayCommand]
        public async Task NavigateToUpdatePasswordPageAsync()
        {
            // Điều hướng sang trang cập nhật mật khẩu. Hãy đảm bảo rằng bạn đã đăng ký route cho trang UpdatePasswordPage
            await Shell.Current.GoToAsync("//UpdatePasswordPage");
        }

        // ─────────────────────────────────────────────────────────────
        // 11) Command Đăng Xuất
        [RelayCommand]
        public async Task NavigateToLogOutAsync()
        {
            await Shell.Current.GoToAsync("//SignInPage");
        }
    }

}
