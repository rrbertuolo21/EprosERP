using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ListarPlanosQuery(
        int Pagina = 1,
        int TamanhoPagina = 25,
        string? Search = null
    ) : IQuery<PagedQueryResult<PlanoListaDto>>;

    public record ObterPlanoPorIdQuery(Guid Id) : IQuery<PlanoDetalhadoDto>;

    public class PlanoListaDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public Guid? GrupoPlanoId { get; set; }
        public string? DescricaoCurta { get; set; }
        public int LimiteUsuarios { get; set; }
        public int LimiteEmpresas { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public bool Ativo { get; set; }
        public int QtdeModulos { get; set; }
        public DateTime CriadoEm { get; set; }
    }

    public class PlanoDetalhadoDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public Guid? GrupoPlanoId { get; set; }
        public string? DescricaoCurta { get; set; }
        public string? DescricaoCompleta { get; set; }
        public int LimiteUsuarios { get; set; }
        public int LimiteEmpresas { get; set; }
        public string? RecursosInclusos { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public bool Ativo { get; set; }
        public DateTime CriadoEm { get; set; }
        public List<ModuloPlanoDto> Modulos { get; set; } = new();
    }

    public class ModuloPlanoDto
    {
        public Guid Id { get; set; }
        public string NomeModulo { get; set; } = string.Empty;
        public string? ModuloGeralId { get; set; }
        public string? Descricao { get; set; }
        public decimal Valor { get; set; }
        public bool Ativo { get; set; }
    }
}
