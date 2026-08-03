using System;
using System.Collections.Generic;
using Epros.Modules.Agricultor.Domain.Enums;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Agricultor.Application.Commands
{
    // ===================== Propriedade rural + talhões =====================

    public record TalhaoInput(string Nome, Guid? CulturaId, string? PoligonoGeoJson, decimal? AreaM2);

    public record CriarPropriedadeCommand(
        string NomeImovel, string? Matricula, string? Car, string? CadItrCafir, string? Caepf,
        ETipoExploracao TipoExploracao, decimal Participacao,
        string? Uf, string? CodigoMunicipioSped, string? Cep, string? Endereco,
        List<TalhaoInput>? Talhoes) : ICommand;

    public class CriarPropriedadeCommandValidator : AbstractValidator<CriarPropriedadeCommand>
    {
        public CriarPropriedadeCommandValidator()
        {
            RuleFor(c => c.NomeImovel).NotEmpty().WithMessage("O nome do imóvel é obrigatório.");
            RuleFor(c => c.Participacao).InclusiveBetween(0, 100);
        }
    }

    public record AlterarPropriedadeCommand(
        Guid PropriedadeId, string NomeImovel, string? Matricula, string? Car, string? CadItrCafir, string? Caepf,
        ETipoExploracao TipoExploracao, decimal Participacao,
        string? Uf, string? CodigoMunicipioSped, string? Cep, string? Endereco) : ICommand;

    public record ExcluirPropriedadeCommand(Guid PropriedadeId) : ICommand;

    public record AdicionarTalhaoCommand(Guid PropriedadeId, string Nome, Guid? CulturaId,
        string? PoligonoGeoJson, decimal? AreaM2) : ICommand;

    // ===================== Cadastros =====================

    public record CriarCulturaCommand(string Nome, string? Codigo) : ICommand;
    public record CriarCategoriaDespesaCommand(string Nome, string? Codigo, bool EhFolhaMaoDeObra) : ICommand;
    public record CriarFornecedorCommand(string Nome, string? CpfCnpj, string? Codigo) : ICommand;
    public record CriarColaboradorCommand(Guid PropriedadeId, string Nome, string? Email, EPapelColaborador Papel) : ICommand;

    // ===================== Safra =====================

    public record CriarSafraCommand(Guid TalhaoId, Guid CulturaId, DateTime? DataPlantio, DateTime? DataColheita) : ICommand;
    public record IniciarSafraCommand(Guid SafraId) : ICommand;
    public record RegistrarColheitaCommand(Guid SafraId, DateTime DataColheita) : ICommand;
    public record EncerrarSafraCommand(Guid SafraId) : ICommand;

    // ===================== Despesa / Receita =====================

    public record RegistrarDespesaCommand(
        Guid PropriedadeId, Guid? TalhaoId, Guid? SafraId, Guid? CategoriaId, Guid? FornecedorId,
        DateTime Data, decimal Valor, string? Referencia) : ICommand;

    public class RegistrarDespesaCommandValidator : AbstractValidator<RegistrarDespesaCommand>
    {
        public RegistrarDespesaCommandValidator()
        {
            RuleFor(c => c.PropriedadeId).NotEmpty();
            RuleFor(c => c.Valor).GreaterThan(0);
        }
    }

    /// <summary>AGR-D04: aterra a natureza fiscal LCDPR na despesa (derivação do overlay Siser). // valida-contador.</summary>
    public record ClassificarDespesaLcdprCommand(
        Guid DespesaId, int CodImovel, int CodConta, ETipoDocumentoLcdpr TipoDoc,
        ETipoLancamentoLcdpr TipoLanc, string? IdPartic) : ICommand;

    public record RegistrarReceitaCommand(
        Guid PropriedadeId, Guid? TalhaoId, Guid? SafraId, DateTime Data,
        decimal Quantidade, decimal Preco, string? Comprador, string? NumNf) : ICommand;

    public class RegistrarReceitaCommandValidator : AbstractValidator<RegistrarReceitaCommand>
    {
        public RegistrarReceitaCommandValidator()
        {
            RuleFor(c => c.PropriedadeId).NotEmpty();
            RuleFor(c => c.Quantidade).GreaterThan(0);
            RuleFor(c => c.Preco).GreaterThan(0);
        }
    }

    /// <summary>AGR-D12 (bloqueante): receita nunca em COD_IMOVEL 000. // valida-contador.</summary>
    public record ClassificarReceitaLcdprCommand(Guid ReceitaId, int CodImovel, int CodConta, string? IdPartic) : ICommand;

    // ===================== Anotação de campo =====================

    public record CriarAnotacaoCommand(string Titulo, string? Descricao, string? Tipo, Guid? TalhaoId,
        Guid? DesignadoA, string? LatLng, bool IsPublic) : ICommand;
}
