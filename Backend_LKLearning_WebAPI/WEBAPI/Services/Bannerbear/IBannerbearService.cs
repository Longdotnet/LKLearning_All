using System.Security.Cryptography.X509Certificates;


namespace WEBAPI.Services.Bannerbear
{
    public interface IBannerbearService
    {
        /// <summary>
        /// Gửi yêu cầu tạo chứng chỉ, chờ render xong rồi trả về PDF bytes.
        /// </summary>
        Task<string> GenerateCertificateImageUrlAsync(BannerRequest req);
    }

}
