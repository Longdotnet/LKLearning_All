using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mauiluanvantotnghiep.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;

namespace mauiluanvantotnghiep.ViewModels
{
    public partial class AllNewWordsViewModel : ObservableObject
    {
        public ObservableCollection<Vocabulary> AllNewVocabularies { get; } = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string generalError;

        public AllNewWordsViewModel()
        {
            LoadAllNewWordsCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadAllNewWordsAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                GeneralError = string.Empty;
                AllNewVocabularies.Clear();

                using var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true
                };
                using var client = new HttpClient(handler);
                
                var url = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyVocabulary/GetNewVocabulariesThisMonth";
                Debug.WriteLine($"[LoadAllNewWords] URL: {url}");

                var resp = await client.GetAsync(url);
                if (!resp.IsSuccessStatusCode)
                {
                    GeneralError = $"Lỗi server: {resp.StatusCode}";
                    return;
                }

                var json = await resp.Content.ReadAsStringAsync();
                Debug.WriteLine($"[LoadAllNewWords] JSON: {json}");

                var items = JsonSerializer.Deserialize<Vocabulary[]>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? Array.Empty<Vocabulary>();

                foreach (var v in items)
                {
                    AllNewVocabularies.Add(v);
                    Debug.WriteLine($"[LoadAllNewWords] Added: {v.Word}, ID: {v.VocabularyId}");
                }

                Debug.WriteLine($"[LoadAllNewWords] Successfully loaded {AllNewVocabularies.Count} new vocabularies");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoadAllNewWords] Exception: {ex}");
                GeneralError = $"Lỗi: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

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

        [RelayCommand]
        private async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
