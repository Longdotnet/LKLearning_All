using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace WEBAPI.Services.Bannerbear
{
    public class BannerbearService : IBannerbearService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly string _templateId;

        public BannerbearService(IConfiguration config, HttpClient httpClient)
        {
            _apiKey = config["Bannerbear:ApiKey"];
            _templateId = config["Bannerbear:DefaultTemplateId"];
            _http = httpClient;

            _http.BaseAddress = new Uri("https://api.bannerbear.com/v2/");
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<string> GenerateCertificateImageUrlAsync(BannerRequest req)
        {
            var payload = new
            {
                template = _templateId,
                modifications = new[]
                {
                    new { name = "name",       text = req.StudentName },
                    new { name = "subtitle",   text = req.Subtitle },
                    new { name = "signature",  text = req.Signature },
                    new { name = "createdate", text = req.CreatedAt.ToString("dd/MM/yyyy") },
                    new { name = "expirydate", text = req.ExpirationDate?.ToString("dd/MM/yyyy") ?? "Vĩnh Viễn" },
                    new { name = "machungchi", text = req.VerificationCode },
                }
            };

            // 2. Gửi POST tạo image
            var postResp = await _http.PostAsJsonAsync("images", payload);
            if (!postResp.IsSuccessStatusCode)
            {
                var err = await postResp.Content.ReadAsStringAsync();
                throw new ApplicationException(
                    $"Bannerbear POST error {postResp.StatusCode}: {err}");
            }

            var meta = await postResp.Content.ReadFromJsonAsync<Bannerreponse>();
            var uid = meta.uid;

            // 3. Poll cho đến khi status = completed
            Bannerreponse detail;
            do
            {
                await Task.Delay(500);
                var getResp = await _http.GetAsync($"images/{uid}");
                if (!getResp.IsSuccessStatusCode)
                {
                    var err = await getResp.Content.ReadAsStringAsync();
                    throw new ApplicationException(
                        $"Bannerbear poll error {getResp.StatusCode}: {err}");
                }
                detail = await getResp.Content.ReadFromJsonAsync<Bannerreponse>();
            }
            while (detail.status != "completed");

            // 4. Trả về URL PNG (ưu tiên image_url_png)
            return detail.image_url_png ?? detail.image_url;
        }

    
    }
}
