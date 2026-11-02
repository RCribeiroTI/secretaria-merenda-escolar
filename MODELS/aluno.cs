namespace SecretariaMerenda.Models
{
    public class Aluno
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Matricula { get; set; } = string.Empty;
        public string Turma { get; set; } = string.Empty;
        public int Idade { get; set; }
        public string RestricaoAlimentar { get; set; } = string.Empty;
        public string CodigoEscola { get; set; } = string.Empty;

        // Construtor sem parâmetros (obrigatório para o Entity Framework Core / SQLite)
        public Aluno() { }

        // Construtor completo para uso no sistema
        public Aluno(int id, string nome, string matricula, string turma, int idade, string restricaoAlimentar, string codigoEscola)
        {
            Id = id;
            Nome = nome;
            Matricula = matricula;
            Turma = turma;
            Idade = idade;
            RestricaoAlimentar = restricaoAlimentar;
            CodigoEscola = codigoEscola;
        }
    }
}