using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json;
using static WebLuanVan_ASP.NET_MVC.Config.ApiConfig;

namespace WebLuanVan_ASP.NET_MVC.Areas.ForgotPassword.Models
{
    public class CXuLy
    {
        // Gửi OTP qua SMS hoặc Email
        public static async Task<string> SendOtpAsync(CForgotPassword dto)
        {
            using var client = new HttpClient();
            string apiUrl = $"{api}QuanLyKhachHang/ForgotPassword";
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(apiUrl, content);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return "OTP đã được gửi thành công.";
            }
            else
            {
                // Trả về lỗi từ backend
                return !string.IsNullOrEmpty(result) ? result : "Gửi OTP thất bại.";
            }
        }

        // Xác thực OTP
        public static async Task<bool> VerifyOtpAsync(CVerifyOtp dto)
        {
            using var client = new HttpClient();
            string apiUrl = $"{api}QuanLyKhachHang/VerifyOtp";
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(apiUrl, content);
            var result = await response.Content.ReadAsStringAsync();

            // Thành công nếu status 200
            return response.IsSuccessStatusCode;
        }

        // Đặt lại mật khẩu
        public static async Task<string> ResetPasswordAsync(CResetPassword dto)
        {
            using var client = new HttpClient();
            string apiUrl = $"{api}QuanLyKhachHang/ResetPassword";
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(apiUrl, content);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return "Đặt lại mật khẩu thành công.";
            }
            else
            {
                // Trả về lỗi từ backend
                return !string.IsNullOrEmpty(result) ? result : "Đặt lại mật khẩu thất bại.";
            }
        }
    }
}
