using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Vendas.Application.Commands
{
    // ===================== Logística de Saída (VEN-LDS) =====================
    // Fonte: EF_7_VENDAS_LOGISTICA_DE_SAIDA_V1. Estilo B (ICommand).

    public record CriarExpedicaoCommand(
        Guid EmpresaId,
        Guid PedidoId,
        Guid? DocumentoFiscalId,
        Guid? RomaneioId,
        DateTime? DataExpedicao,
        string? Observacoes) : ICommand;

    public record DefinirLocalEntregaCommand(
        Guid ExpedicaoId,
        string CpfCnpj,
        string Logradouro,
        string Numero,
        string Complemento,
        string Bairro,
        string? CodigoMunicipio,
        string NomeMunicipio,
        string Uf) : ICommand;

    public record RegistrarEntregaItemCommand(
        Guid ExpedicaoId,
        Guid PedidoItemId,
        Guid? ProdutoId,
        decimal QuantidadeVendida,
        decimal QuantidadeEntregue,
        Guid? UsuarioEntregaId) : ICommand;

    public record ConfirmarExpedicaoCommand(Guid ExpedicaoId) : ICommand;

    public record FaturarExpedicaoCommand(Guid ExpedicaoId, Guid DocumentoFiscalId) : ICommand;

    public record CancelarExpedicaoCommand(Guid ExpedicaoId) : ICommand;
}
