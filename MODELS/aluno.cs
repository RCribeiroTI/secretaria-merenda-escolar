namespace SecretariaMerenda.Models
{
    public class Aluno
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Matricula { get; set; } = string.Empty;
        public string Turma { get; set; } = string.Empty;
        public string RestricaoAlimentar { get; set; } = string.Empty;
        public bool Ativo { get; set; } = true;

        public Aluno(int id, string nome, string matricula, string turma, string restricaoAlimentar = "Nenhuma")
        {
            Id = id;
            Nome = nome;
            Matricula = matricula;
            Turma = turma;
            RestricaoAlimentar = restricaoAlimentar;
        }
    }
}