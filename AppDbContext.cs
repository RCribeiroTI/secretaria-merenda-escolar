using Microsoft.EntityFrameworkCore;
using SecretariaMerenda.Models;

namespace SecretariaMerenda
{
    public class AppDbContext : DbContext
    {
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Aluno> Alunos { get; set; }
        public DbSet<ItemEstoque> Estoque { get; set; }
        public DbSet<HistoricoMovimentacao> HistoricoMovimentacoes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=secretaria_merenda.db");
        }
    }
}