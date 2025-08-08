using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mauiluanvantotnghiep.Models;
using mauiluanvantotnghiep.ViewModels.AppConfig;
using Microsoft.Maui.Storage;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace mauiluanvantotnghiep.ViewModels
{
    public partial class CourseDetailViewModel : ObservableObject
    {
        private readonly HttpClient _httpClient;

        // Thuộc tính Course để binding giao diện chi tiết
        [ObservableProperty]
        private Course course;

        // Thuộc tính đánh dấu trạng thái mở rộng mô tả
        [ObservableProperty]
        private bool isDescriptionExpanded;

        // Computed property: trả về số dòng tối đa cho Label mô tả
        public int DescriptionMaxLines => IsDescriptionExpanded ? int.MaxValue : 3;

        // Computed property: hiển thị text của nút "Xem thêm" hoặc "Ẩn bớt"
        public string ReadMoreText => IsDescriptionExpanded ? "Ẩn bớt" : "Xem thêm";

        // ID của khóa học
        public int CourseId { get; set; }

        // Danh sách bài học sẽ binding lên ListView trong giao diện
        public ObservableCollection<Lesson> Lessons { get; } = new ObservableCollection<Lesson>();


        // Danh sách nhận xét sẽ binding lên ListView trong giao diện
        public ObservableCollection<Review> Reviews { get; } = new ObservableCollection<Review>();


        // 1) Trung bình điểm (làm tròn 1 chữ số)
        public double AverageRating
            => Reviews.Count > 0
               ? Math.Round(Reviews.Average(r => r.Rating), 1)
               : 0;

        // 2) Tổng số nhận xét
        public int TotalReviews => Reviews.Count;
        public CourseDetailViewModel()
        {
            // Khởi tạo HttpClient với handler cho HTTPS (chỉ dùng trong môi trường DEV với chứng chỉ tự ký)
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
            // Khi Reviews có thay đổi (thêm/xóa), raise PropertyChanged
            Reviews.CollectionChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(AverageRating));
                OnPropertyChanged(nameof(TotalReviews));
            };
        }

        private bool isLoaded = false; // Thêm biến này

        public async Task LoadCourseDetailAsync(int courseId)
        {
            if (isLoaded && CourseId == courseId)
                return; // Đã load rồi, không load lại

            CourseId = courseId;
            isLoaded = true;

            // Lấy token từ SecureStorage
            string token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
            {
                // Xử lý khi không có token: thông báo lỗi hay chuyển hướng đăng nhập
                return;
            }

            // Đặt token vào Authorization header
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Gọi API lấy chi tiết khóa học
            string courseUrl = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyKhoaHoc/GetCourseID?id={courseId}";
            var response = await _httpClient.GetAsync(courseUrl);
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                Course = JsonSerializer.Deserialize<Course>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            }
            else
            {
                // Xử lý lỗi nếu cần (ví dụ: log, thông báo cho người dùng, ...)
            }

            // Sau khi load chi tiết khóa học, load danh sách bài học tương ứng
            await LoadLessonsAsync();
        }

        public async Task LoadLessonsAsync()
        {
            // Endpoint trả về danh sách CourseLessonOverview có chứa lessonId và orderNo
            string lessonListUrl = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyBaiHoc/GetCourseLessonByID/{CourseId}";
            var response = await _httpClient.GetAsync(lessonListUrl);
            if (!response.IsSuccessStatusCode)
            {
                // Xử lý lỗi nếu cần
                return;
            }

            string jsonList = await response.Content.ReadAsStringAsync();
            var courseLessonOverviews = JsonSerializer.Deserialize<List<CourseLesson>>(
                jsonList,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (courseLessonOverviews == null || courseLessonOverviews.Count == 0)
                return;

            // Sắp xếp theo thứ tự bài học (orderNo)
            var sortedOverviews = courseLessonOverviews.OrderBy(o => o.OrderNo).ToList();

            Lessons.Clear();

            // Với mỗi overview, gọi endpoint để lấy thông tin chi tiết bài học
            foreach (var overview in sortedOverviews)
            {
                string lessonUrl = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyBaiHoc/GetLessonById/{overview.LessonId}";
                var lessonResponse = await _httpClient.GetAsync(lessonUrl);
                if (lessonResponse.IsSuccessStatusCode)
                {
                    string lessonJson = await lessonResponse.Content.ReadAsStringAsync();
                    var lesson = JsonSerializer.Deserialize<Lesson>(
                        lessonJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                    if (lesson != null)
                    {
                        // Gán OrderNo từ overview cho lesson
                        lesson.OrderNo = overview.OrderNo;
                        Lessons.Add(lesson);
                    }
                }
                // Có thể thêm một khoảng delay nhỏ nếu cần tránh gọi API quá nhanh
            }
        }

        [RelayCommand]
        public async Task StartLesson()
        {
            // 1. Lấy token
            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Lỗi", "Bạn cần đăng nhập để tiếp tục.", "OK");
                return;
            }

            // 2. Gọi API lấy danh sách đăng ký gói của user
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var regUrl = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyDangKyGoiCuoc/GetUserPackageRegistrations";
            var regResp = await _httpClient.GetAsync(regUrl);
            if (!regResp.IsSuccessStatusCode)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Lỗi", "Không lấy được thông tin đăng ký.", "OK");
                return;
            }

            var regJson = await regResp.Content.ReadAsStringAsync();
            var regs = JsonSerializer.Deserialize<List<UserPackageRegistration>>(
                regJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // 3. Kiểm tra xem user đã đăng ký gói của khóa này chưa
            // Giả sử Course có property PackageId
            bool hasSub = regs.Any(r => r.PackageId == Course.PackageId);
            if (!hasSub)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Chưa đăng ký",
                    "Bạn chưa đăng ký gói này. Vui lòng đăng ký để bắt đầu khóa học.",
                    "OK");
                return;
            }
            // 4. Điều hướng sang trang lesson
            // Ví dụ dùng Shell: truyền courseId qua query
            await Shell.Current.GoToAsync($"courselessonpage?courseId={CourseId}");
        }

        // Command dùng chuyển đổi trạng thái mở rộng của phần mô tả
        [RelayCommand]
        private void ToggleDescriptionExpansion()
        {
            IsDescriptionExpanded = !IsDescriptionExpanded;
            // Thông báo cho các thuộc tính phụ thuộc thay đổi để UI cập nhật lại:
            OnPropertyChanged(nameof(DescriptionMaxLines));
            OnPropertyChanged(nameof(ReadMoreText));
        }



        public async Task LoadReviewsAsync()

        {
            // 1. Lấy token
            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Lỗi", "Bạn cần đăng nhập để tiếp tục.", "OK");
                return;
            }

            // 2. Gọi API lấy danh sách đăng ký gói của user
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            string reviewUrl = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyReview/GetReviewsByCourseId/{CourseId}";
            var response = await _httpClient.GetAsync(reviewUrl);
            if (!response.IsSuccessStatusCode)
            {
                // Log lỗi để kiểm tra
                Console.WriteLine($"Lỗi khi tải nhận xét: {response.StatusCode}");
                return;
            }

            string json = await response.Content.ReadAsStringAsync();
            var reviews = JsonSerializer.Deserialize<List<Review>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (reviews == null || reviews.Count == 0)
            {
                Console.WriteLine("Không có nhận xét nào cho khóa học này.");
                return;
            }

            // Sắp xếp và thêm vào Reviews
            Reviews.Clear();
            foreach (var r in reviews.OrderByDescending(x => x.CreatedAt))
                Reviews.Add(r);
            // (Optional) nếu bạn muốn ép raise thêm một lần nữa:
            OnPropertyChanged(nameof(AverageRating));
            OnPropertyChanged(nameof(TotalReviews));
        }


        [RelayCommand]
        public async Task GoToReviewDetail()
        {
            // chuyển shell qua ReviewDetailPage, truyền luôn CourseId
            await Shell.Current.GoToAsync($"reviewdetailpage?courseId={CourseId}");
        }
    }
}