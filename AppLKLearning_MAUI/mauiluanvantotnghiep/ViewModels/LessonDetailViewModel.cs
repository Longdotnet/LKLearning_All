using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mauiluanvantotnghiep.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.ViewModels
{
    public partial class LessonDetailViewModel : ObservableObject
    {
        private readonly HttpClient _http;

        public LessonDetailViewModel()
        {
            _http = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
            });
        }

        [ObservableProperty]
        private int lessonId;

        [ObservableProperty]
        private int courseId;

        [ObservableProperty]
        private string lessonTitle;

        [ObservableProperty]
        private string lessonContent;

        [ObservableProperty]
        private string lessonDescription;

        [ObservableProperty]
        private string urlImageLesson;



        public async Task LoadLessonsAsync(int id)
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrWhiteSpace(token))
            {
                await Shell.Current.DisplayAlert("Lỗi", "Không tìm thấy token đăng nhập.", "OK");
                return;
            }

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var url = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyBaiHoc/GetLessonById/{id}";
            var resp = await _http.GetAsync(url);
            var body = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
            {
                var lesson = JsonSerializer.Deserialize<Lesson>(
                    body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (lesson != null)
                {
                    LessonTitle = lesson.LessonTitle;
                    LessonContent = lesson.LessonContent;
                    LessonDescription = lesson.LessonDescription;
                    UrlImageLesson = lesson.UrlImageLesson;
                }
            }
            else
            {
                await Shell.Current.DisplayAlert(
                    "Lỗi",
                    $"Không tải được bài học: {(int)resp.StatusCode}",
                    "OK");
            }
        }


        [RelayCommand]
        async Task GoToTestAsync()
        {
            await Shell.Current.GoToAsync($"questionpage?lessonId={lessonId}&courseId={courseId}");
        }
    }
}
