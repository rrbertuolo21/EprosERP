using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Unidade serializada (EF Rastreabilidade §7.4 `rlt_numero_serial`). [DECIDIDO D10] serialização
    /// configurável por tipo de produto. RLT-004: serial obrigatório para produto serializado;
    /// RLT-005: serial único no escopo produto/tenant; RLT-015: serial consumido não pode ser reutilizado.
    /// </summary>
    public class NumeroSerial : EntidadeSaaSBase
    {
        public Guid ProdutoId { get; private set; }
        public Guid? LoteId { get; private set; }
        public string Numero { get; private set; } = string.Empty;
        public EStatusNumeroSerial Status { get; private set; } = EStatusNumeroSerial.Disponivel;
        public Guid? FichaEntradaId { get; private set; }
        public Guid? FichaSaidaId { get; private set; }

        protected NumeroSerial() { } // EF Core

        public NumeroSerial(Guid produtoId, string numero, Guid? loteId, Guid? fichaEntradaId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ProdutoId = produtoId;
            Numero = numero ?? string.Empty;
            LoteId = loteId;
            FichaEntradaId = fichaEntradaId;
            Status = EStatusNumeroSerial.Disponivel;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<NumeroSerial>()
                .Requires()
                .IsNotEmpty(ProdutoId, nameof(ProdutoId), "O produto do serial é obrigatório [Origem: NumeroSerial]")
                .IsNotNullOrEmpty(Numero, nameof(Numero), "O número serial é obrigatório para produto serializado [RLT-004] [Origem: NumeroSerial]"));
        }

        /// <summary>RLT-015: serial consumido não pode ser consumido novamente. RLT-016: bloqueado não sai.</summary>
        public void Consumir(Guid fichaSaidaId, string usuario)
        {
            if (Status == EStatusNumeroSerial.Consumido) throw new InvalidOperationException("Serial já consumido não pode ser reutilizado [RLT-015].");
            if (Status == EStatusNumeroSerial.Bloqueado) throw new InvalidOperationException("Serial bloqueado não pode ser selecionado em saída [RLT-016].");
            FichaSaidaId = fichaSaidaId;
            Status = EStatusNumeroSerial.Consumido;
            MarcarAlterado(usuario);
        }

        public void Bloquear(string usuario) { Status = EStatusNumeroSerial.Bloqueado; MarcarAlterado(usuario); }
        public void Desbloquear(string usuario) { if (Status == EStatusNumeroSerial.Bloqueado) { Status = EStatusNumeroSerial.Disponivel; MarcarAlterado(usuario); } }
    }
}
