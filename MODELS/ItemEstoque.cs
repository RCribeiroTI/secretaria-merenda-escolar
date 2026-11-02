using System;

namespace SecretariaMerenda.Models
{
    public class ItemEstoque
    {
        public int Id { get; set; }
        public string NomeAlimento { get; set; } = string.Empty;
        public decimal Quantidade { get; set; }
        public string UnidadeMedida { get; set; } = string.Empty;
        public DateTime DataValidade { get; set; }
        public string CodigoEscola { get; set; } = string.Empty;

        // Construtor sem parâmetros (obrigatório para o EF Core / SQLite)
        public ItemEstoque() { }

        // Construtor completo para cadastro manual (com Id = 0 para auto-incremento do banco)
        public ItemEstoque(string nomeAlimento, decimal quantidade, string unidadeMedida, DateTime dataValidade, string codigoEscola)
        {
            NomeAlimento = nomeAlimento;
            Quantidade = quantidade;
            UnidadeMedida = unidadeMedida;
            DataValidade = dataValidade;
            CodigoEscola = codigoEscola;
        }

        public bool TemEstoqueCritico()
        {
            return Quantidade <= 5;
        }
    }
}