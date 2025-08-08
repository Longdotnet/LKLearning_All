using CommunityToolkit.Mvvm.ComponentModel;
using mauiluanvantotnghiep.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using CommunityToolkit.Mvvm.Input;
using System.Text;

namespace mauiluanvantotnghiep.ViewModels.ExamViewModel
{
    public partial class ExamPageViewModel : ObservableObject
    {
        [ObservableProperty] private string courseName;
        [ObservableProperty] private string name;
        [ObservableProperty] private string description;
        [ObservableProperty] private int? timeLimitSec;
        [ObservableProperty] private int examSetId;
        [ObservableProperty] private decimal? passingScore;

        public int CourseId { get; set; }

        public string TimeLimitDisplay =>
            TimeLimitSec.HasValue && TimeLimitSec.Value > 0
                ? $"Thời gian làm bài: {TimeLimitSec.Value / 60} phút"
                : "Không giới hạn";

        public string PassingScoreDisplay =>
            PassingScore.HasValue
                ? $"Điểm đậu: {PassingScore.Value:0.#} điểm"
                : "Không xác định điểm đậu";

        public async Task LoadExamAsync(int courseId)
        {
            CourseId = courseId;
            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token)) return;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var url = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyExam/GetRandomExamSetByCourse?courseId={courseId}";
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return;

            var json = await response.Content.ReadAsStringAsync();
            var examSet = JsonSerializer.Deserialize<ExamSet>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (examSet != null)
            {
                CourseName = examSet.CourseName;
                Name = examSet.Name;
                Description = examSet.Description;
                TimeLimitSec = examSet.TimeLimitSec;
                ExamSetId = examSet.ExamSetId;
                PassingScore = examSet.PassingScore;
                OnPropertyChanged(nameof(TimeLimitDisplay));
                OnPropertyChanged(nameof(PassingScoreDisplay));
                System.Diagnostics.Debug.WriteLine($"[DEBUG] TimeLimitSec: {TimeLimitSec}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] PassingScore: {PassingScore}");
            }
        }

        [RelayCommand]
        private async Task StartExamAsync()
        {
            try
            {
                // Gọi API để bắt đầu bài kiểm tra
                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await Application.Current.MainPage.DisplayAlert("Lỗi", "Không tìm thấy token xác thực", "OK");
                    return;
                }

                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
                };
                var client = new HttpClient(handler);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Tạo payload cho API
                var requestData = new
                {
                    historyId = 0,
                    userId = 0,
                    fullName = "string",
                    examSetId = ExamSetId,
                    takenAt = DateTime.Now,
                    totalScore = 0,
                    isPassed = true,
                    durationSec = 0
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyExam/StartExamForUser";
                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] StartExamForUser Response: {responseJson}");
                    
                    // Parse response to get historyId
                    var examHistory = JsonSerializer.Deserialize<ExamHistory>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    if (examHistory != null)
                    {
                        // Điều hướng sang trang kiểm tra, truyền examSetId, timeLimitSec, historyId, passingScore và courseId
                        await Shell.Current.GoToAsync($"examquestionpage?examSetId={ExamSetId}&timeLimitSec={TimeLimitSec}&historyId={examHistory.HistoryId}&passingScore={PassingScore}&courseId={CourseId}");
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Lỗi", "Không thể bắt đầu bài kiểm tra", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] StartExamAsync: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Đã xảy ra lỗi khi bắt đầu bài kiểm tra", "OK");
            }
        }

        partial void OnTimeLimitSecChanged(int? oldValue, int? newValue)
        {
            OnPropertyChanged(nameof(TimeLimitDisplay));
        }

        partial void OnPassingScoreChanged(decimal? oldValue, decimal? newValue)
        {
            OnPropertyChanged(nameof(PassingScoreDisplay));
        }
    }

}
