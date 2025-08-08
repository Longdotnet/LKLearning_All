namespace WebLuanVan_ASP.NET_MVC.Config
{
    public static class ApiConfig
    {
        public static string api { get; private set; }

        public static void Initialize(IConfiguration configuration)
        {
            api = configuration["ApiSettings:BaseUrl"] ?? "http://172.21.9.181:7008/api/";
        }
    }
}
