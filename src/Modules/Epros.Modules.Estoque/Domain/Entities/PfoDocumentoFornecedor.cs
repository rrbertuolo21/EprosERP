using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Documento enviado pelo fornecedor (EF Portal do Fornecedor §15.8 `pfo_documento_fornecedor`).
    /// PFO-008: o arquivo é armazenado no repositório documental (GED) — aqui guarda-se o metadado e a
    /// referência (`ArquivoId` = FK GED). VAL-PFO-005: referência precisa ser permitida (cotação/pedido/pré-aviso).
    /// </summary>
    public class PfoDocumentoFornecedor : EntidadeSaaSBase
    {
        public Guid FornecedorId { get; private set; }
        public EReferenciaDocumentoFornecedor ReferenciaTipo { get; private set; }
        public Guid ReferenciaId { get; private set; }
        public string? TipoDocumento { get; private set; }
        public Guid ArquivoId { get; private set; }
        public EStatusDocumentoFornecedor Status { get; private set; } = EStatusDocumentoFornecedor.Enviado;
        public DateTime EnviadoEm { get; private set; }

        protected PfoDocumentoFornecedor() { }

        public PfoDocumentoFornecedor(Guid fornecedorId, EReferenciaDocumentoFornecedor referenciaTipo, Guid referenciaId, string? tipoDocumento, Guid arquivoId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            FornecedorId = fornecedorId;
            ReferenciaTipo = referenciaTipo;
            ReferenciaId = referenciaId;
            TipoDocumento = tipoDocumento;
            ArquivoId = arquivoId;
            Status = EStatusDocumentoFornecedor.Enviado;
            EnviadoEm = DateTime.UtcNow;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<PfoDocumentoFornecedor>()
                .Requires()
                .IsNotEmpty(FornecedorId, nameof(FornecedorId), "O fornecedor do documento é obrigatório [PFO-002] [Origem: PfoDocumentoFornecedor]")
                .IsNotEmpty(ReferenciaId, nameof(ReferenciaId), "A referência do documento é obrigatória [VAL-PFO-005] [Origem: PfoDocumentoFornecedor]")
                .IsNotEmpty(ArquivoId, nameof(ArquivoId), "O arquivo (GED) do documento é obrigatório [PFO-008] [Origem: PfoDocumentoFornecedor]"));
        }

        public void Analisar(string usuario) { Status = EStatusDocumentoFornecedor.EmAnalise; MarcarAlterado(usuario); }
        public void Aceitar(string usuario) { Status = EStatusDocumentoFornecedor.Aceito; MarcarAlterado(usuario); }
        public void Rejeitar(string usuario) { Status = EStatusDocumentoFornecedor.Rejeitado; MarcarAlterado(usuario); }
    }
}
