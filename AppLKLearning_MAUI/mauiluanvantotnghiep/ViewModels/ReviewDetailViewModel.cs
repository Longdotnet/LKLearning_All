using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mauiluanvantotnghiep.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Maui.Storage;
using System;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.ViewModels
{
    public partial class ReviewDetailViewModel : ObservableObject
    {
        private readonly HttpClient _http;
        private int _courseId;

        public ReviewDetailViewModel()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };
            _http = new HttpClient(handler);

            PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(SelectedFilter))
                {
                    ApplyFilter();
                    if (SelectedFilter == ReviewFilter.Mine)
                        _ = LoadReviewStatusAsync();
                }

                if (e.PropertyName == nameof(SelectedFilter) || e.PropertyName == nameof(ReviewStatusMessage))
                {
                    OnPropertyChanged(nameof(IsReviewStatusVisible));
                    OnPropertyChanged(nameof(IsAddReviewVisible));
                    OnPropertyChanged(nameof(AddReviewButtonText));
                }
            };
        }

        public ObservableCollection<Review> AllReviews { get; } = new();
        [ObservableProperty]
        private ObservableCollection<Review> filteredReviews = new();
        [ObservableProperty]
        private Review myReview;
        [ObservableProperty]
        private ReviewFilter selectedFilter = ReviewFilter.All;
        [ObservableProperty]
        private string reviewStatusMessage;
        [ObservableProperty]
        private string addReviewButtonText = "Thêm nhận xét";

        public bool IsReviewStatusVisible => SelectedFilter == ReviewFilter.Mine;

        public bool IsAddReviewVisible
            => SelectedFilter == ReviewFilter.Mine
               && ReviewStatusMessage != "Bạn cần hoàn thành hết các bài học có trong khóa học này.";

        public double AverageRating
            => AllReviews.Any()
               ? Math.Round(AllReviews.Average(r => r.Rating), 1)
               : 0;

        public int TotalReviews => AllReviews.Count;

        [RelayCommand]
        public Task SetFilter(ReviewFilter filter)
        {
            SelectedFilter = filter;
            return Task.CompletedTask;
        }

        [RelayCommand]
        public async Task AddReview()
        {
            var parameters = new Dictionary<string, object>
            {
                { "courseId", _courseId },
                { "existingReview", MyReview }
            };
            await Shell.Current.GoToAsync($"addreviewpage", parameters);
        }

        public async Task LoadAllReviewsAsync(int courseId)
        {
            _courseId = courseId;

            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
                return;

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await _http.GetAsync($"{AppConfig.AppConfig.BaseUrl}/api/QuanLyReview/GetReviewsByCourseId/{courseId}");
            if (!resp.IsSuccessStatusCode)
                return;

            var json = await resp.Content.ReadAsStringAsync();
            var list = JsonSerializer.Deserialize<Review[]>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? Array.Empty<Review>();

            AllReviews.Clear();
            foreach (var r in list.OrderByDescending(r => r.CreatedAt))
                AllReviews.Add(r);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var rawId = jwt.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value
                       ?? jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(rawId, out var uid))
            {
                MyReview = AllReviews.FirstOrDefault(r => r.UserId == uid);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[JWT] parse userId failed, rawId={rawId}");
            }

            OnPropertyChanged(nameof(AverageRating));
            OnPropertyChanged(nameof(TotalReviews));
            ApplyFilter();
        }

        public async Task LoadReviewStatusAsync()
        {
            System.Diagnostics.Debug.WriteLine($"[ReviewDetail] ⇒ LoadReviewStatusAsync start, courseId={_courseId}");

            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
            {
                System.Diagnostics.Debug.WriteLine("[ReviewDetail] Token NULL/empty");
                return;
            }

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var url = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyReview/CheckUserReviewStatus/{_courseId}";
            System.Diagnostics.Debug.WriteLine($"[ReviewDetail] HTTP GET {url}");
            var resp = await _http.GetAsync(url);
            if (!resp.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"[ReviewDetail] Response StatusCode = {resp.StatusCode}");
                return;
            }

            var json = await resp.Content.ReadAsStringAsync();
            ReviewStatusMessage = json;

            AddReviewButtonText = ReviewStatusMessage == "Bạn đã viết nhận xét cho khóa học này. Bạn có thể chỉnh sửa nhận xét."
                ? "Chỉnh sửa nhận xét"
                : "Thêm nhận xét";

            System.Diagnostics.Debug.WriteLine($"[ReviewDetail] ReviewStatusMessage = {ReviewStatusMessage}, AddReviewButtonText = {AddReviewButtonText}");
        }

        private void ApplyFilter()
        {
            var now = DateTime.Now;
            var src = SelectedFilter switch
            {
                ReviewFilter.Newest => AllReviews.Where(r => r.CreatedAt >= now.AddMonths(-1)),
                ReviewFilter.Mine => MyReview != null ? new[] { MyReview } : Array.Empty<Review>(),
                _ => AllReviews
            };
            FilteredReviews = new ObservableCollection<Review>(src);
        }
    }
}