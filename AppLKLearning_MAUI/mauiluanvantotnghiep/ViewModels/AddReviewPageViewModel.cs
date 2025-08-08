using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mauiluanvantotnghiep.Models;
using Microsoft.Maui.Storage;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.ViewModels
{
    public partial class AddReviewPageViewModel : ObservableObject
    {
        private readonly HttpClient _http;

        public AddReviewPageViewModel()
        {
            _http = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
            });
            UpdateStarImages();
        }

        [ObservableProperty]
        int courseId;

        [ObservableProperty]
        int rating = 1;

        [ObservableProperty]
        string comment;

        [ObservableProperty]
        string submitButtonText = "Gửi đánh giá";

        [ObservableProperty]
        string star1Image;
        [ObservableProperty]
        string star2Image;
        [ObservableProperty]
        string star3Image;
        [ObservableProperty]
        string star4Image;
        [ObservableProperty]
        string star5Image;

        public async Task InitializeAsync(int courseId, Review existingReview)
        {
            CourseId = courseId;
            if (existingReview != null)
            {
                Rating = existingReview.Rating;
                Comment = existingReview.Comment;
                SubmitButtonText = "Cập nhật đánh giá";
            }
            else
            {
                Rating = 1;
                Comment = string.Empty;
                SubmitButtonText = "Gửi đánh giá";
            }
            UpdateStarImages();
        }

        [RelayCommand]
        void TapStar(string starValue)
        {
            if (!int.TryParse(starValue, out var star))
                return;

            Rating = star;
            UpdateStarImages();
        }

        void UpdateStarImages()
        {
            const string filled = "startfill.png";
            const string empty = "startempty.png";

            Star1Image = Rating >= 1 ? filled : empty;
            Star2Image = Rating >= 2 ? filled : empty;
            Star3Image = Rating >= 3 ? filled : empty;
            Star4Image = Rating >= 4 ? filled : empty;
            Star5Image = Rating >= 5 ? filled : empty;
        }

        [RelayCommand]
        async Task SubmitReviewAsync()
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrWhiteSpace(token))
            {
                await Shell.Current.DisplayAlert("Lỗi", "Không tìm thấy token đăng nhập.", "OK");
                return;
            }

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var dto = new
            {
                CourseId = CourseId,
                Rating = Rating,
                Comment = Comment?.Trim(),
                ReviewType = "1"
            };

            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var url = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyReview/CreateReviewCourseIDforuser";

            var resp = await _http.PostAsync(url, content);
            var body = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
            {
                await Shell.Current.DisplayAlert(
                    "Thành công",
                    SubmitButtonText == "Cập nhật đánh giá" ? "Đánh giá của bạn đã được cập nhật." : "Đánh giá của bạn đã được ghi nhận.",
                    "OK");
                await Shell.Current.Navigation.PopAsync();
            }
            else
            {
                await Shell.Current.DisplayAlert(
                    "Lỗi",
                    $"Không gửi được đánh giá: {(int)resp.StatusCode} {body}",
                    "OK");
            }
        }
    }
}