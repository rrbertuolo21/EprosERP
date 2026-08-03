using System;
using System.Collections.Generic;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Cabeçalho do documento de inventário físico / contagem cíclica (EF Inventário §16.1).
    /// INV-001: cabeçalho + N itens. Máquina de estados INV §11
    /// (Rascunho → EmContagem → EmConferencia → Aprovado → Ajustado; Cancelado nos estados iniciais).
    /// [DECIDIDO D3] O ajuste na aprovação é por DIFERENÇA, via motor único (kardex).
    /// [DECIDIDO D14] A acurácia é agregada no CABEÇALHO = itens sem divergência / total contados.
    /// </summary>
    public class Inventario : EntidadeSaaSBase
    {
        public Guid EmpresaId { get; private set; }
        public DateTime? DataContagem { get; private set; }
        public ETipoInventario TipoInventario { get; private set; }
        public bool EstoqueAtualizado { get; private set; }
        public ESituacaoInventario Situacao { get; private set; } = ESituacaoInventario.Rascunho;
        /// <summary>[DECIDIDO D14] Acurácia agregada (0..1): itens sem divergência / total de itens contados. ⚠️ valida-contador.</summary>
        public decimal? Acuracidade { get; private set; }
        public string? Observacao { get; private set; }
        public string? MotivoCancelamento { get; private set; }

        public ICollection<InventarioItem> Itens { get; private set; } = new List<InventarioItem>();

        protected Inventario() { } // EF Core

        public Inventario(Guid empresaId, DateTime? dataContagem, ETipoInventario tipoInventario, string? observacao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            DataContagem = dataContagem;
            TipoInventario = tipoInventario;
            Observacao = observacao;
            Situacao = ESituacaoInventario.Rascunho;
            EstoqueAtualizado = false;
            Validar();
        }

        /// <summary>
        /// INV-002: o cabeçalho referencia a empresa QUANDO DISPONÍVEL. Guid.Empty é o bucket
        /// "empresa não segregada" (MotorMovimentacaoEstoque.EmpresaPadrao) — legítimo, não é ausência.
        /// A obrigatoriedade de itens (INV-001) é verificada no handler de criação.
        /// </summary>
        public void Validar()
        {
            Clear();
        }

        public bool PodeContar() => Situacao == ESituacaoInventario.Rascunho || Situacao == ESituacaoInventario.EmContagem;

        public void IniciarContagem(string usuario)
        {
            if (Situacao != ESituacaoInventario.Rascunho)
                throw new InvalidOperationException("Somente inventário em rascunho pode iniciar contagem.");
            Situacao = ESituacaoInventario.EmContagem;
            MarcarAlterado(usuario);
        }

        public void EnviarParaConferencia(string usuario)
        {
            if (Situacao != ESituacaoInventario.EmContagem)
                throw new InvalidOperationException("Somente inventário em contagem pode ir para conferência.");
            Situacao = ESituacaoInventario.EmConferencia;
            MarcarAlterado(usuario);
        }

        /// <summary>INV-014: aprovação libera a geração de ajuste. [D14] grava a acurácia agregada.</summary>
        public void Aprovar(decimal acuracidade, string usuario)
        {
            if (Situacao != ESituacaoInventario.EmConferencia && Situacao != ESituacaoInventario.EmContagem)
                throw new InvalidOperationException("Somente inventário em contagem/conferência pode ser aprovado.");
            Acuracidade = acuracidade;
            Situacao = ESituacaoInventario.Aprovado;
            MarcarAlterado(usuario);
        }

        /// <summary>Marca o inventário como ajustado após aplicar as diferenças no motor único (INV-014/D3).</summary>
        public void MarcarAjustado(string usuario)
        {
            if (Situacao != ESituacaoInventario.Aprovado)
                throw new InvalidOperationException("Somente inventário aprovado pode ser ajustado [INV-015/CA-006].");
            Situacao = ESituacaoInventario.Ajustado;
            EstoqueAtualizado = true;
            MarcarAlterado(usuario);
        }

        public void Cancelar(string motivo, string usuario)
        {
            if (Situacao == ESituacaoInventario.Ajustado || Situacao == ESituacaoInventario.Aprovado)
                throw new InvalidOperationException("Inventário aprovado/ajustado não pode ser cancelado.");
            MotivoCancelamento = motivo;
            Situacao = ESituacaoInventario.Cancelado;
            MarcarAlterado(usuario);
        }
    }
}
