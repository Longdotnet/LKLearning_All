using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mauiluanvantotnghiep.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.ViewModels.VocabularyViewModel
{
    public partial class VocabularyDetailViewModel : ObservableObject
    {
        [ObservableProperty]
        private VocabularyWithMeanings vocabularyWithMeanings;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string generalError;

        [ObservableProperty]
        private bool isFavorite;

        [ObservableProperty]
        private string selectedTab = "Meanings";

        [ObservableProperty]
        private string selectedPartOfSpeech = "All";

        [ObservableProperty]
        private bool isFilterVisible = false;

        // Collection for all meanings (unfiltered)
        private readonly ObservableCollection<VocabularyMeaning> _allMeanings = new();
        
        // Collection for filtered meanings
        public ObservableCollection<VocabularyMeaning> Meanings { get; } = new();

        // Collection for examples
        public ObservableCollection<VocabularyMeaning> Examples { get; } = new();

        // Collection for available parts of speech
        public ObservableCollection<string> PartsOfSpeech { get; } = new();

        private readonly HttpClient _httpClient;

        public VocabularyDetailViewModel(int vocabularyId)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
            
            _ = LoadVocabularyAsync(vocabularyId);
        }

        [RelayCommand]
        private async Task LoadVocabularyAsync(int vocabularyId)
        {
            if (IsLoading || vocabularyId <= 0) return;

            IsLoading = true;
            GeneralError = string.Empty;

            try
            {
                var url = $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyVocabulary/GetVocabularyWithMeanings/{vocabularyId}";
                Debug.WriteLine($"[LoadVocabulary] GET {url}");

                var resp = await _httpClient.GetAsync(url);
                resp.EnsureSuccessStatusCode();

                var json = await resp.Content.ReadAsStringAsync();
                var tmp = JsonSerializer.Deserialize<VocabularyWithMeanings>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                VocabularyWithMeanings = tmp;

                // Populate collections
                await PopulateCollectionsAsync(tmp);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoadVocabulary] Exception: {ex}");
                GeneralError = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task PopulateCollectionsAsync(VocabularyWithMeanings data)
        {
            await Task.Run(() =>
            {
                // Clear collections on UI thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _allMeanings.Clear();
                    Meanings.Clear();
                    Examples.Clear();
                    PartsOfSpeech.Clear();
                });

                if (data?.Meanings != null)
                {
                    var meaningsList = new List<VocabularyMeaning>();
                    var examplesList = new List<VocabularyMeaning>();
                    var partsOfSpeech = new HashSet<string>();

                    foreach (var item in data.Meanings)
                    {
                        meaningsList.Add(item);
                        
                        if (!string.IsNullOrEmpty(item.ExampleSentence))
                            examplesList.Add(item);

                        // Collect unique parts of speech
                        if (!string.IsNullOrEmpty(item.PartOfSpeech))
                            partsOfSpeech.Add(item.PartOfSpeech);
                    }

                    // Update collections on UI thread
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        // Add "All" option first
                        PartsOfSpeech.Add("All");
                        
                        // Add unique parts of speech
                        foreach (var pos in partsOfSpeech.OrderBy(x => x))
                            PartsOfSpeech.Add(pos);

                        // Store all meanings
                        foreach (var meaning in meaningsList)
                            _allMeanings.Add(meaning);
                        
                        // Apply initial filter (show all)
                        ApplyFilter();
                        
                        // Update examples
                        foreach (var example in examplesList)
                            Examples.Add(example);
                    });
                }
            });
        }

        [RelayCommand]
        private void ToggleFilter()
        {
            IsFilterVisible = !IsFilterVisible;
        }

        [RelayCommand]
        private void ApplyPartOfSpeechFilter(string partOfSpeech)
        {
            if (string.IsNullOrEmpty(partOfSpeech)) return;

            SelectedPartOfSpeech = partOfSpeech;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            Meanings.Clear();

            if (SelectedPartOfSpeech == "All")
            {
                // Show all meanings
                foreach (var meaning in _allMeanings)
                    Meanings.Add(meaning);
            }
            else
            {
                // Filter by selected part of speech
                foreach (var meaning in _allMeanings.Where(m => 
                    string.Equals(m.PartOfSpeech, SelectedPartOfSpeech, StringComparison.OrdinalIgnoreCase)))
                    Meanings.Add(meaning);
            }
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            var word = VocabularyWithMeanings?.Word;
            if (string.IsNullOrWhiteSpace(word))
                return;

            try
            {
                var uri = new Uri($"https://www.google.com/search?q={Uri.EscapeDataString(word)}+meaning");
                await Launcher.OpenAsync(uri);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Search] Exception: {ex}");
            }
        }

        [RelayCommand]
        private async Task ToggleFavoriteAsync()
        {
            try
            {
                IsFavorite = !IsFavorite;
                // TODO: Implement API call to save favorite status
                await Task.Delay(500); // Simulate API call
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ToggleFavorite] Exception: {ex}");
                IsFavorite = !IsFavorite; // Revert on error
            }
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        // Cleanup
        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}