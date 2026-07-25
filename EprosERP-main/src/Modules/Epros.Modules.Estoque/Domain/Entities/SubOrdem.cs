using System;
using System.Collections.Generic;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Ordem de subcontratação (EF Subcontratação §7.1 `sub_ordem`). SUB-001: exige subcontratado
    /// (fornecedor). SUB-002: vínculo com ordem de produção quando produtiva. Modelo proposto por autoria (§16);
    /// nenhuma regra fiscal implementada de memória — CFOP de remessa/retorno vem do motor fiscal (SUB-008).
    /// FornecedorId/OrdemProducaoId/EmpresaId são referências externas por FK Guid.
    /// </summary>
    public class SubOrdem : EntidadeSaaSBase
    {
        public Guid? EmpresaId { get; private set; }
        public string? NumeroOrdem { get; private set; }
        public Guid? OrdemProducaoId { get; private set; }
        public Guid FornecedorId { get; private set; }
        public DateTime? DataEmissao { get; private set; }
        public DateTime? DataPrevistaRetorno { get; private set; }
        public EStatusSubOrdem Status { get; private set; } = EStatusSubOrdem.Aberta;
        public string? Observacao { get; private set; }

        // Navegação intra-módulo
        public ICollection<SubOrdemItem> Itens { get; private set; } = new List<SubOrdemItem>();

        protected SubOrdem() { } // EF Core

        public SubOrdem(Guid? empresaId, string? numeroOrdem, Guid? ordemProducaoId, Guid fornecedorId, DateTime? dataEmissao, DateTime? dataPrevistaRetorno, string? observacao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            NumeroOrdem = numeroOrdem;
            OrdemProducaoId = ordemProducaoId;
            FornecedorId = fornecedorId;
            DataEmissao = dataEmissao ?? DateTime.UtcNow;
            DataPrevistaRetorno = dataPrevistaRetorno;
            Observacao = observacao;
            Status = EStatusSubOrdem.Aberta;
            Validar();
        }

        public void AdicionarItem(SubOrdemItem item) => Itens.Add(item);

        public void MarcarEmProcesso(string usuario)
        {
            Status = EStatusSubOrdem.EmProcesso;
            MarcarAlterado(usuario);
        }

        public void MarcarRetornada(string usuario)
        {
            Status = EStatusSubOrdem.Retornada;
            MarcarAlterado(usuario);
        }

        public void Cancelar(string usuario)
        {
            Status = EStatusSubOrdem.Cancelada;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            // SUB-001: subcontratado (fornecedor) obrigatório.
            if (FornecedorId == Guid.Empty)
                AddNotification("FornecedorId", "O subcontratado (fornecedor) é obrigatório [SUB-001] [Origem: SubOrdem]");
        }
    }
}
