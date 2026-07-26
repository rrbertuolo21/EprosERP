using System;
using System.Collections.Generic;
using Epros.Shared.Application.Models;
using MediatR;

namespace Epros.Modules.Financeiro.Application.Commands
{
    // ----- Grupo de Consolidação -----
    public record CriarGrupoConsolidacaoCommand(string? Codigo, string? Nome, string? Descricao) : IRequest<CommandResult>;
    public record AtualizarGrupoConsolidacaoCommand(Guid Id, string? Codigo, string? Nome, string? Descricao) : IRequest<CommandResult>;
    public record AdicionarEmpresaGrupoCommand(Guid GrupoConsolidacaoId, Guid EmpresaId, DateTime? DataInicio, DateTime? DataFim) : IRequest<CommandResult>;
    public record RemoverEmpresaGrupoCommand(Guid Id) : IRequest<CommandResult>;

    // ----- Demonstrativo -----
    public record DemonstrativoLinhaInput(int? Ordem, string? CodigoLinha, string? DescricaoLinha, decimal? Valor, string? TipoLinha);
    public record CriarDemonstrativoCommand(Guid GrupoConsolidacaoId, string Periodo, IReadOnlyList<DemonstrativoLinhaInput> Linhas) : IRequest<CommandResult>;
    public record PublicarDemonstrativoCommand(Guid Id, Guid? UsuarioPublicacaoId) : IRequest<CommandResult>;

    // ----- Balancete Consolidado -----
    public record BalanceteLinhaInput(Guid? ContaId, string? CodigoConta, string? NomeConta, decimal? SaldoAnterior, decimal? Debito, decimal? Credito, decimal? SaldoFinal);
    public record CriarBalanceteConsolidadoCommand(Guid GrupoConsolidacaoId, string Periodo, IReadOnlyList<BalanceteLinhaInput> Linhas) : IRequest<CommandResult>;
    public record PublicarBalanceteConsolidadoCommand(Guid Id) : IRequest<CommandResult>;

    // ----- Eliminação Intercompany -----
    public record RegistrarEliminacaoIntercompanyCommand(Guid GrupoConsolidacaoId, string Periodo, Guid? EmpresaOrigemId, Guid? EmpresaDestinoId, decimal? Valor, string? Descricao) : IRequest<CommandResult>;
}
