using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using static WebLuanVan_ASP.NET_MVC.Config.ApiConfig;

namespace WebLuanVan_ASP.NET_MVC.Areas.UpdateProfile.Models
{

    public class CXuLy
    {
      

        public static bool UpdateUserProfile(UserProfile x, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyKhachHang/CapNhatThongTinUser";
                using HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var res = client.PutAsJsonAsync(strUrl, x).Result;

                if (res.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    // Đọc thông báo lỗi từ API
                    var errorContent = res.Content.ReadAsStringAsync().Result;
                    Console.WriteLine($"API Error: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return false;
            }
        }
        // GetUserInformation
        public static UserProfile? GetUserInformation(string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyKhachHang/LayThongTinUser";
                using HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.GetAsync(strUrl).Result;
                if (res.IsSuccessStatusCode)
                {
                    var json = res.Content.ReadAsStringAsync().Result;
                    var userProfile = System.Text.Json.JsonSerializer.Deserialize<UserProfile>(json, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return userProfile;
                }
                else
                {
                    var errorContent = res.Content.ReadAsStringAsync().Result;
                    Console.WriteLine($"API Error: {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return null;
            }
        }
        public static bool DeleteUserProfile(string token)
        {
            try
            {
                // Add the required user ID parameter to the URL
                string strUrl = $"{api}QuanLyKhachHang/XoaTaiKhoan";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var res = client.DeleteAsync(strUrl).Result;
                return res.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        // Thêm vào file CXuLy.cs trong namespace WebLuanVan_ASP.NET_MVC.Areas.UpdateProfile.Models
public static bool ChangePassword(CChangePassword x, string token)
{
    try
    {
        string strUrl = $"{api}QuanLyKhachHang/ChangePassword";
        using HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var requestData = new 
        {
            CurrentPassword = x.CurrentPassword,
            NewPassword = x.NewPassword,
            ConfirmPassword = x.ConfirmPassword
        };

        var response = client.PostAsJsonAsync(strUrl, requestData).Result;

        if (response.IsSuccessStatusCode)
        {
            return true;
        }
        else
        {
            // Log lỗi từ API
            var errorContent = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine($"API Error: {errorContent}");
            return false;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Exception: {ex.Message}");
        return false;
    }
}
    }
}
