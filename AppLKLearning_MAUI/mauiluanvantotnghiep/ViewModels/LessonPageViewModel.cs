using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mauiluanvantotnghiep.Models;
using mauiluanvantotnghiep.Views.ReportPage;
using mauiluanvantotnghiep.ViewsPopup;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using AppConfig = mauiluanvantotnghiep.ViewModels.AppConfig.AppConfig;

namespace mauiluanvantotnghiep.ViewModels
{
    public partial class LessonPageViewModel : ObservableObject
    {
        private readonly HttpClient _httpClient;
        public ObservableCollection<Lesson> Lessons { get; } = new();
        public ObservableCollection<ExamHistory> ExamHistories { get; } = new();
        
        [ObservableProperty]
        private bool isExamHistoryExpanded = false;
        
        [ObservableProperty]
        private bool hasExamHistory = false;
        
        [ObservableProperty]
        private bool hasPassed = false;
        
        [ObservableProperty]
        private CertificateResponse? userCertificate = null;
        
        private int _userId;
        private int _courseId;

        public LessonPageViewModel()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
            };
            _httpClient = new HttpClient(handler);
        }

        public async Task LoadLessonsByCourseAsync(int courseId)
        {
            Lessons.Clear();

            // Get token + set header
            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
                return;
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Parse userId từ JWT
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            foreach (var c in jwt.Claims)
                System.Diagnostics.Debug.WriteLine($"[JWT] {c.Type} = {c.Value}");

            var rawId = jwt.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value
                     ?? jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(rawId, out var userId))
            {
                System.Diagnostics.Debug.WriteLine("[JWT] parse userId failed");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[JWT] parsed userId = {userId}");

            // Gán
            _userId = userId;
            _courseId = courseId;

            // Lấy danh sách CourseLesson
            var url1 = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyBaiHoc/GetCourseLessonByID/{courseId}";
            var res1 = await _httpClient.GetAsync(url1);
            if (!res1.IsSuccessStatusCode)
                return;

            var json1 = await res1.Content.ReadAsStringAsync();
            var courseLessons = JsonSerializer.Deserialize<List<CourseLesson>>(json1,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (courseLessons == null || !courseLessons.Any())
                return;

            // Palette màu
            string[] paletteHex = { "#F291A3", "#F2B6E2", "#AD82D9", "#ADA2F2", "#B7AEF2" };
            Color[] palette = paletteHex.Select(Color.Parse).ToArray();
            int colorIndex = 0;

            // Duyệt từng CourseLesson
            foreach (var cl in courseLessons.OrderBy(c => c.OrderNo))
            {
                var url2 = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyBaiHoc/GetLessonById/{cl.LessonId}";
                var res2 = await _httpClient.GetAsync(url2);
                if (!res2.IsSuccessStatusCode) continue;

                var json2 = await res2.Content.ReadAsStringAsync();
                var lessonDetail = JsonSerializer.Deserialize<Lesson>(json2,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (lessonDetail == null) continue;

                lessonDetail.OrderNo = cl.OrderNo;
                lessonDetail.RowColor = palette[colorIndex++ % palette.Length];

                try
                {
                    var url3 = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyKetQuaHoc/GetAcademicResults" +
                               $"?userId={userId}&courseId={courseId}&lessonId={lessonDetail.LessonId}";
                    System.Diagnostics.Debug.WriteLine($"[FetchStatus] Calling: {url3}");

                    var res3 = await _httpClient.GetAsync(url3);
                    System.Diagnostics.Debug.WriteLine($"[FetchStatus] StatusCode = {res3.StatusCode}");

                    var raw3 = await res3.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[FetchStatus] Raw response = {raw3}");

                    if (res3.StatusCode == HttpStatusCode.NotFound
                        || raw3.Contains("No academic results", StringComparison.OrdinalIgnoreCase)
                        || raw3.Contains("Không tìm thấy", StringComparison.OrdinalIgnoreCase))
                    {
                        lessonDetail.StatusText = "Start";
                    }
                    else
                    {
                        var arr = JsonSerializer.Deserialize<List<AcademicResult>>(raw3,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        lessonDetail.StatusText = arr?.FirstOrDefault()?.Status ?? "Start";
                    }

                    System.Diagnostics.Debug.WriteLine(
                        $"[FetchStatus] LessonId={lessonDetail.LessonId}, StatusText={lessonDetail.StatusText}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[FetchStatus] Exception: {ex.Message}");
                    lessonDetail.StatusText = "Start";
                }

                Lessons.Add(lessonDetail);
            }

            OnPropertyChanged(nameof(Lessons));
            
            // Load exam history after loading lessons
            await LoadExamHistoryAsync();
            
            // Check if user has passed and load certificate
            await CheckCertificateStatusAsync();
        }

        public async Task LoadExamHistoryAsync()
        {
            try
            {
                ExamHistories.Clear();
                
                var url = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyExam/GetExamHistoryForUser?courseId={_courseId}";
                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var histories = JsonSerializer.Deserialize<List<ExamHistory>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    if (histories != null && histories.Any())
                    {
                        // Sắp xếp theo ngày thi mới nhất và chỉ lấy 5 lần gần nhất
                        var sortedHistories = histories.OrderByDescending(h => h.TakenAt).Take(5).ToList();
                        foreach (var history in sortedHistories)
                        {
                            ExamHistories.Add(history);
                        }
                        HasExamHistory = true;
                    }
                    else
                    {
                        HasExamHistory = false;
                    }
                }
                else
                {
                    HasExamHistory = false;
                }
                
                OnPropertyChanged(nameof(ExamHistories));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoadExamHistory] Exception: {ex.Message}");
                HasExamHistory = false;
            }
        }

        public async Task CheckCertificateStatusAsync()
        {
            try
            {
                var url = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyCertificate/GetListCertificatesCourseIdForUser?courseId={_courseId}";
                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var certificates = JsonSerializer.Deserialize<List<CertificateResponse>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    if (certificates != null && certificates.Any())
                    {
                        // Lấy chứng chỉ mới nhất
                        UserCertificate = certificates.OrderByDescending(c => c.CreatedAt).FirstOrDefault();
                        HasPassed = true;
                    }
                    else
                    {
                        HasPassed = false;
                        UserCertificate = null;
                    }
                }
                else
                {
                    HasPassed = false;
                    UserCertificate = null;
                }
                
                System.Diagnostics.Debug.WriteLine($"[Certificate] HasPassed: {HasPassed}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CheckCertificate] Exception: {ex.Message}");
                HasPassed = false;
                UserCertificate = null;
            }
        }

        // Computed properties for certificate status
        public string CertificateStatusText
        {
            get
            {
                if (UserCertificate == null) return "";
                
                if (UserCertificate.ExpirationDate == null)
                    return "Chứng chỉ vĩnh viễn";
                
                if (UserCertificate.ExpirationDate > DateTime.Now)
                    return $"Còn hạn đến {UserCertificate.ExpirationDate:dd/MM/yyyy}";
                
                return "Chứng chỉ đã hết hạn";
            }
        }

        public bool IsCertificateValid
        {
            get
            {
                if (UserCertificate == null) return false;
                return UserCertificate.ExpirationDate == null || UserCertificate.ExpirationDate > DateTime.Now;
            }
        }

        [RelayCommand]
        public void ToggleExamHistory()
        {
            IsExamHistoryExpanded = !IsExamHistoryExpanded;
        }

        [RelayCommand]
        public async Task ViewCertificateAsync()
        {
            if (UserCertificate == null) return;
            
            // Hiển thị popup chứng chỉ
            var certificatePopup = new CertificateViewPopup(UserCertificate);
            await Application.Current.MainPage.ShowPopupAsync(certificatePopup);
        }

        [RelayCommand]
        public async Task SelectLesson(int lessonId)
        {
            if (lessonId <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Invalid lesson ID", "OK");
                return;
            }

            // Kiểm tra lesson tồn tại
            var lesson = Lessons.FirstOrDefault(l => l.LessonId == lessonId);
            if (lesson == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Lesson not found", "OK");
                return;
            }

            // Tạo body cho API CreateAcademicResult
            var academicResult = new
            {
                academicResultId = 0,
                userId = _userId,
                FullName = "string",
                courseId = _courseId,
                courseName = "string",
                lessonId = lessonId,
                lessonTitle = lesson.LessonTitle ?? "string",
                status = "InProgress",
                timeSpent = 0,
                createdAt = DateTime.UtcNow,    
                updatedAt = DateTime.UtcNow
            };

            // Gửi POST request đến API
            var url = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyKetQuaHoc/CreateAcademicResult";
            var json = JsonSerializer.Serialize(academicResult);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            System.Diagnostics.Debug.WriteLine($"[CreateAcademicResult] Request URL: {url}, Body: {json}");

            try
            {
                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[CreateAcademicResult] StatusCode: {response.StatusCode}, Response: {responseContent}");

                if (response.StatusCode == HttpStatusCode.Conflict) // 409 Conflict
                {
                   
                }
                else if (!response.IsSuccessStatusCode)
                {
                    // Hiển thị lỗi chung cho các mã trạng thái khác
                    await Application.Current.MainPage.DisplayAlert("Error",
                        $"Failed to start lesson: {responseContent}", "OK");
                }

                //điều huướng sang QuestionDeatailPage
                await Shell.Current.GoToAsync($"lessondetailpage?lessonId={lessonId}&courseId={_courseId}");

                //// Luôn điều hướng sang questionpage
                //await Shell.Current.GoToAsync($"questionpage?lessonId={lessonId}&courseId={_courseId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CreateAcademicResult] Exception: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to call API: {ex.Message}", "OK");

                
            }
        }

        [RelayCommand]
        public async Task NavigateToReportAsync(Lesson lesson)
        {
            if (lesson == null)
                return;

            // Điều hướng và truyền courseId, lessonId
            await Shell.Current.GoToAsync($"reportpage?lessonId={lesson.LessonId}&courseId={_courseId}");
        }

        //hàm kiểm tra xem có đủ điều kiện 
        public async Task CheckCanTakeExamAndNavigateAsync()
        {
            // 1. Get token
            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Bạn cần đăng nhập để tiếp tục.", "OK");
                return;
            }

            // 2. Prepare HttpClient
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 3. Call API
            var url = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyExam/CanTakeExamSetForUser?courseId={_courseId}";
            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            // 4. Parse response
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                bool canTakeExam = root.GetProperty("canTakeExam").GetBoolean();
                string message = root.GetProperty("message").GetString();

                if (canTakeExam)
                {
                    Application.Current.MainPage.ShowPopup(new AlertLessonExam(_courseId));
                }
                else
                {
                    Application.Current.MainPage.ShowPopup(new AlertLessonExamFailed());
                }
            }
            catch
            {
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Không kiểm tra được điều kiện làm bài test.", "OK");
            }
        }
        
        [RelayCommand]
        public async Task GoToFinalTestAsync()
        {
            await CheckCanTakeExamAndNavigateAsync();
        }
    }
}