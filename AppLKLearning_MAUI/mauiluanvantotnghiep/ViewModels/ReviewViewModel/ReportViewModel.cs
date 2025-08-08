using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mauiluanvantotnghiep.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.ViewModels.ReviewViewModel
{
    public partial class ReportViewModel : ObservableObject
    {
        [ObservableProperty]
        private int courseId;

        [ObservableProperty]
        private int lessonId;

        [ObservableProperty]
        private string comment = string.Empty;

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private bool canSubmit = false;

        [ObservableProperty]
        private ObservableCollection<Review> reports = new ObservableCollection<Review>();

        public ReportViewModel()
        {
            // Watch for comment changes to enable/disable submit button
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Comment))
                {
                    CanSubmit = !string.IsNullOrWhiteSpace(Comment) && !IsLoading;
                }
            };
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            if (IsLoading) return;

            // Clear comment and go back
            Comment = string.Empty;
            await Shell.Current.GoToAsync("..");
        }

        //load list report
        public async Task LoadReportsAsync()
        {
            if (IsLoading) return;
            
            try
            {
                IsLoading = true;
                
                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await ShowErrorAsync("Lỗi xác thực", "Bạn chưa đăng nhập. Vui lòng đăng nhập lại.");
                    return;
                }

                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        (req, cert, chain, errors) => true
                };
                using var client = new HttpClient(handler);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var url = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyReview/GetUserReportsByCourseAndLesson"
                          + $"?courseId={CourseId}&lessonId={LessonId}";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var list = JsonSerializer.Deserialize<Review[]>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    Reports.Clear();
                    if (list != null)
                    {
                        foreach (var r in list.OrderByDescending(x => x.CreatedAt))
                            Reports.Add(r);
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Reports.Clear();
                }
                else
                {
                    await ShowErrorAsync("Lỗi tải dữ liệu", "Không thể tải lịch sử báo cáo. Vui lòng thử lại.");
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Lỗi tải báo cáo", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SubmitReportAsync()
        {
            if (IsLoading || !CanSubmit) return;

            if (string.IsNullOrWhiteSpace(Comment))
            {
                await ShowErrorAsync("Thiếu thông tin", "Vui lòng nhập nội dung góp ý trước khi gửi.");
                return;
            }

            try
            {
                IsLoading = true;
                CanSubmit = false;

                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await ShowErrorAsync("Lỗi xác thực", "Bạn chưa đăng nhập. Vui lòng đăng nhập lại.");
                    return;
                }

                var request = new Review
                {
                    CourseId = CourseId,
                    LessonId = LessonId,
                    Comment = Comment.Trim(),
                    ReviewType = "2"
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        (req, cert, chain, errors) => true
                };

                using var client = new HttpClient(handler);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var url = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyReview/CreateReportLessonforuser";
                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    await ShowSuccessAsync("Thành công", "Báo cáo của bạn đã được gửi thành công. Cảm ơn bạn đã góp ý!");
                    
                    // Clear comment and reload reports
                    Comment = string.Empty;
                    await LoadReportsAsync();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    await ShowErrorAsync("Lỗi gửi báo cáo", 
                        string.IsNullOrEmpty(errorContent) ? "Không thể gửi báo cáo. Vui lòng thử lại." : errorContent);
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Lỗi kết nối", $"Đã xảy ra lỗi: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                CanSubmit = !string.IsNullOrWhiteSpace(Comment);
            }
        }

        private async Task ShowSuccessAsync(string title, string message)
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(title, message, "OK");
            }
        }

        private async Task ShowErrorAsync(string title, string message)
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(title, message, "OK");
            }
        }
    }
}
