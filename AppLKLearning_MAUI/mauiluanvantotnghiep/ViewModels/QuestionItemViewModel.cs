// ViewModels/QuestionItemViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mauiluanvantotnghiep.Models;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace mauiluanvantotnghiep.ViewModels
{
    public partial class QuestionItemViewModel : ObservableObject
    {
        public Question Model { get; }

        [ObservableProperty]
        bool isCorrect;

        [ObservableProperty]
        string questionLevelName = ""; // THÊM property cho độ khó

        public int QuestionId => Model.QuestionId;
        public string QuestionText => Model.QuestionText;
        public string ImageUrl => Model.ImageUrl;
        public string Explanation => Model.Explanation;
        public string AudioUrl => Model.AudioUrl;
        public string QuestionDescription => Model.QuestionDescription;

        // MCQ / True-False
        public ObservableCollection<OptionItem> Options { get; }
            = new ObservableCollection<OptionItem>();

        // Audio blanks
        public ObservableCollection<BlankItem> Blanks { get; }
            = new ObservableCollection<BlankItem>();

        [ObservableProperty]
        bool isAnswerSubmitted;

        [ObservableProperty]
        string userAnswer;

        // Dành cho FillBlank (type 3)
        [ObservableProperty]
        Color fillBlankBorderColor = Colors.Transparent;

        public IRelayCommand SubmitAnswerCommand { get; }

        public QuestionItemViewModel(Question q)
        {
            Model = q;

            switch (q.QuestionTypeId)
            {
                case 1: // MCQ
                    {
                        // Shuffle options for MCQ
                        // Tạo mảng các lựa chọn từ RawAnswerOptions
                        // Nếu RawAnswerOptions là null hoặc rỗng, sử dụng mảng rỗng
                        // ParsedOptions sẽ chứa các lựa chọn đã được phân tách từ RawAnswerOptions
                        // Nếu RawAnswerOptions là null hoặc rỗng, ParsedOptions sẽ là mảng rỗng
                        // ParsedOptions sẽ được sắp xếp ngẫu nhiên
                        var choices = (q.ParsedOptions ?? Array.Empty<string>()).OrderBy(_ => Guid.NewGuid()).ToArray();
                        foreach (var c in choices)
                        {
                            var oi = new OptionItem(c);
                            oi.OnSelected += (sender, _) => OnOptionSelected(sender as OptionItem);
                            Options.Add(oi);
                        }
                        break;
                    }
                case 2: // True/False
                    {
                        // tác dụng của đoạn này là tạo ra 2 lựa chọn "True" và "False" cho câu hỏi đúng/sai
                        // và sắp xếp ngẫu nhiên
                        var choices = new[] { "True", "False" }.OrderBy(_ => Guid.NewGuid()).ToArray();
                        foreach (var c in choices)
                        {
                            var oi = new OptionItem(c);
                            oi.OnSelected += (sender, _) => OnOptionSelected(sender as OptionItem);
                            Options.Add(oi);
                        }
                        break;
                    }

                // 3 = Fill-in-the-Blank, chỉ cần userAnswer + fillBlankBorderColor
                case 3:
                    break;
                // 4 = Audio → tạo blanks dựa vào CorrectAnswer
                case 4:
                    string[] correctAnswers;
                    try
                    {
                        // Deserialize chuỗi CorrectAnswer thành mảng
                        correctAnswers = JsonSerializer.Deserialize<string[]>(
                            Model.CorrectAnswer ?? "[]",
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        ) ?? Array.Empty<string>();
                        // Log để debug
                        Console.WriteLine($"[QIVM] Parsed CorrectAnswer: {string.Join(", ", correctAnswers)}");
                    }
                    catch (Exception ex)
                    {
                        // Xử lý lỗi nếu CorrectAnswer không phải JSON hợp lệ
                        Console.WriteLine($"[QIVM] ERROR parsing CorrectAnswer: {ex.Message}");
                        correctAnswers = Array.Empty<string>();
                    }

                    // Tạo BlankItem cho mỗi đáp án đúng
                    foreach (var correct in correctAnswers)
                    {
                        Blanks.Add(new BlankItem(correct ?? string.Empty));
                    }
                    break;
                case 8:
                    // 8 = Video → không cần xử lý gì đặc biệt, chỉ hiển thị video
                    break;
            }

            SubmitAnswerCommand = new RelayCommand(OnSubmit);
        }

        // THÊM method để load question level
        public async Task LoadQuestionLevelAsync(HttpClient httpClient)
        {
            if (Model.QuestionLevelId == null || Model.QuestionLevelId == 0)
            {
                QuestionLevelName = "Không xác định";
                return;
            }

            try
            {
                // THÊM: Đảm bảo HttpClient có handler để bypass SSL nếu cần
                HttpClient clientToUse = httpClient;
                
                // Nếu httpClient được truyền vào không có handler bypass SSL, tạo mới
                if (httpClient.DefaultRequestHeaders.Authorization == null && httpClient.Timeout == TimeSpan.FromSeconds(100))
                {
                    // Có vẻ như đây là HttpClient mặc định, tạo mới với handler bypass SSL
                    var handler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
                    };
                    clientToUse = new HttpClient(handler);
                    
                    // Copy authorization header nếu có
                    if (httpClient.DefaultRequestHeaders.Authorization != null)
                    {
                        clientToUse.DefaultRequestHeaders.Authorization = httpClient.DefaultRequestHeaders.Authorization;
                    }
                }

                var url = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyCauHoi/GetQuestionLevelById/{Model.QuestionLevelId}";
                Console.WriteLine($"[QIVM] Loading QuestionLevel from: {url}");
                
                var json = await clientToUse.GetStringAsync(url);
                Console.WriteLine($"[QIVM] QuestionLevel JSON: {json}");
                
                var questionLevel = JsonSerializer.Deserialize<QuestionLevel>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                QuestionLevelName = questionLevel?.QuestionName ?? "Không xác định";
                Console.WriteLine($"[QIVM] Loaded QuestionLevel: {QuestionLevelName} for Question {QuestionId}");
                
                // Dispose HttpClient mới tạo nếu cần
                if (clientToUse != httpClient)
                {
                    clientToUse.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[QIVM] ERROR loading QuestionLevel: {ex.Message}");
                Console.WriteLine($"[QIVM] Exception details: {ex}");
                QuestionLevelName = "Không xác định";
            }
        }

        // Thêm method mới để xử lý single selection
        private void OnOptionSelected(OptionItem selectedOption)
        {
            if (selectedOption == null || IsAnswerSubmitted) return;

            // Bỏ chọn tất cả options khác
            foreach (var option in Options)
            {
                if (option != selectedOption)
                {
                    option.IsSelected = false;
                }
            }

            // Cập nhật UserAnswer
            UserAnswer = selectedOption.Text;
        }

        void OnSubmit()
        {
            if (IsAnswerSubmitted) return;
            bool result = false;
            
            Console.WriteLine($"[DEBUG] Starting OnSubmit for QuestionType: {Model.QuestionTypeId}");
            
            switch (Model.QuestionTypeId)
            {
                case 1: // MCQ
                case 2: // True/False
                    result = string.Equals(UserAnswer?.Trim(),
                                          Model.CorrectAnswer?.Trim(),
                                          StringComparison.OrdinalIgnoreCase);
                    
                    Console.WriteLine($"[DEBUG] UserAnswer: '{UserAnswer}', CorrectAnswer: '{Model.CorrectAnswer}', Result: {result}");
                    Console.WriteLine($"[DEBUG] Options count: {Options.Count}");
                    
                    foreach (var opt in Options)
                    {
                        Console.WriteLine($"[DEBUG] BEFORE - Option '{opt.Text}': IsSelected={opt.IsSelected}, BackgroundColor={opt.BackgroundColor}");
                        
                        // Reset trước
                        opt.BackgroundColor = Colors.Transparent;
                        
                        // Đáp án đúng luôn hiện màu xanh
                        if (opt.Text == Model.CorrectAnswer)
                        {
                            opt.BackgroundColor = Colors.LightGreen;
                            Console.WriteLine($"[DEBUG] Set CORRECT option '{opt.Text}' to LightGreen");
                        }
                        // Nếu người dùng chọn sai, hiện màu đỏ cho option được chọn
                        else if (opt.IsSelected && opt.Text != Model.CorrectAnswer)
                        {
                            opt.BackgroundColor = Colors.LightCoral;
                            Console.WriteLine($"[DEBUG] Set WRONG option '{opt.Text}' to LightCoral");
                        }
                        
                        Console.WriteLine($"[DEBUG] AFTER - Option '{opt.Text}': IsSelected={opt.IsSelected}, BackgroundColor={opt.BackgroundColor}");
                    }
                    break;

                case 3: // Fill-in-the-Blank
                    var u = UserAnswer?.Trim() ?? "";
                    var c = Model.CorrectAnswer?.Trim() ?? "";
                    result = u.Equals(c, StringComparison.OrdinalIgnoreCase);
                    FillBlankBorderColor = result ? Colors.LightGreen : Colors.LightCoral;
                    break;

                case 4: // Audio
                    result = Blanks.All(b =>
                        string.Equals(b.UserText?.Trim(),
                                      b.CorrectAnswer?.Trim(),
                                      StringComparison.OrdinalIgnoreCase));
                    
                    foreach (var b in Blanks)
                    {
                        var t = b.UserText?.Trim() ?? "";
                        b.BorderColor = t.Equals(b.CorrectAnswer.Trim(),
                            StringComparison.OrdinalIgnoreCase)
                            ? Colors.LightGreen
                            : Colors.LightCoral;
                    }
                    break;
            }
            
            IsCorrect = result;
            IsAnswerSubmitted = true;
            Console.WriteLine($"[DEBUG] OnSubmit completed. IsAnswerSubmitted={IsAnswerSubmitted}");
        }
    }
}
