using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mauiluanvantotnghiep.Models;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using System.Threading;
using Microsoft.AspNetCore.SignalR.Client;

namespace mauiluanvantotnghiep.ViewModels
{
    public partial class CoursePageViewModel : ObservableObject
    {
        // Dữ liệu gốc chứa các package kèm danh sách khóa học
        public ObservableCollection<PackageWithCourses> PackagesWithCourses { get; set; }
            = new ObservableCollection<PackageWithCourses>();

        // Collection hiển thị (được lọc theo từ khóa tìm kiếm)
        [ObservableProperty]
        private ObservableCollection<PackageWithCourses> filteredPackagesWithCourses = new ObservableCollection<PackageWithCourses>();

        // Thuộc tính chứa từ khóa tìm kiếm. (TwoWay binding với SearchBar)
        [ObservableProperty]
        private string searchText = string.Empty;

        // Danh sách banner
        [ObservableProperty]
        private ObservableCollection<Banner> banners = new ObservableCollection<Banner>();

        // Properties cho auto-scroll carousel
        [ObservableProperty]
        private int currentBannerPosition = 0;

        // Property để kiểm tra có đang search không
        [ObservableProperty]
        private bool isSearching = false;

        // Property để ẩn/hiện banner khi search
        [ObservableProperty]
        private bool showBanner = true;

        // Timer cho auto-scroll
        private System.Threading.Timer _carouselTimer;
        private readonly object _timerLock = new object();

        // Debouncing for search
        private CancellationTokenSource _searchCancellationTokenSource;
        private readonly SemaphoreSlim _searchSemaphore = new SemaphoreSlim(1, 1);
        private const int SEARCH_DELAY_MS = 300; // 300ms debounce

        // Event để thông báo scroll lên đầu
        public event EventHandler ScrollToTopRequested;

        private HubConnection _hubConnection;

        partial void OnSearchTextChanged(string value)
        {
            // Cancel previous search operation
            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource = new CancellationTokenSource();

            // Start debounced search
            _ = Task.Run(() => PerformDebouncedSearchAsync(value, _searchCancellationTokenSource.Token));
        }

        private async Task PerformDebouncedSearchAsync(string searchValue, CancellationToken cancellationToken)
        {
            try
            {
                // Wait for debounce delay
                await Task.Delay(SEARCH_DELAY_MS, cancellationToken);

                // Check if cancelled during delay
                if (cancellationToken.IsCancellationRequested)
                    return;

                // Acquire semaphore to prevent concurrent searches
                await _searchSemaphore.WaitAsync(cancellationToken);

                try
                {
                    await ExecuteSearchAsync(searchValue, cancellationToken);
                }
                finally
                {
                    _searchSemaphore.Release();
                }
            }
            catch (OperationCanceledException)
            {
                // Search was cancelled - this is expected
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Search error: {ex.Message}");
            }
        }

        private async Task ExecuteSearchAsync(string searchValue, CancellationToken cancellationToken)
        {
            // Update UI state first
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                IsSearching = !string.IsNullOrWhiteSpace(searchValue);
                ShowBanner = !IsSearching;

                if (IsSearching)
                {
                    PauseAutoScroll();
                }
                else
                {
                    ResumeAutoScroll();
                }
            });

