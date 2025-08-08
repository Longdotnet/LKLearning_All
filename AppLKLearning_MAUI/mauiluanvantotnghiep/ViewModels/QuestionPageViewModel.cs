// ViewModels/QuestionPageViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mauiluanvantotnghiep.Models;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.ViewModels
{
    public partial class QuestionPageViewModel : ObservableObject
    {
        private readonly HttpClient _http;

        [ObservableProperty]
        ObservableCollection<QuestionItemViewModel> questions;

        [ObservableProperty]
        int currentIndex;

        [ObservableProperty]
        int lessonId;

        [ObservableProperty]
        int courseId;

        [ObservableProperty]
        string lessonStatus; // Thêm thuộc tính để lưu trạng thái bài học

        public QuestionPageViewModel()
        {
            _http = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
            });
            Questions = new ObservableCollection<QuestionItemViewModel>();
        }

        public QuestionItemViewModel CurrentQuestion =>
            (CurrentIndex >= 0 && CurrentIndex < Questions.Count)
                ? Questions[CurrentIndex]
                : null;

        public ObservableCollection<QuestionItemViewModel> CurrentQuestionCollection =>
            CurrentQuestion != null
                ? new ObservableCollection<QuestionItemViewModel> { CurrentQuestion }
                : new ObservableCollection<QuestionItemViewModel>();

        public bool CanGoPrev => CurrentIndex > 0;
        public bool CanGoNext => CurrentIndex < Questions.Count - 1;

        public bool IsLastQuestion => CurrentIndex == Questions.Count - 1 && (LessonStatus == "InProgress" || LessonStatus == "Failed"); // Cập nhật logic

        public bool IsCompleted => LessonStatus == "Completed"; // Thêm thuộc tính để kiểm tra trạng thái Completed
        [RelayCommand]
        public async Task LoadQuestionsAsync(int lessonId)
        {
            Debug.WriteLine($"[QPV] ⇒ LoadQuestionsAsync start (lessonId={lessonId})");
            Questions.Clear();

            // 1. Lấy token
            var token = await SecureStorage.GetAsync("auth_token");
            Debug.WriteLine($"[QPV] Token {(string.IsNullOrWhiteSpace(token) ? "NULL/empty" : "OK")}");
            if (string.IsNullOrWhiteSpace(token))
                return;

            // 2. Thêm header Bearer cho tất cả các request
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Gọi API lấy danh sách câu hỏi
            var urlList = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyCauHoi/GetLessonQuestionByID/{lessonId}";
            Debug.WriteLine($"[QPV] GET LessonQuestionByID URL: {urlList}");
            string listJson;
            try
            {
                listJson = await _http.GetStringAsync(urlList);
                Debug.WriteLine($"[QPV] Response List JSON: {listJson}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[QPV] ERROR fetching question list: {ex.Message}");
                return;
            }

            var overviews = JsonSerializer.Deserialize<List<LessonQuestion>>(listJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Debug.WriteLine($"[QPV] Parsed {overviews?.Count ?? 0} overview items");

            if (overviews == null || !overviews.Any())
                return;

            // Lặp qua từng câu hỏi để lấy chi tiết
            foreach (var ov in overviews.OrderBy(x => x.OrderNo))
            {
                var urlDetail = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyCauHoi/GetQuestionById/{ov.QuestionId}";
                Debug.WriteLine($"[QPV] GET QuestionById URL: {urlDetail}");
                string detailJson;
                try
                {
                    detailJson = await _http.GetStringAsync(urlDetail);
                    Debug.WriteLine($"[QPV] Response Detail JSON: {detailJson}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[QPV] ERROR fetching question detail: {ex.Message}");
                    continue;
                }

                var q = JsonSerializer.Deserialize<Question>(detailJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (q == null) continue;

                if (!string.IsNullOrWhiteSpace(q.RawAnswerOptions))
                {
                    try
                    {
                        q.ParsedOptions = JsonSerializer.Deserialize<string[]>(q.RawAnswerOptions);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[QPV] ERROR parsing RawAnswerOptions: {ex.Message}");
                        q.ParsedOptions = Array.Empty<string>();
                    }
                }
                else
                {
                    q.ParsedOptions = Array.Empty<string>();
                }

                var questionViewModel = new QuestionItemViewModel(q);
                
                // THÊM: Load question level
                await questionViewModel.LoadQuestionLevelAsync(_http);
                
                Questions.Add(questionViewModel);
            }

            // 3. Gọi API GetAcademicResultsForUser với token đã được thêm vào header
            var statusUrl = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyKetQuaHoc/GetAcademicResultsForUser?courseId={CourseId}&lessonId={LessonId}";
            Debug.WriteLine($"[QPV] GET AcademicResultsForUser URL: {statusUrl}");
            string statusJson;
            try
            {
                statusJson = await _http.GetStringAsync(statusUrl);
                Debug.WriteLine($"[QPV] Response Status JSON: {statusJson}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[QPV] ERROR fetching academic results: {ex.Message}");
                LessonStatus = "InProgress"; // Mặc định nếu lỗi
                return;
            }

            var results = JsonSerializer.Deserialize<List<AcademicResult>>(statusJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (results != null && results.Any())
            {
                LessonStatus = results.First().Status;
            }
            else
            {
                LessonStatus = "InProgress"; // Mặc định nếu không có dữ liệu
            }
            Debug.WriteLine($"[QPV] LessonStatus set to {LessonStatus}");

            CurrentIndex = Questions.Count > 0 ? 0 : -1;
            Debug.WriteLine($"[QPV] Load complete, total questions={Questions.Count}, CurrentIndex={CurrentIndex}");

            OnPropertyChanged(nameof(CurrentQuestion));
            OnPropertyChanged(nameof(CurrentQuestionCollection));
            OnPropertyChanged(nameof(CanGoPrev));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(IsLastQuestion));
            OnPropertyChanged(nameof(IsCompleted));
        }
        [RelayCommand(CanExecute = nameof(CanGoPrev))]
        void Prev()
        {
            CurrentIndex--;
            OnPropertyChanged(nameof(CurrentQuestion));
            OnPropertyChanged(nameof(CurrentQuestionCollection));
            OnPropertyChanged(nameof(CanGoPrev));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(IsLastQuestion));
            OnPropertyChanged(nameof(IsCompleted));
        }

        [RelayCommand(CanExecute = nameof(CanGoNext))]
        void Next()
        {
            CurrentIndex++;
            OnPropertyChanged(nameof(CurrentQuestion));
            OnPropertyChanged(nameof(CurrentQuestionCollection));
            OnPropertyChanged(nameof(CanGoPrev));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(IsLastQuestion));
            OnPropertyChanged(nameof(IsCompleted));
        }

        [RelayCommand(CanExecute = nameof(IsLastQuestion))]
        public async Task CompleteLessonAsync()
        {
            int totalQuestions = Questions.Count;
            int correctCount = Questions.Count(q => q.IsCorrect);
            double ratio = totalQuestions > 0
                ? (double)correctCount / totalQuestions
                : 0;

            bool passed = ratio >= 0.8;
            string status = passed ? "Completed" : "Failed";
            string title = passed ? "Hoàn thành" : "Chưa hoàn thành";
            string message = passed
                ? "Bạn đã hoàn thành bài học!"
                : "Bạn chưa hoàn thành khóa học. Vui lòng trả lời đúng trên 80%.";

            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrWhiteSpace(token))
                return;

            var url = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyKetQuaHoc/UpdateAcademicResultForUser" +
                      $"?lessonId={LessonId}&courseId={CourseId}&status={status}";
            Debug.WriteLine($"[QPV] Complete URL: {url}");

            using var request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage resp;
            try
            {
                resp = await _http.SendAsync(request);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[QPV] ERROR sending PUT: {ex}");
                await Shell.Current.DisplayAlert("Lỗi", "Không thể kết nối máy chủ.", "OK");
                return;
            }

            if (resp.IsSuccessStatusCode)
            {
                LessonStatus = status; // Cập nhật trạng thái sau khi hoàn thành
                OnPropertyChanged(nameof(IsLastQuestion));
                OnPropertyChanged(nameof(IsCompleted));
                await Shell.Current.DisplayAlert(title, message, "OK");
                await Shell.Current.Navigation.PopAsync();
            }
            else
            {
                var body = await resp.Content.ReadAsStringAsync();
                await Shell.Current.DisplayAlert(
                    "Lỗi",
                    $"Không thể cập nhật ({(int)resp.StatusCode})\n{body}",
                    "OK");
            }
        }

        [RelayCommand]
        public async Task BackToLessonsAsync()
        {
            await Shell.Current.Navigation.PopAsync();
        }
    }
}
