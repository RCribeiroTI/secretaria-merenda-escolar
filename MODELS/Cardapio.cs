using System;

namespace SecretariaMerenda.Models
{
    public class Cardapio
    {
        public int Id { get; set; }
        public string DiaDaSemana { get; set; } = string.Empty;
        public string Refeicao { get; set; } = string.Empty;
        public string DescricaoPrato { get; set; } = string.Empty;
        public DateTime DataServico { get; set; }

        // Construtor sem parâmetros (obrigatório para o Entity Framework Core)
        public Cardapio() { }

        // Construtor com parâmetros
        public Cardapio(int id, string diaDaSemana, string refeicao, string descricaoPrato, DateTime dataServico)
        {
            Id = id;
            DiaDaSemana = diaDaSemana;
            Refeicao = refeicao;
            DescricaoPrato = descricaoPrato;
            DataServico = dataServico;
        }
    }
}