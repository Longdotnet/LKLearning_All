using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mauiluanvantotnghiep.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;

namespace mauiluanvantotnghiep.ViewModels.VocabularyViewModel
{
    [QueryProperty(nameof(CategoryId), "categoryId")]
    [QueryProperty(nameof(CategoryName), "categoryName")]
    public partial class CategoryVocabularyListViewModel : ObservableObject
    {
        public ObservableCollection<Vocabulary> Vocabularies { get; } = new();

        [ObservableProperty]
        private int categoryId;

        [ObservableProperty]
        private string categoryName = "Danh sách từ vựng";

        [ObservableProperty]
        private string searchText;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool isRefreshing;

        [ObservableProperty]
        private bool isLoadingMore;

        [ObservableProperty]
        private string errorMessage;

        [ObservableProperty]
        private bool hasError;

        [ObservableProperty]
        private bool hasMoreItems = true;

        [ObservableProperty]
        private bool isSearching;

        private int _currentPage = 1;
        private const int _pageSize = 20;
        private List<int> _allVocabularyIds = new();
        private List<Vocabulary> _allVocabularies = new(); // Store all loaded vocabularies for search

        public CategoryVocabularyListViewModel()
        {
        }

        partial void OnCategoryIdChanged(int value)
        {
            if (value > 0)
            {
                _ = LoadInitialDataAsync();
            }
        }

        // Handle search text changes with debouncing
        partial void OnSearchTextChanged(string value)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(300); // Debounce delay
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    if (SearchText == value) // Only search if text hasn't changed
                    {
                        await PerformSearchAsync();
                    }
                });
            });
        }

        [RelayCommand]
        private async Task LoadInitialDataAsync()
        {
            if (IsLoading || CategoryId <= 0) return;

            try
            {
                IsLoading = true;
                HasError = false;
                _currentPage = 1;
                HasMoreItems = true;

                await LoadVocabularyIdsAsync();
                await LoadAllVocabulariesAsync(); // Load all vocabularies for search
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoadInitialData] Exception: {ex}");
                HasError = true;
                ErrorMessage = $"Lỗi tải dữ liệu: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            if (IsRefreshing) return;

            try
            {
                IsRefreshing = true;
                HasError = false;
                _currentPage = 1;
                HasMoreItems = true;
                Vocabularies.Clear();
                _allVocabularies.Clear();
                SearchText = string.Empty;

                await LoadVocabularyIdsAsync();
                await LoadAllVocabulariesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Refresh] Exception: {ex}");
                HasError = true;
                ErrorMessage = $"Lỗi làm mới: {ex.Message}";
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private async Task LoadMoreAsync()
        {
            if (IsLoadingMore || !HasMoreItems || !string.IsNullOrWhiteSpace(SearchText)) return;

            try
            {
                IsLoadingMore = true;
                _currentPage++;
                await LoadVocabulariesPageAsync(_currentPage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoadMore] Exception: {ex}");
                _currentPage--; // Rollback page number
            }
            finally
            {
                IsLoadingMore = false;
            }
        }

        private async Task LoadVocabularyIdsAsync()
        {
            using var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true
            };
            using var client = new HttpClient(handler);

            var mappingUrl = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyVocabulary/GetListVocabularyCategoryMappingByCategoryId/{CategoryId}";
            Debug.WriteLine($"[LoadVocabularyIds] URL: {mappingUrl}");

            var response = await client.GetAsync(mappingUrl);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var mappings = JsonSerializer.Deserialize<VocabularyCategoryMapping[]>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? Array.Empty<VocabularyCategoryMapping>();

            _allVocabularyIds = mappings.Select(m => m.VocabularyId).ToList();
            Debug.WriteLine($"[LoadVocabularyIds] Found {_allVocabularyIds.Count} vocabulary IDs");
        }

        private async Task LoadAllVocabulariesAsync()
        {
            using var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true
            };
            using var client = new HttpClient(handler);

            _allVocabularies.Clear();

            foreach (var vocabId in _allVocabularyIds)
            {
                try
                {
                    var vocabUrl = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyVocabulary/GetVocabularyById/{vocabId}";
                    var response = await client.GetAsync(vocabUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var vocabulary = JsonSerializer.Deserialize<Vocabulary>(
                            json,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        );

                        if (vocabulary != null)
                        {
                            _allVocabularies.Add(vocabulary);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[LoadAllVocabularies] Error loading vocabulary {vocabId}: {ex.Message}");
                }
            }

            Debug.WriteLine($"[LoadAllVocabularies] Loaded {_allVocabularies.Count} vocabularies");

            // Display first page
            await LoadVocabulariesPageAsync(1);
        }

        private async Task LoadVocabulariesPageAsync(int page)
        {
            var startIndex = (page - 1) * _pageSize;
            var vocabulariesToShow = _allVocabularies.Skip(startIndex).Take(_pageSize).ToList();

            if (page == 1)
            {
                Vocabularies.Clear();
            }

                // Add to UI
            foreach (var vocab in vocabulariesToShow)
            {
                Vocabularies.Add(vocab);
            }

            // Check if we have more items
            HasMoreItems = startIndex + _pageSize < _allVocabularies.Count;

            Debug.WriteLine($"[LoadVocabulariesPage] Page {page}: Loaded {vocabulariesToShow.Count} items. HasMore: {HasMoreItems}");
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            await PerformSearchAsync();
        }

        private async Task PerformSearchAsync()
        {
            if (IsSearching) return;

            try
            {
                IsSearching = true;
                Debug.WriteLine($"[Search] Searching for: '{SearchText}'");

                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    // Show all vocabularies with pagination
                    _currentPage = 1;
                    await LoadVocabulariesPageAsync(1);
                    HasMoreItems = _pageSize < _allVocabularies.Count;
                }
                else
                {
                    // Filter vocabularies based on search text
                    var searchTerm = SearchText.Trim().ToLowerInvariant();
                    var filteredVocabularies = _allVocabularies.Where(v =>
                        (v.Word?.ToLowerInvariant().Contains(searchTerm) == true) ||
                        (v.Pronunciation?.ToLowerInvariant().Contains(searchTerm) == true)
                    ).ToList();

                    Debug.WriteLine($"[Search] Found {filteredVocabularies.Count} matching vocabularies");

                    // Update UI
                    Vocabularies.Clear();
                    foreach (var vocab in filteredVocabularies)
                    {
                        Vocabularies.Add(vocab);
                    }

                    // Disable load more when searching
                    HasMoreItems = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Search] Exception: {ex}");
                ErrorMessage = $"Lỗi tìm kiếm: {ex.Message}";
                HasError = true;
            }
            finally
            {
                IsSearching = false;
            }
        }

        [RelayCommand]
        private async Task VocabularyTappedAsync(Vocabulary vocabulary)
        {
            if (vocabulary?.VocabularyId > 0)
            {
                // SỬA: Dùng relative routing thay vì absolute routing
                await Shell.Current.GoToAsync($"vocabularydetailpage?vocabularyId={vocabulary.VocabularyId}");
            }
        }

        [RelayCommand]
        private async Task BackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
