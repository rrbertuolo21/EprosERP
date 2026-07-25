using System.Collections.Generic;
using System.Linq;

namespace Epros.Shared.Application.Models
{
    public class CommandResult
    {
        public bool Sucesso { get; private set; }
        public string? Mensagem { get; private set; }
        public object? Dados { get; private set; }
        public bool Block { get; private set; }
        public IEnumerable<string> Erros { get; private set; } = Enumerable.Empty<string>();

        protected CommandResult() { }

        public static CommandResult Ok(string mensagem, object? dados = null)
        {
            return new CommandResult
            {
                Sucesso = true,
                Mensagem = mensagem,
                Dados = dados
            };
        }

        public static CommandResult Falha(IEnumerable<string> erros, string? mensagem = null, object? dados = null, bool block = false)
        {
            return new CommandResult
            {
                Sucesso = false,
                Mensagem = mensagem ?? "Ocorreram erros de validação/processamento.",
                Erros = erros,
                Dados = dados,
                Block = block
            };
        }

        public static CommandResult Falha(string erro, string? mensagem = null, bool block = false)
        {
            return new CommandResult
            {
                Sucesso = false,
                Mensagem = mensagem ?? "Ocorreu um erro no processamento.",
                Erros = new List<string> { erro },
                Block = block
            };
        }
    }
}
