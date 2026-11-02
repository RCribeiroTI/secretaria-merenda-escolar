# 🏫 SecretariaMerenda - Sistema de Gestão de Merenda Escolar

Sistema desenvolvido em **C# (.NET 10)** com **Entity Framework Core** e **SQLite**, focado no controle logístico de merendas escolares utilizando o princípio **FEFO** (*First Expired, First Out* - Primeiro que Vence, Primeiro que Sai).

---

## 🚀 Funcionalidades Principais

* **Segurança e Autenticação:**
  * Criptografia de senhas utilizando o padrão **BCrypt**.
  * Validação de senhas fortes (mínimo de 8 caracteres, letras, números e símbolos).
  * Proteção contra força bruta (bloqueio temporário após 3 tentativas incorretas).
  * Controles de acesso por perfis (`Administrador Geral`, `Gestor(a)`, `Secretário(a)`, `Pedagogo(a)`).
  * Cadastro público com aprovação pendente pelo Administrador.

* **Gestão de Estoque (FEFO):**
  * Cadastro de gêneros alimentícios com quantidade e unidade de medida.
  * Ordenação automática por **data de validade (FEFO)**.
  * Alertas visuais automáticos: `⚠️ VENCIMENTO PRÓXIMO` (≤ 7 dias) e `🚨 QTD CRÍTICA` (≤ 5 unidades).
  * Baixa controlada de itens no estoque com exclusão automática de saldo zerado.

* **Auditoria e Transparência:**
  * Registro detalhado de logs de todas as ações no banco SQLite (`HistoricoMovimentacoes`).
  * Relatório geral de auditoria acessível ao Administrador Geral.

---

## 🛠️ Tecnologias Utilizadas

* **Linguagem:** C# (.NET 10 SDK)
* **Banco de Dados:** SQLite (`secretaria_merenda.db`)
* **ORM:** Entity Framework Core (`Microsoft.EntityFrameworkCore.Sqlite`)
* **Segurança:** BCrypt.Net-Next

---

## 📦 Como Executar o Projeto Localmente

1. **Abra o terminal na pasta do projeto:**
   ```bash
   cd SecretariaMerenda