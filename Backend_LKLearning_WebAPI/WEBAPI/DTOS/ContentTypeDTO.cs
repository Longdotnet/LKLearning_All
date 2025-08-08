namespace WEBAPI.DTOS
{
    public class ContentTypeDTO
    {
        public int ContentTypeId { get; set; }

        public string TypeName { get; set; } = null!;

        public string? TypeDescription { get; set; }
    }
}
