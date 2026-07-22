using System;
using System.Collections.Generic;

namespace Epros.Modules.Aplicativo.Application.Dtos
{
    public record UsuarioDto(
        Guid Id,
        string Nome,
        string Email,
        string? Telefone,
        string Status,
        string Tipo,
        bool MfaHabilitado,
        string? ApiKey
    );

    public record UsuarioEmpresaVinculoDto(
        Guid EmpresaId,
        string RazaoSocial,
        bool EhAdmin,
        Guid? PerfilUsuarioId,
        string Cargo,
        string Departamento,
        decimal LimiteDesconto
    );

    public record PreferenciaUsuarioDto(
        string? Idioma,
        string? Tema,
        string? Avatar,
        bool RecebeNotificacoes,
        string? Timezone,
        string? FormatoData,
        string? FormatoHora,
        string? FormatoNumero,
        string? PreferenciasJson
    );

    public record UsuarioDetalhadoDto(
        Guid Id,
        string Nome,
        string Email,
        string? Telefone,
        string Status,
        string Tipo,
        bool MfaHabilitado,
        string? ApiKey,
        DateTime? ApiKeyCreated,
        DateTime? ApiKeyExpiration,
        DateTime? ApiKeyLastUsed,
        int ApiKeyRateLimit,
        PreferenciaUsuarioDto? Preferencias,
        IEnumerable<UsuarioEmpresaVinculoDto> Empresas
    );

    public record HistoricoLoginDto(
        Guid Id,
        string EmailTentado,
        string IpAddress,
        string UserAgent,
        bool Sucesso,
        string? Detalhes,
        DateTime CriadoEm
    );

    public record ImpersonacaoResultDto(
        string Token,
        DateTime Expiracao,
        Guid UsuarioAlvoId,
        string NomeAlvo,
        Guid? EmpresaId,
        string TenantId,
        Guid SessaoImpersonacaoId
    );
}
