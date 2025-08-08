namespace WEBAPI.DTOS
{
    public class CategoriesDTO
    {
        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = null!;

        public string? Description { get; set; }
    }
}
