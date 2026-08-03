using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GRC.Application.Commands
{
    // GRC — Taxonomia normativa única (D-TEC-05): catálogo compartilhado + rastreabilidade.

    /// <summary>Cria um nó no catálogo único (Politica/Obrigacao/Controle/Risco), opcionalmente sob um pai.</summary>
    public record CriarNoTaxonomiaCommand(
        string Codigo,
        string Tipo, // Politica, Obrigacao, Controle, Risco
        string Nome,
        Guid? CatalogoPaiId
    ) : ICommand;

    public class CriarNoTaxonomiaCommandValidator : AbstractValidator<CriarNoTaxonomiaCommand>
    {
        public CriarNoTaxonomiaCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty();
            RuleFor(c => c.Nome).NotEmpty();
            RuleFor(c => c.Tipo).Must(t => t == "Politica" || t == "Obrigacao" || t == "Controle" || t == "Risco")
                .WithMessage("O tipo deve ser 'Politica', 'Obrigacao', 'Controle' ou 'Risco'.");
        }
    }

    /// <summary>Cria uma aresta de rastreabilidade entre dois itens da taxonomia (ex.: controle 'mitiga' risco).</summary>
    public record VincularTaxonomiaCommand(
        string OrigemTipo,
        Guid OrigemId,
        string DestinoTipo,
        Guid DestinoId,
        string Natureza // deriva_de, atende, mitiga, operacionaliza, cobre, origina
    ) : ICommand;

    public class VincularTaxonomiaCommandValidator : AbstractValidator<VincularTaxonomiaCommand>
    {
        public VincularTaxonomiaCommandValidator()
        {
            RuleFor(c => c.OrigemId).NotEmpty();
            RuleFor(c => c.DestinoId).NotEmpty();
            RuleFor(c => c.Natureza).NotEmpty();
        }
    }

    /// <summary>D-TEC-05 — classifica um agregado (POL/REG/CIA/RIS) num nó do catálogo (FK opcional).</summary>
    public record ClassificarAgregadoTaxonomiaCommand(
        string AgregadoTipo, // Politica, Obrigacao, Controle, Risco
        Guid AgregadoId,
        Guid TaxonomiaNormativaId
    ) : ICommand;

    public class ClassificarAgregadoTaxonomiaCommandValidator : AbstractValidator<ClassificarAgregadoTaxonomiaCommand>
    {
        public ClassificarAgregadoTaxonomiaCommandValidator()
        {
            RuleFor(c => c.AgregadoId).NotEmpty();
            RuleFor(c => c.TaxonomiaNormativaId).NotEmpty();
            RuleFor(c => c.AgregadoTipo).Must(t => t == "Politica" || t == "Obrigacao" || t == "Controle" || t == "Risco")
                .WithMessage("O tipo do agregado deve ser 'Politica', 'Obrigacao', 'Controle' ou 'Risco'.");
        }
    }
}
