using System;

namespace SecretariaMerenda.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string Perfil { get; set; } = string.Empty; // GESTOR(A), PEDAGOGO(A), SECRETARIO(A), ADMINISTRADOR GERAL
        public string NomeEscola { get; set; } = string.Empty;
        public string CodigoEscola { get; set; } = string.Empty;
        public bool Ativo { get; set; } = true;

        public Usuario() { }

        public Usuario(int id, string nome, string email, string senha, string perfil, string nomeEscola = "", string codigoEscola = "")
        {
            Id = id;
            Nome = nome;
            Email = email;
            Senha = senha;
            Perfil = perfil;
            NomeEscola = nomeEscola;
            CodigoEscola = codigoEscola;
            Ativo = true;
        }
    }
}