
using System.Text.Json;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Models;
using WebLuanVan_ASP.NET_MVC.Areas.Login.Models.DTOS;
using static WebLuanVan_ASP.NET_MVC.Config.ApiConfig;

namespace WebLuanVan_ASP.NET_MVC.Areas.Login.Models
{
    public class CXuLy
    {

     

        public static async Task<TokenResultDTO> xacThucLogIN(DangNhapDTO x)
        {
            try
            {
            
                string strUrl = $"{api}QuanLyKhachHang/DangNhap";
                HttpClient client = new HttpClient();
                var res = client.PostAsJsonAsync(strUrl, x);
                res.Wait();
                if (res.Result.IsSuccessStatusCode)
                {
                    string Json= await res.Result.Content.ReadAsStringAsync();
                    var tokenResult = JsonSerializer.Deserialize<TokenResultDTO>(Json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    // xuất thông báo ra json tokenResult
                    Console.WriteLine(JsonSerializer.Serialize(tokenResult));
                    return tokenResult;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
