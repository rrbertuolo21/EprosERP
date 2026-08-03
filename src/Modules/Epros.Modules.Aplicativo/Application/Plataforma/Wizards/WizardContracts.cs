using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Aplicativo.Application.Plataforma.Wizards
{
    // ===================== Commands =====================

    public record CriarDefinicaoWizardCommand(string Codigo, string Nome, string? Descricao, bool Publico) : ICommand;

    public class CriarDefinicaoWizardCommandValidator : AbstractValidator<CriarDefinicaoWizardCommand>
    {
        public CriarDefinicaoWizardCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty();
            RuleFor(c => c.Nome).NotEmpty();
        }
    }

    public record AdicionarEtapaWizardCommand(Guid DefinicaoId, int Ordem, string Titulo, string? Descricao) : ICommand;

    public record AdicionarCampoWizardCommand(
        Guid EtapaId, string Chave, string Rotulo, string Tipo, bool Obrigatorio, string? OpcoesJson, int Ordem) : ICommand;

    public record PublicarDefinicaoWizardCommand(Guid DefinicaoId) : ICommand;

    public record IniciarExecucaoWizardCommand(Guid DefinicaoId, bool CanalPublico) : ICommand;

    /// <summary>Responde a etapa atual. Campos são sanitizados (HTML/embed) e os obrigatórios validados.</summary>
    public record ResponderEtapaWizardCommand(Guid ExecucaoId, Dictionary<string, string> Respostas) : ICommand;

    // ===================== Queries =====================

    public record ObterDefinicoesWizardQuery(bool ApenasAtivos = false) : IQuery<IReadOnlyList<DefinicaoWizardDto>>;

    public record ObterDefinicaoWizardPorIdQuery(Guid Id) : IQuery<DefinicaoWizardDetalheDto?>;

    public record ObterExecucoesWizardQuery(Guid? DefinicaoId = null, string? Status = null) : IQuery<IReadOnlyList<ExecucaoWizardDto>>;

    // ===================== DTOs =====================

    public record DefinicaoWizardDto(Guid Id, string Codigo, string Nome, string? Descricao, bool Publico, bool Ativo);

    public record CampoWizardDto(Guid Id, string Chave, string Rotulo, string Tipo, bool Obrigatorio, string? OpcoesJson, int Ordem);

    public record EtapaWizardDto(Guid Id, int Ordem, string Titulo, string? Descricao, IReadOnlyList<CampoWizardDto> Campos);

    public record DefinicaoWizardDetalheDto(DefinicaoWizardDto Definicao, IReadOnlyList<EtapaWizardDto> Etapas);

    public record ExecucaoWizardDto(Guid Id, Guid DefinicaoId, string Status, int EtapaAtualOrdem, bool CanalPublico, DateTime CriadoEm);
}
