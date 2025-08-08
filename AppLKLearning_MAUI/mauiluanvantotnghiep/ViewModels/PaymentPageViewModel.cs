using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Text.Json;
using mauiluanvantotnghiep.Models;
using System.Collections.Specialized;
using System.Net.Http.Headers;

namespace mauiluanvantotnghiep.ViewModels
{
    public partial class PaymentPageViewModel : ObservableObject
    {
        private string _paymentUrl;
        public string PaymentUrl
        {
            get => _paymentUrl;
            set => SetProperty(ref _paymentUrl, value);
        }

        private string _orderId;
        public string OrderId
        {
            get => _orderId;
            set => SetProperty(ref _orderId, value);
        }

        private string _paymentMethod;
        public string PaymentMethod
        {
            get => _paymentMethod;
            set => SetProperty(ref _paymentMethod, value);
        }

        private readonly HttpClient _httpClient;

        public PaymentPageViewModel()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
        }

        [RelayCommand]
        async Task HandleWebNavigating(WebNavigatingEventArgs e)
        {
            try
            {
                // Kiểm tra xem URL có phải là callback của VnPay không
                if (e.Url.StartsWith($"{AppConfig.AppConfig.BaseUrl}/api/payment/vnpay-callback"))
                {
                    // Hủy điều hướng trong WebView
                    e.Cancel = true;

                    // Gọi URL callback để lấy phản hồi JSON
                    var response = await _httpClient.GetAsync(e.Url);
                    response.EnsureSuccessStatusCode(); // Đảm bảo yêu cầu thành công

                    // Đọc và phân tích JSON
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var paymentResult = JsonSerializer.Deserialize<PaymentResult>(jsonResponse);

                    // Xử lý phản hồi JSON
                    if (paymentResult != null && paymentResult.Success && paymentResult.VnPayResponseCode == "00")
                    {
                        // Hiển thị thông báo thành công
                        await Shell.Current.DisplayAlert("Thành công", $"Thanh toán thành công! Mã giao dịch: {paymentResult.TransactionId}", "OK");

                        // Điều hướng về PackagePage
                        await Shell.Current.GoToAsync("//PackagePage");

                        // Làm mới dữ liệu PackagePage nếu cần
                        if (Shell.Current.CurrentPage?.BindingContext is PackagePageViewModel viewModel)
                        {
                            viewModel.LoadPackagesAndRegistrations();
                        }
                    }
                    else
                    {
                        // Xử lý thanh toán thất bại
                        await Shell.Current.DisplayAlert("Lỗi", $"Thanh toán không thành công.", "OK");
                        await Shell.Current.GoToAsync("//PackagePage");
                    }
                }
                // Kiểm tra callback của ZaloPay
                else if (e.Url.Contains("payment-callback") && e.Url.Contains("status="))
                {
                    // Hủy điều hướng trong WebView
                    e.Cancel = true;

                    await HandleZaloPayCallback(e.Url);
                }
            }
            catch (HttpRequestException ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Lỗi kết nối: {ex.Message}", "OK");
                System.Diagnostics.Debug.WriteLine($"Lỗi HTTP: {ex}");
                await Shell.Current.GoToAsync("//PackagePage");
            }
            catch (JsonException ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Lỗi xử lý dữ liệu: {ex.Message}", "OK");
                System.Diagnostics.Debug.WriteLine($"Lỗi phân tích JSON: {ex}");
                await Shell.Current.GoToAsync("//PackagePage");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Đã xảy ra lỗi: {ex.Message}", "OK");
                System.Diagnostics.Debug.WriteLine($"Lỗi WebView: {ex}");
                await Shell.Current.GoToAsync("//PackagePage");
            }
        }

