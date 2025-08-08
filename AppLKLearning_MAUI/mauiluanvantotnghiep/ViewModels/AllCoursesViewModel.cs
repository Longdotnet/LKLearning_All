using CommunityToolkit.Mvvm.ComponentModel;
using mauiluanvantotnghiep.Models;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Threading;
using System.Diagnostics;

namespace mauiluanvantotnghiep.ViewModels
{
    public partial class AllCoursesViewModel : ObservableObject
    {
        private readonly HttpClient _client;

        // Original courses collection
        private List<Course> _allCourses = new();

        [ObservableProperty]
        private ObservableCollection<Course> courses = new();

        // Search functionality
        [ObservableProperty]
        private string searchText = string.Empty;

        // Loading and refresh states
        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private bool isRefreshing = false;

        // Package information
        [ObservableProperty]
        private string packageName = "Tất cả khóa học";

        [ObservableProperty]
        private string pageTitle = "Khóa Học";

        // Statistics
        [ObservableProperty]
        private int totalCourses = 0;

        // Explicit boolean properties thay vì computed properties
        [ObservableProperty]
        private bool hasCourses = false;

        [ObservableProperty]
        private bool showEmptyState = false;

        [ObservableProperty]
        private bool hasError = false;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        // Course count text property
        [ObservableProperty]
        private string courseCountText = "0 khóa học";

        // Search debouncing
        private CancellationTokenSource _searchCancellationTokenSource;
        private const int SEARCH_DELAY_MS = 300;

        // Commands
        public ICommand CourseSelectedCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ClearSearchCommand { get; }

        public AllCoursesViewModel()
        {
            // Bypass SSL tự ký (DEV)
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true
            };
            _client = new HttpClient(handler);

            // Initialize commands
            CourseSelectedCommand = new Command<int>(async (courseId) => await OnCourseSelected(courseId));
            SearchCommand = new Command(OnSearchExecuted);
            RefreshCommand = new Command(async () => await OnRefresh());
            ClearSearchCommand = new Command(OnClearSearch);

            Debug.WriteLine("[AllCoursesViewModel] Initialized");
        }

