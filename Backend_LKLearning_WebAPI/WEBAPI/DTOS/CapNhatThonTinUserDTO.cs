namespace WEBAPI.DTOS
{
    public class CapNhatThonTinUserDTO
    {
        public string? Email { get; set; }
        public string? Fullname { get; set; }

        public string? Password { get; set; }

        public int? Age { get; set; }

        public string? Phone { get; set; }

        public bool? Gender { get; set; }

        public DateOnly? DateofBirth { get; set; }
        public int? RoleId { get; set; }


        // Thêm 2 trường dưới
        public int? AvatarId { get; set; }    // Nếu client muốn chọn avatar cụ thể
    }
}
