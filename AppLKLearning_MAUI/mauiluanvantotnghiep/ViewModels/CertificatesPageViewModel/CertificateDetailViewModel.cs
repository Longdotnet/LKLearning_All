using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mauiluanvantotnghiep.Models;
using Microsoft.Maui.Storage;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.ViewModels.CertificatesPageViewModel
{
    public partial class CertificateDetailViewModel : ObservableObject
    {
        private readonly HttpClient _httpClient;

        [ObservableProperty]
        private CertificateResponse certificate;

        [ObservableProperty]
        private bool isLoading = false;

        // Solana Explorer base URL
        private const string SOLANA_EXPLORER_URL = "https://explorer.solana.com/tx/";

        public CertificateDetailViewModel()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
        }

        [RelayCommand]
        public async Task LoadCertificateDetailAsync(int certificateId)
        {
            try
            {
                IsLoading = true;

                string token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await Shell.Current.DisplayAlert("Lỗi", "Bạn cần đăng nhập để xem chi tiết chứng chỉ.", "OK");
                    return;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // API endpoint for getting certificate detail
                string apiUrl = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyCertificate/GetCertificateById/{certificateId}";
                
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Calling API: {apiUrl}");
                
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] API Response: {json}");
                    
                    // Configure JSON options to match API response format
                    var options = new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true,
                        // Remove CamelCase policy since API returns camelCase already
                        WriteIndented = true
                    };
                    
                    Certificate = JsonSerializer.Deserialize<CertificateResponse>(json, options);
                    
                    if (Certificate != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] Certificate loaded successfully:");
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] - Course: {Certificate.CourseName}");
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] - Student: {Certificate.Fullname}");
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] - Type: {Certificate.CertificateTypeName}");
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] - Verification Code: {Certificate.VerificationCode}");
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] - Signature: {Certificate.Signature}");
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] - Created: {Certificate.CreatedAt}");
                        
                        // Trigger property changed notifications for UI binding
                        OnPropertyChanged(nameof(Certificate));
                        OnPropertyChanged(nameof(IsCertificateValid));
                        OnPropertyChanged(nameof(HasCertificateType));
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("[DEBUG] Certificate deserialization returned null");
                        await Shell.Current.DisplayAlert("Lỗi", "Không thể đọc dữ liệu chứng chỉ.", "OK");
                    }
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] API Error: {response.StatusCode} - {errorContent}");
                    await Shell.Current.DisplayAlert("Lỗi", $"Không thể tải thông tin chứng chỉ. Status: {response.StatusCode}", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Exception in LoadCertificateDetailAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Stack trace: {ex.StackTrace}");
                await Shell.Current.DisplayAlert("Lỗi", $"Đã xảy ra lỗi: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task CopyVerificationCodeAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(Certificate?.VerificationCode))
                {
                    await Clipboard.SetTextAsync(Certificate.VerificationCode);
                    await Shell.Current.DisplayAlert("Thành công", "Đã copy mã xác thực vào clipboard!", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Lỗi", "Không tìm thấy mã xác thực để copy.", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Copy verification code error: {ex.Message}");
                await Shell.Current.DisplayAlert("Lỗi", $"Không thể copy: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task CopySignatureAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(Certificate?.Signature))
                {
                    await Clipboard.SetTextAsync(Certificate.Signature);
                    await Shell.Current.DisplayAlert("Thành công", "Đã copy chữ ký blockchain vào clipboard!", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Lỗi", "Không tìm thấy chữ ký blockchain để copy.", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Copy signature error: {ex.Message}");
                await Shell.Current.DisplayAlert("Lỗi", $"Không thể copy: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task VerifyOnSolanaAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(Certificate?.Signature))
                {
                    // Build Solana Explorer URL with the signature
                    string solanaUrl = $"{SOLANA_EXPLORER_URL}{Certificate.Signature}";
                    
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Opening Solana URL: {solanaUrl}");
                    
                    // Open the URL in the default browser
                    await Browser.OpenAsync(solanaUrl, BrowserLaunchMode.SystemPreferred);
                }
                else
                {
                    await Shell.Current.DisplayAlert("Lỗi", "Không tìm thấy chữ ký blockchain để xác minh.", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Browser error: {ex.Message}");
                await Shell.Current.DisplayAlert("Lỗi", $"Không thể mở Solana Explorer: {ex.Message}", "OK");
            }
        }

        // Method to check if certificate has complete data
        public bool IsCertificateValid => Certificate != null && 
                                         !string.IsNullOrEmpty(Certificate.VerificationCode) && 
                                         !string.IsNullOrEmpty(Certificate.Signature);

        // Helper property to check if certificate type is available
        public bool HasCertificateType => !string.IsNullOrEmpty(Certificate?.CertificateTypeName);

        // Property to get formatted certificate type display
        public string CertificateTypeDisplay => HasCertificateType ? 
                                               Certificate.CertificateTypeName : 
                                               "Không xác định";

        // Cleanup resources
        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        ~CertificateDetailViewModel()
        {
            Dispose();
        }
    }
}
