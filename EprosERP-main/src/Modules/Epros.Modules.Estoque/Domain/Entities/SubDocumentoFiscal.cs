using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Referência a documentos fiscais de remessa/retorno da subcontratação (EF Subcontratação §6
    /// `sub_documento_fiscal`). SUB-008: CFOP de remessa e retorno vem do MOTOR FISCAL — aqui apenas se
    /// referencia/armazena a informação, sem calcular ou inventar a regra fiscal (Negócio nunca de memória:
    /// consultar Negocio-acumulado/fiscal). Modelo proposto por autoria (§16).
    /// </summary>
    public class SubDocumentoFiscal : EntidadeSaaSBase
    {
        public Guid OrdemId { get; private set; }
        public Guid? EnvioId { get; private set; }
        public Guid? RetornoId { get; private set; }
        /// <summary>Referência ao documento fiscal eletrônico (emitido pelo motor fiscal).</summary>
        public Guid? DocumentoFiscalId { get; private set; }
        /// <summary>CFOP de remessa — proveniente do motor fiscal (SUB-008), nunca calculado aqui.</summary>
        public string? CfopRemessa { get; private set; }
        /// <summary>CFOP de retorno — proveniente do motor fiscal (SUB-008), nunca calculado aqui.</summary>
        public string? CfopRetorno { get; private set; }

        protected SubDocumentoFiscal() { } // EF Core

        public SubDocumentoFiscal(Guid ordemId, Guid? envioId, Guid? retornoId, Guid? documentoFiscalId, string? cfopRemessa, string? cfopRetorno, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            OrdemId = ordemId;
            EnvioId = envioId;
            RetornoId = retornoId;
            DocumentoFiscalId = documentoFiscalId;
            CfopRemessa = cfopRemessa;
            CfopRetorno = cfopRetorno;
        }
    }
}
