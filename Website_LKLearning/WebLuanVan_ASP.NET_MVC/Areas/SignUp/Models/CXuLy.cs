
using WebLuanVan_ASP.NET_MVC.Areas.Login.Models.DTOS;
using static WebLuanVan_ASP.NET_MVC.Config.ApiConfig;


namespace WebLuanVan_ASP.NET_MVC.Areas.SignUp.Models.DTOS
{
    public class CXuLy
    {
       

        public static bool DangKy(DangKyDTO x)
        {
            try
            {
                string strUrl = $"{api}QuanLyKhachHang/DangKy";
                HttpClient client = new HttpClient();
                var res = client.PostAsJsonAsync(strUrl, x);
                res.Wait();
                if (res.Result.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
      
    }
}