        partial void OnSearchTextChanged(string value)
        {
            Debug.WriteLine($"[AllCoursesViewModel] Search text changed: '{value}'");
            
            // Cancel previous search   
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

                if (cancellationToken.IsCancellationRequested)
                    return;

                await MainThread.InvokeOnMainThreadAsync(() => FilterCourses(searchValue));
            }
            catch (OperationCanceledException)
            {
                // Expected when search is cancelled
            }
        }

        private void FilterCourses(string searchValue = null)
        {
            var search = searchValue ?? SearchText;
            Debug.WriteLine($"[AllCoursesViewModel] Filtering courses with: '{search}', Total courses: {_allCourses.Count}");

            if (string.IsNullOrWhiteSpace(search))
            {
                Courses = new ObservableCollection<Course>(_allCourses);
            }
            else
            {
                var normalizedSearch = search.Trim().ToLowerInvariant();
                var filtered = _allCourses.Where(c =>
                    c.CourseName?.ToLowerInvariant().Contains(normalizedSearch) == true ||
                    c.Description?.ToLowerInvariant().Contains(normalizedSearch) == true)
                    .ToList();

                Courses = new ObservableCollection<Course>(filtered);
            }

            UpdateComputedProperties();
            Debug.WriteLine($"[AllCoursesViewModel] Filtered result: {Courses.Count} courses");
        }

        private void UpdateComputedProperties()
        {
            // UPDATE: Không check IsLoading ở đây để courses hiển thị ngay
            HasCourses = Courses.Any();
            ShowEmptyState = !IsLoading && !HasCourses && !HasError;
            CourseCountText = $"{Courses.Count}/{TotalCourses} khóa học";
            
            Debug.WriteLine($"[AllCoursesViewModel] Updated properties - HasCourses: {HasCourses}, ShowEmptyState: {ShowEmptyState}, CourseCountText: {CourseCountText}");
        }

        private async Task OnCourseSelected(int courseId)
        {
            Debug.WriteLine($"[AllCoursesViewModel] Course selected: {courseId}");
            await Shell.Current.GoToAsync($"coursesdetailpage?courseId={courseId}");
        }

        private void OnSearchExecuted()
        {
            Debug.WriteLine("[AllCoursesViewModel] Search executed");
            FilterCourses();
        }

        private void OnClearSearch()
        {
            Debug.WriteLine("[AllCoursesViewModel] Clear search");
            SearchText = string.Empty;
        }

        private async Task OnRefresh()
        {
            Debug.WriteLine("[AllCoursesViewModel] Refresh started");
            IsRefreshing = true;
            try
            {
                // Reload current package data
                if (_currentPackageId > 0)
                {
                    await LoadAsync(_currentPackageId);
                }
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private int _currentPackageId;

        /// <summary>
        /// Gọi API lấy danh sách tất cả khóa học theo packageId
        /// </summary>
        public async Task LoadAsync(int packageId)
        {
            Debug.WriteLine($"[AllCoursesViewModel] LoadAsync started for packageId: {packageId}");
            
            try
            {
                IsLoading = true;
                HasError = false;
                _currentPackageId = packageId;

                // 1. Lấy token
                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrWhiteSpace(token))
                {
                    HasError = true;
                    ErrorMessage = "Không tìm thấy token xác thực";
                    Debug.WriteLine("[AllCoursesViewModel] No auth token found");
                    return;
                }

                // 2. Thêm header Bearer
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // 3. Load package info first
                await LoadPackageInfo(packageId);

                // 4. Gọi API lấy courses
                string url = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyGoiCuoc/GetCoursesByPackage/{packageId}";
                Debug.WriteLine($"[AllCoursesViewModel] Calling API: {url}");
                
                var resp = await _client.GetAsync(url);
                
                if (!resp.IsSuccessStatusCode)
                {
                    HasError = true;
                    ErrorMessage = $"Lỗi tải dữ liệu: {resp.StatusCode}";
                    Debug.WriteLine($"[AllCoursesViewModel] API failed with status: {resp.StatusCode}");
                    return;
                }

                // 5. Đọc và deserialize
                var json = await resp.Content.ReadAsStringAsync();
                Debug.WriteLine($"[AllCoursesViewModel] API response: {json}");
                
                var list = JsonSerializer.Deserialize<List<Course>>(json,
                             new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // 6. Gán vào collections - QUAN TRỌNG: Set courses trước khi tắt loading
                _allCourses = list ?? new List<Course>();
                TotalCourses = _allCourses.Count;
                Courses = new ObservableCollection<Course>(_allCourses);

                Debug.WriteLine($"[AllCoursesViewModel] Loaded {_allCourses.Count} courses");

                // 7. Update page title
                PageTitle = $"{PackageName} ({TotalCourses})";

                // 8. Update computed properties TRƯỚC KHI tắt loading
                UpdateComputedProperties();
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Lỗi: {ex.Message}";
                Debug.WriteLine($"[AllCoursesViewModel] Exception: {ex.Message}");
                UpdateComputedProperties(); // Update để hiển thị error state
            }
            finally
            {
                // 9. Tắt loading SAU KHI đã update UI
                IsLoading = false;
                Debug.WriteLine($"[AllCoursesViewModel] LoadAsync finished. Final state - HasCourses: {HasCourses}, TotalCourses: {TotalCourses}, Courses.Count: {Courses.Count}");
            }
        }

        private async Task LoadPackageInfo(int packageId)
        {
            try
            {
                string url = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyGoiCuoc/GetPackage/{packageId}";
                var resp = await _client.GetAsync(url);

                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    var package = JsonSerializer.Deserialize<Package>(json,
                                 new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (package != null)
                    {
                        PackageName = package.PackageName ?? "Gói khóa học";
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AllCoursesViewModel] LoadPackageInfo error: {ex.Message}");
                PackageName = "Gói khóa học";
            }
        }

        public void Dispose()
        {
            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource?.Dispose();
            _client?.Dispose();
        }
    }
}
