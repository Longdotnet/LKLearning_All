using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mauiluanvantotnghiep.Models;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Timers;
using System.Text;
using mauiluanvantotnghiep.ViewsPopup;
using CommunityToolkit.Maui.Views;

namespace mauiluanvantotnghiep.ViewModels
{
    public partial class ExamQuestionPageViewModel : ObservableObject
    {
        private readonly HttpClient _http;

        [ObservableProperty]
        ObservableCollection<QuestionItemViewModel> questions = new();

        [ObservableProperty]
        int currentIndex;

        [ObservableProperty]
        int examSetId;

        [ObservableProperty]
        int historyId;

        [ObservableProperty]
        decimal passingScore;

        [ObservableProperty]
        int courseId;

        private System.Timers.Timer _timer;
        private int _remainingSeconds;
        private int _totalSeconds;
        private DateTime _examStartTime;

        [ObservableProperty]
        string timerDisplay = "00:00";

        [ObservableProperty]
        double progressValue = 0.0;

        [ObservableProperty]
        bool isSubmitting = false;

        // ✅ THÊM: Properties mới cho giao diện cải thiện
        [ObservableProperty]
        string examTitle = "Bài Kiểm Tra";

        [ObservableProperty]
        int totalQuestions;

        [ObservableProperty]
        int answeredQuestionsCount;

        public ExamQuestionPageViewModel()
        {
            _http = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
            });
            _examStartTime = DateTime.Now;
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

        // ✅ THÊM: Property mới cho hiển thị tiến trình câu hỏi
        public string QuestionProgressText => $"Câu {CurrentIndex + 1} / {Questions.Count}";

        // Hàm nhận thời gian và khởi tạo timer
        public void StartCountdown(int totalSeconds)
        {
            _totalSeconds = totalSeconds;
            _remainingSeconds = totalSeconds;
            _examStartTime = DateTime.Now;
            UpdateTimerDisplay();
            UpdateProgressValue();

            Debug.WriteLine($"[DEBUG] Starting countdown: {totalSeconds} seconds");

            if (totalSeconds > 0) // Chỉ khởi tạo timer nếu có giới hạn thời gian
            {
                _timer = new System.Timers.Timer(1000);
                _timer.Elapsed += async (s, e) =>
                {
                    if (_remainingSeconds > 0)
                    {
                        _remainingSeconds--;
                        UpdateTimerDisplay();
                        UpdateProgressValue();
                    }
                    else
                    {
                        _timer.Stop();
                        // Tự động nộp bài khi hết giờ
                        await SubmitExamInternalAsync(true);
                    }
                };
                _timer.Start();
            }
        }

