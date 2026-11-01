namespace SecretariaMerenda.Models
{
    public class Cardapio
    {
        public int Id { get; set; }
        public string DiaDaSemana { get; set; } = string.Empty;
        public string Refeicao { get; set; } = string.Empty;
        public string DescricaoPrato { get; set; } = string.Empty;
        public DateTime DataServico { get; set; }

        public Cardapio(int id, string diaDaSemana, string refeicao, string descricaoPrato, DateTime dataServico)
        {
            Id = id;
            DiaDaSemana = diaDaSemana;
            Refeicao = refeicao;
            DescricaoPrato = descricaoPrato;
            DataServico = dataServico;
        }

        // Checa se o prato do cardápio oferece risco de alergia ao aluno
        public bool PossuiRiscoParaAluno(Aluno aluno)
        {
            if (string.IsNullOrWhiteSpace(aluno.RestricaoAlimentar) || aluno.RestricaoAlimentar.Equals("Nenhuma", StringComparison.OrdinalIgnoreCase))
                return false;

            return DescricaoPrato.Contains(aluno.RestricaoAlimentar, StringComparison.OrdinalIgnoreCase) 
                || (aluno.RestricaoAlimentar.Contains("Lactose", StringComparison.OrdinalIgnoreCase) && DescricaoPrato.Contains("Leite", StringComparison.OrdinalIgnoreCase));
        }
    }
}