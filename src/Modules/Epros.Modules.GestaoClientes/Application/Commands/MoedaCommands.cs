using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using Epros.Modules.GestaoClientes.Application.Queries;
using FluentValidation;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record CriarMoedaCommand(string CodigoISO, string Nome, string Simbolo, int CasasDecimais) : ICommand;
    public record AtualizarMoedaCommand(Guid Id, string CodigoISO, string Nome, string Simbolo, int CasasDecimais) : ICommand;
    public record ExcluirMoedaCommand(Guid Id) : ICommand;

    public class CriarMoedaCommandValidator : AbstractValidator<CriarMoedaCommand>
    {
        public CriarMoedaCommandValidator()
        {
            RuleFor(m => m.CodigoISO).NotEmpty().Length(3).WithMessage("Código ISO deve ter 3 caracteres.");
            RuleFor(m => m.Simbolo).NotEmpty().MaximumLength(5);
            RuleFor(m => m.CasasDecimais).GreaterThanOrEqualTo(0);
        }
    }
}

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ListarMoedasCatalogoQuery() : IQuery<List<MoedaDto>>;

    public class MoedaDto
    {
        public Guid Id { get; set; }
        public string CodigoISO { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Simbolo { get; set; } = string.Empty;
        public int CasasDecimais { get; set; }
    }
}
