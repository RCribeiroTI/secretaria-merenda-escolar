using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SecretariaMerenda.Models;

namespace SecretariaMerenda
{
    class Program
    {
        private static readonly Dictionary<string, (int tentativas, DateTime? bloqueioAte)> _falhasLogin = new();

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();

                if (!db.Usuarios.Any())
                {
                    Console.Clear();
                    Console.WriteLine("========================================================");
                    Console.WriteLine(" 🛡️ [PRIMEIRA EXECUÇÃO] Configure o Administrador Geral");
                    Console.WriteLine("========================================================");
                    Console.WriteLine("Email do admin: admin@merenda.gov.br\n");

                    string senhaAdmin;
                    while (true)
                    {
                        Console.Write("Defina a senha do administrador: ");
                        senhaAdmin = LerSenhaSegura();

                        if (!ValidarSenhaForte(senhaAdmin, out string erroSenha))
                        {
                            Console.WriteLine($"❌ {erroSenha}\n");
                            continue;
                        }

                        Console.Write("Confirme a senha: ");
                        string confirmacao = LerSenhaSegura();

                        if (senhaAdmin != confirmacao)
                        {
                            Console.WriteLine("❌ As senhas não conferem.\n");
                            continue;
                        }
                        break;
                    }

                    string senhaHashAdmin = BCrypt.Net.BCrypt.HashPassword(senhaAdmin);
                    db.Usuarios.Add(new Usuario(0, "Administrador Sistema", "admin@merenda.gov.br", senhaHashAdmin, "ADMINISTRADOR GERAL", "Sede Central", "ADM001"));
                    db.SaveChanges();

                    Console.WriteLine("\n✅ Administrador criado com sucesso!");
                    Console.WriteLine("Pressione qualquer tecla para continuar...");
                    Console.ReadKey(true);
                }
            }

            bool sistemaRodando = true;

