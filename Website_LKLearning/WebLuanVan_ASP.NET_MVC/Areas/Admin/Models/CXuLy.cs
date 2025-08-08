using System.Net.Http.Headers;
using System.Text;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using NuGet.Common;
using OfficeOpenXml;
using OfficeOpenXml;
using ClosedXML.Excel;
using static WebLuanVan_ASP.NET_MVC.Config.ApiConfig;
using Microsoft.Extensions.Primitives;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using WebLuanVan_ASP.NET_MVC.Areas.Admin.Services.Solana;
namespace WebLuanVan_ASP.NET_MVC.Areas.Admin.Models
{
    public class CXuLy
    {
        // Hard-code cấu hình FCM (có thể thay bằng cách đọc từ appsettings.json nếu cần)
        private static readonly string ServiceAccountKeyPath = "wwwroot/credentials/service-account-key.json";
        private static readonly string ProjectId = "testnotifications-66d15";

        // gửi thông báo cho các ứng dụng đã tải( dùng Firebase, cần FCM Token của từng máy )
        public static async Task<(bool Success, string Message)> SendFcmNotificationToTopicAsync(string topic, string title, string body)
        {
            try
            {
                if (string.IsNullOrEmpty(ServiceAccountKeyPath) || string.IsNullOrEmpty(ProjectId))
                {
                    return (false, "FCM configuration is missing.");
                }

                var credential = GoogleCredential.FromFile(ServiceAccountKeyPath)
                    .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");
                var accessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    var data = new
                    {
                        message = new
                        {
                            topic = topic,
                            notification = new
                            {
                                title = title,
                                body = body
                            }
                        }
                    };

                    var json = JsonConvert.SerializeObject(data);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(
                        $"https://fcm.googleapis.com/v1/projects/{ProjectId}/messages:send",
                        content);

                    var result = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        return (true, "Gửi thông báo thành công");
                    }
                    else
                    {
                        return (false, $"Gửi thông báo thất bại: {response.StatusCode} - {result}");
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi gửi thông báo: {ex.Message}");
            }
        }
        public static bool AddQuestionToExamSet(CExamSetQuestion dto, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyExam/AddQuestionToExamSet";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.PostAsJsonAsync(strUrl, dto);
                res.Wait();
                return res.Result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // ImportExcel như cái tên
        public static (bool Success, string Message) ImportExcel(IFormFile excelFile, string token)
            {
            if (excelFile == null || excelFile.Length == 0)
            {
                return (false, "Vui lòng chọn file Excel!");
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    excelFile.CopyTo(stream);
                    using (var workbook = new ClosedXML.Excel.XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheet(1); // Lấy sheet đầu tiên
                        var rowCount = worksheet.RowsUsed().Count();

                        var questions = new List<CQuestion>();
                        for (int row = 2; row <= rowCount; row++) // Bắt đầu từ row 2 (bỏ header)
                        {
                            string contentTypeIdStr = worksheet.Cell(row, 2).Value.ToString().Trim(); // Cột 2: contentTypeId
                            string questionTypeIdStr = worksheet.Cell(row, 4).Value.ToString().Trim(); // Cột 4: questionTypeId
                            string questionLevelIdStr = worksheet.Cell(row, 10).Value.ToString().Trim(); // Cột 10: questionLevelId

                            if (!int.TryParse(contentTypeIdStr, out int contentTypeId))
                            {
                                return (false, $"Lỗi ở hàng {row}, cột contentTypeId: '{contentTypeIdStr}' không phải số.");
                            }
                            if (!int.TryParse(questionTypeIdStr, out int questionTypeId))
                            {
                                return (false, $"Lỗi ở hàng {row}, cột questionTypeId: '{questionTypeIdStr}' không phải số.");
                            }
                            if (!int.TryParse(questionLevelIdStr, out int questionLevelId))
                            {
                                return (false, $"Lỗi ở hàng {row}, cột questionLevelIdStr: '{questionLevelIdStr}' không phải số.");
                            }
                            var question = new CQuestion
                            {
                                ContentTypeId = contentTypeId, // Cột 2
                                QuestionText = worksheet.Cell(row, 3).Value.ToString(), // Cột 3
                                QuestionTypeId = questionTypeId, // Cột 4
                                AnswerOptions = worksheet.Cell(row, 5).Value.ToString(), // Cột 5
                                CorrectAnswer = worksheet.Cell(row, 6).Value.ToString(), // Cột 6
                                ImageUrl = worksheet.Cell(row, 7).Value.ToString(), // Cột 7
                                AudioUrl = worksheet.Cell(row, 8).Value.ToString(), // Cột 8
                                Explanation = worksheet.Cell(row, 9).Value.ToString(), // Cột 9
                                QuestionLevelId = questionLevelId, // Cột 10
                                QuestionDescription = worksheet.Cell(row, 11).Value.ToString() // cột 11
                            };
                            questions.Add(question);
                        }

                        // Gọi API để insert multiple questions
                        string apiUrl = $"{api}QuanLyCauHoi/InsertMultipleQuestions";
                        HttpClient client = new HttpClient();
                        client.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                        var content = new StringContent(JsonConvert.SerializeObject(questions), Encoding.UTF8, "application/json");
                        var res = client.PostAsync(apiUrl, content);
                        res.Wait();

                        if (res.Result.IsSuccessStatusCode)
                        {
                            return (true, "Import thành công!");
                        }
                        else
                        {
                            var error = res.Result.Content.ReadAsStringAsync().Result;
                            return (false, "Lỗi: " + error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, "Lỗi khi xử lý file: " + ex.Message);
            }
        }
        // CapNhatTrangThaiDonHangZaloPay
        public static bool CapNhatTrangThaiDonHangZaloPay(string appTransId, string status, string token)
        {
            try
            {
                string strUrl = $"{api}payment/update-order-status";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var dto = new
                {
                    AppTransId = appTransId,
                    Status = status
                };

                var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
                var res = client.PostAsync(strUrl, content);
                res.Wait();

                return res.Result.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi cập nhật trạng thái đơn hàng ZaloPay: {ex.Message}");
                return false;
            }
        }

        // Dành cho Message gọi từ backend 
        public static async Task<List<CConversation>> GetAdminConversationsAsync(int adminId, string token)
        {
            try
            {
                string strUrl = $"{api}Conversations/admin/{adminId}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = await client.GetFromJsonAsync<List<CConversation>>(strUrl);
                return res ?? new List<CConversation>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAdminConversationsAsync: {ex.Message}");
                return new List<CConversation>();
            }
        }
        public static COnlineUsersAndAdminsResult GetOnlineUsersAndAdminsAsync(string token)
        {
            string url = $"{api}Conversations/online-users-admins";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var res = client.GetAsync(url);
            res.Wait();
            if (!res.Result.IsSuccessStatusCode)
                return new COnlineUsersAndAdminsResult();

            var json = res.Result.Content.ReadAsStringAsync();
            json.Wait();
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<COnlineUsersAndAdminsResult>(json.Result);
            return result ?? new COnlineUsersAndAdminsResult();
        }

        public static async Task<CConversation> UserCreateConversationAsync(string token)
        {
            try
            {
                string strUrl = $"{api}Conversations/user-create";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                // Gửi POST không có body
                var res = await client.PostAsync(strUrl, null);
                if (!res.IsSuccessStatusCode)
                {
                    var errorContent = await res.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error Response: {errorContent}");
                    return null;
                }
                // Đọc JSON trả về thành CConversation
                return await res.Content.ReadFromJsonAsync<CConversation>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UserCreateConversationAsync: {ex.Message}");
                return null;
            }
        }
        public static async Task<List<CConversation>> GetUserConversationsAsync(int userId, string token)
        {
            try
            {
                string strUrl = $"{api}Conversations/user/{userId}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = await client.GetFromJsonAsync<List<CConversation>>(strUrl);
                return res ?? new List<CConversation>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserConversationsAsync: {ex.Message}");
                return new List<CConversation>();
            }
        }
        public static async Task<CConversation> GetConversationBetweenAdminAndUserAsync(int adminId, int userId, string token)
        {
            try
            {
                string strUrl = $"{api}Conversations/between?adminId={adminId}&userId={userId}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = await client.GetFromJsonAsync<CConversation>(strUrl);
                return res;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetConversationBetweenAdminAndUserAsync: {ex.Message}");
                return null;
            }
        }


        public static async Task<CConversation> CreateConversationAsync(CConversation conversation, string token)
        {
            try
            {
                string strUrl = $"{api}Conversations";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = await client.PostAsJsonAsync(strUrl, conversation);
                res.EnsureSuccessStatusCode();
                return await res.Content.ReadFromJsonAsync<CConversation>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateConversationAsync: {ex.Message}");
                return null;
            }
        }

        public static async Task<List<CMessage>> GetMessagesAsync(int conversationId, string token)
        {
            try
            {
                string strUrl = $"{api}Messages/conversation/{conversationId}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = await client.GetFromJsonAsync<List<CMessage>>(strUrl);
                return res ?? new List<CMessage>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetMessagesAsync: {ex.Message}");
                return new List<CMessage>();
            }
        }

        public static async Task<bool> SendMessageAsync(CMessage message, string token)
        {
            try
            {
                string strUrl = $"{api}Messages/send";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = await client.PostAsJsonAsync(strUrl, message);
                if (!res.IsSuccessStatusCode)
                {
                    var errorContent = await res.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error Response: {errorContent}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendMessageAsync: {ex.Message}");
                return false;
            }
        }
        
        // ExamSet
        public static List<CExamSet> GetListExamSet(string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyExam/GetAllExamSets";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.GetFromJsonAsync<List<CExamSet>>(strUrl);
                res.Wait();
                return res.Result ?? new List<CExamSet>();
            }
            catch
            {
                return new List<CExamSet>();
            }
        }
        // Lấy bộ đề theo khóa học
        public static List<CExamSet> GetExamSetsByCourse(int courseId, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyExam/GetExamSetsByCourse?courseId={courseId}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.GetFromJsonAsync<List<CExamSet>>(strUrl);
                res.Wait();
                return res.Result ?? new List<CExamSet>();
            }
            catch
            {
                return new List<CExamSet>();
            }
        }

        // Lấy chi tiết bộ đề
        public static CExamSet GetExamSetById(int id, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyExam/GetExamSetById/{id}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.GetFromJsonAsync<CExamSet>(strUrl);
                res.Wait();
                return res.Result;
            }
            catch
            {
                return null;
            }
        }

        // Tạo bộ đề mới
        public static bool CreateExamSet(CExamSet examSet, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyExam/CreateExamSet";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.PostAsJsonAsync(strUrl, examSet);
                res.Wait();
                return res.Result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // Sửa bộ đề
        public static bool UpdateExamSet(int id, CExamSet examSet, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyExam/UpdateExamSet/{id}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.PutAsJsonAsync(strUrl, examSet);
                res.Wait();
                return res.Result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // Xóa bộ đề
        public static bool DeleteExamSet(int id, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyExam/DeleteExamSet/{id}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.DeleteAsync(strUrl);
                res.Wait();
                return res.Result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        // Lấy danh sách câu hỏi của bộ đề
        public static List<CExamSetQuestion> GetQuestionsByExamSet(int examSetId, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyExam/GetQuestionsByExamSet/{examSetId}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.GetFromJsonAsync<List<CExamSetQuestion>>(strUrl);
                res.Wait();
                return res.Result ?? new List<CExamSetQuestion>();
            }
            catch
            {
                return new List<CExamSetQuestion>();
            }
        }
        // DeleteQuestionFromExamSet
        public static bool DeleteQuestionFromExamSet(int examSetId, int questionId, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyExam/DeleteQuestionFromExamSet/?examSetId={examSetId}&questionId={questionId}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.DeleteAsync(strUrl);
                res.Wait();
                return res.Result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        // SwapExamQuestionOrder 
        public static bool SwapExamQuestionOrder(CSwapExamQuestionOrder dto, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyExam/SwapExamQuestionOrder";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.PutAsJsonAsync(strUrl, dto);
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
        // Xóa nhiều câu hỏi từ bộ đề 
        public static bool DeleteMultipleQuestionsFromExamSet(List<CExamSetQuestion> questions, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyExam/DeleteMultipleQuestionsFromExamSet";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Chuyển sang DTO nếu cần, ở đây dùng luôn CExamSetQuestion nếu cấu trúc giống ExamSetQuestionDTO
                var res = client.PostAsJsonAsync(strUrl, questions);
                res.Wait();
                return res.Result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        // Cập nhật thứ tự câu hỏi trong bộ đề
        public static bool UpdateExamQuestionOrder(int examSetId, int questionId, int questionOrder, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyExam/UpdateExamQuestionOrder";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var dto = new
                {
                    ExamSetId = examSetId,
                    QuestionId = questionId,
                    QuestionOrder = questionOrder
                };

                var res = client.PutAsJsonAsync(strUrl, dto);
                res.Wait();
                return res.Result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        // Chứng chỉ
        public static List<CCertificate> GetListCertificate(string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyCertificate/GetListCertificates";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.GetFromJsonAsync<List<CCertificate>>(strUrl);
                res.Wait();
                return res.Result ?? new List<CCertificate>();
            }
            catch
            {
                return new List<CCertificate>();
            }
        }
        // Lấy chi tiết Certificate theo Id
        public static CCertificate GetCertificateById(int id, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyCertificate/GetCertificateById/{id}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.GetFromJsonAsync<CCertificate>(strUrl);
                res.Wait();
                return res.Result;
            }
            catch
            {
                return null;
            }
        }

        // Xóa Certificate theo Id
        public static bool DeleteCertificate(int id, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyCertificate/DeleteCertificate/{id}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.DeleteAsync(strUrl);
                res.Wait();
                return res.Result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // Lấy danh sách CertificateType
        public static List<CCertificateType> GetListCertificateTypes(string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyCertificate/GetListCertificateTypes";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.GetFromJsonAsync<List<CCertificateType>>(strUrl);
                res.Wait();
                return res.Result ?? new List<CCertificateType>();
            }
            catch
            {
                return new List<CCertificateType>();
            }
        }

        // Lấy chi tiết CertificateType theo Id
        public static CCertificateType GetCertificateTypeById(int id, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyCertificate/GetCertificateTypeById/{id}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.GetFromJsonAsync<CCertificateType>(strUrl);
                res.Wait();
                return res.Result;
            }
            catch
            {
                return null;
            }
        }

        // Thêm mới CertificateType
        public static bool AddCertificateType(CCertificateType certificateType, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyCertificate/AddCertificateType";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.PostAsJsonAsync(strUrl, certificateType);
                res.Wait();
                return res.Result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        // Xác thực chứng chỉ real hay không
        public static CertificateVerifyResult VerifyCertificate(string verifyCode)
        {
            try
            {
                string url = $"{api}Certificates/VerifyCertificate";
                var request = new VerifyRequest { VerifyCode = verifyCode };
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(request);

                using (var client = new HttpClient())
                {
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = client.PostAsync(url, content).Result;
                    if (!response.IsSuccessStatusCode)
                        return new CertificateVerifyResult { Message = "Không xác thực được chứng chỉ hoặc mã không hợp lệ." };

                    var responseJson = response.Content.ReadAsStringAsync().Result;
                    var result = Newtonsoft.Json.JsonConvert.DeserializeObject<CertificateVerifyResult>(responseJson);
                    return result;
                }
            }
            catch
            {
                return new CertificateVerifyResult { Message = "Lỗi hệ thống khi xác thực chứng chỉ." };
            }
        }
        // Cập nhật CertificateType
        public static bool UpdateCertificateType(int id, CCertificateType certificateType, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyCertificate/UpdateCertificateType/{id}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.PutAsJsonAsync(strUrl, certificateType);
                res.Wait();
                return res.Result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // Xóa CertificateType
        public static bool DeleteCertificateType(int id, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyCertificate/DeleteCertificateType/{id}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.DeleteAsync(strUrl);
                res.Wait();
                return res.Result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>

        // C# - CXuLy.cs (bổ sung vào class CXuLy)
        public static List<CVocabulary> GetListVocabulary(string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyVocabulary/GetListVocabulary";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.GetFromJsonAsync<List<CVocabulary>>(strUrl);
                res.Wait();
                return res.Result ?? new List<CVocabulary>();
            }
            catch
            {
                return new List<CVocabulary>();
            }
        }
            public static List<CAvatar> GetListAvatar(string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyAvatar/GetListAvatar";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.GetFromJsonAsync<List<CAvatar>>(strUrl);
                res.Wait();
                return res.Result ?? new List<CAvatar>();
            }
            catch
            {
                return new List<CAvatar>();
            }
        }

        public static List<CUserExamHistory> GetUserExamHistory(int userId)
        {
            try
            {
                string strUrl = $"{api}QuanLyExam/GetUserExamHistory?userId={userId}";
                HttpClient client = new HttpClient();
                var res = client.GetFromJsonAsync<List<CUserExamHistory>>(strUrl);
                res.Wait();
                return res.Result ?? new List<CUserExamHistory>();
            }
            catch
            {
                return new List<CUserExamHistory>();
            }
        }

        public static List<CVocabulary> GetListVocabularyUser(string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyVocabulary/GetListVocabulary";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.GetFromJsonAsync<List<CVocabulary>>(strUrl);
                res.Wait();
                var vocabList = res.Result ?? new List<CVocabulary>();

                // Fetch meanings for each vocabulary
                foreach (var vocab in vocabList)
                {
                    vocab.Meanings = GetMeaningsByVocabularyId(vocab.VocabularyId, token);
                }
                return vocabList;
            }
            catch
            {
                return new List<CVocabulary>();
            }
        }

        // C# - CXuLy.cs (bổ sung vào class CXuLy)
        public static bool AddVocabularyCategoryMapping(CVocabularyCategoryMapping ctm,string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyVocabulary/AddVocabularyCategoryMapping";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.PostAsJsonAsync(strUrl, ctm);
                res.Wait();
                return res.Result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        public static bool CreateVocabulary(CVocabulary vocab, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyVocabulary/CreateVocabulary";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.PostAsJsonAsync(strUrl, vocab);
                res.Wait();
                return res.Result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        public static bool UpdateVocabularyCategoryMapping(CVocabularyCategoryMapping ctm,string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyVocabulary/UpdateVocabularyCategoryMapping/{ctm.VocabularyId}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.PutAsJsonAsync(strUrl, ctm);
                res.Wait();
                return res.Result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        public static bool UpdateVocabulary(CVocabulary vocab, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyVocabulary/UpdateVocabulary/{vocab.VocabularyId}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.PutAsJsonAsync(strUrl, vocab);
                res.Wait();
                return res.Result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        public static bool DeleteVocabularyCategoryMapping(int vocabularyId, int vocabularyCategoryId, DateTime dateAdded, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyVocabulary/DeleteVocabularyCategoryMapping";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var dto = new
                {
                    vocabularyId = vocabularyId,
                    vocabularyCategoryId = vocabularyCategoryId,
                    dateAdded = dateAdded
                };

                var json = JsonConvert.SerializeObject(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Use SendAsync to send a DELETE with a body
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri(strUrl),
                    Content = content
                };

                var res = client.SendAsync(request);
                res.Wait();
                return res.Result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        public static bool DeleteVocabulary(int id, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyVocabulary/DeleteVocabulary/{id}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.DeleteAsync(strUrl);
                res.Wait();
                return res.Result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        public static bool AddVocabularyCategory(CVocabularyCategory category, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyVocabulary/AddVocabularyCategory";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.PostAsJsonAsync(strUrl, category);
                res.Wait();
                return res.Result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public static bool UpdateVocabularyCategory(int id, CVocabularyCategory category, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyVocabulary/UpdateVocabularyCategory/{id}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.PutAsJsonAsync(strUrl, category);
                res.Wait();
                return res.Result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public static bool DeleteVocabularyCategory(int id, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyVocabulary/DeleteVocabularyCategory/{id}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.DeleteAsync(strUrl);
                res.Wait();
                return res.Result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }


        public static CVocabulary GetVocabularyById(int id, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyVocabulary/GetVocabularyById/{id}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.GetFromJsonAsync<CVocabulary>(strUrl);
                res.Wait();
                return res.Result;
            }
            catch
            {
                return null;
            }
        }

        public static List<CVocabularyMeaning> GetMeaningsByVocabularyId(int vocabularyId, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyVocabulary/GetMeaningsByVocabularyId/{vocabularyId}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.GetFromJsonAsync<List<CVocabularyMeaning>>(strUrl);
                res.Wait();
                return res.Result ?? new List<CVocabularyMeaning>();
            }
            catch
            {
                return new List<CVocabularyMeaning>();
            }
        }

        public static List<CVocabularyCategory> GetListVocabularyCategory(string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyVocabulary/GetListVocabularyCategory";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.GetFromJsonAsync<List<CVocabularyCategory>>(strUrl);
                res.Wait();
                return res.Result ?? new List<CVocabularyCategory>();
            }
            catch
            {
                return new List<CVocabularyCategory>();
            }
        }

        public static List<CVocabularyCategoryMapping> GetListVocabularyCategoryMapping(string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyVocabulary/GetListVocabularyCategoryMapping";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.GetFromJsonAsync<List<CVocabularyCategoryMapping>>(strUrl);
                res.Wait();
                return res.Result ?? new List<CVocabularyCategoryMapping>();
            }
            catch
            {
                return new List<CVocabularyCategoryMapping>();
            }
        }

        public static bool deleteOrders(string orderId, string token)
        {
            try
            {
                string strUrl = $"{api}payment/DeleteOrders/" + orderId;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var res = client.DeleteAsync(strUrl);
                res.Wait();
                return res.Result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        // Lấy review theo ID
        public static CReview GetReviewById(int reviewId, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyReview/GetById/{reviewId}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var res = client.GetFromJsonAsync<CReview>(strUrl);
                res.Wait();
                return res.Result;
            }
            catch
            {
                return null;
            }
        }


        // Xóa review
        public static bool DeleteReview(int reviewId, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyReview/DeleteReview/{reviewId}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.DeleteAsync(strUrl);
                res.Wait();
                return res.Result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // Sửa review
        public static bool UpdateReview(int reviewId, CReview review, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyReview/UpdateReview/{reviewId}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.PutAsJsonAsync(strUrl, review);
                res.Wait();
                return res.Result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // Thêm review mới (nếu cần)
        public static bool CreateReview(CReview review, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyReview/CreateReviewCourseID";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.PostAsJsonAsync(strUrl, review);
                res.Wait();
                Console.WriteLine("URL: " + strUrl);
                Console.WriteLine("Payload: " + JsonConvert.SerializeObject(review));
                if (!res.Result.IsSuccessStatusCode)
                {
                    var error = res.Result.Content.ReadAsStringAsync().Result;
                    Console.WriteLine("API Error: " + error);
                }

                return res.Result.IsSuccessStatusCode;

            }
            catch
            {
                return false;
            }
        }

        public static List<CReview> getDSReview()
        {
            try
            {
                string strUrl = $"{api}QuanLyReview/GetDSReviewByCourse";
                HttpClient client = new HttpClient();
                var res = client.GetFromJsonAsync<List<CReview>>(strUrl);
                res.Wait();
                var result = res.Result;
                if (result == null)
                {
                    Console.WriteLine("Deserialization returned null. Check CReview class structure.");
                }
                return result ?? new List<CReview>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in getDSReview: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return new List<CReview>(); // Return empty list instead of null to avoid ViewBag.Error
            }
        }
        public static List<CReview> getDSReport()
        {
            try
            {
                string strUrl = $"{api}QuanLyReview/GetDSReport";
                HttpClient client = new HttpClient();
                var res = client.GetFromJsonAsync<List<CReview>>(strUrl);
                res.Wait();
                var result = res.Result;
                if (result == null)
                {
                    Console.WriteLine("Deserialization returned null. Check CReview class structure.");
                }
                return result?.Where(r => r.ReviewType == "2" || r.ReviewType == "3").ToList() ?? new List<CReview>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in getDSReport: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return new List<CReview>();
            }
        }

        public static bool CreateReport(CReview report, string token)
        {
            try
            {
                string strUrl = report.ReviewType == "2"
                    ? $"{api}QuanLyReview/CreateReportLesson"
                    : $"{api}QuanLyReview/CreateReportCourse";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var res = client.PostAsJsonAsync(strUrl, report);
                res.Wait();
                Console.WriteLine("URL: " + strUrl);
                Console.WriteLine("Payload: " + JsonConvert.SerializeObject(report));
                if (!res.Result.IsSuccessStatusCode)
                {
                    var error = res.Result.Content.ReadAsStringAsync().Result;
                    Console.WriteLine("API Error: " + error);
                }
                return res.Result.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateReport: {ex.Message}");
                return false;
            }
        }
        public static List<COrders> getDSOrders(string token)
        {
            try
            {
                string strUrl = $"{api}payment/GetListOrders";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var res = client.GetFromJsonAsync<List<COrders>>(strUrl);
                res.Wait();
                return res.Result;
            }
            catch
            {
                return null;
            }
        }
        public static List<COrders> GetDSOrderByUserId(string token)
        {
            try
            {
                string strUrl = $"{api}payment/GetDSOrderByUserId";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var res = client.GetFromJsonAsync<List<COrders>>(strUrl);
                res.Wait();
                return res.Result;
            }
            catch
            {
                return null;
            }
        }
        public static List<CPaymentMethod> getDSPaymentMethod(string token)
        {
            try
            {
                string strUrl = $"{api}payment/GetListPaymentMethods";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var res = client.GetFromJsonAsync<List<CPaymentMethod>>(strUrl);
                res.Wait();
                return res.Result;
            }
            catch
            {
                return null;
            }
        }
        public static bool UpdatePaymentMethod(CPaymentMethod dto, string token)
        {
            try
            {
                string strUrl = $"{api}payment/UpdatePaymentMethod/{dto.PaymentMethodId}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var res = client.PutAsJsonAsync(strUrl, dto);
                res.Wait();
                if (res.Result.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorContent = res.Result.Content.ReadAsStringAsync().Result;
                    Console.WriteLine($"Error Response: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdatePaymentMethod: {ex.Message}");
                return false;
            }
        }
        public static bool DeletePaymentMethod(string id, string token)
        {
            try
            {
                string strUrl = $"{api}payment/DeletePaymentMethod/" + id;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var res = client.DeleteAsync(strUrl);
                res.Wait();
                if (res.Result.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorContent = res.Result.Content.ReadAsStringAsync().Result;
                    Console.WriteLine($"Error Response: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeletePaymentMethod: {ex.Message}");
                return false;
            }
        }
        public static bool AddPaymentMethod(CPaymentMethod dto, string token)
        {
            try
            {
                string strUrl = $"{api}payment/AddPaymentMethod";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var res = client.PostAsJsonAsync(strUrl, dto);
                res.Wait();
                if (res.Result.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorContent = res.Result.Content.ReadAsStringAsync().Result;
                    Console.WriteLine($"Error Response: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddPaymentMethod: {ex.Message}");
                return false;
            }
        }
        public static bool UpdateQuestionOrder(CLessonQuestion dto, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyCauHoi/UpdateQuestionOrder";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var res = client.PutAsJsonAsync(strUrl, dto);
                res.Wait();
                if (res.Result.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorContent = res.Result.Content.ReadAsStringAsync().Result;
                    Console.WriteLine($"Error Response: {errorContent}");
                    return false;
                }

            
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateQuestionOrder: {ex.Message}");
                return false;
            }
        }
        public static bool UpdateLessonOrder(CCourseLessons dto, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyBaiHoc/UpdateLessonOrder";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var res = client.PutAsJsonAsync(strUrl, dto);
                res.Wait();
                if (res.Result.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorContent = res.Result.Content.ReadAsStringAsync().Result;
                    Console.WriteLine($"Error Response: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateLessonOrder: {ex.Message}");
                return false;
            }
        }
        public static List<CAcademicResult> GetDSAcademicResult(string token)
        {

            try
            {
                string strUrl = $"{api}QuanLyKetQuaHoc/GetAcademicResults";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                   new AuthenticationHeaderValue("Bearer", token);
                var res = client.GetFromJsonAsync<List<CAcademicResult>>(strUrl);
                res.Wait();
                return res.Result;
            }
            catch
            {
                return null;
            }
        }
        
        public static List<CAcademicResult> GetDSAcademicResultByUserId(string userId,string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyKetQuaHoc/GetAcademicResults?userId=" + userId;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
              new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.GetFromJsonAsync<List<CAcademicResult>>(strUrl);
                res.Wait();
                return res.Result;
            }
            catch
            {
                return null;
            }
        }
        public static List<CAcademicResult> GetDSAcademicResultByUserIdAndCourseId(string userId,string courseId,string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyKetQuaHoc/GetAcademicResults?userId={userId}3&courseId={courseId}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
              new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.GetFromJsonAsync<List<CAcademicResult>>(strUrl);
                res.Wait();
                return res.Result;
            }
            catch
            {
                return null;
            }
        }
        public static List<CAcademicResult> GetAcademicResultByUserIdAndCourseIdAndLessonId(string userId, string courseId,string lessonId,string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyKetQuaHoc/GetAcademicResults?userId={userId}3&courseId={courseId}&lessonId={lessonId}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
              new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.GetFromJsonAsync<List<CAcademicResult>>(strUrl);
                res.Wait();
                return res.Result;
            }
            catch
            {
                return null;
            }
        }


        public static List<CQuestionLevel> getDSQuestionLevel()
        {
            try
            {
                string strUrl = $"{api}QuanLyCauHoi/GetQuestionLevels";
                HttpClient client = new HttpClient();
                var res = client.GetFromJsonAsync<List<CQuestionLevel>>(strUrl);
                res.Wait();
                return res.Result;
            }
            catch
            {
                return null;
            }
        }

        public static CQuestionLevel getQuestionLevelById(string id)
        {
            try
            {
                string strUrl = $"{api}QuanLyCauHoi/GetQuestionLevelById/" + id;
                HttpClient client = new HttpClient();
                var res = client.GetFromJsonAsync<CQuestionLevel>(strUrl);
                res.Wait();
                return res.Result;
            }
            catch
            {
                return null;
            }
        }
        public static bool themQuestionLevel(CQuestionLevel x, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyCauHoi/InsertQuestionLevel";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
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
        public static bool suaQuestionLevel(string id, CQuestionLevel x, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyCauHoi/UpdateQuestionLevel/" + id;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
               new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.PutAsJsonAsync(strUrl, x);
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
        public static bool xoaQuestionLevel(string id, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyCauHoi/DeleteQuestionLevel/" + id;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
               new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.DeleteAsync(strUrl);
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

        public static List<CCourse> getDSCourse()
        {
            try
            {
                // làm sao dùng api trong strUrl



                string strUrl = $"{api}QuanLyKhoaHoc/GetListCourse";
                HttpClient client = new HttpClient();
                var res = client.GetFromJsonAsync<List<CCourse>>(strUrl);
                res.Wait();
                return res.Result;
            }
            catch
            {
                return null;
            }
        }
        public static List<CRole> getRole(string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyKhachHang/GetListRole";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.GetFromJsonAsync<List<CRole>>(strUrl);
                res.Wait();
                return res.Result;
            }
            catch
            {
                return null;
            }
        }
        // Lấy Banner 
        public static List<CBanner> getBanner()
        {
            try
            {
                string strUrl = $"{api}QuanLyBanner/GetListBanners";
                HttpClient client = new HttpClient();
                var res = client.GetFromJsonAsync<List<CBanner>>(strUrl);
                res.Wait();
                return res.Result;
            }
            catch
            {
                return null;
            }
        }
        // lấy banner đầu tiền
        public static CBanner getBannerById(string id)
        {
            try
            {
                string strUrl = $"{api}QuanLyBanner/GetBannerById/{id}";
                HttpClient client = new HttpClient();
                var res = client.GetFromJsonAsync<CBanner>(strUrl);
                res.Wait();
                return res.Result;
            }
            catch
            {
                return null;
            }
        }
        // Thêm banner 
        public static bool themBanner(CBanner x,string token) {
            try
            {
                string strUrl = $"{api}QuanLyBanner/AddBanner";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
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
     
        public static bool suaBanner(string id, CBanner x, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyBanner/UpdateBanner/" + id;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
               new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.PutAsJsonAsync(strUrl, x);
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
        public static bool xoaBanner(string id, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyBanner/DeleteBanner/" + id;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
               new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.DeleteAsync(strUrl);
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

        public static List<CContentType> getDSContentType()
        {
            try
            {
                string strUrl = $"{api}QuanLyCauHoi/GetListQuestionContent";
                HttpClient client = new HttpClient();
                var res = client.GetFromJsonAsync<List<CContentType>>(strUrl);
                res.Wait();
                return res.Result;
            }
            catch
            {
                return null;
            }
        }
        public static CContentType getContentTypeById(string id)
        {
            try
            {
                string strUrl = $"{api}QuanLyCauHoi/GetQuestionContentById/" + id;
                HttpClient client = new HttpClient();
                var res = client.GetFromJsonAsync<CContentType>(strUrl);
                res.Wait();
                return res.Result;
            }
            catch
            {
                return null;
            }
        }
        public static bool themContentType(CContentType x, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyCauHoi/InsertQuestionContent";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
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
        public static bool xoaContentType(string id, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyCauHoi/DeleteQuestionContent/" + id;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
               new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.DeleteAsync(strUrl);
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
        public static bool suaContentType(string id, CContentType x, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyCauHoi/UpdateQuestionContent/" + id;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
               new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.PutAsJsonAsync(strUrl, x);
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
        public static List<CCategory> getDSCategory()
        {
            try
            {
                string strUrl = $"{api}QuanLyKhoaHoc/GetListCategories";
                HttpClient client = new HttpClient();

                var res = client.GetFromJsonAsync<List<CCategory>>(strUrl);
                res.Wait();
                return res.Result;
            }
            catch
            {
                return null;
            }
        }
        public static List<CLevel> getDSLevel()
        {
            try
            {
                string strUrl = $"{api}QuanLyKhoaHoc/GetListLevels";
                HttpClient client = new HttpClient();

                var res = client.GetFromJsonAsync<List<CLevel>>(strUrl);
                res.Wait();
                return res.Result;
            }
            catch
            {
                return null;
            }
        }
        public static List<CUsers> getDSUsers()
        {
            try
            {
                string strUrl = $"{api}QuanLyKhachHang/ListDanhSachTaiKhoan";
                HttpClient client = new HttpClient();

            
                var res = client.GetFromJsonAsync<List<CUsers>>(strUrl);
                res.Wait();
                return res.Result;
            }
            catch
            {
                return null;
            }
        }
        public static List<CLesson> getDSLesson()
        {
            try
            {
                string strUrl = $"{api}QuanLyBaiHoc/GetListLesson";
                HttpClient client = new HttpClient();
                var res = client.GetFromJsonAsync<List<CLesson>>(strUrl);
                res.Wait();
                return res.Result;
            }
            catch
            {
                return null;
            }
        }
        public static List<CLessonQuestion> GetLessonQuestionByID(string id,string token)
        {

            try
            {
                string strUrl = $"{api}QuanLyCauHoi/GetLessonQuestionByID/" +id;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var res = client.GetFromJsonAsync<List<CLessonQuestion>>(strUrl);
                res.Wait();
                return res.Result;
            }
            catch
            {
                return null;
            }
        }
        public static List<CQuestion> getDSQuestion()
        {
            try
            {
                string strUrl = $"{api}QuanLyCauHoi/GetListQuestion";
                HttpClient client = new HttpClient();
            
                var res = client.GetFromJsonAsync<List<CQuestion>>(strUrl);
                res.Wait();
                return res.Result;
            }
            catch
            {
                return null;
            }
        }

        public static bool themLessonVaoKhoaHoc(int courseId, int lessonId, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyBaiHoc/AddLessonToCourse";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var requestData = new CCourseLessons
                {
                    CourseId = courseId,
                    LessonId = lessonId,
                   
                };

                // Serialize manually to inspect JSON
                string jsonPayload = JsonConvert.SerializeObject(requestData);
                Console.WriteLine($"Request Payload: {jsonPayload}");

                var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
                var res = client.PostAsync(strUrl, content);
                res.Wait();

                if (!res.Result.IsSuccessStatusCode)
                {
                    var errorContent = res.Result.Content.ReadAsStringAsync().Result;
                    Console.WriteLine($"Error Response: {errorContent}");
                }

                return res.Result.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in themLessonVaoKhoaHoc: {ex.Message}");
                return false;
            }
        }
        public static bool xoaLessonRaKhoiKhoaHoc(string courseId, string lessonId, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyBaiHoc/RemoveLessonFromCourse/" + courseId + "/" + lessonId;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.DeleteAsync(strUrl);
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
        public static bool themQuestionVaoBaiHoc(int lessonId, int questionId, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyCauHoi/AddQuestionToLesson";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var requestData = new CLessonQuestion
                {
                    LessonId = lessonId,
                 
                    QuestionId = questionId,

                };

                // Serialize manually to inspect JSON
                string jsonPayload = JsonConvert.SerializeObject(requestData);
                Console.WriteLine($"Request Payload: {jsonPayload}");

                var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
                var res = client.PostAsync(strUrl, content);
                res.Wait();

                if (!res.Result.IsSuccessStatusCode)
                {
                    var errorContent = res.Result.Content.ReadAsStringAsync().Result;
                    Console.WriteLine($"Error Response: {errorContent}");
                }

                return res.Result.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in themQuestionVaoBaiHoc: {ex.Message}");
                return false;
            }

        }
        public static bool xoaQuestionRaKhoiBaiHoc(string lessonId, string questionId, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyCauHoi/RemoveQuestionFromLesson?lessonId=" + lessonId + "&questionId=" + questionId;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.DeleteAsync(strUrl);
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
        public static List<CCourseLessons> GetCourseLessonByID(string id, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyBaiHoc/GetCourseLessonByID/" + id;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = client.GetFromJsonAsync<List<CCourseLessons>>(strUrl);
                res.Wait();
                Console.WriteLine($"GetCourseLessonByID Result: {JsonConvert.SerializeObject(res.Result)}");
                return res.Result ?? new List<CCourseLessons>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCourseLessonByID: {ex.Message}");
                return new List<CCourseLessons>();
            }
        }
        public static bool SwapLessonOrder(CSwapLessonOrder dto, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyBaiHoc/SwapLessonOrder";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.PutAsJsonAsync(strUrl, dto);
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SwapLessonOrder: {ex.Message}");
                return false;
            }
        }
        public static bool SwapQuestionOrder(CSwapQuestionOrder dto, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyCauHoi/SwapQuestionOrder";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.PutAsJsonAsync(strUrl, dto);
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SwapQuestionOrder: {ex.Message}");
                return false;
            }
        }
        

        public static List<CQuestion> GetQuestionsByLessonId(string id)
        {
            try
            {
                string strUrl = $"{api}QuanLyCauHoi/GetQuestionsByLessonId/" + id;
                HttpClient client = new HttpClient();

                var res = client.GetFromJsonAsync<List<CQuestion>>(strUrl);
                res.Wait();
                return res.Result;
            }
            catch
            {
                return null;
            }
        }
        public static List<CQuestionType> getDSQuestionType()
        {
            try
            {
                string strUrl = $"{api}QuanLyCauHoi/GetListQuestionType";
                HttpClient client = new HttpClient();

                var res = client.GetFromJsonAsync<List<CQuestionType>>(strUrl);
                res.Wait();
                return res.Result;
            }
            catch
            {
                return null;
            }
        }
        public static List<CUserPackage> getDSUserPackage( )
        {
            try
            {
                string strUrl = $"{api}QuanLyDangKyGoiCuoc/GetUserPackageRegistrations";
                HttpClient client = new HttpClient();
                var res = client.GetFromJsonAsync<List<CUserPackage>>(strUrl);
                res.Wait();
                return res.Result;
            }
            catch
            {
                return null;
            }
        }
        public static List<CPackage> getDSPackage()
        {
            try
            {
                string strUrl = $"{api}QuanLyGoiCuoc/GetListPackages";
                HttpClient client = new HttpClient();
                var res = client.GetFromJsonAsync<List<CPackage>>(strUrl);
                res.Wait();
                return res.Result;
            }
            catch
            {
                return null;
            }
        }

        public static bool xoaCourse(string id, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyKhoaHoc/DeleteCourse/" + id;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
              new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.DeleteAsync(strUrl);
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
        public static bool xoaRole(string id, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyKhachHang/XoaRole/" + id;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
               new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.DeleteAsync(strUrl);
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
        public static bool xoaUserPackage(int userId, int packageId, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyDangKyGoiCuoc/Delete/{userId}/{packageId}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var res = client.DeleteAsync(strUrl);
                res.Wait();
                return res.Result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public static bool xoaUserByAdmin(string id, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyKhachHang/XoaTaiKhoan/" + id;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
              new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.DeleteAsync(strUrl);
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
        public static bool xoaCategory(string id, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyKhoaHoc/DeleteCategories/" + id;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
               new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.DeleteAsync(strUrl);
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
        public static bool xoaLevel(string id, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyKhoaHoc/DeleteLevels/" + id;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
               new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.DeleteAsync(strUrl);
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
        public static bool xoaQuestionType(string id, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyCauHoi/DeleteQuestionType/" + id;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
               new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.DeleteAsync(strUrl);
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
        public static bool xoaQuestion(string id, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyCauHoi/DeleteQuestion/" + id;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
               new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.DeleteAsync(strUrl);
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
        public static bool xoaLesson(string id, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyBaiHoc/DeleteLesson/" + id;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
               new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.DeleteAsync(strUrl);
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
        public static bool xoaPackage(string id, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyGoiCuoc/DeletePackage/" + id;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
               new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.DeleteAsync(strUrl);
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
        public static bool themCourse(CCourse x, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyKhoaHoc/InsertCourse";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
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
        public static bool themUser(CUsers x, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyKhachHang/ThemUser";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
               new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
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
        public static bool themCategory(CCategory x, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyKhoaHoc/InsertCategories";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
               new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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
        public static bool themLevel(CLevel x, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyKhoaHoc/InsertLevels";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
               new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
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
        public static bool themQuestionType(CQuestionType x, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyCauHoi/InsertQuestionType";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
               new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
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
        public static bool themQuestion(CQuestion x, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyCauHoi/InsertQuestion";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
               new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
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
        public static bool themLesson(CLesson x, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyBaiHoc/InsertLesson";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
               new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
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
        public static bool themPackage(CPackage x, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyGoiCuoc/InsertPackage";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
               new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
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
        public static bool themUserPackage(CUserPackage x, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyDangKyGoiCuoc/Create";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
               new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
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

        public static CCategory getCategoryById(string id)
        {
            try
            {
                string strUrl = $"{api}QuanLyKhoaHoc/GetCategoryID?id=" + id;
                HttpClient client = new HttpClient();

                var res = client.GetFromJsonAsync<CCategory>(strUrl);
                res.Wait();
                if (res.Result != null)
                {
                    return res.Result;
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
        public static CUsers LayThongTinUser(string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyKhachHang/LayThongTinUser";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
               new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.GetFromJsonAsync<CUsers>(strUrl);
                res.Wait();
                if (res.Result != null)
                {
                    return res.Result;
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
        public static CUsers getUserById(string id, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyKhachHang/LayThongTinUserBangID/" + id;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
               new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.GetFromJsonAsync<CUsers>(strUrl);
                res.Wait();
                if (res.Result != null)
                {
                    return res.Result;
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
        public static CCourse getCourseById(string id)
        {
            try
            {
                string strUrl = $"{api}QuanLyKhoaHoc/GetCourseID?id=" + id;
                HttpClient client = new HttpClient();

                var res = client.GetFromJsonAsync<CCourse>(strUrl);
                res.Wait();
                if (res.Result != null)
                {
                    return res.Result;
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
        public static CLesson getLessonById(string id)
        {
            try
            {
                string strUrl = $"{api}QuanLyBaiHoc/GetLessonById/" + id;
                HttpClient client = new HttpClient();

                var res = client.GetFromJsonAsync<CLesson>(strUrl);
                res.Wait();
                if (res.Result != null)
                {
                    return res.Result;
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

        public static CLevel getLevelById(string id)
        {
            try
            {
                string strUrl = $"{api}QuanLyKhoaHoc/GetLevelID/" + id;
                HttpClient client = new HttpClient();
                var res = client.GetFromJsonAsync<CLevel>(strUrl);
                res.Wait();
                if (res.Result != null)
                {
                    return res.Result;
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
        public static CQuestionType getQuestionTypeById(string id)
        {
            try
            {
                string strUrl = $"{api}QuanLyCauHoi/GetQuestionTypeById/" + id;
                HttpClient client = new HttpClient();

                var res = client.GetFromJsonAsync<CQuestionType>(strUrl);
                res.Wait();
                if (res.Result != null)
                {
                    return res.Result;
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
        public static CQuestion getQuestionById(string id)
        {
            try
            {
                string strUrl = $"{api}QuanLyCauHoi/GetQuestionById/" + id;
                HttpClient client = new HttpClient();

                var res = client.GetFromJsonAsync<CQuestion>(strUrl);
                res.Wait();
                if (res.Result != null)
                {
                    return res.Result;
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
        public static CUserPackage GetPackageByUserIdAndByPackageId(string userId, string packageId)
        {
            try
            {
                string strUrl = $"{api}QuanLyDangKyGoiCuoc/GetPackageByUserIdAndByPackageId/" + userId + "/" + packageId;
                HttpClient client = new HttpClient();
                var res = client.GetFromJsonAsync<CUserPackage>(strUrl);
                res.Wait();
                if (res.Result != null)
                {
                    return res.Result;
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
        public static List<CUserPackage> getPackagesByUserId(string id)
        {
            try
            {
                string strUrl = $"{api}QuanLyDangKyGoiCuoc/GetPackageByUser/" + id;
                HttpClient client = new HttpClient();
                var res = client.GetFromJsonAsync<List<CUserPackage>>(strUrl);
                res.Wait();
                if (res.Result != null)
                {
                    return res.Result;
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
        public static CPackage getPackageById(string id)
        {
            try
            {
                string strUrl = $"{api}QuanLyGoiCuoc/GetPackageById/" + id;
                HttpClient client = new HttpClient();

                var res = client.GetFromJsonAsync<CPackage>(strUrl);
                res.Wait();
                if (res.Result != null)
                {
                    return res.Result;
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
        public static bool editCourse(string id, CCourse x, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyKhoaHoc/UpdateCourse/" + id;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                Console.WriteLine($"Sending to {strUrl}: {JsonConvert.SerializeObject(x)}");

                var res = client.PutAsJsonAsync(strUrl, x);
                res.Wait();

                if (!res.Result.IsSuccessStatusCode)
                {
                    var errorContent = res.Result.Content.ReadAsStringAsync().Result;
                    Console.WriteLine($"API Error: {res.Result.StatusCode} - {errorContent}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex}");
                return false;
            }
        }
        public static bool editRole(string id, CRole x, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyKhachHang/SuaRole/" + id;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.PutAsJsonAsync(strUrl, x);
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
        public static bool editUserById(string id, CUsers x, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyKhachHang/CapNhatThongTinUserTheoID/" + id;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                // Log the request payload for debugging
                Console.WriteLine($"Sending user update for ID {id}: {JsonConvert.SerializeObject(x)}");

                var res = client.PutAsJsonAsync(strUrl, x);
                res.Wait();

                if (!res.Result.IsSuccessStatusCode)
                {
                    // Read the error response for more details
                    var errorContent = res.Result.Content.ReadAsStringAsync().Result;
                    Console.WriteLine($"API Error: {res.Result.StatusCode} - {errorContent}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in editUserById: {ex}");
                return false;
            }
        }

        public static bool editLesson(string id, CLesson x, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyBaiHoc/UpdateLesson/" + id;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.PutAsJsonAsync(strUrl, x);
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
        public static bool editCategory(string id, CCategory x, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyKhoaHoc/UpdateCategories/" + id;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.PutAsJsonAsync(strUrl, x);
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
        public static bool editLevel(string id, CLevel x, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyKhoaHoc/UpdateLevels/" + id;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.PutAsJsonAsync(strUrl, x);
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
        public static bool editQuestionType(string id, CQuestionType x, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyCauHoi/UpdateQuestionType/" + id;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.PutAsJsonAsync(strUrl, x);
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
        public static bool editQuestion(string id, CQuestion x, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyCauHoi/UpdateQuestion/" + id;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.PutAsJsonAsync(strUrl, x);
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
        public static bool updateUserPackage(int userId, int packageId, CUserPackage x, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyDangKyGoiCuoc/Update/{userId}/{packageId}";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var res = client.PutAsJsonAsync(strUrl, x);
                res.Wait();
                return res.Result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        public static bool updatePackage(string id, CPackage x, string token)
        {
            try
            {
                string strUrl = $"{api}QuanLyGoiCuoc/UpdatePackage/" + id;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var res = client.PutAsJsonAsync(strUrl, x);
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
