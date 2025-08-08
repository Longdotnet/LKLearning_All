namespace WEBAPI.DTOS
{
    public class QuestionTypeDTO
    {
        public int QuestionTypeId { get; set; }

        public string TypeName { get; set; } = null!;

        public string? TypeDescription { get; set; }
    }
}
