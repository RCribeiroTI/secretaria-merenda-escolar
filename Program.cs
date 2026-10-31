using System;
using System.Collections.Generic;

namespace SecretariaMerendaAvancada
{
    // Classe que representa o registro no histórico de movimentações
    public class Transacao
    {
        public DateTime DataHora { get; set; }
        public string Tipo { get; set; } // "BAIXA" ou "REPOSICAO"
        public int Quantidade { get; set; }
        public string Detalhes { get; set; }

        public Transacao(string tipo, int quantidade, string detalhes)
        {
            DataHora = DateTime.Now;
            Tipo = tipo;
            Quantidade = quantidade;
            Detalhes = detalhes;
        }
    }

    // Classe que representa um item do estoque da merenda
    public class ItemMerenda
    {
        public string NomeItem { get; private set; }
        public int QuantidadeKg { get; private set; }
        public int EstoqueMinimoKg { get; private set; }
        
        // Novas coleções para Histórico e Mídia Digital
        public List<Transacao> Historico { get; private set; } = new List<Transacao>();
        public List<string> MidiasAnexadas { get; private set; } = new List<string>(); // URLs ou caminhos de áudio, foto, vídeo

        public ItemMerenda(string nome, int quantidade, int estoqueMinimo)
        {
            NomeItem = nome;
            QuantidadeKg = quantidade;
            EstoqueMinimoKg = estoqueMinimo;
            Historico.Add(new Transacao("CADASTRO", quantidade, "Item cadastrado no sistema."));
        }

        // Método de Reposição de Estoque
        public void AdicionarEstoque(int quantidadeEntrada, string autorizacao)
        {
            if (quantidadeEntrada <= 0)
            {
                Console.WriteLine("❌ A quantidade de reposição deve ser maior que zero.");
                return;
            }

            QuantidadeKg += quantidadeEntrada;
            Historico.Add(new Transacao("REPOSICAO", quantidadeEntrada, $"Reposição autorizada por: {autorizacao}"));
            Console.WriteLine($"✅ [REPOSIÇÃO] Adicionados {quantidadeEntrada}kg de {NomeItem}. Estoque atual: {QuantidadeKg}kg.");
        }

        // Método para registrar a saída de alimentos com Validação em 2 Etapas
        public void DarBaixa(int quantidadeConsumida, string senhaPin)
        {
            // ETAPA 1 DE SEGURANÇA: Validar se há estoque suficiente
            if (quantidadeConsumida <= 0)
            {
                Console.WriteLine("❌ Quantidade inválida para baixa.");
                return;
            }

            if (quantidadeConsumida > QuantidadeKg)
            {
                Console.WriteLine($"❌ Erro de Segurança: Tentativa de baixa ({quantidadeConsumida}kg) maior que o estoque atual ({QuantidadeKg}kg)!");
                return;
            }

            // ETAPA 2 DE SEGURANÇA: Autenticação por PIN/Senha da Secretaria
            string pinCorreto = "1234"; // Exemplo simulado de PIN seguro
            if (senhaPin != pinCorreto)
            {
                Console.WriteLine("❌ [FALHA DE SEGURANÇA] PIN incorreto! Operação de baixa cancelada.");
                return;
            }

            // Executa a baixa se passou pelas duas etapas
            QuantidadeKg -= quantidadeConsumida;
            Historico.Add(new Transacao("BAIXA", quantidadeConsumida, "Baixa diária de merenda realizada com sucesso."));
            
            Console.WriteLine($"[BAIXA] Consumidos {quantidadeConsumida}kg de {NomeItem}. Restam {QuantidadeKg}kg no estoque.");
            
            // Regra de negócio: Alerta automático de estoque crítico
            if (QuantidadeKg <= EstoqueMinimoKg)
            {
                Console.WriteLine($"⚠️ [ALERTA SECRETARIA] Estoque crítico de {NomeItem}! Restam apenas {QuantidadeKg}kg (Mínimo recomendado: {EstoqueMinimoKg}kg).");
            }
        }

