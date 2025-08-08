using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.ViewModels
{
    public partial class PaymentGatewayPageViewModel : ObservableObject
    {
        [ObservableProperty]
        int packageId;

        [ObservableProperty]
        decimal price;

        private readonly HttpClient _httpClient;

        public PaymentGatewayPageViewModel()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
        }

        [RelayCommand]
        async Task SelectVnPay()
        {
            await ProcessPayment("vnpay");
        }

        [RelayCommand]
        async Task SelectZaloPay()
        {
            await ProcessPayment("zalopay");
        }

        private async Task ProcessPayment(string paymentMethod)
        {
            try
            {
                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await Shell.Current.DisplayAlert("Lỗi", "Bạn chưa đăng nhập", "OK");
                    return;
                }
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                string url;
                object payload;

                if (paymentMethod == "vnpay")
                {
                    url = $"{AppConfig.AppConfig.BaseUrl}/api/payment/create-vnpay?packageId={PackageId}";
                    payload = new
                    {
                        orderID = "",
                        amount = Price,
                        orderDescription = $"Đăng ký gói cước số {PackageId}",
                        returnUrl = (string)null,
                        vnp_ReturnUrl = (string)null
                    };
                }
                else if (paymentMethod == "zalopay")
                {
                    url = $"{AppConfig.AppConfig.BaseUrl}/api/payment/create-zalopay";
                    
                    // Parse userId từ JWT token
                    var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
                    var rawId = jwt.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value
                             ?? jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                    if (!int.TryParse(rawId, out var userId))
                    {
                        await Shell.Current.DisplayAlert("Lỗi", "Không thể xác định thông tin người dùng", "OK");
                        return;
                    }

                    // Tạo orderId unique dựa trên timestamp
                    var orderId = $"ZLP_{DateTime.Now:yyyyMMddHHmmss}_{userId}";

                    payload = new
                    {
                        amount = (int)Price,
                        description = $"Đăng ký gói cước số {PackageId}",
                        orderId = orderId,
                        userId = userId,
                        packageId = PackageId
                    };
                }
                else
                {
                    throw new ArgumentException("Phương thức thanh toán không được hỗ trợ.");
                }

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var resp = await _httpClient.PostAsync(url, content);
                resp.EnsureSuccessStatusCode();

                var respJson = await resp.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(respJson);
                var paymentUrl = doc.RootElement.GetProperty("paymentUrl").GetString();

                if (!string.IsNullOrWhiteSpace(paymentUrl))
                {
                    // Đối với ZaloPay, lấy orderId từ response để truyền qua PaymentPage
                    if (paymentMethod == "zalopay")
                    {
                        var responseOrderId = doc.RootElement.GetProperty("orderId").GetString();
                        System.Diagnostics.Debug.WriteLine($"ZaloPay OrderId from response: {responseOrderId}");
                        
                        await Shell.Current.GoToAsync($"//PaymentPage?paymentUrl={Uri.EscapeDataString(paymentUrl)}&orderId={Uri.EscapeDataString(responseOrderId)}&paymentMethod={paymentMethod}");
                    }
                    else
                    {
                        await Shell.Current.GoToAsync($"//PaymentPage?paymentUrl={Uri.EscapeDataString(paymentUrl)}&paymentMethod={paymentMethod}");
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", ex.Message, "OK");
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }
    }
}
