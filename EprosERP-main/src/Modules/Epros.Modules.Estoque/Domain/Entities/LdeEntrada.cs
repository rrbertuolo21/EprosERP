using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Entidade central da entrada física/logística vinculada à compra (EF Logística de Entrada §15.1 `lde_entrada`).
    /// CompraId, FornecedorId, LocalEntregaId e DocumentoEntradaId são FKs Guid (referências externas/intra-módulo).
    /// Máquina de estados: Rascunho → Confirmado → Faturado / Cancelado / Estornado (EF §11).
    /// LDE-012/013/014/015: confirmação exige fornecedor, emitente, itens e documento vinculado (validado no handler).
    /// </summary>
    public class LdeEntrada : EntidadeSaaSBase
    {
        public Guid CompraId { get; private set; }
        public Guid FornecedorId { get; private set; }
        public Guid? LocalEntregaId { get; private set; }
        public Guid? DocumentoEntradaId { get; private set; }
        public ESituacaoEntradaLogistica Situacao { get; private set; } = ESituacaoEntradaLogistica.Rascunho;
        public DateTime? DataConfirmacao { get; private set; }
        public string? MotivoCancelamentoEstorno { get; private set; }

        protected LdeEntrada() { } // EF Core

        public LdeEntrada(Guid compraId, Guid fornecedorId, Guid? localEntregaId, Guid? documentoEntradaId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompraId = compraId;
            FornecedorId = fornecedorId;
            LocalEntregaId = localEntregaId;
            DocumentoEntradaId = documentoEntradaId;
            Situacao = ESituacaoEntradaLogistica.Rascunho;
            Validar();
        }

        public void VincularLocalEntrega(Guid localEntregaId, string alteradoPor)
        {
            LocalEntregaId = localEntregaId;
            MarcarAlterado(alteradoPor);
        }

        public void VincularDocumento(Guid documentoEntradaId, string alteradoPor)
        {
            DocumentoEntradaId = documentoEntradaId;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>LDE-012/013/014/015: só confirma a partir de Rascunho.</summary>
        public void Confirmar(string usuario)
        {
            Situacao = ESituacaoEntradaLogistica.Confirmado;
            DataConfirmacao = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }

        public void Faturar(string usuario)
        {
            Situacao = ESituacaoEntradaLogistica.Faturado;
            MarcarAlterado(usuario);
        }

        /// <summary>LDE-018: cancelamento com motivo publica evento de reversão.</summary>
        public void Cancelar(string motivo, string usuario)
        {
            Situacao = ESituacaoEntradaLogistica.Cancelado;
            MotivoCancelamentoEstorno = motivo;
            MarcarAlterado(usuario);
        }

        /// <summary>LDE-019: estorno exige motivo e permissão específica (checada no handler/ABAC).</summary>
        public void Estornar(string motivo, string usuario)
        {
            Situacao = ESituacaoEntradaLogistica.Estornado;
            MotivoCancelamentoEstorno = motivo;
            MarcarAlterado(usuario);
        }

        public bool PodeConfirmar() => Situacao == ESituacaoEntradaLogistica.Rascunho;
        public bool EstaCancelada() => Situacao == ESituacaoEntradaLogistica.Cancelado || Situacao == ESituacaoEntradaLogistica.Estornado;

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<LdeEntrada>()
                .Requires()
                .AreNotEquals(CompraId, Guid.Empty, nameof(CompraId), "A compra vinculada à entrada é obrigatória [LDE-001] [Origem: LdeEntrada]")
                .AreNotEquals(FornecedorId, Guid.Empty, nameof(FornecedorId), "O fornecedor da entrada é obrigatório [LDE-012] [Origem: LdeEntrada]"));
        }
    }
}