            if (string.IsNullOrWhiteSpace(searchValue))
            {
                // Reset to original data
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    FilteredPackagesWithCourses.Clear();
                    foreach (var item in PackagesWithCourses)
                    {
                        FilteredPackagesWithCourses.Add(item);
                    }
                });
                return;
            }

            // Perform filtering on background thread
            var filteredResults = await Task.Run(async () =>
            {
                var normalizedSearch = searchValue.Trim().ToLowerInvariant();
                var results = new List<PackageWithCourses>();

                foreach (var package in PackagesWithCourses)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    // Filter courses for this package
                    var filteredCourses = package.Courses
                        .Where(c => c.CourseName?.ToLowerInvariant().Contains(normalizedSearch) == true)
                        .ToList();

                    if (filteredCourses.Any())
                    {
                        results.Add(new PackageWithCourses
                        {
                            Package = package.Package,
                            Courses = filteredCourses
                        });
                    }

                    // Small delay to prevent blocking
                    if (results.Count % 10 == 0)
                    {
                        await Task.Delay(1, cancellationToken);
                    }
                }

                return results;
            }, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return;

            // Update UI on main thread
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                FilteredPackagesWithCourses.Clear();
                foreach (var item in filteredResults)
                {
                    FilteredPackagesWithCourses.Add(item);
                }

                // Scroll to top after updating results
                if (IsSearching)
                {
                    ScrollToTopRequested?.Invoke(this, EventArgs.Empty);
                }
            });
        }

        // Partial method được gọi khi Banners thay đổi
        partial void OnBannersChanged(ObservableCollection<Banner> value)
        {
            // Bắt đầu auto-scroll khi có banner data và không đang search
            if (value != null && value.Count > 1 && !IsSearching)
            {
                StartCarouselAutoScroll();
            }
        }

        // COMMANDS CỦA BẠN
        public ICommand CourseSelectedCommand { get; }
        public ICommand ShowAllCoursesCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand NavigateToChatCommand { get; }

        private readonly HttpClient _httpClient;

        public CoursePageViewModel()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);

            // Khởi tạo command của bạn
            CourseSelectedCommand = new Command<int>(async (courseId) => await OnCourseSelected(courseId));
            ShowAllCoursesCommand = new Command<int>(async (packageId) => await OnShowAllCourses(packageId));
            SearchCommand = new Command(OnSearchExecuted);
            NavigateToChatCommand = new Command(async () => await NavigateToChatAsync());

            // Load dữ liệu ban đầu
            LoadData();
        }

        private void OnSearchExecuted()
        {
            // Trigger immediate search without debounce when search button is pressed
            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource = new CancellationTokenSource();
            
            _ = Task.Run(() => ExecuteSearchAsync(SearchText, _searchCancellationTokenSource.Token));
        }

        private async Task OnCourseSelected(int courseId)
        {
            // Điều hướng sang trang detail, truyền tham số courseId
            await Shell.Current.GoToAsync($"coursesdetailpage?courseId={courseId}");
        }

        private async Task OnShowAllCourses(int packageId)
        {
            // Điều hướng sang trang hiển thị tất cả khóa học của package
            await Shell.Current.GoToAsync($"allcoursespage?packageId={packageId}");
        }

        private async Task NavigateToChatAsync()
        {
            try
            {
                // Kiểm tra token trước khi chuyển trang
                string token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await Shell.Current.DisplayAlert("Lỗi", "Bạn cần đăng nhập để sử dụng tính năng chat.", "OK");
                    return;
                }

                // Điều hướng đến trang chat
                await Shell.Current.GoToAsync($"chatdashboardpage");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Không thể mở trang chat: {ex.Message}", "OK");
            }
        }
            
        private async void LoadData()
        {
            try
            {
                string token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                    return;

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Load danh sách banner
                string bannersUrl = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyBanner/GetListBanners";
                var bannersResponse = await _httpClient.GetAsync(bannersUrl);
                if (bannersResponse.IsSuccessStatusCode)
                {
                    string bannersJson = await bannersResponse.Content.ReadAsStringAsync();
                    var optionsbanner = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var bannerList = JsonSerializer.Deserialize<List<Banner>>(bannersJson, optionsbanner) ?? new List<Banner>();
                    Banners = new ObservableCollection<Banner>(bannerList.Where(b => b.IsActive)); // Chỉ lấy banner active
                }

                // API lấy danh sách các package
                string packagesUrl = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyGoiCuoc/GetListPackages";
                var packagesResponse = await _httpClient.GetAsync(packagesUrl);
                if (!packagesResponse.IsSuccessStatusCode)
                    return;
                string packagesJson = await packagesResponse.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var packages = JsonSerializer.Deserialize<List<Package>>(packagesJson, options);

                if (packages != null)
                {
                    foreach (var package in packages)
                    {
                        string coursesUrl = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyGoiCuoc/GetCoursesByPackage/{package.PackageId}";
                        var coursesResponse = await _httpClient.GetAsync(coursesUrl);
                        List<Course> coursesList = new List<Course>();

                        if (coursesResponse.IsSuccessStatusCode)
                        {
                            string coursesJson = await coursesResponse.Content.ReadAsStringAsync();
                            coursesList = JsonSerializer.Deserialize<List<Course>>(coursesJson, options) ?? new List<Course>();
                        }

                        PackagesWithCourses.Add(new PackageWithCourses
                        {
                            Package = package,
                            Courses = coursesList
                        });
                    }
                }

                // Ban đầu, danh sách hiển thị là toàn bộ dữ liệu
                FilteredPackagesWithCourses = new ObservableCollection<PackageWithCourses>(PackagesWithCourses);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", ex.Message, "OK");
            }
        }

        // Methods cho auto-scroll carousel
        public void StartCarouselAutoScroll()
        {
            lock (_timerLock)
            {
                if (_carouselTimer != null || Banners == null || Banners.Count <= 1 || IsSearching)
                    return;

                _carouselTimer = new System.Threading.Timer(OnCarouselTimerTick, null,
                    TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(4)); // Chuyển mỗi 4 giây
            }
        }

        public void StopCarouselAutoScroll()
        {
            lock (_timerLock)
            {
                _carouselTimer?.Dispose();
                _carouselTimer = null;
            }
        }

        private void OnCarouselTimerTick(object state)
        {
            try
            {
                if (Banners == null || Banners.Count <= 1 || IsSearching)
                    return;

                // Update position trên main thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    CurrentBannerPosition = (CurrentBannerPosition + 1) % Banners.Count;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Carousel timer error: {ex.Message}");
            }
        }

        // Pause auto-scroll khi user tương tác
        public void PauseAutoScroll()
        {
            StopCarouselAutoScroll();
        }

        // Resume auto-scroll sau khi user ngừng tương tác
        public void ResumeAutoScroll()
        {
            if (Banners != null && Banners.Count > 1 && !IsSearching)
            {
                // Delay một chút trước khi resume
                Task.Delay(2000).ContinueWith(_ => StartCarouselAutoScroll());
            }
        }

        // ✅ THÊM RELAYCOMMANDS CHO UTILITY BUTTONS
        [RelayCommand]
        private async Task NavigateToStoryAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("storypage");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Không thể mở trang câu chuyện: {ex.Message}", "OK");
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task NavigateToProfileAsync()
        {
            try
            {
                await Shell.Current.GoToAsync("userprofilepage");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Không thể mở trang hồ sơ: {ex.Message}", "OK");
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task NavigateToChatUtilityAsync()
        {
            try
            {
                // Kiểm tra token trước khi chuyển trang
                string token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await Shell.Current.DisplayAlert("Lỗi", "Bạn cần đăng nhập để sử dụng tính năng chat.", "OK");
                    return;
                }

                await Shell.Current.GoToAsync("chatdashboardpage");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Không thể mở trang chat: {ex.Message}", "OK");
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task NavigateToCertificatesAsync()
        {
            try
            {
                // Kiểm tra token trước khi chuyển trang
                string token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await Shell.Current.DisplayAlert("Lỗi", "Bạn cần đăng nhập để xem chứng chỉ.", "OK");
                    return;
                }

                // Điều hướng đến trang chứng chỉ
                await Shell.Current.GoToAsync("certificatespage");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Không thể mở trang chứng chỉ: {ex.Message}", "OK");
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        // Cleanup khi ViewModel bị dispose
        public void Dispose()
        {
            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource?.Dispose();
            _searchSemaphore?.Dispose();
            StopCarouselAutoScroll();
            _httpClient?.Dispose();
        }

        // Finalizer
        ~CoursePageViewModel()
        {
            Dispose();
        }
    }
}