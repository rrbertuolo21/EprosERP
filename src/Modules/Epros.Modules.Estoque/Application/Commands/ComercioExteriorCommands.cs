using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Estoque.Application.Commands
{
    /// <summary>
    /// Comandos do submódulo Comércio Exterior / Importação (CD1 / EF COMERCIO_EXTERIOR). Cobrem: dados de
    /// comércio exterior na compra (incoterm/moeda/câmbio, CEX-005), o parâmetro de rateio landed
    /// (CEX-003/NF-02, desligado por padrão) e a nacionalização (entrada no Estoque D1 + títulos, CEX-006).
    /// ⚠️ Tributos/base/CFOP = valida-contador: os montantes entram FACTUALMENTE; o sistema apenas apropria.
    /// </summary>
    public record DefinirComercioExteriorCompraCommand(
        Guid CompraId,
        EIncotermCompra Incoterm,
        string? Moeda = null,
        decimal? TaxaCambio = null
    ) : ICommand;

    /// <summary>Upsert do parâmetro de rateio landed por tenant/empresa (default desligado — NF-02).</summary>
    public record SalvarRateioLandedConfigCommand(
        bool Habilitado,
        bool IncluirTributos,
        bool IncluirFrete,
        bool IncluirDespesas,
        ERateioLandedMetodo Metodo,
        Guid? EmpresaId = null
    ) : ICommand;

    /// <summary>
    /// Nacionaliza a importação da compra: publica entrada no Estoque (motor único D1) com o custo — landed
    /// quando a config estiver habilitada — e os títulos financeiros de tributos/frete. Idempotente por
    /// compra. Os montantes são FACTUAIS (valida-contador informa); o sistema só apropria estruturalmente.
    /// </summary>
    public record NacionalizarImportacaoCommand(
        Guid CompraId,
        decimal MontanteTributos = 0m,
        decimal MontanteFrete = 0m,
        decimal MontanteDespesas = 0m,
        Guid? EmpresaId = null
    ) : ICommand;
}
