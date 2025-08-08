using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;
using mauiluanvantotnghiep.Models;

namespace mauiluanvantotnghiep.ViewModels
{
    public partial class StoryViewModel : ObservableObject
    {
        // NEW ChatGPT-42 API constants - replacing OpenAI
        const string ChatGptApiUrl = "https://chatgpt-42.p.rapidapi.com/chat";
        const string RapidApiKey = "9abe7f5a38msh2339f590ff1d110p13c781jsnfbe0acb057aa";
        
        // TTS API constants
        const string TtsApiUrl = "https://large-text-to-speech.p.rapidapi.com/tts";

        // Enhanced Genre collection with icons and colors
        public ObservableCollection<Genre> Genres { get; } = new()
        {
            new Genre { Name = "Fantasy", DisplayName = "Phiêu lưu", Icon = "🏰", Color = "#9C27B0", Description = "Thế giới phép thuật và phiêu lưu" },
            new Genre { Name = "Sci-Fi", DisplayName = "Khoa học viễn tưởng", Icon = "🚀", Color = "#2196F3", Description = "Công nghệ và tương lai" },
            new Genre { Name = "Mystery", DisplayName = "Bí ẩn", Icon = "🔍", Color = "#795548", Description = "Giải mã những điều bí ẩn" },
            new Genre { Name = "Romance", DisplayName = "Lãng mạn", Icon = "💖", Color = "#E91E63", Description = "Tình yêu và cảm xúc" },
            new Genre { Name = "Horror", DisplayName = "Kinh dị", Icon = "👻", Color = "#424242", Description = "Hồi hộp và sợ hãi" },
            new Genre { Name = "Adventure", DisplayName = "Mạo hiểm", Icon = "⛰️", Color = "#FF9800", Description = "Hành trình khám phá" },
            new Genre { Name = "Comedy", DisplayName = "Hài hước", Icon = "😄", Color = "#FFEB3B", Description = "Vui vẻ và giải trí" },
            new Genre { Name = "Drama", DisplayName = "Tâm lý", Icon = "🎭", Color = "#607D8B", Description = "Cảm xúc sâu sắc" },
            new Genre { Name = "Thriller", DisplayName = "Ly kỳ", Icon = "⚡", Color = "#F44336", Description = "Căng thẳng và hồi hộp" },
            new Genre { Name = "Historical Fiction", DisplayName = "Lịch sử", Icon = "🏛️", Color = "#8BC34A", Description = "Câu chuyện quá khứ" }
        };

        // Enhanced AgeGroup collection with icons and colors
        public ObservableCollection<AgeGroup> AgeGroups { get; } = new()
        {
            new AgeGroup { Name = "Children", DisplayName = "Trẻ em", Icon = "👶", Color = "#FF9800", Description = "Câu chuyện đơn giản và vui nhộn", AgeRange = "3-7 tuổi" },
            new AgeGroup { Name = "Pre-teen", DisplayName = "Tiểu học", Icon = "🧒", Color = "#4CAF50", Description = "Khám phá và học hỏi", AgeRange = "8-12 tuổi" },
            new AgeGroup { Name = "Teenager", DisplayName = "Thanh thiếu niên", Icon = "👦", Color = "#2196F3", Description = "Phiêu lưu và tình bạn", AgeRange = "13-17 tuổi" },
            new AgeGroup { Name = "Young Adult", DisplayName = "Người trẻ", Icon = "👨", Color = "#9C27B0", Description = "Tình yêu và sự nghiệp", AgeRange = "18-25 tuổi" },
            new AgeGroup { Name = "Adult", DisplayName = "Người lớn", Icon = "👔", Color = "#795548", Description = "Cuộc sống và trải nghiệm", AgeRange = "26+ tuổi" },
            new AgeGroup { Name = "Senior", DisplayName = "Người cao tuổi", Icon = "👴", Color = "#607D8B", Description = "Trí tuệ và kỷ niệm", AgeRange = "60+ tuổi" }
        };

