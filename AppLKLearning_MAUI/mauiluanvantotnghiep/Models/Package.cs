using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mauiluanvantotnghiep.Models
{
   public partial class Package : ObservableObject
    {
        public int PackageId { get; set; }
        public string PackageName { get; set; }
        public int? DurationDay { get; set; }
        public decimal Price { get; set; }

        public string UrlImage { get; set; } = string.Empty;


        // Thuộc tính này dùng để xác định xem người dùng đã đăng ký gói này hay chưa.
        // Khi thay đổi, giao diện sẽ nhận thông báo và cập nhật lại
        [ObservableProperty]
        private bool isRegistered;

         // Thêm thông tin từ Registration
        [ObservableProperty]
        private DateTime registrationDate;

        [ObservableProperty]
        private DateTime? expirationDate;


        [ObservableProperty]
        private string expirationDateDisplay = "vĩnh viễn"; // Giá trị mặc định

        // Cập nhật ExpirationDateDisplay khi ExpirationDate thay đổi
        partial void OnExpirationDateChanged(DateTime? oldValue, DateTime? newValue)
        {
            ExpirationDateDisplay = newValue.HasValue ? newValue.Value.ToString("dd/MM/yyyy") : "vĩnh viễn";
        }
    }
}
