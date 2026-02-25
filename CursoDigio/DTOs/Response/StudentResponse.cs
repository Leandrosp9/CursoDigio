namespace CursoDigio.DTOs.Response
{
    public class StudentResponse
    {
        public int Id { get; set; }
        public string NomeCompleto { get; set; } = string.Empty;
        public DateTime DataCadastro { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? Email { get; set; }
    }
}
