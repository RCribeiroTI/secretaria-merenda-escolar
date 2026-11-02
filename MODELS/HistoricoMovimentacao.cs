using System;

namespace SecretariaMerenda.Models
{
    public class HistoricoMovimentacao
    {
        public int Id { get; set; }
        public string UsuarioNome { get; set; } = string.Empty;
        public string UsuarioPerfil { get; set; } = string.Empty;
        public string Acao { get; set; } = string.Empty;
        public string Detalhes { get; set; } = string.Empty;
        public DateTime DataHora { get; set; } = DateTime.Now;
        public string CodigoEscola { get; set; } = string.Empty;

        public HistoricoMovimentacao() { }

        public HistoricoMovimentacao(string usuarioNome, string usuarioPerfil, string acao, string detalhes, string codigoEscola)
        {
            UsuarioNome = usuarioNome;
            UsuarioPerfil = usuarioPerfil;
            Acao = acao;
            Detalhes = detalhes;
            DataHora = DateTime.Now;
            CodigoEscola = codigoEscola;
        }
    }
}