        // Selected values
        [ObservableProperty] Genre selectedGenre;
        [ObservableProperty] AgeGroup selectedAgeGroup; 
        [ObservableProperty] bool isGenrePickerVisible;
        [ObservableProperty] bool isAgePickerVisible;

        // Outputs
        [ObservableProperty] string story = "";
        [ObservableProperty] string storyVietnamese = ""; // Thêm bản dịch tiếng Việt
        [ObservableProperty] bool isBusy;
        [ObservableProperty] string errorMessage = "";
        [ObservableProperty] string audioUrl;
        [ObservableProperty] string readStoryURL;

        // New properties for enhanced UI
        [ObservableProperty] bool isStoryGenerated;
        [ObservableProperty] string generationStatus = "";
        [ObservableProperty] double generationProgress;

        // Bilingual display properties
        [ObservableProperty] bool isEnglishVisible = true;
        [ObservableProperty] bool isVietnameseVisible = false;
        [ObservableProperty] bool isBilingualMode = false;
        [ObservableProperty] string currentLanguageMode = "🇬🇧 Tiếng Anh";

        // Thêm property mới cho HTML content
        [ObservableProperty] HtmlWebViewSource htmlStoryContent;

        // Helper để binding IsEnabled cho nút Generate
        public bool IsNotBusy => !IsBusy;

        public StoryViewModel()
        {
            Debug.WriteLine("[StoryViewModel] Initialized.");
            SelectedGenre = Genres[0];
            SelectedAgeGroup = AgeGroups[0];
        }

        // Command to show/hide genre picker
        [RelayCommand]
        void ToggleGenrePicker()
        {
            IsGenrePickerVisible = !IsGenrePickerVisible;
        }

        // Command to show/hide age picker
        [RelayCommand]
        void ToggleAgePicker()
        {
            IsAgePickerVisible = !IsAgePickerVisible;
        }

        // Command to select genre
        [RelayCommand]
        void SelectGenre(Genre genre)
        {
            // Unselect all genres
            foreach (var g in Genres)
                g.IsSelected = false;
            
            // Select the chosen genre
            genre.IsSelected = true;
            SelectedGenre = genre;
            IsGenrePickerVisible = false;
            
            Debug.WriteLine($"[SelectGenre] Selected: {genre.DisplayName}");
        }

        // Command to select age group
        [RelayCommand]
        void SelectAgeGroup(AgeGroup ageGroup)
        {
            // Unselect all age groups
            foreach (var ag in AgeGroups)
                ag.IsSelected = false;
            
            // Select the chosen age group
            ageGroup.IsSelected = true;
            SelectedAgeGroup = ageGroup;
            IsAgePickerVisible = false;
            
            Debug.WriteLine($"[SelectAgeGroup] Selected: {ageGroup.DisplayName}");
        }

        // NEW: Commands for language switching
        [RelayCommand]
        void ToggleLanguageMode()
        {
            if (!IsBilingualMode)
            {
                if (IsEnglishVisible)
                {
                    // Switch to Vietnamese only
                    IsEnglishVisible = false;
                    IsVietnameseVisible = true;
                    CurrentLanguageMode = "🇻🇳 Tiếng Việt";
                }
                else
                {
                    // Switch to Bilingual
                    IsEnglishVisible = true;
                    IsVietnameseVisible = true;
                    IsBilingualMode = true;
                    CurrentLanguageMode = "🌐 Song ngữ";
                }
            }
            else
            {
                // Switch back to English only
                IsEnglishVisible = true;
                IsVietnameseVisible = false;
                IsBilingualMode = false;
                CurrentLanguageMode = "🇬🇧 Tiếng Anh";
            }
            
            // Update HTML display
            CreateHtmlStoryContent();
        }

