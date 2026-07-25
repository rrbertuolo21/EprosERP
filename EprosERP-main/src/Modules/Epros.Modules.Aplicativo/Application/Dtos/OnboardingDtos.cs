using System;
using System.Collections.Generic;

namespace Epros.Modules.Aplicativo.Application.Dtos
{
    public record ConfiguracaoEmpresaDto(
        Guid EmpresaId,
        string Nome,
        string Email,
        string? Telefone,
        string? Endereco,
        int TimeZoneId,
        string DateFormat,
        int CurrencyId,
        decimal VatPercentage,
        int VatType,
        int CurrencyPosition,
        string? FooterText,
        string? Logo,
        string? Favicon
    );

    public record IdiomaDto(
        string Code,
        string Name,
        string CountryCode,
        bool Enabled
    );

    public record ContextoSessaoDto(
        string TenantId,
        Guid UsuarioId,
        int QtdeCadastroEmpresa,
        int QtdeCadastroUsuario,
        bool Block,
        IEnumerable<UsuarioEmpresaDto> Empresas
    );
}
