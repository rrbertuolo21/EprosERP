using System;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Vendas.Application.Commands
{
    // ===================== Portal do Cliente (VEN-PCL) =====================
    // Fonte: EF_7_VENDAS_PORTAL_DO_CLIENTE_V1. Estilo B (ICommand).

    public record CriarPortalUsuarioClienteCommand(
        Guid ClienteId,
        string Nome,
        string Email,
        string? Telefone,
        bool AdministradorCliente) : ICommand;

    public record DefinirPortalPermissaoCommand(
        Guid UsuarioClienteId,
        EPortalRecurso Recurso,
        bool PodeVisualizar,
        bool PodeCriar,
        bool PodeBaixar,
        bool PodeAdministrar) : ICommand;

    public record CriarPortalFormularioCommand(string? Codigo, string Nome, string? Descricao, bool Publico, string? ConfiguracaoCampos) : ICommand;

    public record PublicarPortalFormularioCommand(Guid FormularioId) : ICommand;

    public record AtribuirPortalFormularioResponsavelCommand(Guid FormularioId, Guid UsuarioInternoId, string? Papel) : ICommand;

    public record AbrirPortalSolicitacaoCommand(
        Guid? ClienteId,
        Guid? UsuarioClienteId,
        Guid? FormularioId,
        string? Assunto,
        string? Descricao,
        string? DadosFormulario) : ICommand;

    public record ResponderPortalSolicitacaoCommand(Guid SolicitacaoId) : ICommand;
}