        private void UpdateTimerDisplay()
        {
            var display = _totalSeconds > 0 
                ? $"{_remainingSeconds / 60:D2}:{_remainingSeconds % 60:D2}"
                : "Không giới hạn";
            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
            {
                TimerDisplay = display;
            });
        }

        private void UpdateProgressValue()
        {
            var value = _totalSeconds > 0 ? (double)(_totalSeconds - _remainingSeconds) / _totalSeconds : 0.0;
            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
            {
                ProgressValue = value;
            });
        }

        // ✅ THÊM: Method để cập nhật số lượng câu hỏi đã trả lời
        private void UpdateAnsweredQuestionsCount()
        {
            AnsweredQuestionsCount = GetAnsweredQuestionsCount();
            OnPropertyChanged(nameof(QuestionProgressText));
        }

        [RelayCommand]
        public async Task LoadQuestionsAsync(int examSetId)
        {
            try
            {
                Debug.WriteLine($"[DEBUG] Starting LoadQuestionsAsync for examSetId: {examSetId}");
                
                Questions.Clear();
                ExamSetId = examSetId;

                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrWhiteSpace(token))
                {
                    Debug.WriteLine("[DEBUG] No auth token found");
                    return;
                }

                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                // FIX: Correct the URL
                var url = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyExam/GetQuestionsByExamSet/{examSetId}";
                Debug.WriteLine($"[DEBUG] API URL: {url}");
                
                string json;
                try
                {
                    json = await _http.GetStringAsync(url);
                    Debug.WriteLine($"[DEBUG] API Response received: {json.Length} characters");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ExamQPV] ERROR fetching questions: {ex.Message}");
                    await Application.Current.MainPage.DisplayAlert("Lỗi", $"Không thể tải câu hỏi: {ex.Message}", "OK");
                    return;
                }

                var questions = JsonSerializer.Deserialize<List<Question>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (questions == null || !questions.Any())
                {
                    Debug.WriteLine("[DEBUG] No questions found or deserialization failed");
                    await Application.Current.MainPage.DisplayAlert("Thông báo", "Không tìm thấy câu hỏi cho bài kiểm tra này", "OK");
                    return;
                }

                Debug.WriteLine($"[DEBUG] Found {questions.Count} questions");

                foreach (var q in questions.OrderBy(x => x.QuestionOrder))
                {
                    // Nếu có RawAnswerOptions, parse ra ParsedOptions
                    if (!string.IsNullOrWhiteSpace(q.RawAnswerOptions))
                    {
                        try
                        {
                            q.ParsedOptions = JsonSerializer.Deserialize<string[]>(q.RawAnswerOptions);
                        }
                        catch
                        {
                            q.ParsedOptions = System.Array.Empty<string>();
                        }
                    }
                    else
                    {
                        q.ParsedOptions = System.Array.Empty<string>();
                    }

                    var questionViewModel = new QuestionItemViewModel(q);
                    
                    // ✅ THÊM: Load question level nếu cần
                    await questionViewModel.LoadQuestionLevelAsync(_http);
                    
                    Questions.Add(questionViewModel);
                }

                CurrentIndex = Questions.Count > 0 ? 0 : -1;
                TotalQuestions = Questions.Count; // ✅ THÊM
                UpdateAnsweredQuestionsCount(); // ✅ THÊM
                
                Debug.WriteLine($"[DEBUG] Loaded {Questions.Count} questions, CurrentIndex: {CurrentIndex}");
                
                OnPropertyChanged(nameof(CurrentQuestion));
                OnPropertyChanged(nameof(CurrentQuestionCollection));
                OnPropertyChanged(nameof(CanGoPrev));
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(QuestionProgressText)); // ✅ THÊM
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DEBUG] LoadQuestionsAsync Error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Lỗi", $"Đã xảy ra lỗi: {ex.Message}", "OK");
            }
        }

        [RelayCommand(CanExecute = nameof(CanGoPrev))]
        void Prev()
        {
            CurrentIndex--;
            UpdateAnsweredQuestionsCount(); // ✅ THÊM
            OnPropertyChanged(nameof(CurrentQuestion));
            OnPropertyChanged(nameof(CurrentQuestionCollection));
            OnPropertyChanged(nameof(CanGoPrev));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(QuestionProgressText)); // ✅ THÊM
        }

        [RelayCommand(CanExecute = nameof(CanGoNext))]
        void Next()
        {
            CurrentIndex++;
            UpdateAnsweredQuestionsCount(); // ✅ THÊM
            OnPropertyChanged(nameof(CurrentQuestion));
            OnPropertyChanged(nameof(CurrentQuestionCollection));
            OnPropertyChanged(nameof(CanGoPrev));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(QuestionProgressText)); // ✅ THÊM
        }

        [RelayCommand]
        public async Task SubmitExamAsync()
        {
            if (IsSubmitting) return; // Prevent multiple submissions

            // ✅ SỬA: Sử dụng popup đẹp thay vì DisplayAlert
            var answeredCount = GetAnsweredQuestionsCount();
            var timeRemaining = _totalSeconds > 0 ? TimerDisplay : "Không giới hạn";
            
            var confirmationPopup = new SubmitConfirmationPopup(Questions.Count, answeredCount, timeRemaining);
            var result = await Application.Current.MainPage.ShowPopupAsync(confirmationPopup);

            // Kiểm tra kết quả từ popup
            if (result is bool isConfirmed && isConfirmed)
            {
                // Proceed with submission
                await SubmitExamInternalAsync(false);
            }
            // Nếu user hủy, không làm gì cả - quay lại exam
        }

        // ✅ THÊM: Method để đếm số câu đã trả lời
        private int GetAnsweredQuestionsCount()
        {
            int count = 0;
            
            foreach (var question in Questions)
            {
                bool isAnswered = false;
                
                switch (question.Model.QuestionTypeId)
                {
                    case 1: // MCQ
                    case 2: // True/False
                        isAnswered = question.Options.Any(o => o.IsSelected);
                        break;
                        
                    case 3: // Fill in the blank
                        isAnswered = !string.IsNullOrWhiteSpace(question.UserAnswer);
                        break;
                        
                    case 4: // Audio
                        isAnswered = question.Blanks.Any(b => !string.IsNullOrWhiteSpace(b.UserText));
                        break;
                }
                
                if (isAnswered) count++;
            }
            
            return count;
        }

        private async Task SubmitExamInternalAsync(bool isAutoSubmit = false)
        {
            if (IsSubmitting) return; // Prevent multiple submissions
            
            IsSubmitting = true;

            try
            {
                // Calculate total score based on QuestionScore from each question
                decimal totalScore = CalculateTotalScore();
                
                // Calculate duration
                int durationSec = (int)(DateTime.Now - _examStartTime).TotalSeconds;
                
                // Determine if passed
                bool isPassed = totalScore >= PassingScore;

                // Stop timer if running
                _timer?.Stop();

                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await Application.Current.MainPage.DisplayAlert("Lỗi", "Không tìm thấy token xác thực", "OK");
                    return;
                }

                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Prepare submission data
                var submissionData = new
                {
                    historyId = HistoryId,
                    userId = 0,
                    fullName = "string",
                    examSetId = ExamSetId,
                    takenAt = DateTime.Now,
                    totalScore = totalScore,
                    isPassed = isPassed,
                    durationSec = durationSec
                };

                var json = JsonSerializer.Serialize(submissionData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyExam/SubmitExam/{HistoryId}";
                var response = await _http.PutAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[DEBUG] SubmitExam Response: {responseJson}");

                    // Hiển thị popup tương ứng với kết quả
                    if (isPassed)
                    {
                        //await CreateCertificateAndShowPassedPopupAsync(totalScore);
                        await TestExamPassedPopupAsync();
                    }
                    else
                    {
                        await ShowFailedPopupAsync(totalScore);
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Lỗi", "Không thể nộp bài. Vui lòng thử lại.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] SubmitExamAsync: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Đã xảy ra lỗi khi nộp bài", "OK");
            }
            finally
            {
                IsSubmitting = false;
            }
        }

        private async Task CreateCertificateAndShowPassedPopupAsync(decimal totalScore)
        {
            try
            {
                Debug.WriteLine($"[DEBUG] Creating certificate for CourseId: {CourseId}");

                var requestData = new
                {
                    CourseId = CourseId,
                    UserId = 0,
                    StudentName = "string",
                    Subtitle = "string",
                    Signature = "string",
                    CreatedAt = DateTime.Now,
                    ExpirationDate = DateTime.Now.AddYears(1),
                    VerificationCode = "string"
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"{AppConfig.AppConfig.BaseUrl}/api/Certificates/CreateCertificates";
                var response = await _http.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[DEBUG] CreateCertificate Response: {responseJson}");

                    var certificate = JsonSerializer.Deserialize<Certificate>(responseJson, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (certificate != null && !string.IsNullOrEmpty(certificate.ImageUrl))
                    {
                        // ✅ SỬA: Thêm CourseId vào constructor
                        var passedPopup = new ExamPassedPopup(certificate, totalScore, PassingScore, CourseId);
                        await Application.Current.MainPage.ShowPopupAsync(passedPopup);
                    }
                }
                else
                {
                    Debug.WriteLine($"[ERROR] CreateCertificate failed: {response.StatusCode}");
                    await Application.Current.MainPage.DisplayAlert("Thông báo", "Không thể tạo chứng chỉ, nhưng bạn đã hoàn thành bài kiểm tra thành công!", "OK");
                    // Quay về LessonPage
                    await Shell.Current.GoToAsync($"//courselessonpage?courseId={CourseId}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] CreateCertificateAndShowPopupAsync: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Thông báo", "Không thể tạo chứng chỉ, nhưng bạn đã hoàn thành bài kiểm tra thành công!", "OK");
                // Quay về LessonPage
                await Shell.Current.GoToAsync($"//courselessonpage?courseId={CourseId}");
            }
        }

        private async Task ShowFailedPopupAsync(decimal totalScore)
        {
            try
            {
                // Hiển thị popup trượt
                var failedPopup = new ExamFailedPopup(totalScore, PassingScore, CourseId);
                await Application.Current.MainPage.ShowPopupAsync(failedPopup);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] ShowFailedPopupAsync: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Thông báo", "Bạn chưa đạt yêu cầu để vượt qua bài kiểm tra!", "OK");
                // Quay về LessonPage
                await Shell.Current.GoToAsync($"//courselessonpage?courseId={CourseId}");
            }
        }

        private decimal CalculateTotalScore()
        {
            decimal totalScore = 0;

            foreach (var question in Questions)
            {
                bool isCorrect = false;

                switch (question.Model.QuestionTypeId)
                {
                    case 1: // MCQ
                    case 2: // True/False
                        var selectedOption = question.Options.FirstOrDefault(o => o.IsSelected);
                        if (selectedOption != null)
                        {
                            isCorrect = selectedOption.Text.Equals(question.Model.CorrectAnswer, StringComparison.OrdinalIgnoreCase);
                        }
                        break;

                    case 3: // Fill in the blank
                        isCorrect = !string.IsNullOrWhiteSpace(question.UserAnswer) && 
                                   question.UserAnswer.Trim().Equals(question.Model.CorrectAnswer?.Trim(), StringComparison.OrdinalIgnoreCase);
                        break;

                    case 4: // Audio
                        // Check if all blanks are filled correctly
                        isCorrect = question.Blanks.All(blank => 
                            !string.IsNullOrWhiteSpace(blank.UserText) && 
                            blank.UserText.Trim().Equals(blank.CorrectAnswer?.Trim(), StringComparison.OrdinalIgnoreCase));
                        break;
                }

                if (isCorrect && question.Model.QuestionScore.HasValue)
                {
                    totalScore += question.Model.QuestionScore.Value;
                    Debug.WriteLine($"[DEBUG] Question {question.Model.QuestionId}: Correct! Score: {question.Model.QuestionScore.Value}");
                }
                else
                {
                    Debug.WriteLine($"[DEBUG] Question {question.Model.QuestionId}: Incorrect or no score");
                }
            }

            Debug.WriteLine($"[DEBUG] Total Score Calculated: {totalScore}");
            return totalScore;
        }

        [RelayCommand]
        public async Task TestExamPassedPopupAsync()
        {
            try
            {
                // Create a test certificate with the URL you provided
                var testCertificate = new Certificate
                {
                    ImageUrl = "https://images.bannerbear.com/direct/e8DW5pz4jQVMxQw7aR/requests/000/097/683/893/aMBJ5DWdLYP2Vn0B6XRNjrp4Z/c5a41822c55cffc153a70dd6452d307f75bb3c8a.png",
                    CertificateId = 999 // Test ID
                };

                // Test data
                decimal testTotalScore = 85.5m;
                decimal testPassingScore = 70.0m;
                int testCourseId = 1;

                Debug.WriteLine($"[TEST] Creating test ExamPassedPopup with:");
                Debug.WriteLine($"[TEST] ImageUrl: {testCertificate.ImageUrl}");
                Debug.WriteLine($"[TEST] TotalScore: {testTotalScore}");
                Debug.WriteLine($"[TEST] PassingScore: {testPassingScore}");
                Debug.WriteLine($"[TEST] CourseId: {testCourseId}");

                // Create and show test popup
                var testPopup = new ExamPassedPopup(testCertificate, testTotalScore, testPassingScore, testCourseId);
                await Application.Current.MainPage.ShowPopupAsync(testPopup);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TEST ERROR] TestExamPassedPopupAsync: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Test Error", ex.Message, "OK");
            }
        }
    }
}