        // Adicionar mídia digital (Foto da nota fiscal, Áudio de inventário, Vídeo de inspeção)
        public void AnexarMidia(string tipoMidia, string caminhoOuUrl)
        {
            string registro = $"[{tipoMidia.ToUpper()}] {caminhoOuUrl} (Adicionado em: {DateTime.Now})";
            MidiasAnexadas.Add(registro);
            Console.WriteLine($"📎 Mídia anexada com sucesso ao item {NomeItem}!");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Uso de Coleções para gerenciar múltiplos itens
            List<ItemMerenda> estoqueEscola = new List<ItemMerenda>
            {
                new ItemMerenda("Arroz Tipo 1", 50, 20),
                new ItemMerenda("Feijão Carioca", 40, 15),
                new ItemMerenda("Leite Em Pó", 30, 10)
            };

            bool executando = true;

            while (executando)
            {
                Console.Clear();
                Console.WriteLine("=== SISTEMA DE GESTÃO DE MERENDA ESCOLAR (SECRETARIA) ===");
                Console.WriteLine("🔐 [Segurança Ativa: Criptografia SSL/TLS simulada & Conexão com Servidor em nuvem]");
                Console.WriteLine();
                Console.WriteLine("1. Listar Estoque");
                Console.WriteLine("2. Dar Baixa em Alimento (Validação em 2 Etapas)");
                Console.WriteLine("3. Repor Estoque");
                Console.WriteLine("4. Ver Histórico de Movimentações");
                Console.WriteLine("5. Anexar Mídia Digital (Foto/Áudio/Vídeo)");
                Console.WriteLine("0. Sair");
                Console.Write("\nEscolha uma opção: ");

                string opcao = Console.ReadLine();
                Console.WriteLine();

                switch (opcao)
                {
                    case "1":
                        Console.WriteLine("--- ESTOQUE ATUAL ---");
                        foreach (var item in estoqueEscola)
                        {
                            string alerta = item.QuantidadeKg <= item.EstoqueMinimoKg ? " ⚠️ [CRÍTICO]" : "";
                            Console.WriteLine($"- {item.NomeItem}: {item.QuantidadeKg}kg (Mínimo: {item.EstoqueMinimoKg}kg){alerta}");
                        }
                        break;

                    case "2":
                        Console.Write("Digite o nome exato do item para baixa: ");
                        string nomeBaixa = Console.ReadLine();
                        var itemBaixa = estoqueEscola.Find(i => i.NomeItem.Equals(nomeBaixa, StringComparison.OrdinalIgnoreCase));

                        if (itemBaixa != null)
                        {
                            Console.Write("Quantidade em kg a consumir: ");
                            if (int.TryParse(Console.ReadLine(), out int qtd))
                            {
                                // Validação de Segurança em 2 Etapas: Solicita PIN
                                Console.Write("🔐 Digite o PIN de Segurança da Secretaria (Ex: 1234): ");
                                string pin = Console.ReadLine();

                                itemBaixa.DarBaixa(qtd, pin);
                            }
                            else
                            {
                                Console.WriteLine("❌ Quantidade inválida.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("❌ Item não encontrado no estoque.");
                        }
                        break;

                    case "3":
                        Console.Write("Digite o nome exato do item para reposição: ");
                        string nomeReposta = Console.ReadLine();
                        var itemReposta = estoqueEscola.Find(i => i.NomeItem.Equals(nomeReposta, StringComparison.OrdinalIgnoreCase));

                        if (itemReposta != null)
                        {
                            Console.Write("Quantidade em kg para adicionar: ");
                            if (int.TryParse(Console.ReadLine(), out int qtdRep))
                            {
                                Console.Write("Nome do responsável pela autorização: ");
                                string resp = Console.ReadLine();

                                itemReposta.AdicionarEstoque(qtdRep, resp);
                            }
                            else
                            {
                                Console.WriteLine("❌ Quantidade inválida.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("❌ Item não encontrado.");
                        }
                        break;

                    case "4":
                        Console.Write("Digite o nome do item para ver o histórico: ");
                        string nomeHist = Console.ReadLine();
                        var itemHist = estoqueEscola.Find(i => i.NomeItem.Equals(nomeHist, StringComparison.OrdinalIgnoreCase));

                        if (itemHist != null)
                        {
                            Console.WriteLine($"--- HISTÓRICO DE {itemHist.NomeItem.ToUpper()} ---");
                            foreach (var transacao in itemHist.Historico)
                            {
                                Console.WriteLine($"[{transacao.DataHora}] {transacao.Tipo}: {transacao.Quantidade}kg - {transacao.Detalhes}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("❌ Item não encontrado.");
                        }
                        break;

                    case "5":
                        Console.Write("Digite o nome do item para anexar mídia: ");
                        string nomeMidia = Console.ReadLine();
                        var itemMidia = estoqueEscola.Find(i => i.NomeItem.Equals(nomeMidia, StringComparison.OrdinalIgnoreCase));

                        if (itemMidia != null)
                        {
                            Console.Write("Tipo de mídia (Foto, Audio, Video): ");
                            string tipo = Console.ReadLine();
                            Console.Write("Caminho do arquivo ou URL (ex: /imagens/nota1.jpg): ");
                            string caminho = Console.ReadLine();

                            itemMidia.AnexarMidia(tipo, caminho);
                        }
                        else
                        {
                            Console.WriteLine("❌ Item não encontrado.");
                        }
                        break;

                    case "0":
                        executando = false;
                        Console.WriteLine("Encerrando o sistema de merenda... Até logo!");
                        break;

                    default:
                        Console.WriteLine("❌ Opção inválida. Tente novamente.");
                        break;
                }

                if (executando)
                {
                    Console.WriteLine("\nPressione qualquer tecla para continuar...");
                    Console.ReadKey();
                }
            }
        }
    }
}
