using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mauiluanvantotnghiep.Models;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.ViewModels.VocabularyViewModel
{
    // DTO ánh xạ response mapping
    public class CategoryMapping
    {
        public int VocabularyId { get; set; }
        public DateTime DateAdded { get; set; }
    }

    public partial class FlashcardViewModel : ObservableObject
    {
        private const string RemovedWordsKey = "RemovedVocabularyIds";

        [ObservableProperty]
        private ObservableCollection<VocabularyWithMeanings> vocabularyList = new();

        [ObservableProperty]
        private int currentIndex;

        [ObservableProperty]
        private string currentWord;

        [ObservableProperty]
        private string currentPronunciation;

        [ObservableProperty]
        private string currentMeaning;

        [ObservableProperty]
        private string currentPartOfSpeech;

        [ObservableProperty]
        private string currentTranslatedMeaning;

        [ObservableProperty]
        private string currentAudioUrlUk;

        [ObservableProperty]
        private string currentAudioUrlUs;

        [ObservableProperty]
        private double progress;

        [ObservableProperty]
        private double cardFontSize = 18;

        [ObservableProperty]
        private bool isFlipped;

        [ObservableProperty]
        private string errorMessage;

        // lưu category hiện tại để Reset
        private int categoryId;

        // bộ nhớ ID đã loại
        private HashSet<int> removedWordIds = new();

        public FlashcardViewModel()
        {
            LoadRemovedIds();
        }

        private void LoadRemovedIds()
        {
            var json = Preferences.Get(RemovedWordsKey, string.Empty);
            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    removedWordIds = JsonSerializer.Deserialize<HashSet<int>>(json)
                                     ?? new HashSet<int>();
                }
                catch
                {
                    removedWordIds = new HashSet<int>();
                }
            }
        }

        private void SaveRemovedIds()
        {
            var json = JsonSerializer.Serialize(removedWordIds);
            Preferences.Set(RemovedWordsKey, json);
        }

        [RelayCommand]
        private async Task LoadVocabulariesAsync(int categoryId)
        {
            try
            {
                ErrorMessage = string.Empty;
                VocabularyList.Clear();
                CurrentIndex = 0;
                IsFlipped = false;
                this.categoryId = categoryId;

                using var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
                using var client = new HttpClient(handler);

                // 1) Lấy mapping
                var mapUrl =
                  $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyVocabulary/" +
                  $"GetListVocabularyCategoryMappingByCategoryId/{categoryId}";
                var mapResp = await client.GetAsync(mapUrl);
                if (!mapResp.IsSuccessStatusCode)
                {
                    ErrorMessage = $"Mapping API lỗi {(int)mapResp.StatusCode}";
                    return;
                }
                var mappings = await mapResp.Content.ReadFromJsonAsync<List<CategoryMapping>>();

                // 2) Fetch chi tiết và lọc những ID đã loại
                foreach (var m in mappings.Where(m => !removedWordIds.Contains(m.VocabularyId)))
                {
                    var vocabUrl =
                      $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyVocabulary/" +
                      $"GetVocabularyById/{m.VocabularyId}";
                    var vocabResp = await client.GetAsync(vocabUrl);
                    if (!vocabResp.IsSuccessStatusCode) continue;

                    var vocabData = await vocabResp.Content
                        .ReadFromJsonAsync<VocabularyWithMeanings>();
                    if (vocabData?.Meanings?.Any() != true) continue;

                    vocabData.Meanings = new List<VocabularyMeaning>
                        { vocabData.Meanings.First() };
                    VocabularyList.Add(vocabData);
                }

                if (VocabularyList.Any())
                    UpdateCard();
                else
                    ErrorMessage = "Bạn đã học hết tất cả flashcards.";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DEBUG] Exception in LoadVocabulariesAsync: {ex}");
                ErrorMessage = $"Lỗi khi tải dữ liệu: {ex.Message}";
            }
        }

        private void UpdateCard()
        {
            if (CurrentIndex < 0 || CurrentIndex >= VocabularyList.Count)
                return;

            var card = VocabularyList[CurrentIndex];
            CurrentWord = card.Word;
            CurrentPronunciation = card.Pronunciation;
            CurrentAudioUrlUk = card.AudioUrlUk;
            CurrentAudioUrlUs = card.AudioUrlUs;

            var m = card.Meanings.First();
            CurrentMeaning = m.Meaning;
            CurrentPartOfSpeech = m.PartOfSpeech;
            CurrentTranslatedMeaning = m.TranslatedMeaning;

            Progress = VocabularyList.Count == 0
                ? 0
                : (double)(CurrentIndex + 1) / VocabularyList.Count;
        }

        [RelayCommand]
        private void Flip() => IsFlipped = !IsFlipped;

        [RelayCommand]
        private void Next()
        {// Nếu không còn flashcards nào
            if (VocabularyList.Count == 0)
            {
                ErrorMessage = "Bạn đã học hết tất cả flashcards.";
                return;
            }

            // Nếu chưa phải cuối danh sách => +1, còn lại quay về 0
            if (CurrentIndex < VocabularyList.Count - 1)
                CurrentIndex++;
            else
                CurrentIndex = 0;

            // Reset trạng thái flip và lỗi
            IsFlipped = false;
            ErrorMessage = string.Empty;

            UpdateCard();
        }

        [RelayCommand]
        private void Remove()
        {
            if (CurrentIndex < 0 || CurrentIndex >= VocabularyList.Count)
                return;

            var id = VocabularyList[CurrentIndex].VocabularyId;
            removedWordIds.Add(id);
            SaveRemovedIds();

            VocabularyList.RemoveAt(CurrentIndex);
            if (VocabularyList.Count == 0)
            {
                ErrorMessage = "Bạn đã loại bỏ hết các từ!";
            }
            else
            {
                if (CurrentIndex >= VocabularyList.Count)
                    CurrentIndex = VocabularyList.Count - 1;
                UpdateCard();
            }
        }

        [RelayCommand]
        private async Task Reset()
        {
            removedWordIds.Clear();
            Preferences.Remove(RemovedWordsKey);
            await LoadVocabulariesAsync(categoryId);
        }
    }
}
