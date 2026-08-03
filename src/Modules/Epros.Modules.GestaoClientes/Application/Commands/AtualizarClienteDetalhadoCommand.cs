using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record AtualizarClienteDetalhadoCommand(
        Guid Id,
        string RazaoSocial,
        string Cnpj,
        string Email,
        Guid PlanoId,
        Guid? RevendaId,
        Guid? VendedorId,
        int DiaVencimento,
        bool Ativo,
        string? Telefone,
        string? NomeContato,
        bool IsDemo,
        string? TokenAcesso,
        List<SalvarEnderecoDto> Enderecos,
        List<SalvarComposicaoDto> Composicoes,
        int? CotaUsuarios = null,
        int? CotaEmpresas = null,
        int? CotaPermissoes = null,
        // 1.06 — override (snapshot) do limite de CLIENTES do tenant; null = usa Plano.LimiteClientes.
        int? CotaClientes = null
    ) : ICommand;

    public class SalvarEnderecoDto
    {
        public Guid Id { get; set; }
        public int TipoEndereco { get; set; }
        public Guid PaisId { get; set; }
        public Guid MunicipioId { get; set; }
        public Guid? SubdivisaoId { get; set; }
        public string Uf { get; set; } = string.Empty;
        public string? Cep { get; set; }
        public string Logradouro { get; set; } = string.Empty;
        public string? Numero { get; set; }
        public string? Complemento { get; set; }
        public string Bairro { get; set; } = string.Empty;
        public string? Referencia { get; set; }
        public bool Principal { get; set; }
    }

    public class SalvarComposicaoDto
    {
        public Guid Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime DataInicial { get; set; }
        public DateTime? DataFinal { get; set; }
        public bool PodeReajustar { get; set; }
    }

    public class AtualizarClienteDetalhadoCommandValidator : AbstractValidator<AtualizarClienteDetalhadoCommand>
    {
        public AtualizarClienteDetalhadoCommandValidator()
        {
            RuleFor(c => c.Id)
                .NotEmpty().WithMessage("O ID do cliente é obrigatório.");

            RuleFor(c => c.RazaoSocial)
                .NotEmpty().WithMessage("A Razão Social é obrigatória.");

            RuleFor(c => c.Cnpj)
                .NotEmpty().WithMessage("O CNPJ é obrigatório.");

            RuleFor(c => c.Email)
                .EmailAddress().WithMessage("O E-mail fornecido não é válido.");

            RuleFor(c => c.PlanoId)
                .NotEmpty().WithMessage("O ID do Plano é obrigatório.");

            RuleFor(c => c.DiaVencimento)
                .InclusiveBetween(1, 31).WithMessage("O dia de vencimento deve ser entre 1 e 31.");
        }
    }
}
