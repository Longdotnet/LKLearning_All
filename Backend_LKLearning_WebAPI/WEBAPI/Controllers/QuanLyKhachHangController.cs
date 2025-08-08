using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WEBAPI.DTOS;
using WEBAPI.Models;
using WEBAPI.Services.Models;
using Message = WEBAPI.Services.Models.Message;

namespace WEBAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuanLyKhachHangController : ControllerBase
    {
        private readonly LuanvantienganhContext db;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly HttpClient _httpClient;
        private readonly ILogger<QuanLyKhachHangController> _logger;

        public QuanLyKhachHangController(LuanvantienganhContext context, IConfiguration configuration, ILogger<QuanLyKhachHangController> logger
        , IMemoryCache cache, HttpClient httpClient)
        {
            db = context;
            _configuration = configuration;
            _cache = cache;
            _httpClient = httpClient;
            _logger = logger;
        }

        #region Support Methods
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSecret = _configuration["Jwt:Secret"];
            var key = Encoding.ASCII.GetBytes(jwtSecret);
            string role = user.RoleId == 2 ? "Admin" : "User";
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, role)
                }),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private async Task<(bool Success, string ErrorMessage)> SendOtpViaInfoBip(string phoneNumber, string otp)
        {
            const int maxRetries = 2;
            int attempt = 0;

            while (attempt <= maxRetries)
            {
                try
                {
                    var apiKey = _configuration["InfoBip:ApiKey"];
                    var baseUrl = _configuration["InfoBip:BaseUrl"];
                    var senderId = _configuration["InfoBip:SenderId"];

                    if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(senderId))
                    {
                        var errorMessage = "Cấu hình InfoBip không đầy đủ: ApiKey, BaseUrl hoặc SenderId bị thiếu.";
                        _logger.LogError(errorMessage);
                        return (false, errorMessage);
                    }

                    var message = new InfoBipMessage
                    {
                        messages = new List<Message>
                        {
                            new Message
                            {
                                from = senderId,
                                destinations  = new List<Destination>
                                {
                                    new Destination { to = phoneNumber }
                                },
                                text = $"Mã OTP của bạn là: {otp}. Mã có hiệu lực trong 5 phút."
                            }
                        }
                    };

                    var jsonRequest = JsonConvert.SerializeObject(message, Formatting.Indented);
                    _logger.LogInformation("JSON request gửi đến InfoBip: {JsonRequest}", jsonRequest);

                    var content = new StringContent(jsonRequest, Encoding.UTF8);
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                    _httpClient.DefaultRequestHeaders.Clear();
                    _httpClient.DefaultRequestHeaders.Add("Authorization", $"App {apiKey}");
                    _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    var response = await _httpClient.PostAsync($"{baseUrl}/sms/2/text/advanced", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        var errorMessage = $"Gửi SMS OTP thất bại. Mã lỗi: {response.StatusCode}, Nội dung lỗi: {errorContent}";
                        _logger.LogError(errorMessage);
                        if (attempt < maxRetries)
                        {
                            _logger.LogWarning("Thử lại lần {Attempt} cho số {PhoneNumber}", attempt + 1, phoneNumber);
                            attempt++;
                            await Task.Delay(1000);
                            continue;
                        }

                        return (false, errorMessage);
                    }

                    _logger.LogInformation("Gửi SMS OTP thành công đến số {PhoneNumber}", phoneNumber);
                    return (true, null);
                }
                catch (Exception ex)
                {
                    var errorMessage = $"Lỗi khi gửi SMS OTP đến {phoneNumber}: {ex.Message}";
                    _logger.LogError(ex, errorMessage);

                    if (attempt < maxRetries)
                    {
                        _logger.LogWarning("Thử lại lần {Attempt} cho số {PhoneNumber}", attempt + 1, phoneNumber);
                        attempt++;
                        await Task.Delay(1000);
                        continue;
                    }

                    return (false, errorMessage);
                }
            }

            return (false, "Gửi SMS OTP thất bại sau nhiều lần thử.");
        }

        #endregion

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO dto)
        {
            try
            {
                if (dto == null || !dto.IsValid() || string.IsNullOrEmpty(dto.Method))
                {
                    _logger.LogWarning("Yêu cầu ForgotPassword không hợp lệ: chỉ được cung cấp một trong hai trường Phone hoặc Email và Method không được để trống.");
                    return BadRequest("Chỉ được cung cấp một trong hai trường: số điện thoại hoặc email, và phương thức gửi OTP.");
                }


                var user = await db.Users.FirstOrDefaultAsync(u => u.Phone == dto.Phone || u.Email == dto.Email);
                if (user == null)
                {
                    _logger.LogWarning("Số điện thoại hoặc email không tồn tại: {Phone}, {Email}", dto.Phone, dto.Email);
                    return NotFound("Số điện thoại hoặc email không tồn tại trong hệ thống.");
                }

                var otp = new Random().Next(100000, 999999).ToString();
                var cacheKey = string.IsNullOrEmpty(dto.Phone) ? $"OTP:{dto.Email}" : $"OTP:{dto.Phone}";
                _cache.Set(cacheKey, otp, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

                bool sent = false;
                string errorMessage = null;

                if (dto.Method == "SMS" && !string.IsNullOrEmpty(dto.Phone))
                {
                    (sent, errorMessage) = await SendOtpViaInfoBip(dto.Phone, otp);
                }
                else if (dto.Method == "Email" && !string.IsNullOrEmpty(dto.Email))
                {
                    (sent, errorMessage) = await SendOtpViaEmail(dto.Email, otp);
                }
                else
                {
                    return BadRequest("Phương thức gửi OTP không hợp lệ hoặc thông tin liên lạc bị thiếu.");
                }

                if (!sent)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, $"Không thể gửi OTP. Chi tiết lỗi: {errorMessage}");
                }

                return Ok($"OTP đã được gửi qua {dto.Method}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi server khi xử lý ForgotPassword cho {Phone}, {Email}: {Message}", dto?.Phone, dto?.Email, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpPost("VerifyOtp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDTO dto)
        {
            try
            {
                if (dto == null || (string.IsNullOrEmpty(dto.Phone) && string.IsNullOrEmpty(dto.Email)) || string.IsNullOrEmpty(dto.Otp))
                {
                    return BadRequest("Số điện thoại, email hoặc OTP không được để trống.");
                }

                var cacheKey = string.IsNullOrEmpty(dto.Phone) ? $"OTP:{dto.Email}" : $"OTP:{dto.Phone}";
                if (!_cache.TryGetValue(cacheKey, out string storedOtp) || storedOtp != dto.Otp)
                {
                    return BadRequest("OTP không hợp lệ hoặc đã hết hạn.");
                }

                return Ok("Xác minh OTP thành công.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi server: {ex.Message}");
            }
        }
        private async Task<(bool Success, string ErrorMessage)> SendOtpViaEmail(string email, string otp)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");
                var host = smtpSettings["Host"];
                var port = int.Parse(smtpSettings["Port"]);
                var enableSsl = bool.Parse(smtpSettings["EnableSsl"]);
                var senderEmail = smtpSettings["SenderEmail"];
                var senderPassword = smtpSettings["SenderPassword"];

                using (var client = new System.Net.Mail.SmtpClient(host, port))
                {
                    client.EnableSsl = enableSsl;
                    client.Credentials = new System.Net.NetworkCredential(senderEmail, senderPassword);

                    var mailMessage = new System.Net.Mail.MailMessage
                    {
                        From = new System.Net.Mail.MailAddress(senderEmail, "LK Learning"),
                        Subject = "Mã OTP để đặt lại mật khẩu",
                        Body = $"Mã OTP của bạn là: {otp}. Mã có hiệu lực trong 5 phút.",
                        IsBodyHtml = false
                    };
                    mailMessage.To.Add(email);

                    await client.SendMailAsync(mailMessage);
                    _logger.LogInformation("Gửi email OTP thành công đến {Email}", email);
                    return (true, null);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Lỗi khi gửi email OTP đến {email}: {ex.Message}";
                _logger.LogError(ex, errorMessage);
                return (false, errorMessage);
            }
        }
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            try
            {
                if (dto == null || (string.IsNullOrEmpty(dto.Phone) && string.IsNullOrEmpty(dto.Email)) ||
                    string.IsNullOrEmpty(dto.Otp) || string.IsNullOrEmpty(dto.NewPassword) || string.IsNullOrEmpty(dto.ConfirmPassword))
                {
                    return BadRequest("Các trường không được để trống.");
                }

                if (dto.NewPassword != dto.ConfirmPassword)
                {
                    return BadRequest("Mật khẩu mới và xác nhận mật khẩu không khớp.");
                }

                var cacheKey = string.IsNullOrEmpty(dto.Phone) ? $"OTP:{dto.Email}" : $"OTP:{dto.Phone}";
                if (!_cache.TryGetValue(cacheKey, out string storedOtp) || storedOtp != dto.Otp)
                {
                    return BadRequest("OTP không hợp lệ hoặc đã hết hạn.");
                }

                var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (user == null)
                {
                    return NotFound(" email không tồn tại trong hệ thống.");
                }

                user.Password = HashPassword(dto.NewPassword);
                await db.SaveChangesAsync();
                _cache.Remove(cacheKey);

                return Ok("Đặt lại mật khẩu thành công.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi server: {ex.Message}");
            }
        }


        [Authorize(Roles = "User,Admin")]
        [HttpGet("LayDanhSachAvatar")]
        public async Task<IActionResult> LayDanhSachAvatar()
        {
            // Lấy toàn bộ bản ghi Avatar
            var avatars = await db.Avatars
                .Select(a => new AvatarDTO
                {
                    AvatarId = a.AvatarId,
                    UrlPath = a.UrlPath,

                })
                .ToListAsync();

            return Ok(avatars);
        }

        // POST: api/QuanLyKhachHang/DangKy
        [HttpPost("DangKy")]
        public async Task<IActionResult> Dangky(DangKyDTO dangkydto)
        {
            // Kiểm tra xem email này đã được đăng ký chưa.
            if (await db.Users.AnyAsync(u => u.Email == dangkydto.Email))
            {
                return BadRequest("Email đã tồn tại trong hệ thống");
            }

            // So sánh mật khẩu và xác nhận mật khẩu.
            if (dangkydto.Password != dangkydto.ConfirmPassword)
            {
                return BadRequest("Mật khẩu và Xác Nhận Mật Khẩu không trùng");
            }

            // Tạo đối tượng User mới.
            User newUser = new User
            {
                Fullname = "Học viên",
                Email = dangkydto.Email,
                Password = HashPassword(dangkydto.Password),
                RoleId = 1, // Giả sử 1 là Role của người dùng thường

            };

            // 4. Lấy ngẫu nhiên 1 AvatarId từ bảng Avatars
            var totalAvatars = await db.Avatars.CountAsync();
            if (totalAvatars > 0)
            {
                var rnd = new Random();
                var skipCount = rnd.Next(totalAvatars);
                var avatar = await db.Avatars
                                        .OrderBy(a => a.AvatarId)
                                        .Skip(skipCount)
                                        .FirstOrDefaultAsync();
                newUser.AvatarId = avatar.AvatarId;
            }


            db.Users.Add(newUser);
            await db.SaveChangesAsync();

            // Truy vấn để lấy gói cước mặc định có tên "FREE"
            var freePackage = await db.Packages.FirstOrDefaultAsync(p => p.PackageName == "FREE");
            if (freePackage == null)
            {
                // Nếu không tồn tại gói cước "FREE", bạn có thể thông báo lỗi hoặc xử lý theo ý muốn
                return BadRequest("Gói cước mặc định không tồn tại.");
            }

            // Tạo đối tượng đăng ký gói cước sử dụng PackageId của freePackage
            UserPackageRegistration registration = new UserPackageRegistration
            {
                PackageId = freePackage.PackageId,
                UserId = newUser.UserId,  // Sử dụng UserId được gán sau SaveChanges
                RegistrationDate = DateTime.Now,
                ExpirationDate = null
            };
            db.UserPackageRegistrations.Add(registration);
            await db.SaveChangesAsync();

            return Ok("Đăng Ký Thành Công");
        }
        // POST: api/QuanLyKhachHang/DangNhap
        [HttpPost("DangNhap")]
        public async Task<IActionResult> DangNhap(DangNhapDTO dangNhapDTO)
        {
            // Tìm kiếm user theo email kèm theo thông tin Role
            var user = await db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == dangNhapDTO.Email);

            if (user == null)
            {
                return Unauthorized("Sai email hoặc mật khẩu.");
            }

            // So sánh mật khẩu: mật khẩu nhập vào được băm theo SHA256 để so sánh với mật khẩu đã lưu (đã băm khi đăng ký)
            if (user.Password != HashPassword(dangNhapDTO.Password))
            {
                return Unauthorized("Sai email hoặc mật khẩu.");
            }

            // Nếu đăng nhập thành công, tạo JWT token
            var token = GenerateJwtToken(user);

            // Cập nhật LastLoginDate cho user (lưu lại ngày giờ đăng nhập mới nhất)
            user.LastLoginDate = DateTime.Now;

            await db.SaveChangesAsync();

            // Trả về token cùng với role của người dùng và LastLogiListDanhSachTaiKhoannDate định dạng dd/MM/yyyy HH:mm:ss
            return Ok(new
            {
                token,
                role = user.Role?.RoleName,
                lastLoginDate = user.LastLoginDate.HasValue ? user.LastLoginDate.Value.ToString("dd/MM/yyyy HH:mm:ss") : null
            });
        }

        //cái cập nhật này phải truyền token và id user sẽ là trong token .nên là xài api này chỉ cập nhật được thông tin của chính nó là token
        //là cập nhật của người truyền vào thôi á
        [Authorize(Roles = "User,Admin")]
        [HttpPut("CapNhatThongTinUser")]
        public async Task<IActionResult> CapNhatThongTinUser(CapNhatThonTinUserDTO updateDto)
        {
            try
            {
                // Lấy thông tin user từ token
                string userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out int userId))
                {
                    return Unauthorized("Không lấy được thông tin người dùng từ token.");
                }

                // Tìm user trong database
                var user = await db.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound("Không tìm thấy thông tin người dùng.");
                }

                // Validate dữ liệu đầu vào
                if (string.IsNullOrEmpty(updateDto.Email) || string.IsNullOrEmpty(updateDto.Fullname))
                {
                    return BadRequest("Email và Fullname là bắt buộc");
                }

                // Cập nhật thông tin
                user.Email = updateDto.Email;
                user.Fullname = updateDto.Fullname;

                // Chỉ cập nhật password nếu được cung cấp
                if (!string.IsNullOrEmpty(updateDto.Password))
                {
                    user.Password = HashPassword(updateDto.Password);
                }

                user.Age = updateDto.Age;
                user.Phone = updateDto.Phone;
                user.Gender = updateDto.Gender;
                user.AvatarId = updateDto.AvatarId; // Nếu client muốn chọn avatar cụ thể


                if (updateDto.DateofBirth.HasValue)
                {
                    user.DateofBirth = updateDto.DateofBirth;
                }

                await db.SaveChangesAsync();
                return Ok("Cập nhật thông tin thành công!");
            }
            catch (Exception ex)
            {
                // Log lỗi ở đây
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("CapNhatThongTinUserTheoID/{id}")]
        public async Task<IActionResult> CapNhatThongTinUser(int id, [FromBody] CapNhatThonTinUserDTO updateDto)
        {
            if (updateDto == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }

            var user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("Không tìm thấy thông tin người dùng.");
            }

            user.Email = updateDto.Email;
            user.Fullname = updateDto.Fullname;
            if (!string.IsNullOrEmpty(updateDto.Password))
            {
                user.Password = HashPassword(updateDto.Password);
            }
            user.Age = updateDto.Age;
            user.Phone = updateDto.Phone;
            user.Gender = updateDto.Gender;
            user.DateofBirth = updateDto.DateofBirth;
            user.RoleId = updateDto.RoleId;
            try
            {
                await db.SaveChangesAsync();
                return Ok("Cập nhật thông tin thành công!");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Cập nhật thông tin không thành công");
            }
        }

        //endpoint lấy thông tin này dành token .Là token nào sẽ trả về thông tin của token đó .
        //thường dùng để lấy thông tin người đang đăng nhập.
        [Authorize(Roles = "User,Admin")]
        [HttpGet("LayThongTinUser")]
        public async Task<IActionResult> LayThongTinUser()
        {
            // Lấy thông tin UserId từ token (từ Claim có key ClaimTypes.NameIdentifier)
            string userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized("Không lấy được thông tin người dùng từ token.");
            }

            // Tìm đối tượng user theo UserId từ cơ sở dữ liệu
            var user = await db.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("Không tìm thấy thông tin người dùng.");
            }

            // Tạo đối tượng DTO chứa thông tin người dùng để trả về
            var userInfo = new ThongTinUserDTO
            {
                UserId = user.UserId,
                Email = user.Email,
                Fullname = user.Fullname,
                Age = user.Age,
                Phone = user.Phone,
                Gender = user.Gender,
                DateofBirth = user.DateofBirth,
                LastLoginDate = user.LastLoginDate,
                AvatarUrl = user.AvatarId.HasValue
                    ? db.Avatars.Where(a => a.AvatarId == user.AvatarId).Select(a => a.UrlPath).FirstOrDefault()
                    : null,

            };

            return Ok(userInfo);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("LayThongTinUserBangID/{id}")]
        public async Task<IActionResult> LayThongTinUser(int id)
        {
            var user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("Không tìm thấy thông tin người dùng.");
            }

            var userInfo = new UserDTO
            {
                UserId = user.UserId,
                Email = user.Email,
                Fullname = user.Fullname,
                Age = user.Age,
                Phone = user.Phone,
                Gender = user.Gender,
                DateofBirth = user.DateofBirth,
                LastLoginDate = user.LastLoginDate,
                RoleId = user.RoleId,
                AvatarUrl = user.AvatarId.HasValue
                    ? db.Avatars.Where(a => a.AvatarId == user.AvatarId).Select(a => a.UrlPath).FirstOrDefault()
                    : null,
            };

            return Ok(userInfo);
        }

        [Authorize(Roles = "User,Admin,Staff")]
        [HttpDelete("XoaTaiKhoan")]
        public async Task<IActionResult> XoaTaiKhoan()
        {
            string userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized("Không lấy được thông tin người dùng từ token.");
            }

            var user = await db.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("Không tìm thấy tài khoản cần xóa.");
            }

            var registrations = db.UserPackageRegistrations.Where(r => r.UserId == userId);
            db.UserPackageRegistrations.RemoveRange(registrations);
            db.Users.Remove(user);
            await db.SaveChangesAsync();

            return Ok("Xóa tài khoản thành công!");
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> UpdatePassword([FromBody] ChangePasswordDTO dto)
        {
            try
            {
                // Lấy thông tin user từ token
                string userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out int userId))
                {
                    return Unauthorized("Không lấy được thông tin người dùng từ token.");
                }

                // Tìm user trong database
                var user = await db.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound("Không tìm thấy người dùng.");
                }

                // Validate dữ liệu đầu vào
                if (string.IsNullOrEmpty(dto.CurrentPassword) ||
                    string.IsNullOrEmpty(dto.NewPassword) ||
                    string.IsNullOrEmpty(dto.ConfirmPassword))
                {
                    return BadRequest("Các trường mật khẩu không được để trống.");
                }

                // Kiểm tra mật khẩu cũ: băm mật khẩu đầu vào và so sánh với mật khẩu đã băm lưu trong database
                if (user.Password != HashPassword(dto.CurrentPassword))
                {
                    return BadRequest("Mật khẩu cũ không chính xác.");
                }

                // Kiểm tra mật khẩu mới và xác nhận mật khẩu có khớp nhau không
                if (dto.NewPassword != dto.ConfirmPassword)
                {
                    return BadRequest("Mật khẩu mới và xác nhận mật khẩu không khớp.");
                }

                // Cập nhật mật khẩu mới (băm mật khẩu trước khi lưu)
                user.Password = HashPassword(dto.NewPassword);

                await db.SaveChangesAsync();
                return Ok("Cập nhật mật khẩu thành công!");
            }
            catch (Exception ex)
            {
                // Log lỗi tại đây nếu cần (vd. ghi ra file log)
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi server: {ex.Message}");
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("UpdatePasswordById/{id}")]
        public async Task<IActionResult> UpdatePasswordById(int id, [FromBody] ChangePasswordDTO dto)
        {
            try
            {
                if (dto == null ||
                    string.IsNullOrEmpty(dto.NewPassword) ||
                    string.IsNullOrEmpty(dto.ConfirmPassword))
                {
                    return BadRequest("Các trường mật khẩu mới không được để trống.");
                }

                if (dto.NewPassword != dto.ConfirmPassword)
                {
                    return BadRequest("Mật khẩu mới và xác nhận mật khẩu không khớp.");
                }

                var user = await db.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound("Không tìm thấy người dùng với ID đã cung cấp.");
                }

                user.Password = HashPassword(dto.NewPassword);
                await db.SaveChangesAsync();
                return Ok("Cập nhật mật khẩu thành công!");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi server: {ex.Message}");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("ThemUser")]
        public async Task<IActionResult> Themuser([FromBody] UserDTO x)
        {
            if (x == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }
            var user = new User
            {
                Fullname = x.Fullname,
                Email = x.Email,
                Password = HashPassword(x.Password),
                Age = x.Age,
                Phone = x.Phone,
                Gender = x.Gender,
                DateofBirth = x.DateofBirth,
                RoleId = x.RoleId,
                LastLoginDate = null
            };
            try
            {
                db.Users.Add(user);
                await db.SaveChangesAsync();
                return Ok("Thêm thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Thêm không thành công");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetListRole")]
        public async Task<IActionResult> GetListRole()
        {
            var roles = await db.Roles.ToListAsync();
            var roleDtos = roles.Select(r => new
            {
                r.RoleId,
                r.RoleName
            }).ToList();
            return Ok(roleDtos);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("ThemRole")]
        public async Task<IActionResult> ThemRole([FromBody] RoleDTO roleDto)
        {
            if (roleDto == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }
            var existingRole = await db.Roles.FirstOrDefaultAsync(r => r.RoleName == roleDto.RoleName);
            if (existingRole != null)
            {
                return BadRequest("Role đã tồn tại");
            }
            var newRole = new Role
            {
                RoleName = roleDto.RoleName
            };
            try
            {
                db.Roles.Add(newRole);
                await db.SaveChangesAsync();
                return Ok("Thêm role thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Thêm role không thành công");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("XoaRole/{id}")]
        public async Task<IActionResult> XoaRole(int id)
        {
            var role = await db.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound("Không tìm thấy role cần xóa.");
            }
            db.Roles.Remove(role);
            await db.SaveChangesAsync();
            return Ok("Xóa role thành công.");
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("SuaRole/{id}")]
        public async Task<IActionResult> SuaRole(int id, [FromBody] RoleDTO roleDto)
        {
            if (roleDto == null)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }
            var role = await db.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound("Không tìm thấy role cần sửa.");
            }
            role.RoleName = roleDto.RoleName;
            try
            {
                await db.SaveChangesAsync();
                return Ok("Sửa role thành công");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Sửa role không thành công");
            }
        }

        [Authorize(Roles = "User,Admin")]
        [HttpDelete("XoaTaiKhoan/{id}")]
        public async Task<IActionResult> XoaTaiKhoan(int id)
        {
            string userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int currentUserId))
            {
                return Unauthorized("Không lấy được thông tin người dùng từ token.");
            }

            string role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role == "User" && currentUserId != id)
            {
                return Forbid("Bạn không có quyền xóa tài khoản của người khác.");
            }

            var userToDelete = await db.Users.FindAsync(id);
            if (userToDelete == null)
            {
                return NotFound("Không tìm thấy tài khoản.");
            }
            var registrations = db.UserPackageRegistrations.Where(r => r.UserId == userToDelete.UserId);
            db.UserPackageRegistrations.RemoveRange(registrations);
            db.Users.Remove(userToDelete);
            await db.SaveChangesAsync();

            return Ok("Xóa tài khoản thành công.");
        }

        [HttpGet("ListDanhSachTaiKhoan")]
        public async Task<IActionResult> ListDanhSachTaiKhoan()
        {
            var danhSachTaiKhoan = await db.Users
                .Select(u => new
                {
                    u.UserId,
                    u.Email,
                    u.Fullname,
                    u.Age,
                    u.Phone,
                    u.Gender,
                    u.DateofBirth,
                    u.RoleId,
                    u.LastLoginDate,
                    u.UserPackageRegistrations
                })
                .ToListAsync();

            return Ok(danhSachTaiKhoan);
        }

        private string FormatDateTimeFull(DateTime dateTime)
        {
            return dateTime.ToString("dd/MM/yyyy HH:mm:ss");
        }
    }
}