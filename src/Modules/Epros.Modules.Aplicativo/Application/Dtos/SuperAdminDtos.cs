using System;

namespace Epros.Modules.Aplicativo.Application.Dtos
{
    public record UsuarioInternoDto(
        Guid Id,
        string Nome,
        string Email,
        string Timezone,
        bool PrimaryAdmin
    );

    public record SystemSettingDto(
        Guid Id,
        string Chave,
        string Valor,
        string Escopo,
        bool IsSecret
    );

    public record ExecucaoMassaGlobalDto(
        Guid Id,
        string Descricao,
        string ActionPayload,
        string Status,
        Guid? AprovadoPor,
        string CriadoPor,
        DateTime CriadoEm
    );

    public record CustomPageDto(
        Guid Id,
        string Slug,
        string Status,
        string Conteudo
    );

    public record NewsletterSubscriberDto(
        Guid Id,
        string Email,
        bool Ativo,
        DateTime CriadoEm
    );

    public record DashboardGlobalDto(
        int TotalTenants,
        int TotalAssinaturasAtivas,
        decimal ReceitaEstimadaMRR,
        int NovasAssinaturasMes,
        decimal ChurnRate,
        decimal ReceitaTotal
    );

    public record SuperAdminClienteDto(
        Guid Id,
        string TenantId,
        string RazaoSocial,
        string Cnpj,
        string Email,
        string PlanoNome,
        string StatusSaaS,
        bool Ativo
    );
}
