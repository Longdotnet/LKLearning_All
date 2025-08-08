using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mauiluanvantotnghiep.Models;
using mauiluanvantotnghiep.ViewModels.AppConfig;
using Microsoft.Maui.Storage;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.ViewModels
{
    public partial class PackagePageViewModel : ObservableObject
    {
        // Tất cả gói
        public ObservableCollection<Package> Packages { get; } = new();

        // Nhóm đã active
        public ObservableCollection<Package> ActivePackages { get; } = new();

        // Nhóm chưa active
        public ObservableCollection<Package> InactivePackages { get; } = new();

        [ObservableProperty]
        bool hasActivePackages;

        [ObservableProperty]
        bool hasInactivePackages;

        // Properties cho popup
        [ObservableProperty]
        bool isPackageDetailVisible;

        [ObservableProperty]
        Package selectedPackageDetail;

        private readonly HttpClient _httpClient;
        private readonly List<Registration> regs = new(); // Thêm danh sách Registration

        public PackagePageViewModel()
        {
            // bỏ qua SSL self-signed cho DEV
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);

            LoadPackagesAndRegistrations();
        }

        public async void LoadPackagesAndRegistrations()
        {
            try
            {
                // Nếu đã login thì set header Bearer
                var token = await SecureStorage.GetAsync("auth_token");
                if (!string.IsNullOrEmpty(token))
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                // 1) Load list package
                var pkgJson = await _httpClient.GetStringAsync(
                    $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyGoiCuoc/GetListPackages");
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var list = JsonSerializer.Deserialize<Package[]>(pkgJson, options)
                           ?? Array.Empty<Package>();

                Packages.Clear();
                foreach (var p in list)
                {
                    p.IsRegistered = false;
                    Packages.Add(p);
                }

                // 2) Load list đăng ký của user
                var regJson = await _httpClient.GetStringAsync(
                    $"{AppConfig.AppConfig.BaseUrl}/api/QuanLyDangKyGoiCuoc/GetUserPackageRegistrations");
                regs.Clear();
                regs.AddRange(JsonSerializer.Deserialize<Registration[]>(regJson, options) ?? Array.Empty<Registration>());
                var owned = regs.Select(r => r.PackageId).ToHashSet();

                foreach (var p in Packages)
                {
                    var registration = regs.FirstOrDefault(r => r.PackageId == p.PackageId); // Chỉ lọc theo PackageId
                    if (registration != null)
                    {
                        p.IsRegistered = true;
                        p.RegistrationDate = registration.RegistrationDate;
                        p.ExpirationDate = registration.ExpirationDate; // Model sẽ tự cập nhật ExpirationDateDisplay
                        if (string.IsNullOrEmpty(p.PackageName))
                            p.PackageName = $"Gói {p.PackageId}"; // Placeholder
                    }
                }

                // 3) Phân nhóm & cập nhật flags
                RefreshGroups();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadPackages error: {ex}");
            }
        }

        private void RefreshGroups()
        {
            ActivePackages.Clear();
            InactivePackages.Clear();

            foreach (var p in Packages)
            {
                if (p.IsRegistered)
                {
                    // Tìm registration tương ứng và gán ngày
                    var registration = regs.FirstOrDefault(r => r.PackageId == p.PackageId);
                    if (registration != null)
                    {
                        p.RegistrationDate = registration.RegistrationDate;
                        p.ExpirationDate = registration.ExpirationDate;
                    }
                    ActivePackages.Add(p);
                }
                else
                {
                    InactivePackages.Add(p);
                }
            }

            HasActivePackages = ActivePackages.Any();
            HasInactivePackages = InactivePackages.Any();
        }

        // Commands cho popup
        [RelayCommand]
        void ShowPackageDetail(Package package)
        {
            if (package != null)
            {
                SelectedPackageDetail = package;
                IsPackageDetailVisible = true;
            }
        }

        [RelayCommand]
        void ClosePackageDetail()
        {
            IsPackageDetailVisible = false;
            SelectedPackageDetail = null;
        }

        [RelayCommand]
        async Task BuyAsync(Package pkg)
        {
            Debug.WriteLine("BuyAsync called");
            Debug.WriteLine($"pkg is null: {pkg == null}");
            if (pkg == null)
            {
                await Shell.Current.DisplayAlert("Lỗi", "Gói cước không hợp lệ", "OK");
                return;
            }

            // Đóng popup nếu đang mở
            if (IsPackageDetailVisible)
            {
                ClosePackageDetail();
            }

            Debug.WriteLine($"PackageId: {pkg.PackageId}");
            Debug.WriteLine($"Price: {pkg.Price}");
            var parameters = new Dictionary<string, object>
            {
                { "packageId", pkg.PackageId.ToString() },
                { "price", pkg.Price.ToString() }
            };
            Debug.WriteLine($"Parameters: packageId={parameters["packageId"]}, price={parameters["price"]}");

            await Shell.Current.GoToAsync("PaymentgatewayPage", parameters);
        }
    }
}