            while (sistemaRodando)
            {
                Usuario? userLogado = null;
                bool emExecucaoLogin = true;

                while (emExecucaoLogin && userLogado == null)
                {
                    Console.Clear();
                    Console.WriteLine("============================================");
                    Console.WriteLine("         MERENDA ESCOLAR DIGITAL            ");
                    Console.WriteLine("============================================");
                    Console.WriteLine("1. Acessar o Sistema (Login)");
                    Console.WriteLine("2. Cadastrar Novo Usuário (Pendente de Aprovação)");
                    Console.WriteLine("0. Sair do Sistema");
                    Console.WriteLine("============================================");
                    Console.Write("Escolha uma opção: ");

                    while (Console.KeyAvailable) Console.ReadKey(true);
                    string opcaoInicial = Console.ReadLine()?.Trim() ?? "";

                    switch (opcaoInicial)
                    {
                        case "1":
                            userLogado = EfetuarLogin();
                            break;
                        case "2":
                            CadastrarUsuarioPublico();
                            break;
                        case "0":
                            sistemaRodando = false;
                            emExecucaoLogin = false;
                            Console.WriteLine("\nSaindo do sistema... Até logo!");
                            return;
                        default:
                            Console.WriteLine("\n❌ Opção inválida! Pressione Enter.");
                            Console.ReadKey(true);
                            break;
                    }
                }

                if (userLogado == null) continue;

                RegistrarHistorico(userLogado, "Login no Sistema", $"Usuário {userLogado.Nome} acessou o sistema.", userLogado.CodigoEscola);

                Console.Clear();
                Console.WriteLine($"\n✅ Login Efetuado! Bem-vindo(a), {userLogado.Nome} [{userLogado.Perfil}]");
                Console.WriteLine($"🏫 Escola: {userLogado.NomeEscola} (Cód: {userLogado.CodigoEscola})");
                Console.WriteLine("\nPressione qualquer tecla para abrir o menu...");
                Console.ReadKey(true);

                bool rodandoMenu = true;
                while (rodandoMenu)
                {
                    Console.Clear();
                    Console.WriteLine("============================================");
                    Console.WriteLine($" MENU PRINCIPAL - MERENDA");
                    Console.WriteLine($" Usuário: {userLogado.Nome} [{userLogado.Perfil}]");
                    Console.WriteLine($" Escola: {userLogado.NomeEscola} | Cód: {userLogado.CodigoEscola}");
                    Console.WriteLine("============================================");

                    bool ehAdmin = userLogado.Perfil.ToUpper() == "ADMINISTRADOR GERAL";
                    bool ehGestorOuPedagogo = userLogado.Perfil.ToUpper() is "GESTOR(A)" or "PEDAGOGO(A)" or "ADMINISTRADOR GERAL";
                    bool ehSecretarioOuGestor = userLogado.Perfil.ToUpper() is "GESTOR(A)" or "SECRETARIO(A)" or "ADMINISTRADOR GERAL";

                    if (ehSecretarioOuGestor) Console.WriteLine("1. Gestão de Estoque e Alertas (FEFO / Baixas)");
                    if (ehGestorOuPedagogo || ehAdmin) Console.WriteLine("2. Relatório de Histórico e Auditoria");
                    Console.WriteLine("3. Outros Registros e Mídia");
                    if (ehAdmin) Console.WriteLine("4. Painel do Administrador Geral");
                    Console.WriteLine("0. Fazer Logout (Trocar de Usuário)");
                    Console.WriteLine("============================================");
                    Console.Write("Escolha uma opção: ");

                    while (Console.KeyAvailable) Console.ReadKey(true);
                    string opcao = Console.ReadLine()?.Trim() ?? "";

                    switch (opcao)
                    {
                        case "1":
                            if (ehSecretarioOuGestor) MenuEstoqueEAlertas(userLogado);
                            else AcessoNegado();
                            break;
                        case "2":
                            if (ehGestorOuPedagogo || ehAdmin) ListarHistoricoMovimentacoes(userLogado);
                            else AcessoNegado();
                            break;
                        case "3":
                            MenuOutrosAssuntos();
                            break;
                        case "4":
                            if (ehAdmin) MenuAdmGeral(userLogado);
                            else AcessoNegado();
                            break;
                        case "0":
                            rodandoMenu = false;
                            RegistrarHistorico(userLogado, "Logout", $"Usuário {userLogado.Nome} saiu do sistema.", userLogado.CodigoEscola);
                            Console.WriteLine("\nSaindo para a tela de login...");
                            System.Threading.Thread.Sleep(1200);
                            break;
                        default:
                            Console.WriteLine("\n❌ Opção inválida! Pressione Enter.");
                            Console.ReadKey(true);
                            break;
                    }
                }
            }
        }

        // --- SUBMENU DE GESTÃO DE ESTOQUE (FEFO) ---
        static void MenuEstoqueEAlertas(Usuario userLogado)
        {
            bool noEstoque = true;
            while (noEstoque)
            {
                Console.Clear();
                Console.WriteLine("============================================");
                Console.WriteLine("   📦 GESTÃO DE ESTOQUE E ALERTAS (FEFO)    ");
                Console.WriteLine("============================================");
                Console.WriteLine("1. Cadastrar Novo Item no Estoque");
                Console.WriteLine("2. Listar Estoque (Ordenado por Validade)");
                Console.WriteLine("3. Dar Baixa em Item do Estoque");
                Console.WriteLine("0. Voltar ao Menu Principal");
                Console.WriteLine("============================================");
                Console.Write("Escolha uma opção: ");

                string op = Console.ReadLine()?.Trim() ?? "";

                switch (op)
                {
                    case "1":
                        CadastrarAlimentoEstoque(userLogado);
                        break;
                    case "2":
                        ListarEstoqueFEFO(userLogado);
                        break;
                    case "3":
                        DarBaixaEstoque(userLogado);
                        break;
                    case "0":
                        noEstoque = false;
                        break;
                    default:
                        Console.WriteLine("\n❌ Opção inválida!");
                        Console.ReadKey(true);
                        break;
                }
            }
        }

        static void CadastrarAlimentoEstoque(Usuario userLogado)
        {
            Console.Clear();
            Console.WriteLine("--- 🍏 CADASTRO DE NOVO ALIMENTO ---");
            
            Console.Write("Nome do Alimento (ex: Arroz Parboilizado 5kg): ");
            string nome = Console.ReadLine()?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(nome)) { ExibirErro("Nome obrigatório."); return; }

            Console.Write("Quantidade: ");
            if (!decimal.TryParse(Console.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal qtd) || qtd <= 0)
            {
                ExibirErro("Quantidade inválida.");
                return;
            }

            Console.Write("Unidade de Medida (ex: kg, L, pacotes, caixas): ");
            string unidade = Console.ReadLine()?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(unidade)) { ExibirErro("Unidade de medida obrigatória."); return; }

            Console.Write("Data de Validade (DD/MM/AAAA): ");
            if (!DateTime.TryParseExact(Console.ReadLine()?.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dataValidade))
            {
                ExibirErro("Data inválida. Use o formato DD/MM/AAAA.");
                return;
            }

            using (var db = new AppDbContext())
            {
                var item = new ItemEstoque(nome, qtd, unidade, dataValidade, userLogado.CodigoEscola);
                db.Estoque.Add(item);
                db.SaveChanges();

                RegistrarHistorico(userLogado, "Entrada no Estoque", $"Adicionado: {nome} ({qtd} {unidade}) com validade para {dataValidade:dd/MM/yyyy}.", userLogado.CodigoEscola);
                Console.WriteLine($"\n✅ Item '{nome}' cadastrado com sucesso no estoque!");
            }
            Console.ReadKey(true);
        }

        static void ListarEstoqueFEFO(Usuario userLogado)
        {
            Console.Clear();
            Console.WriteLine("--- 📋 ITENS EM ESTOQUE (ORDEM FEFO - PRIMEIRO QUE VENCE, PRIMEIRO QUE SAI) ---");

            using (var db = new AppDbContext())
            {
                var itens = db.Estoque
                    .Where(i => userLogado.Perfil.ToUpper() == "ADMINISTRADOR GERAL" || i.CodigoEscola == userLogado.CodigoEscola)
                    .OrderBy(i => i.DataValidade)
                    .ToList();

                if (!itens.Any())
                {
                    Console.WriteLine("\nNenhum alimento cadastrado no estoque desta escola.");
                }
                else
                {
                    foreach (var i in itens)
                    {
                        string alertaVencimento = i.DataValidade <= DateTime.Now.AddDays(7) ? " ⚠️ VENCIMENTO PRÓXIMO!" : "";
                        string alertaQtd = i.TemEstoqueCritico() ? " 🚨 QTD CRÍTICA!" : "";
                        Console.WriteLine($"[ID: {i.Id}] {i.NomeAlimento} | Qtd: {i.Quantidade} {i.UnidadeMedida} | Validade: {i.DataValidade:dd/MM/yyyy}{alertaVencimento}{alertaQtd}");
                    }
                }
            }
            Console.WriteLine("\nPressione Enter para voltar.");
            Console.ReadKey(true);
        }

        static void DarBaixaEstoque(Usuario userLogado)
        {
            Console.Clear();
            Console.WriteLine("--- 📉 BAIXA DE ITEM NO ESTOQUE ---");

            using (var db = new AppDbContext())
            {
                var itens = db.Estoque
                    .Where(i => userLogado.Perfil.ToUpper() == "ADMINISTRADOR GERAL" || i.CodigoEscola == userLogado.CodigoEscola)
                    .OrderBy(i => i.DataValidade)
                    .ToList();

                if (!itens.Any())
                {
                    ExibirErro("Não há itens no estoque para dar baixa.");
                    return;
                }

                foreach (var i in itens)
                {
                    Console.WriteLine($"[ID: {i.Id}] {i.NomeAlimento} - Qtd Disponível: {i.Quantidade} {i.UnidadeMedida} (Val: {i.DataValidade:dd/MM/yyyy})");
                }

                Console.Write("\nDigite o ID do item para dar baixa: ");
                if (!int.TryParse(Console.ReadLine(), out int id)) { ExibirErro("ID inválido."); return; }

                var item = db.Estoque.FirstOrDefault(i => i.Id == id);
                if (item == null) { ExibirErro("Item não encontrado."); return; }

                Console.Write($"Quantidade a retirar (Atual: {item.Quantidade} {item.UnidadeMedida}): ");
                if (!decimal.TryParse(Console.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal qtdBaixa) || qtdBaixa <= 0)
                {
                    ExibirErro("Quantidade inválida.");
                    return;
                }

                if (qtdBaixa > item.Quantidade)
                {
                    ExibirErro("A quantidade informada é maior do que o saldo em estoque.");
                    return;
                }

                item.Quantidade -= qtdBaixa;
                if (item.Quantidade == 0)
                {
                    db.Estoque.Remove(item);
                }

                db.SaveChanges();
                RegistrarHistorico(userLogado, "Baixa no Estoque", $"Baixa de {qtdBaixa} {item.UnidadeMedida} no item {item.NomeAlimento}.", userLogado.CodigoEscola);
                Console.WriteLine($"\n✅ Baixa efetuada com sucesso! Saldo restante: {item.Quantidade} {item.UnidadeMedida}.");
            }
            Console.ReadKey(true);
        }

        static void AcessoNegado()
        {
            Console.WriteLine("\n❌ Acesso negado: Seu perfil não possui permissão para acessar este módulo.");
            Console.WriteLine("Pressione Enter para continuar.");
            Console.ReadKey(true);
        }

        static bool ValidarSenhaForte(string senha, out string mensagemErro)
        {
            if (string.IsNullOrWhiteSpace(senha) || senha.Length < 8)
            {
                mensagemErro = "A senha deve ter no mínimo 8 caracteres.";
                return false;
            }

            bool temLetra = Regex.IsMatch(senha, @"[a-zA-Z]");
            bool temNumero = Regex.IsMatch(senha, @"[0-9]");
            bool temEspecial = Regex.IsMatch(senha, @"[@#$%&*!?\-_=+]");

            if (!temLetra || !temNumero || !temEspecial)
            {
                mensagemErro = "A senha deve conter letras, números e pelo menos um caractere especial (@#$%&*!?-_=+).";
                return false;
            }

            mensagemErro = string.Empty;
            return true;
        }

        static bool ValidarEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        static void RegistrarHistorico(Usuario user, string acao, string detalhes, string codigoEscolaAlvo)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var historico = new HistoricoMovimentacao(user.Nome, user.Perfil, acao, detalhes, codigoEscolaAlvo);
                    db.HistoricoMovimentacoes.Add(historico);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string logMsg = $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] Erro Auditoria: {ex.Message}\n";
                File.AppendAllText("log_erros.txt", logMsg);
            }
        }

        static string LerSenhaSegura()
        {
            StringBuilder senha = new StringBuilder();
            ConsoleKeyInfo tecla;
            do
            {
                tecla = Console.ReadKey(true);
                if (tecla.Key != ConsoleKey.Backspace && tecla.Key != ConsoleKey.Enter)
                {
                    senha.Append(tecla.KeyChar);
                    Console.Write("*");
                }
                else if (tecla.Key == ConsoleKey.Backspace && senha.Length > 0)
                {
                    senha.Remove(senha.Length - 1, 1);
                    Console.Write("\b \b");
                }
            } while (tecla.Key != ConsoleKey.Enter);
            Console.WriteLine();
            return senha.ToString();
        }

        static Usuario? EfetuarLogin()
        {
            Console.Clear();
            Console.WriteLine("--- 🔑 LOGIN ---");
            Console.Write("Digite o Email: ");
            string email = Console.ReadLine()?.Trim().ToLower() ?? "";

            if (_falhasLogin.TryGetValue(email, out var statusBloqueio) && statusBloqueio.bloqueioAte.HasValue)
            {
                if (DateTime.Now < statusBloqueio.bloqueioAte.Value)
                {
                    int segundosRestantes = (int)(statusBloqueio.bloqueioAte.Value - DateTime.Now).TotalSeconds;
                    Console.WriteLine($"\n❌ Conta temporariamente bloqueada. Tente em {segundosRestantes} segundos.");
                    Console.ReadKey(true);
                    return null;
                }
                else
                {
                    _falhasLogin[email] = (0, null);
                }
            }

            Console.Write("Digite a Senha: ");
            string senha = LerSenhaSegura();

            using (var db = new AppDbContext())
            {
                var usuario = db.Usuarios.FirstOrDefault(u => u.Email.ToLower() == email);

                if (usuario == null || !usuario.Ativo || !BCrypt.Net.BCrypt.Verify(senha, usuario.Senha))
                {
                    int tentativas = 1;
                    if (_falhasLogin.ContainsKey(email))
                    {
                        tentativas = _falhasLogin[email].tentativas + 1;
                    }

                    DateTime? bloqueio = null;
                    if (tentativas >= 3)
                    {
                        bloqueio = DateTime.Now.AddSeconds(30);
                        Console.WriteLine("\n❌ Conta bloqueada por 30 segundos devido a 3 tentativas incorretas.");
                    }
                    else
                    {
                        Console.WriteLine($"\n❌ Acesso Negado: Credenciais inválidas ({tentativas}/3).");
                    }

                    _falhasLogin[email] = (tentativas, bloqueio);
                    Console.WriteLine("Pressione Enter para continuar.");
                    Console.ReadKey(true);
                    return null;
                }

                _falhasLogin[email] = (0, null);
                return usuario;
            }
        }

        static void CadastrarUsuarioPublico()
        {
            Console.Clear();
            Console.WriteLine("--- 👤 CADASTRO DE NOVO USUÁRIO ---");
            Console.Write("Nome completo: ");
            string nome = Console.ReadLine()?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(nome)) { ExibirErro("Nome obrigatório."); return; }

            Console.Write("Email: ");
            string email = Console.ReadLine()?.Trim() ?? "";
            if (!ValidarEmail(email)) { ExibirErro("E-mail inválido."); return; }

            Console.Write("Senha: ");
            string senha = LerSenhaSegura();
            if (!ValidarSenhaForte(senha, out string erroSenha)) { ExibirErro(erroSenha); return; }

            Console.Write("Nome da Escola: ");
            string nomeEscola = Console.ReadLine()?.Trim() ?? "";
            Console.Write("Código da Escola (Ex: ESCOLA01): ");
            string codigoEscola = Console.ReadLine()?.Trim().ToUpper() ?? "";

            if (string.IsNullOrWhiteSpace(nomeEscola) || string.IsNullOrWhiteSpace(codigoEscola))
            {
                ExibirErro("Nome e Código da Escola são obrigatórios.");
                return;
            }

            Console.WriteLine("\nSelecione o Perfil:");
            Console.WriteLine("1. Secretário(a)");
            Console.WriteLine("2. Pedagogo(a)");
            Console.Write("Opção: ");
            string op = Console.ReadLine()?.Trim() ?? "";
            string perfil = op == "2" ? "PEDAGOGO(A)" : "SECRETARIO(A)";

            using (var db = new AppDbContext())
            {
                if (db.Usuarios.Any(u => u.Email.ToLower() == email.ToLower()))
                {
                    ExibirErro("Já existe um usuário cadastrado com esse email!");
                    return;
                }

                string senhaHash = BCrypt.Net.BCrypt.HashPassword(senha);
                var novoUser = new Usuario(0, nome, email, senhaHash, perfil, nomeEscola, codigoEscola) { Ativo = false };
                db.Usuarios.Add(novoUser);
                db.SaveChanges();

                RegistrarHistorico(novoUser, "Cadastro Público", $"Solicitação de cadastro pendente para {nome} ({perfil}).", codigoEscola);
                Console.WriteLine($"\n✅ Cadastro realizado com sucesso! Sua conta está PENDENTE de aprovação por um Administrador.");
            }
            Console.ReadKey(true);
        }

        static void ExibirErro(string msg)
        {
            Console.WriteLine($"\n❌ {msg}");
            Console.WriteLine("Pressione Enter para retornar.");
            Console.ReadKey(true);
        }

        static void MenuAdmGeral(Usuario userLogado)
        {
            bool rodandoAdm = true;
            while (rodandoAdm)
            {
                Console.Clear();
                Console.WriteLine("============================================");
                Console.WriteLine("         PAINEL DO ADMINISTRADOR GERAL      ");
                Console.WriteLine("============================================");
                Console.WriteLine("1. Listar / Aprovar Usuários Cadastrados");
                Console.WriteLine("2. Cadastrar Novo Usuário (Aprovado Direto)");
                Console.WriteLine("3. Desativar / Ativar Conta de Usuário");
                Console.WriteLine("4. Redefinir Senha de um Usuário");
                Console.WriteLine("5. Relatório Geral de Auditoria do Sistema");
                Console.WriteLine("0. Voltar ao Menu Principal");
                Console.WriteLine("============================================");
                Console.Write("Escolha uma opção: ");

                while (Console.KeyAvailable) Console.ReadKey(true);
                string opcaoAdm = Console.ReadLine()?.Trim() ?? "";

                switch (opcaoAdm)
                {
                    case "1": ListarUsuariosCadastrados(); break;
                    case "2": CadastrarUsuarioAdmin(userLogado); break;
                    case "3": AlternarStatusUsuario(userLogado); break;
                    case "4": RedefinirSenhaUsuarioAdm(userLogado); break;
                    case "5": RelatorioAuditoriaGlobal(); break;
                    case "0": rodandoAdm = false; break;
                    default: Console.WriteLine("\n❌ Opção inválida!"); Console.ReadKey(true); break;
                }
            }
        }

        static void CadastrarUsuarioAdmin(Usuario adminLogado)
        {
            Console.Clear();
            Console.WriteLine("--- 👤 CADASTRO DE USUÁRIO (ADMIN) ---");
            Console.Write("Nome completo: ");
            string nome = Console.ReadLine()?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(nome)) { ExibirErro("Nome obrigatório."); return; }

            Console.Write("Email: ");
            string email = Console.ReadLine()?.Trim() ?? "";
            if (!ValidarEmail(email)) { ExibirErro("E-mail inválido."); return; }

            Console.Write("Senha: ");
            string senha = LerSenhaSegura();
            if (!ValidarSenhaForte(senha, out string erro)) { ExibirErro(erro); return; }

            Console.Write("Nome da Escola: ");
            string nomeEscola = Console.ReadLine()?.Trim() ?? "";
            Console.Write("Código da Escola: ");
            string codigoEscola = Console.ReadLine()?.Trim().ToUpper() ?? "";

            if (string.IsNullOrWhiteSpace(nomeEscola) || string.IsNullOrWhiteSpace(codigoEscola))
            {
                ExibirErro("Escola e Código obrigatórios.");
                return;
            }

            Console.WriteLine("\nSelecione o Perfil:");
            Console.WriteLine("1. Gestor(a)");
            Console.WriteLine("2. Pedagogo(a)");
            Console.WriteLine("3. Secretário(a)");
            Console.WriteLine("4. Administrador Geral");
            Console.Write("Opção: ");

            string perfil = Console.ReadLine()?.Trim() switch
            {
                "1" => "GESTOR(A)",
                "2" => "PEDAGOGO(A)",
                "3" => "SECRETARIO(A)",
                "4" => "ADMINISTRADOR GERAL",
                _ => "SECRETARIO(A)"
            };

            using (var db = new AppDbContext())
            {
                if (db.Usuarios.Any(u => u.Email.ToLower() == email.ToLower())) { ExibirErro("Email já cadastrado."); return; }

                string senhaHash = BCrypt.Net.BCrypt.HashPassword(senha);
                db.Usuarios.Add(new Usuario(0, nome, email, senhaHash, perfil, nomeEscola, codigoEscola) { Ativo = true });
                db.SaveChanges();

                RegistrarHistorico(adminLogado, "Cadastro de Usuário (Admin)", $"Admin cadastrou {nome} ({perfil}).", codigoEscola);
                Console.WriteLine($"\n✅ Usuário {nome} ({perfil}) cadastrado e ativado com sucesso!");
            }
            Console.ReadKey(true);
        }

        static void ListarUsuariosCadastrados()
        {
            Console.Clear();
            Console.WriteLine("--- 👥 USUÁRIOS CADASTRADOS ---");
            using (var db = new AppDbContext())
            {
                foreach (var u in db.Usuarios.ToList())
                {
                    string status = u.Ativo ? "Ativo" : "Pendente/Inativo";
                    Console.WriteLine($"[ID: {u.Id}] {u.Nome} - {u.Email} | Perfil: {u.Perfil} | [{status}]");
                }
            }
            Console.WriteLine("\nPressione Enter para voltar.");
            Console.ReadKey(true);
        }

        static void AlternarStatusUsuario(Usuario adminLogado)
        {
            Console.Clear();
            Console.WriteLine("--- 🔄 APROVAR / ATIVAR / DESATIVAR USUÁRIO ---");

            using (var db = new AppDbContext())
            {
                foreach (var u in db.Usuarios.ToList())
                {
                    Console.WriteLine($"[ID: {u.Id}] {u.Nome} ({u.Email}) - [{(u.Ativo ? "Ativo" : "Pendente/Inativo")}]");
                }

                Console.Write("\nDigite o ID do usuário para alternar status: ");
                if (!int.TryParse(Console.ReadLine(), out int id)) { ExibirErro("ID inválido."); return; }

                var user = db.Usuarios.FirstOrDefault(u => u.Id == id);
                if (user == null) { ExibirErro("Usuário não encontrado."); return; }

                if (user.Id == adminLogado.Id)
                {
                    ExibirErro("Você não pode alterar o status da sua própria conta!");
                    return;
                }

                user.Ativo = !user.Ativo;
                db.SaveChanges();

                string statusStr = user.Ativo ? "Ativado" : "Desativado";
                Console.WriteLine($"\n✅ Sucesso! Usuário {statusStr}.");
            }
            Console.ReadKey(true);
        }

        static void RedefinirSenhaUsuarioAdm(Usuario adminLogado)
        {
            Console.Clear();
            Console.WriteLine("--- 🔑 REDEFINIR SENHA (ADMIN) ---");

            using (var db = new AppDbContext())
            {
                foreach (var u in db.Usuarios.ToList())
                {
                    Console.WriteLine($"[ID: {u.Id}] {u.Nome} ({u.Email})");
                }

                Console.Write("\nDigite o ID do usuário: ");
                if (!int.TryParse(Console.ReadLine(), out int id)) { ExibirErro("ID inválido."); return; }

                var user = db.Usuarios.FirstOrDefault(u => u.Id == id);
                if (user == null) { ExibirErro("Usuário não encontrado."); return; }

                Console.Write("Digite a nova senha: ");
                string novaSenha = LerSenhaSegura();

                if (!ValidarSenhaForte(novaSenha, out string erro)) { ExibirErro(erro); return; }

                user.Senha = BCrypt.Net.BCrypt.HashPassword(novaSenha);
                db.SaveChanges();

                Console.WriteLine("\n✅ Senha redefinida com sucesso!");
            }
            Console.ReadKey(true);
        }

        static void RelatorioAuditoriaGlobal()
        {
            Console.Clear();
            Console.WriteLine("--- 📊 RELATÓRIO GERAL DE AUDITORIA ---");
            using (var db = new AppDbContext())
            {
                foreach (var h in db.HistoricoMovimentacoes.OrderByDescending(h => h.DataHora).Take(30).ToList())
                {
                    Console.WriteLine($"[{h.DataHora:dd/MM/yyyy HH:mm:ss}] {h.UsuarioNome} - Ação: {h.Acao}");
                }
            }
            Console.WriteLine("\nPressione Enter para voltar.");
            Console.ReadKey(true);
        }

        static void ListarHistoricoMovimentacoes(Usuario u) { RelatorioAuditoriaGlobal(); }
        static void MenuOutrosAssuntos() { Console.Clear(); Console.WriteLine("--- Outros Registros --- \n(Pressione Enter)"); Console.ReadKey(true); }
    }
}