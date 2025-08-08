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


namespace mauiluanvantotnghiep.ViewModels.VocabularyViewModel
{
    public partial class CategoryVocabularyViewModel : ObservableObject
    {
        // Danh sách category
        public ObservableCollection<VocabularyCategory> Categories { get; } = new();

        [ObservableProperty]
        private bool isLoadingCategories;

        [ObservableProperty]
        private string generalError;

        public CategoryVocabularyViewModel()
        {
            // Tải danh sách category khi khởi tạo
            LoadCategoriesCommand.Execute(null);
        }

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
                Debug.WriteLine($"[LoadCategories] JSON: {json}");
                var items = JsonSerializer.Deserialize<VocabularyCategory[]>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? Array.Empty<VocabularyCategory>();

                var colorHexes = new[]
                   {
                    "#7F55B1", "#9B7EBD", "#F49BAB","#F7CFD8","#A6D6D6","#FFE1E0","#F4F8D3","#8E7DBE"
                };

                var rnd = new Random();
                Categories.Clear();

                foreach (var c in items)
                {
                    // Random lấy mã màu từ danh sách
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


         [RelayCommand]
        private async Task NavigateToFlashcardAsync(int categoryId)
        {
            try
            {
                await Shell.Current.GoToAsync($"flashcardpage?categoryId={categoryId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NavigateToFlashcard] Exception: {ex}");
                GeneralError = $"Lỗi khi chuyển đến Flashcard: {ex.Message}";
            }
        }
    }
}
