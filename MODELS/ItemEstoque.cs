namespace SecretariaMerenda.Models
{
    public class ItemEstoque
    {
        public int Id { get; set; }
        public string NomeAlimento { get; set; } = string.Empty;
        public decimal Quantidade { get; set; }
        public string UnidadeMedida { get; set; } = string.Empty;
        public DateTime DataValidade { get; set; }

        public ItemEstoque(int id, string nomeAlimento, decimal quantidade, string unidadeMedida, DateTime dataValidade)
        {
            Id = id;
            NomeAlimento = nomeAlimento;
            Quantidade = quantidade;
            UnidadeMedida = unidadeMedida;
            DataValidade = dataValidade;
        }

        // Alerta: Estoque igual ou abaixo do limite (padrão: 5 unidades/kg/litros)
        public bool TemEstoqueCritico(decimal limiteMinimo = 5.0m)
        {
            return Quantidade <= limiteMinimo;
        }

        // Alerta: Vence nos próximos N dias (padrão: 7 dias)
        public bool EstaProximoDaValidade(int diasMargem = 7)
        {
            TimeSpan diferenca = DataValidade.Date - DateTime.Now.Date;
            return diferenca.TotalDays >= 0 && diferenca.TotalDays <= diasMargem;
        }
    }
}