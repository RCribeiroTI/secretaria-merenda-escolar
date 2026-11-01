using System;
using System.Collections.Generic;
using SecretariaMerenda.Models;

namespace SecretariaMerenda
{
    class Program
    {
        static void Main(string[] args)
        {
            // Base de usuários simulada (GESTOR, PEDAGOGO, SECRETARIO)
            List<Usuario> usuarios = new List<Usuario>
            {
                new Usuario(1, "Ana Gestora", "gestor@merenda.gov.br", "123456", "GESTOR"),
                new Usuario(2, "Carlos Pedagogo", "pedagogo@escola.gov.br", "123456", "PEDAGOGO"),
                new Usuario(3, "Mariana Secretaria", "secretaria@escola.gov.br", "123456", "SECRETARIO")
            };

            Console.WriteLine("============================================");
            Console.WriteLine("   SECRETARIA DE MERENDA ESCOLAR - LOGIN    ");
            Console.WriteLine("============================================");

            Console.Write("Digite o Email: ");
            string email = Console.ReadLine() ?? "";
            Console.Write("Digite a Senha: ");
            string senha = Console.ReadLine() ?? "";

            Usuario? userLogado = usuarios.Find(u => u.Autenticar(email, senha));

            if (userLogado == null)
            {
                Console.WriteLine("\n❌ Acesso Negado: Email ou Senha incorretos.");
                return;
            }

            Console.WriteLine($"\n✅ Login efetuado! Bem-vindo(a), {userLogado.Nome} | Perfil: [{userLogado.Perfil}]");
            Console.WriteLine("============================================");

            // --- TESTE DE ALERTAS DE ESTOQUE ---
            Console.WriteLine("\n--- 🔔 PAINEL DE ALERTAS DO ESTOQUE ---");
            ItemEstoque leite = new ItemEstoque(1, "Leite Integral", 3.0m, "Litros", DateTime.Now.AddDays(4));

            if (leite.TemEstoqueCritico())
                Console.WriteLine($"⚠️ ESTOQUE CRÍTICO: '{leite.NomeAlimento}' tem apenas {leite.Quantidade} {leite.UnidadeMedida} restantes!");

            if (leite.EstaProximoDaValidade(7))
                Console.WriteLine($"🚨 ALERTA DE VALIDADE: '{leite.NomeAlimento}' vence em {leite.DataValidade:dd/MM/yyyy}!");

            // --- TESTE DE RESTRIÇÃO ALIMENTAR NO CARDÁPIO ---
            Console.WriteLine("\n--- ⚠️ ALERTA DE SAÚDE / REFEIÇÃO ---");
            Aluno aluno1 = new Aluno(1, "Joãozinho", "2026002", "4º Ano B", "Lactose");
            Cardapio cardapioHoje = new Cardapio(1, "Segunda-feira", "Lanche", "Vitamina de Leite com Banana", DateTime.Now);

            if (cardapioHoje.PossuiRiscoParaAluno(aluno1))
            {
                Console.WriteLine($"🚨 ATENÇÃO: O prato '{cardapioHoje.DescricaoPrato}' oferece risco ao aluno {aluno1.Nome} (Restrição: {aluno1.RestricaoAlimentar})!");
            }

            Console.WriteLine("\n============================================");
        }
    }
}