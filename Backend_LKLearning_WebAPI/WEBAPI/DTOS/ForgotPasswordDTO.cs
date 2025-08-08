namespace WEBAPI.DTOS
{
    public class ForgotPasswordDTO
    {
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string Method { get; set; } // "SMS" or "Email"
        public bool IsValid()
        {
            bool hasPhone = !string.IsNullOrWhiteSpace(Phone);
            bool hasEmail = !string.IsNullOrWhiteSpace(Email);
            // Only one must be provided
            // ý nghĩa của hàm này là kiểm tra xem người dùng có cung cấp số điện thoại hoặc email hay không, và chỉ một trong hai trường này được phép có giá trị.
            // Ý NGHĨA CỦA ^
            // Toán tử XOR (^) sẽ trả về true nếu chỉ một trong hai điều kiện là true, và false nếu cả hai đều là true hoặc cả hai đều là false.
            return hasPhone ^ hasEmail;
        }


    }

    public class VerifyOtpDTO
    {
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string Otp { get; set; }
    }

    public class ResetPasswordDTO
    {
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Otp { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}