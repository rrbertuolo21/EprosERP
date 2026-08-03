using System;
using Epros.Shared.Application.Models;
using MediatR;

namespace Epros.Modules.Financeiro.Application.Commands
{
    // ----- Arrendamento mercantil (IFRS-16 / CPC 06 R2) — FIN-GCF extensão -----

    /// <summary>
    /// Cria um contrato de arrendamento e calcula o reconhecimento inicial (passivo = VP das
    /// contraprestações à taxa incremental; direito de uso correspondente). Taxa/prazo INFORMADOS.
    /// </summary>
    public record CriarContratoArrendamentoCommand(
        string Descricao, Guid? PessoaId, DateTime DataInicio, decimal ValorContraprestacao,
        int QuantidadeParcelas, decimal TaxaIncrementalPeriodo, bool PagamentoAntecipado = false,
        decimal CustosDiretosIniciais = 0m, decimal IncentivosRecebidos = 0m) : IRequest<CommandResult>;

    public record EncerrarContratoArrendamentoCommand(Guid Id, string Motivo, DateTime? DataEncerramento) : IRequest<CommandResult>;
}
