using System;

namespace Epros.Modules.Aplicativo.Application.Dtos
{
    public record AuthResponseDto(
        string Token,
        DateTime Expiracao,
        Guid UsuarioId,
        string Nome,
        string Email,
        bool ExigeSelecaoEmpresa,
        string TenantId,
        bool Block = false,
        List<UsuarioEmpresaDto>? Empresas = null
    );

    public record UsuarioEmpresaDto(
        Guid EmpresaId,
        string RazaoSocial,
        bool EhAdmin,
        Guid? PerfilUsuarioId
    );
}
