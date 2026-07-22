using System;
using System.Collections.Generic;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    // ===== Papel =====
    public record CriarPapelCommand(
        string Name,
        string? Label,
        string? GuardName,
        bool Editable,
        bool RoleSystem,
        ETipoPapel? RoleType,
        string? RoleHomepage,
        string? Modules,
        List<Guid>? CapacidadeIds
    ) : ICommand;

    public record AtualizarPapelCommand(
        Guid Id,
        string Name,
        string? Label,
        string? GuardName,
        bool Editable,
        ETipoPapel? RoleType,
        string? RoleHomepage,
        string? Modules,
        List<Guid>? CapacidadeIds
    ) : ICommand;

    public record DeletarPapelCommand(Guid Id) : ICommand;

    // ===== Capacidade =====
    public record CriarCapacidadeCommand(
        string Name,
        string? Label,
        string? Module,
        string? AddOn,
        string? PermissionKey
    ) : ICommand;

    public record AtualizarCapacidadeCommand(
        Guid Id,
        string Name,
        string? Label,
        string? Module,
        string? AddOn,
        string? PermissionKey
    ) : ICommand;

    public record DeletarCapacidadeCommand(Guid Id) : ICommand;

    // ===== Atribuição de papel a usuário =====
    public record AtribuirPapelUsuarioCommand(Guid UsuarioId, Guid PapelId, string? ModelType) : ICommand;
    public record RemoverPapelUsuarioCommand(Guid Id) : ICommand;

    // ===== Nível de usuário =====
    public record CriarNivelUsuarioCommand(
        int LevelId,
        string Label,
        bool CanUpload,
        int WaitBetweenDownloads,
        int DownloadSpeed,
        long MaxStorageBytes,
        bool ShowSiteAdverts,
        bool ShowUpgradeScreen,
        int DaysToKeepInactiveFiles,
        int ConcurrentUploads,
        int ConcurrentDownloads,
        int DownloadsPer24Hours,
        long MaxDownloadFilesizeAllowed,
        int MaxRemoteDownloadUrls,
        long MaxUploadSize,
        ENivelUsuarioTipo LevelType,
        bool OnUpgradePage
    ) : ICommand;

    public record AdicionarPrecoNivelUsuarioCommand(
        Guid NivelUsuarioId,
        string PricingLabel,
        string PackagePricingType,
        string? Period,
        long? DownloadAllowance,
        decimal Price
    ) : ICommand;
}
