using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mauiluanvantotnghiep.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.ViewModels
{
    public partial class SearchVocabulariesViewModel : ObservableObject
    {
        // Danh sách category
        public ObservableCollection<VocabularyCategory> Categories { get; } = new();

        // Danh sách vocabulary new
        public ObservableCollection<Vocabulary> NewVocabularies { get; } = new();

        // Danh sách từ vựng theo category
        public ObservableCollection<Vocabulary> CategoryVocabularies { get; } = new();

        [ObservableProperty]
        private VocabularyCategory selectedCategory;

        // Danh sách từ vựng tìm kiếm
        public ObservableCollection<Vocabulary> Vocabularies { get; } = new();

        [ObservableProperty]
        private string searchText;

        [ObservableProperty]
        private bool isLoadingCategories;

        [ObservableProperty]
        private bool isLoadingNewWords;

        [ObservableProperty]
        private bool isLoadingCategoryVocabularies;

        [ObservableProperty]
        private string generalError;

        [ObservableProperty]
        private string selectedCategoryName;

        public SearchVocabulariesViewModel()
        {
            // Khi khởi tạo, load category
            LoadCategoriesCommand.Execute(null);
            LoadNewWordsCommand.Execute(null);
        }

        // Command để xử lý khi click vào category
        [RelayCommand]
        private async Task CategoryTappedAsync(VocabularyCategory category)
        {
            if (category == null) return;

            Debug.WriteLine($"[CategoryTapped] Selected: {category.VocabularyCategoryName}, ID: {category.VocabularyCategoryId}");
            
            // Navigate to dedicated page với parameters
            await Shell.Current.GoToAsync($"categoryvocabularylistpage?categoryId={category.VocabularyCategoryId}&categoryName={Uri.EscapeDataString(category.VocabularyCategoryName)}");
        }

        [RelayCommand]
        private async Task LoadVocabulariesByCategoryAsync(int categoryId)
        {
            if (IsLoadingCategoryVocabularies) return;

            try
            {
                IsLoadingCategoryVocabularies = true;
                GeneralError = string.Empty;
                CategoryVocabularies.Clear();

                using var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true
                };
                using var client = new HttpClient(handler);

                // Bước 1: Lấy danh sách vocabulary IDs theo category
                var mappingUrl = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyVocabulary/GetListVocabularyCategoryMappingByCategoryId/{categoryId}";
                Debug.WriteLine($"[LoadVocabulariesByCategory] Mapping URL: {mappingUrl}");

                var mappingResponse = await client.GetAsync(mappingUrl);
                if (!mappingResponse.IsSuccessStatusCode)
                {
                    GeneralError = $"Lỗi tải danh sách mapping: {mappingResponse.StatusCode}";
                    return;
                }

                var mappingJson = await mappingResponse.Content.ReadAsStringAsync();
                Debug.WriteLine($"[LoadVocabulariesByCategory] Mapping JSON: {mappingJson}");

                var mappings = JsonSerializer.Deserialize<VocabularyCategoryMapping[]>(
                    mappingJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? Array.Empty<VocabularyCategoryMapping>();

                Debug.WriteLine($"[LoadVocabulariesByCategory] Found {mappings.Length} vocabulary mappings");

                // Bước 2: Lấy chi tiết từng vocabulary theo ID
                var vocabulariesList = new List<Vocabulary>();
                
                foreach (var mapping in mappings.Take(10)) // Giới hạn 10 từ đầu tiên để tránh quá tải
                {
                    try
                    {
                        var vocabUrl = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyVocabulary/GetVocabularyById/{mapping.VocabularyId}";
                        Debug.WriteLine($"[LoadVocabulariesByCategory] Vocab URL: {vocabUrl}");

                        var vocabResponse = await client.GetAsync(vocabUrl);
                        if (vocabResponse.IsSuccessStatusCode)
                        {
                            var vocabJson = await vocabResponse.Content.ReadAsStringAsync();
                            Debug.WriteLine($"[LoadVocabulariesByCategory] Vocab JSON: {vocabJson}");

                            var vocabulary = JsonSerializer.Deserialize<Vocabulary>(
                                vocabJson,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                            );

                            if (vocabulary != null)
                            {
                                vocabulariesList.Add(vocabulary);
                                Debug.WriteLine($"[LoadVocabulariesByCategory] Added: {vocabulary.Word}, ID: {vocabulary.VocabularyId}");
                            }
                        }
                        else
                        {
                            Debug.WriteLine($"[LoadVocabulariesByCategory] Failed to load vocabulary {mapping.VocabularyId}: {vocabResponse.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[LoadVocabulariesByCategory] Error loading vocabulary {mapping.VocabularyId}: {ex.Message}");
                    }
                }

                // Bước 3: Cập nhật UI
                CategoryVocabularies.Clear();
                foreach (var vocab in vocabulariesList)
                {
                    CategoryVocabularies.Add(vocab);
                }

                Debug.WriteLine($"[LoadVocabulariesByCategory] Successfully loaded {CategoryVocabularies.Count} vocabularies");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoadVocabulariesByCategory] Exception: {ex}");
                GeneralError = $"Lỗi: {ex.Message}";
            }
            finally
            {
                IsLoadingCategoryVocabularies = false;
            }
        }

        [RelayCommand]
        private async Task LoadNewWordsAsync()
        {
            if (IsLoadingNewWords) return;
            try
            {
                IsLoadingNewWords = true;
                using var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true
                };
                using var client = new HttpClient(handler);
                var url = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyVocabulary/GetNewVocabulariesThisMonth";
                var resp = await client.GetAsync(url);
                if (!resp.IsSuccessStatusCode)
                {
                    GeneralError = $"Lỗi server: {resp.StatusCode}";
                    return;
                }

                var json = await resp.Content.ReadAsStringAsync();
                Debug.WriteLine($"[LoadNewWords] JSON: {json}");
                var items = JsonSerializer.Deserialize<Vocabulary[]>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? Array.Empty<Vocabulary>();

                NewVocabularies.Clear();
                foreach (var v in items.Take(3))
                {
                    Debug.WriteLine($"[LoadNewWords] Added: {v.Word}, ID: {v.VocabularyId}");
                    NewVocabularies.Add(v);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoadNewWords] Exception: {ex}");
                GeneralError = $"Lỗi: {ex.Message}";
            }
            finally
            {
                IsLoadingNewWords = false;
            }
        }

        // Command load category
        [RelayCommand]
        private async Task LoadCategoriesAsync()
        {
            if (IsLoadingCategories) return;
            try
            {
                IsLoadingCategories = true;
                using var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true
                };
                using var client = new HttpClient(handler);
                var url = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyVocabulary/GetListVocabularyCategory";
                var resp = await client.GetAsync(url);
                if (!resp.IsSuccessStatusCode)
                {
                    GeneralError = $"Lỗi server: {resp.StatusCode}";
                    return;
                }

                var json = await resp.Content.ReadAsStringAsync();
                var items = JsonSerializer.Deserialize<VocabularyCategory[]>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? Array.Empty<VocabularyCategory>();

                var colorHexes = new[]
                {
                    "#7F55B1", "#9B7EBD", "#F49BAB","#F7CFD8","#A6D6D6","#FFE1E0","#F4F8D3","#8E7DBE"
                };

                Categories.Clear();
                var rnd = new Random();
                foreach (var c in items)
                {
                    var hex = colorHexes[rnd.Next(colorHexes.Length)];
                    // Convert từ HEX sang Color
                    c.BackgroundColor = Color.FromArgb(hex);
                    Categories.Add(c);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoadCategories] Exception: {ex}");
                GeneralError = $"Lỗi: {ex.Message}";
            }
            finally
            {
                IsLoadingCategories = false;
            }
        }

        // Khi SearchText thay đổi
        partial void OnSearchTextChanged(string value)
        {
            SearchCommand.NotifyCanExecuteChanged();
            if (string.IsNullOrWhiteSpace(value))
            {
                Vocabularies.Clear();
                GeneralError = string.Empty;
            }
        }

        private bool CanSearch() =>
            !IsLoadingCategories && !string.IsNullOrWhiteSpace(SearchText);

        [RelayCommand(CanExecute = nameof(CanSearch))]
        private async Task SearchAsync()
        {
            if (IsLoadingCategories) return;
            IsLoadingCategories = true;
            GeneralError = string.Empty;
            Vocabularies.Clear();

            try
            {
                using var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true
                };
                using var client = new HttpClient(handler);

                var url = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyVocabulary/get-word/{Uri.EscapeDataString(SearchText.Trim())}";
                Debug.WriteLine($"[Search] URL: {url}");

                var resp = await client.GetAsync(url);
                Debug.WriteLine($"[Search] Status: {resp.StatusCode}");

                if (!resp.IsSuccessStatusCode)
                {
                    GeneralError = "Không thể kết nối server.";
                    return;
                }

                var json = await resp.Content.ReadAsStringAsync();
                Debug.WriteLine($"[Search] JSON: {json}");

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var item = new Vocabulary
                {
                    VocabularyId = root.GetProperty("vocabularyId").GetInt32(),
                    Word = root.GetProperty("word").GetString() ?? string.Empty,
                    Pronunciation = root.GetProperty("pronunciation").GetString(),
                    AudioUrlUk = root.GetProperty("audioUrlUk").GetString(),
                    AudioUrlUs = root.GetProperty("audioUrlUs").GetString(),
                    CreatedAt = root.GetProperty("createdAt").GetDateTime()
                };

                Debug.WriteLine($"[Search] Parsed: Word={item.Word}, ID={item.VocabularyId}");
                Vocabularies.Add(item);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Search] Exception: {ex}");
                GeneralError = $"Lỗi: {ex.Message}";
            }
            finally
            {
                IsLoadingCategories = false;
                SearchCommand.NotifyCanExecuteChanged();
            }
        }

        // Command để xử lý khi nhấn vào từ vựng
        [RelayCommand]
        private async Task VocabularyTappedAsync(Vocabulary vocabulary)
        {
            if (vocabulary != null)
            {
                Debug.WriteLine($"[VocabularyTapped] Selected: {vocabulary.Word}, ID: {vocabulary.VocabularyId}");
                if (vocabulary.VocabularyId > 0)
                {
                    await Shell.Current.GoToAsync($"vocabularydetailpage?vocabularyId={vocabulary.VocabularyId}");
                }
                else
                {
                    Debug.WriteLine("[VocabularyTapped] Invalid VocabularyId");
                }
            }
            else
            {
                Debug.WriteLine("[VocabularyTapped] Vocabulary is null");
            }
        }

        //chuyển sang trang CategoryVocabulary
        [RelayCommand]
        private async Task NavigateToCategoryAsync()
        {
            try
            {
                // Điều hướng sang trang CategoryVocabulary
                await Shell.Current.GoToAsync("categoryvocabularypage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NavigateToCategory] Exception: {ex}");
                GeneralError = $"Lỗi khi chuyển trang: {ex.Message}";
            }
        }



        //chuyển sang xem tất cả
        [RelayCommand]
        private async Task ViewAllNewWordsAsync()
        {
            try
            {
                // Navigate to a new page showing all new words
                await Shell.Current.GoToAsync("allnewwordspage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ViewAllNewWords] Exception: {ex}");
                GeneralError = $"Lỗi khi chuyển trang: {ex.Message}";
            }
        }
    }
}
