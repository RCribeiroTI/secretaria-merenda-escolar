namespace SecretariaMerenda.Models
{
    public class ItemEstoque
    {
        public int Id { get; set; }
        public string NomeAlimento { get; set; } = string.Empty; // ex: Arroz, Feijão, Leite
        public decimal Quantidade { get; set; }                  // ex: 50.5
        public string UnidadeMedida { get; set; } = string.Empty; // ex: Kg, Litros, Pacotes
        public DateTime DataValidade { get; set; }
        
        public ItemEstoque(int id, string nomeAlimento, decimal quantidade, string unidadeMedida, DateTime dataValidade)
        {
            Id = id;
            NomeAlimento = nomeAlimento;
            Quantidade = quantidade;
            UnidadeMedida = unidadeMedida;
            DataValidade = dataValidade;
        }
    }
}