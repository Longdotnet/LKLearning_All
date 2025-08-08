using Newtonsoft.Json;

namespace WebLuanVan_ASP.NET_MVC.Areas.ForgotPassword.Models
{
    public class CForgotPassword
    {
        [JsonProperty("phone")]
        public string Phone { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("method")]
        public string Method { get; set; } // "SMS" or "Email"
    }

    public class CVerifyOtp
    {
        [JsonProperty("phone")]
        public string Phone { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("otp")]
        public string Otp { get; set; }
    }

    public class CResetPassword
    {
        [JsonProperty("phone")]
        public string Phone { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("otp")]
        public string Otp { get; set; }
        [JsonProperty("newPassword")]
        public string NewPassword { get; set; }
        [JsonProperty("confirmPassword")]
        public string ConfirmPassword { get; set; }
    }
}