using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record AlterarLogoEmpresaCommand(Guid EmpresaId, string? Logo) : ICommand;

    public record ExcluirEmpresaCommand(Guid Id) : ICommand;

    public record UploadCertificadoDigitalCommand(
        Guid EmpresaId,
        string ArquivoBase64,
        string Senha
    ) : ICommand;

    public record ExcluirCertificadoDigitalCommand(Guid EmpresaId, Guid CertificadoId) : ICommand;

    public record TestarEmailEmpresaCommand(Guid EmpresaId) : ICommand;
}
