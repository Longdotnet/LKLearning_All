using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mauiluanvantotnghiep.Models;
using mauiluanvantotnghiep.Views.TransactionPage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.ViewModels
{
    public partial class FilterOption : ObservableObject
    {
        public string Value { get; }
        public string Display { get; }
        [ObservableProperty] private bool isSelected;
        public string IconSource => IsSelected
          ? "tick.png"
          : "checkbox_empty.png";

        public FilterOption(string value, string display, bool selected = false)
        {
            Value = value;
            Display = display;
            IsSelected = selected;
        }

        partial void OnIsSelectedChanged(bool _)
          => OnPropertyChanged(nameof(IconSource));
    }

    public partial class TransactionsViewModel : ObservableObject
    {

      
        // raw + grouped
        private List<Orders> _rawList = new();
        [ObservableProperty] private ObservableCollection<TransactionGroup> groupedTransactions = new();
        [ObservableProperty] private bool isBusy;

        // search + status‐filter
        [ObservableProperty] private string searchText;
        partial void OnSearchTextChanged(string _) => BuildGroups();

        [ObservableProperty] private string selectedFilter = "All";

        // month‐popup
        [ObservableProperty] private bool isMonthPopupVisible;
        public ObservableCollection<FilterOption> MonthOptions { get; } = new();

        // banner
        [ObservableProperty] private ObservableCollection<Banner> banners = new();
        [ObservableProperty] private int bannerPosition;
        private System.Threading.Timer _bannerTimer;

        // commands
        public IRelayCommand<Orders> NavigateToDetailCommand { get; }
        public IRelayCommand<string> FilterCommand { get; }
        public IAsyncRelayCommand LoadTransactionsCommand { get; }
        public IRelayCommand OpenMonthPopupCommand { get; }
        public IRelayCommand CloseMonthPopupCommand { get; }
        public IRelayCommand<FilterOption> SelectMonthOptionCommand { get; }

        readonly HttpClient _client;
        readonly string _baseUrl = AppConfig.AppConfig.BaseUrl;
        readonly Dictionary<int, string> _packageNameCache = new();

        public TransactionsViewModel()
        {
            _client = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                (req, cert, chain, errs) => true
            });

            FilterCommand = new RelayCommand<string>(f => {
                SelectedFilter = f;
                BuildGroups();
            });

            LoadTransactionsCommand = new AsyncRelayCommand(LoadTransactionsAsync);

            OpenMonthPopupCommand = new RelayCommand(() => {
                IsMonthPopupVisible = true;
            });
            CloseMonthPopupCommand = new RelayCommand(() => {
                IsMonthPopupVisible = false;
            });
            SelectMonthOptionCommand = new RelayCommand<FilterOption>(opt => {
                foreach (var o in MonthOptions) o.IsSelected = false;
                opt.IsSelected = true;
                IsMonthPopupVisible = false;
                BuildGroups();
            });

            _ = LoadTransactionsCommand.ExecuteAsync(null);


            NavigateToDetailCommand = new RelayCommand<Orders>(async tx =>
            {
                if (tx == null) return;
                var param = new Dictionary<string, object> { { "Transaction", tx } };
                await Shell.Current.GoToAsync(nameof(TransactionDetailsPage), param);
            });
        }

        private async Task LoadTransactionsAsync()
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                groupedTransactions.Clear();

                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrWhiteSpace(token)) return;
                _client.DefaultRequestHeaders.Authorization =
                  new AuthenticationHeaderValue("Bearer", token);

                // Fetch transactions
                var resp = await _client.GetAsync($"{_baseUrl}/api/payment/mytransactions");
                resp.EnsureSuccessStatusCode();
                var js = await resp.Content.ReadAsStringAsync();
                _rawList = JsonSerializer.Deserialize<List<Orders>>(js,
                  new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                           ?? new();

                foreach (var tx in _rawList)
                {
                    tx.StatusIcon = tx.Status switch
                    {
                        "Success" => "success.png",
                        "Pending" => "pending.png",
                        "Thất Bại" => "failed.png",
                        _ => "ic_default.png"
                    };
                    tx.GatewayIcon = tx.PaymentMethodId switch
                    {
                        1 => "zalopay.png",
                        2 => "vnpay.png",
                        3 => "ic_zalopay.png",
                        _ => "ic_gateway_default.png"
                    };
                    tx.PackageName = await GetPackageNameAsync(tx.PackageId);
                }

                // Fetch banners
                var bannerResp = await _client.GetAsync($"{_baseUrl}/api/QuanLyBanner/GetListBanners");
                bannerResp.EnsureSuccessStatusCode();
                var bannerJs = await bannerResp.Content.ReadAsStringAsync();
                var bannerList = JsonSerializer.Deserialize<List<Banner>>(bannerJs,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                Banners = new ObservableCollection<Banner>(bannerList);

                BuildMonthOptions();
                BuildGroups();
            }
            finally { IsBusy = false; }
        }

        public void StartBannerTimer()
        {
            _bannerTimer = new System.Threading.Timer(_ =>
            {
                if (Banners.Count == 0) return;
                var nextPosition = (BannerPosition + 1) % Banners.Count;
                MainThread.BeginInvokeOnMainThread(() => BannerPosition = nextPosition);
            }, null, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3));
        }

        public void StopBannerTimer()
        {
            _bannerTimer?.Dispose();
            _bannerTimer = null;
        }

        private void BuildMonthOptions()
        {
            MonthOptions.Clear();
            MonthOptions.Add(new FilterOption("All", "Tất cả", true));

            var months = _rawList
                .Where(x => x.CreatedAt.HasValue)
                .Select(x => new { x.CreatedAt.Value.Year, x.CreatedAt.Value.Month }) // Chỉ lấy năm và tháng
                .Distinct() // Loại bỏ trùng lặp dựa trên năm và tháng
                .OrderByDescending(d => d.Year)
                .ThenByDescending(d => d.Month);

            foreach (var dt in months)
                MonthOptions.Add(new FilterOption(
                    $"{dt.Year}-{dt.Month:00}",
                    $"Tháng {dt.Month:00}/{dt.Year}"
                ));
        }

        private void BuildGroups()
        {
            groupedTransactions.Clear();

            var filtered = _rawList.Where(tx =>
              (SelectedFilter == "All"
               || (SelectedFilter == "Success" && tx.Status == "Success")
               || (SelectedFilter == "Pending" && tx.Status == "Pending")
               || (SelectedFilter == "Failed" && tx.Status == "Failed"))
              && (string.IsNullOrWhiteSpace(SearchText)
                  || tx.PackageName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true)
            );

            var sel = MonthOptions.FirstOrDefault(o => o.IsSelected)?.Value;
            if (!string.IsNullOrWhiteSpace(sel) && sel != "All")
                filtered = filtered.Where(tx =>
                  $"{tx.CreatedAt:yyyy-MM}" == sel);

            var groups = filtered
              .Where(x => x.CreatedAt.HasValue)
              .GroupBy(x => new { x.CreatedAt.Value.Year, x.CreatedAt.Value.Month })
              .OrderByDescending(g => g.Key.Year)
              .ThenByDescending(g => g.Key.Month)
              .Select(g =>
              {
                  var title = $"Tháng {g.Key.Month:00}/{g.Key.Year}";
                  var grp = new TransactionGroup(title);
                  foreach (var tx in g.OrderByDescending(x => x.CreatedAt))
                      grp.Add(tx);
                  return grp;
              });

            foreach (var grp in groups)
                groupedTransactions.Add(grp);
        }

        private async Task<string> GetPackageNameAsync(int id)
        {
            if (_packageNameCache.TryGetValue(id, out var n)) return n;
            try
            {
                var r = await _client.GetAsync($"{_baseUrl}/api/QuanLyGoiCuoc/GetPackageById/{id}");
                if (!r.IsSuccessStatusCode) return "(Không rõ)";
                using var d = JsonDocument.Parse(await r.Content.ReadAsStringAsync());
                if (d.RootElement.TryGetProperty("packageName", out var pn))
                {
                    var s = pn.GetString() ?? "(Không rõ)";
                    _packageNameCache[id] = s;
                    return s;
                }
            }
            catch { }
            return "(Không rõ)";
        }
    }
}