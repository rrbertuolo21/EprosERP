using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GRC.Application.Commands
{
    // GRC-REG avançado — certificado com cofre (D-REG-01), validação local determinística (D-REG-04),
    // obrigação com framework/prazo (D-REG-05) e alertas de vencimento parametrizados (D-REG-02).

    /// <summary>
    /// D-REG-01 — cadastra certificado completo. O segredo NÃO trafega aqui: recebe apenas as
    /// REFERÊNCIAS de cofre (T5). A senha/chave nunca entra no módulo.
    /// </summary>
    public record CadastrarCertificadoCompletoCommand(
        Guid EmpresaId,
        string Cnpj,
        string Serial,
        string Tipo,   // A1, A3
        string Origem, // Empresa, EscritorioContador
        string? Nome,
        string? ResumoCertificado,
        DateTime ValidadeInicio,
        DateTime ValidadeFim,
        string SenhaReferenciaSegura,
        string CaminhoReferenciaSegura
    ) : ICommand;

    public class CadastrarCertificadoCompletoCommandValidator : AbstractValidator<CadastrarCertificadoCompletoCommand>
    {
        public CadastrarCertificadoCompletoCommandValidator()
        {
            RuleFor(c => c.EmpresaId).NotEmpty();
            RuleFor(c => c.Cnpj).NotEmpty();
            RuleFor(c => c.Serial).NotEmpty();
            RuleFor(c => c.Tipo).Must(t => t == "A1" || t == "A3").WithMessage("O tipo deve ser 'A1' ou 'A3'.");
            RuleFor(c => c.Origem).Must(o => o == "Empresa" || o == "EscritorioContador")
                .WithMessage("A origem deve ser 'Empresa' ou 'EscritorioContador'.");
            RuleFor(c => c.SenhaReferenciaSegura).NotEmpty().WithMessage("A referencia de senha no cofre e obrigatoria.");
            RuleFor(c => c.CaminhoReferenciaSegura).NotEmpty().WithMessage("A referencia de caminho no cofre e obrigatoria.");
        }
    }

    /// <summary>
    /// D-REG-04 — validação local determinística do certificado (sem DFe, que está atrás de gate
    /// D-AMB-03). O chamador informa se a empresa existe e o CNPJ dela (a base Empresa vive fora do GRC).
    /// </summary>
    public record ValidarCertificadoLocalCommand(
        Guid CertificadoId,
        bool EmpresaExiste,
        string CnpjEmpresa
    ) : ICommand;

    public class ValidarCertificadoLocalCommandValidator : AbstractValidator<ValidarCertificadoLocalCommand>
    {
        public ValidarCertificadoLocalCommandValidator()
        {
            RuleFor(c => c.CertificadoId).NotEmpty();
        }
    }

    /// <summary>D-REG-05 — define nome/framework/prazo próprio da obrigação regulatória.</summary>
    public record DefinirObrigacaoRegistroCommand(
        Guid RegistroId,
        string? Nome,
        string? Framework,
        DateTime? DataInicio,
        DateTime? DataVencimento
    ) : ICommand;

    public class DefinirObrigacaoRegistroCommandValidator : AbstractValidator<DefinirObrigacaoRegistroCommand>
    {
        public DefinirObrigacaoRegistroCommandValidator()
        {
            RuleFor(c => c.RegistroId).NotEmpty();
        }
    }

    /// <summary>D-REG-02 — cria um vencimento no calendário com janelas de alerta (default 30/15/7).</summary>
    public record CriarVencimentoRegulatorioCommand(
        Guid? CertificadoId,
        Guid? RegistroId,
        string Descricao,
        DateTime DataVencimento,
        string? DiasAlerta,
        Guid? ResponsavelId,
        string? Tratamento
    ) : ICommand;

    public class CriarVencimentoRegulatorioCommandValidator : AbstractValidator<CriarVencimentoRegulatorioCommand>
    {
        public CriarVencimentoRegulatorioCommandValidator()
        {
            RuleFor(c => c.Descricao).NotEmpty();
            RuleFor(c => c).Must(c => c.CertificadoId != null || c.RegistroId != null)
                .WithMessage("O vencimento deve estar vinculado a um certificado ou registro.");
        }
    }

    /// <summary>
    /// D-REG-02 — rotina de alerta: varre vencimentos pendentes e marca 'Alertado' os que caem na
    /// janela de alerta, emitindo evento. Executada por job agendado.
    /// </summary>
    public record AvaliarVencimentosRegulatoriosCommand() : ICommand;
}