        private async Task HandleZaloPayCallback(string callbackUrl)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"ZaloPay callback URL: {callbackUrl}");

                // Parse các tham số từ URL
                var uri = new Uri(callbackUrl);
                var query = HttpUtility.ParseQueryString(uri.Query);

                var amount = query["amount"];
                var appId = query["appid"];
                var appTransId = query["apptransid"];
                var status = query["status"];
                var discountAmount = query["discountamount"];
                var pmcId = query["pmcid"];

                System.Diagnostics.Debug.WriteLine($"ZaloPay callback - Amount: {amount}, AppId: {appId}, AppTransId: {appTransId}, Status: {status}");

                // Kiểm tra status để xác định giao dịch thành công
                // ZaloPay: status=1 nghĩa là thành công, status=0 hoặc khác nghĩa là thất bại
                bool isSuccess = status == "1";

                if (isSuccess)
                {
                    // Sử dụng OrderId từ response thay vì appTransId từ callback
                    string orderIdToVerify = !string.IsNullOrEmpty(OrderId) ? OrderId : appTransId;
                    System.Diagnostics.Debug.WriteLine($"Using OrderId for verification: {orderIdToVerify}");

                    // Gọi API verify ZaloPay để xác nhận giao dịch
                    bool verificationResult = await VerifyZaloPayTransaction(orderIdToVerify);

                    if (verificationResult)
                    {
                        // Format số tiền để hiển thị đẹp hơn
                        string formattedAmount = "";
                        if (decimal.TryParse(amount, out decimal amountValue))
                        {
                            formattedAmount = amountValue.ToString("N0") + " VNĐ";
                        }
                        else
                        {
                            formattedAmount = amount + " VNĐ";
                        }

                        // Hiển thị thông báo thành công
                        await Shell.Current.DisplayAlert("Thành công", 
                            $"Thanh toán ZaloPay thành công!\n" +
                            $"Số tiền: {formattedAmount}\n" +
                            $"Mã đơn hàng: {orderIdToVerify}", "OK");

                        // Điều hướng về PackagePage
                        await Shell.Current.GoToAsync("//PackagePage");

                        // Làm mới dữ liệu PackagePage nếu cần
                        if (Shell.Current.CurrentPage?.BindingContext is PackagePageViewModel viewModel)
                        {
                            viewModel.LoadPackagesAndRegistrations();
                        }
                    }
                    else
                    {
                        // Xác thực thất bại
                        await Shell.Current.DisplayAlert("Lỗi", 
                            "Không thể xác thực giao dịch ZaloPay. Vui lòng liên hệ hỗ trợ.", "OK");
                        await Shell.Current.GoToAsync("//PackagePage");
                    }
                }
                else
                {
                    // Xử lý thanh toán thất bại
                    await Shell.Current.DisplayAlert("Lỗi", 
                        $"Thanh toán ZaloPay không thành công.\n" +
                        $"Trạng thái: {status}", "OK");
                    await Shell.Current.GoToAsync("//PackagePage");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi xử lý ZaloPay callback: {ex}");
                await Shell.Current.DisplayAlert("Lỗi", $"Lỗi xử lý kết quả thanh toán: {ex.Message}", "OK");
                await Shell.Current.GoToAsync("//PackagePage");
            }
        }

        private async Task<bool> VerifyZaloPayTransaction(string orderId)
        {
            try
            {
                // Lấy token để xác thực API
                var token = await SecureStorage.GetAsync("auth_token");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                // Gọi API verify ZaloPay
                var verifyUrl = $"{AppConfig.AppConfig.BaseUrl}/api/payment/zalopay/verify?orderId={orderId}";
                System.Diagnostics.Debug.WriteLine($"Calling ZaloPay verify API: {verifyUrl}");

                var response = await _httpClient.GetAsync(verifyUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"ZaloPay verify response: {jsonResponse}");

                    var verifyResult = JsonSerializer.Deserialize<ZaloPayVerifyResult>(jsonResponse, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return verifyResult?.IsSuccess == true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"ZaloPay verify API failed with status: {response.StatusCode}");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Error content: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in VerifyZaloPayTransaction: {ex.Message}");
                return false;
            }
        }
    }


   
}