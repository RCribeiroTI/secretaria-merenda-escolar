namespace SecretariaMerenda.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string Perfil { get; set; } = string.Empty; // GESTOR, PEDAGOGO ou SECRETARIO
        public bool Ativo { get; set; } = true;

        public Usuario(int id, string nome, string email, string senha, string perfil)
        {
            Id = id;
            Nome = nome;
            Email = email;
            Senha = senha;
            Perfil = perfil.ToUpper();
        }

        public bool Autenticar(string emailDigitado, string senhaDigitada)
        {
            return Email.Equals(emailDigitado, StringComparison.OrdinalIgnoreCase) && Senha == senhaDigitada && Ativo;
        }
    }
}