        // 1. Generate Story, Vietnamese Translation & Audio with progress updates
        [RelayCommand(CanExecute = nameof(CanGenerate))]
        async Task GenerateAllAsync()
        {
            Debug.WriteLine("[GenerateAllAsync] Starting...");
            try
            {   
                IsBusy = true;
                IsStoryGenerated = false;
                GenerationProgress = 0;
                Story = "";
                StoryVietnamese = "";
                ErrorMessage = "";
                AudioUrl = null;
                ReadStoryURL = null;

                // Step 1: Generate English Story using ChatGPT-42 API
                GenerationStatus = "🔮 Đang tạo câu chuyện tiếng Anh...";
                GenerationProgress = 0.25;
                
                var prompt = BuildPrompt();
                Debug.WriteLine($"[GenerateAllAsync] Prompt: {prompt}");
                Story = await CallChatGptApiAsync(prompt);
                Debug.WriteLine("[GenerateAllAsync] English story generated."); 
                
                GenerationProgress = 0.5;

                // Step 2: Translate to Vietnamese
                GenerationStatus = "🌐 Đang dịch sang tiếng Việt...";
                if (!string.IsNullOrWhiteSpace(Story))
                {
                    StoryVietnamese = await TranslateToVietnameseAsync(Story);
                    Debug.WriteLine("[GenerateAllAsync] Vietnamese translation generated.");
                }
                
                GenerationProgress = 0.75;
                IsStoryGenerated = true;

                // Step 3: Generate Audio for English version
                GenerationStatus = "🎵 Đang tạo âm thanh...";
                if (!string.IsNullOrWhiteSpace(Story))
                {
                    await ReadStoryAsync();
                }
                
                GenerationStatus = "✨ Hoàn thành!";
                GenerationProgress = 1.0;
                
                // Clear status after delay
                await Task.Delay(2000);
                GenerationStatus = "";
                GenerationProgress = 0;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi: {ex.Message}";
                Debug.WriteLine($"[GenerateAllAsync] Error: {ex.Message} - StackTrace: {ex.StackTrace}");
                Story = "";
                StoryVietnamese = "";
                AudioUrl = null;
                ReadStoryURL = null;
                IsStoryGenerated = false;
                await Application.Current.MainPage.DisplayAlert("Lỗi", ErrorMessage, "OK");
            }
            finally
            {
                IsBusy = false;
                GenerationStatus = "";
                GenerationProgress = 0;
                Debug.WriteLine("[GenerateAllAsync] Set IsBusy to false.");
                GenerateAllCommand.NotifyCanExecuteChanged();
                ReadStoryCommand.NotifyCanExecuteChanged();
            }
        }

        // NEW: Translate English story to Vietnamese using ChatGPT
        private async Task<string> TranslateToVietnameseAsync(string englishStory)
        {
            try
            {
                var translationPrompt = $@"Translate the following English story to Vietnamese. 
Keep the same structure, style, and formatting. Make the translation natural and fluent for Vietnamese readers.
Preserve any emojis and maintain the storytelling tone.

English story to translate:
{englishStory}

Please provide only the Vietnamese translation, no additional comments.";

                var vietnameseTranslation = await CallChatGptApiAsync(translationPrompt);
                Debug.WriteLine($"[TranslateToVietnameseAsync] Translation completed, length: {vietnameseTranslation.Length}");
                
                return vietnameseTranslation;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TranslateToVietnameseAsync] Error: {ex.Message}");
                return "Không thể dịch câu chuyện sang tiếng Việt. Vui lòng thử lại.";
            }
        }

        // 2. Read Aloud using new TTS API (two-step process)
        [RelayCommand(CanExecute = nameof(CanRead))]
        async Task ReadStoryAsync()
        {
            Debug.WriteLine("[ReadStoryAsync] Starting...");
            if (string.IsNullOrWhiteSpace(Story))
            {
                Debug.WriteLine("[ReadStoryAsync] Story is empty or null, skipping.");
                return;
            }

            // Nếu chưa có AudioUrl, gọi API để tạo
            if (string.IsNullOrWhiteSpace(AudioUrl))
            {
                try
                {
                    // Step 1: Create TTS job
                    string jobId = await CreateTtsJobAsync();
                    Debug.WriteLine($"[ReadStoryAsync] TTS Job created with ID: {jobId}");

                    // Step 2: Poll for result
                    string audioUrl = await PollTtsJobAsync(jobId);
                    
                    AudioUrl = audioUrl;
                    ReadStoryURL = AudioUrl;
                    Debug.WriteLine($"[TTS] Audio URL assigned: {AudioUrl}, ReadStoryURL: {ReadStoryURL}");
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Lỗi đọc to: {ex.Message}";
                    Debug.WriteLine($"[ReadStoryAsync] Error: {ex.Message} - StackTrace: {ex.StackTrace}");
                    AudioUrl = null;
                    ReadStoryURL = null;
                    await Application.Current.MainPage.DisplayAlert("Lỗi", ErrorMessage, "OK");
                    return;
                }
            }
        }

        // Create TTS job and return job ID (improved)
        private async Task<string> CreateTtsJobAsync()
        {
            Debug.WriteLine("[CreateTtsJobAsync] Starting...");
            
            // Clean and truncate the story text for TTS with better limits
            var cleanStory = CleanTextForTts(Story);
            
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(60);
            
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(TtsApiUrl),
                Headers =
                {
                    { "x-rapidapi-key", RapidApiKey },
                    { "x-rapidapi-host", "large-text-to-speech.p.rapidapi.com" },
                },
                Content = new StringContent(JsonSerializer.Serialize(new { text = cleanStory }))
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
                }
            };

            Debug.WriteLine($"[CreateTtsJobAsync] Cleaned text length: {cleanStory.Length}");
            Debug.WriteLine($"[CreateTtsJobAsync] Sending TTS job creation request...");
            
            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            
            var body = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"[CreateTtsJobAsync] Response Body: {body}");

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            
            if (root.TryGetProperty("id", out var idElement))
            {
                var jobId = idElement.GetString();
                if (string.IsNullOrEmpty(jobId))
                {
                    throw new InvalidOperationException("Job ID is null or empty");
                }
                return jobId;
            }
            else
            {
                throw new InvalidOperationException("Không tìm thấy job ID trong phản hồi");
            }
        }

        // Simple text cleaning method - focus on Unicode escape sequences
        private string CleanTextForTts(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            // Step 1: Remove markdown and formatting
            var cleaned = text
                .Replace("**", "") // Bold markdown
                .Replace("*", "")  // Italic markdown  
                .Replace("#", "")  // Headers
                .Replace("_", "")  // Underscore formatting
                .Replace("\n", " ") // Line breaks to spaces
                .Replace("\r", " ") // Carriage returns to spaces
                .Replace("  ", " "); // Double spaces to single

            // Step 2: Remove Unicode escape sequences (like \ud83c\udf1f)
            cleaned = Regex.Replace(cleaned, @"\\u[0-9a-fA-F]{4}", "");

            // Step 3: Remove or replace problematic punctuation
            cleaned = cleaned.Replace("\"", ""); // Remove double quotes completely
            cleaned = cleaned.Replace("'", ""); // Remove single quotes/apostrophes
            cleaned = cleaned.Replace("\u201C", ""); // Left double quotation mark
            cleaned = cleaned.Replace("\u201D", ""); // Right double quotation mark
            cleaned = cleaned.Replace("\u2018", ""); // Left single quotation mark
            cleaned = cleaned.Replace("\u2019", ""); // Right single quotation mark

            // Step 4: Replace problematic Unicode characters with safe alternatives
            cleaned = Regex.Replace(cleaned, @"[\u2014\u2013]", "-"); // Em/En dash to hyphen
            cleaned = Regex.Replace(cleaned, @"[\u201C\u201D\u201E\u201F]", ""); // Smart quotes - remove completely
            cleaned = Regex.Replace(cleaned, @"[\u2018\u2019\u201A\u201B]", ""); // Smart apostrophes - remove completely

            // Step 5: Remove any remaining special characters that might cause issues
            cleaned = Regex.Replace(cleaned, @"[^\w\s\.,!?;:\-()]", ""); // Keep only word chars, spaces, and basic punctuation

            // Step 6: Clean up multiple spaces and trim
            cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();
            
            Debug.WriteLine($"[CleanTextForTts] Original length: {text?.Length}, Cleaned length: {cleaned.Length}");
            Debug.WriteLine($"[CleanTextForTts] Cleaned text preview: {cleaned.Substring(0, Math.Min(150, cleaned.Length))}...");
            
            return cleaned;
        }

        // Poll TTS job until completion and return audio URL
        private async Task<string> PollTtsJobAsync(string jobId)
        {
            Debug.WriteLine($"[PollTtsJobAsync] Starting polling for job ID: {jobId}");
            
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30);
            
            var maxAttempts = 30; // Maximum 30 attempts (about 5 minutes with 10s delays)
            var delay = TimeSpan.FromSeconds(10);
            
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri($"{TtsApiUrl}?id={jobId}"),
                        Headers =
                        {
                            { "x-rapidapi-key", RapidApiKey },
                            { "x-rapidapi-host", "large-text-to-speech.p.rapidapi.com" },
                        },
                    };

                    Debug.WriteLine($"[PollTtsJobAsync] Attempt {attempt}: Checking job status...");
                    using var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    
                    var body = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[PollTtsJobAsync] Response Body: {body}");

                    using var doc = JsonDocument.Parse(body);
                    var root = doc.RootElement;
                    
                    if (root.TryGetProperty("status", out var statusElement))
                    {
                        var status = statusElement.GetString();
                        Debug.WriteLine($"[PollTtsJobAsync] Job status: {status}");
                        
                        if (status == "success")
                        {
                            if (root.TryGetProperty("url", out var urlElement))
                            {
                                var audioUrl = urlElement.GetString();
                                if (string.IsNullOrEmpty(audioUrl))
                                {
                                    throw new InvalidOperationException("Audio URL is null or empty");
                                }
                                Debug.WriteLine($"[PollTtsJobAsync] Success! Audio URL: {audioUrl}");
                                return audioUrl;
                            }
                            else
                            {
                                throw new InvalidOperationException("Không tìm thấy URL audio trong phản hồi success");
                            }
                        }
                        else if (status == "processing")
                        {
                            Debug.WriteLine($"[PollTtsJobAsync] Job still processing, waiting {delay.TotalSeconds}s before next attempt...");
                            await Task.Delay(delay);
                            continue;
                        }
                        else
                        {
                            throw new InvalidOperationException($"TTS job failed with status: {status}");
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Không tìm thấy status trong phản hồi");
                    }
                }
                catch (Exception ex) when (attempt < maxAttempts)
                {
                    Debug.WriteLine($"[PollTtsJobAsync] Attempt {attempt} failed: {ex.Message}, retrying...");
                    await Task.Delay(delay);
                    continue;
                }
            }
            
            throw new InvalidOperationException($"TTS job timed out after {maxAttempts} attempts");
        }

        // Enhanced prompt building with better structure (English only)
        string BuildPrompt()
        {
            var ageGroup = SelectedAgeGroup?.Name ?? "Children";

            var prompt = $@"Create an engaging {SelectedGenre?.Name} story suitable for {ageGroup}. 
Write the story in English.

Requirements:
- Length: 150-300 words
- Include dialogue if appropriate
- Use descriptive language
- Make it educational and entertaining
- Include emojis to make it more engaging
- Structure: Title, story content
- Ensure the story has a clear beginning, middle, and end

Just return the story content with title, no additional questions or comments.";

            Debug.WriteLine($"[BuildPrompt] Generated Prompt: {prompt}");
            return prompt;
        }

        // Enhanced validation
        private bool CanGenerate()
        {
            bool isValid = !IsBusy &&
                          SelectedGenre != null &&
                          SelectedAgeGroup != null;
            Debug.WriteLine($"[CanGenerate] IsValid: {isValid}");
            return isValid;
        }

        bool CanRead() => !IsBusy && !string.IsNullOrWhiteSpace(Story);

        // Property change handlers
        partial void OnIsBusyChanged(bool oldValue, bool newValue)
        {
            Debug.WriteLine($"[OnIsBusyChanged] Changed from {oldValue} to {newValue}");
            OnPropertyChanged(nameof(IsNotBusy));
            GenerateAllCommand.NotifyCanExecuteChanged();
            ReadStoryCommand.NotifyCanExecuteChanged();
        }

        partial void OnStoryChanged(string oldValue, string newValue)
        {
            Debug.WriteLine($"[OnStoryChanged] Changed from '{oldValue}' to '{newValue}'");
            ReadStoryCommand.NotifyCanExecuteChanged();
            IsStoryGenerated = !string.IsNullOrWhiteSpace(newValue);
            
            // Tạo HTML content
            CreateHtmlStoryContent();
        }

        partial void OnStoryVietnameseChanged(string oldValue, string newValue)
        {
            // Cập nhật HTML khi có bản dịch mới
            CreateHtmlStoryContent();
        }

        // Create HTML content for WebView display with bilingual support   
        private void CreateHtmlStoryContent()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Story))
                {
                    HtmlStoryContent = null;
                    return;
                }

                var htmlBuilder = new StringBuilder();
                htmlBuilder.Append(@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <style>
                        body {
                            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                            line-height: 1.6;
                            margin: 20px;
                            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                            color: #333;
                            min-height: 100vh;
                        }
                        .story-container {
                            background: white;
                            border-radius: 15px;
                            padding: 25px;
                            box-shadow: 0 8px 32px rgba(0,0,0,0.1);
                            margin-bottom: 20px;
                        }
                        .language-section {
                            margin-bottom: 20px;
                            padding: 15px;
                            border-radius: 10px;
                            border-left: 4px solid;
                        }
                        .english-section {
                            background: #f0f8ff;
                            border-left-color: #2196F3;
                        }
                        .vietnamese-section {
                            background: #fff8f0;
                            border-left-color: #FF9800;
                        }
                        .language-title {
                            font-weight: bold;
                            font-size: 16px;
                            margin-bottom: 10px;
                            display: flex;
                            align-items: center;
                            gap: 8px;
                        }
                        .story-text {
                            font-size: 16px;
                            text-align: justify;
                            white-space: pre-wrap;
                            line-height: 1.7;
                        }
                        .story-title {
                            font-size: 24px;
                            font-weight: bold;
                            color: #4a5568;
                            text-align: center;
                            margin-bottom: 20px;
                            border-bottom: 2px solid #e2e8f0;
                            padding-bottom: 10px;
                        }
                        .genre-badge {
                            display: inline-block;
                            background: #667eea;
                            color: white;
                            padding: 5px 15px;
                            border-radius: 20px;
                            font-size: 12px;
                            margin-bottom: 15px;
                        }
                        @media (max-width: 600px) {
                            body { margin: 10px; }
                            .story-container { padding: 15px; }
                            .story-text { font-size: 14px; }
                        }
                    </style>
                </head>
                <body>
                    <div class='story-container'>
                ");

                // Add genre badge
                if (SelectedGenre != null)
                {
                    htmlBuilder.Append($"<div class='genre-badge'>{SelectedGenre.Icon} {SelectedGenre.DisplayName}</div>");
                }

                // Extract title from English story
                var storyLines = Story.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                var title = "";
                var englishContent = Story;

                if (storyLines.Length > 0)
                {
                    var firstLine = storyLines[0].Trim();
                    if (firstLine.Length < 100 && !firstLine.EndsWith(".") && !firstLine.EndsWith("!") && !firstLine.EndsWith("?"))
                    {
                        title = firstLine;
                        englishContent = string.Join("\n", storyLines.Skip(1)).Trim();
                    }
                }

                // Add title if found
                if (!string.IsNullOrEmpty(title))
                {
                    htmlBuilder.Append($"<div class='story-title'>{System.Net.WebUtility.HtmlEncode(title)}</div>");
                }

                // Add English section if visible
                if (IsEnglishVisible)
                {
                    htmlBuilder.Append(@"
                        <div class='language-section english-section'>
                            <div class='language-title'>
                                🇬🇧 English Version
                            </div>
                            <div class='story-text'>");
                    htmlBuilder.Append(System.Net.WebUtility.HtmlEncode(englishContent));
                    htmlBuilder.Append("</div></div>");
                }

                // Add Vietnamese section if visible and available
                if (IsVietnameseVisible && !string.IsNullOrWhiteSpace(StoryVietnamese))
                {
                    // Extract Vietnamese content (remove title if present)
                    var vietnameseLines = StoryVietnamese.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    var vietnameseContent = StoryVietnamese;
                    
                    if (vietnameseLines.Length > 0)
                    {
                        var firstVietnameseLine = vietnameseLines[0].Trim();
                        if (firstVietnameseLine.Length < 100 && !firstVietnameseLine.EndsWith(".") && !firstVietnameseLine.EndsWith("!") && !firstVietnameseLine.EndsWith("?"))
                        {
                            vietnameseContent = string.Join("\n", vietnameseLines.Skip(1)).Trim();
                        }
                    }

                    htmlBuilder.Append(@"
                        <div class='language-section vietnamese-section'>
                            <div class='language-title'>
                                🇻🇳 Bản dịch tiếng Việt
                            </div>
                            <div class='story-text'>");
                    htmlBuilder.Append(System.Net.WebUtility.HtmlEncode(vietnameseContent));
                    htmlBuilder.Append("</div></div>");
                }

                htmlBuilder.Append(@"
                    </div>
                </body>
                </html>");

                HtmlStoryContent = new HtmlWebViewSource
                {
                    Html = htmlBuilder.ToString()
                };

                Debug.WriteLine("[CreateHtmlStoryContent] Bilingual HTML content created successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CreateHtmlStoryContent] Error: {ex.Message}");
                HtmlStoryContent = null;
            }
        }

        // ChatGPT-42 API call
        async Task<string> CallChatGptApiAsync(string prompt)
        {
            Debug.WriteLine("[CallChatGptApiAsync] Starting...");
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(60);
            
            var req = new HttpRequestMessage(HttpMethod.Post, ChatGptApiUrl);
            req.Headers.Add("x-rapidapi-key", RapidApiKey);
            req.Headers.Add("x-rapidapi-host", "chatgpt-42.p.rapidapi.com");

            var payload = new
            {
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },
                model = "gpt-4o-mini"
            };

            req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            using var res = await client.SendAsync(req);
            var body = await res.Content.ReadAsStringAsync();
            Debug.WriteLine($"[CallChatGptApiAsync] Response Status: {res.StatusCode}");
            Debug.WriteLine($"[CallChatGptApiAsync] Response Body: {body}");

            // Handle specific error codes
            switch (res.StatusCode)
            {
                case System.Net.HttpStatusCode.Unauthorized:
                    throw new InvalidOperationException("API key không hợp lệ. Vui lòng kiểm tra lại.");
                
                case System.Net.HttpStatusCode.TooManyRequests:
                    throw new InvalidOperationException("Đã vượt quá giới hạn request. Vui lòng thử lại sau.");
                
                case System.Net.HttpStatusCode.BadRequest:
                    throw new InvalidOperationException("Request không hợp lệ. Vui lòng kiểm tra lại prompt.");
                
                case System.Net.HttpStatusCode.InternalServerError:
                    throw new InvalidOperationException("Lỗi server ChatGPT. Vui lòng thử lại sau.");
                
                case System.Net.HttpStatusCode.ServiceUnavailable:
                    throw new InvalidOperationException("Dịch vụ ChatGPT tạm thời không khả dụng. Vui lòng thử lại sau.");
            }

            res.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            
            if (!root.TryGetProperty("choices", out var choicesEl) || choicesEl.GetArrayLength() == 0)
            {
                throw new KeyNotFoundException("Không tìm thấy 'choices' trong phản hồi ChatGPT");
            }

            var firstChoice = choicesEl[0];
            if (!firstChoice.TryGetProperty("message", out var messageEl))
            {
                throw new KeyNotFoundException("Không tìm thấy 'message' trong choice");
            }

            if (!messageEl.TryGetProperty("content", out var contentEl))
            {
                throw new KeyNotFoundException("Không tìm thấy 'content' trong message");
            }

            var result = contentEl.GetString() ?? "";
            Debug.WriteLine($"[CallChatGptApiAsync] Result length: {result.Length}");
            return result;
        }
    }
}