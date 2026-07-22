using System;
using System.Collections.Generic;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    public class OrdemProducao : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string ProdutoAcabadoSku { get; private set; } = string.Empty;
        public decimal QuantidadePlanejada { get; private set; }
        public decimal QuantidadeProduzida { get; private set; }
        public decimal QuantidadeRefugada { get; private set; }
        public string Status { get; private set; } = "Pendente"; // Pendente, EmProducao, Encerrada, Cancelada
        public DateTime DataAbertura { get; private set; }
        public DateTime? DataInicio { get; private set; }
        public DateTime? DataFim { get; private set; }
        public decimal CustoTotalProducao { get; private set; }
        public List<ApontamentoProducao> Apontamentos { get; private set; } = new();

        protected OrdemProducao() { } // EF Core

        public OrdemProducao(string codigo, string produtoAcabadoSku, decimal quantidadePlanejada, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<OrdemProducao>()
                .Requires()
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O código da ordem é obrigatório.")
                .IsNotNullOrEmpty(produtoAcabadoSku, nameof(ProdutoAcabadoSku), "O SKU do produto acabado é obrigatório.")
                .IsGreaterThan(quantidadePlanejada, 0, nameof(QuantidadePlanejada), "A quantidade planejada deve ser maior que zero.")
            );

            Codigo = codigo;
            ProdutoAcabadoSku = produtoAcabadoSku;
            QuantidadePlanejada = quantidadePlanejada;
            QuantidadeProduzida = 0;
            QuantidadeRefugada = 0;
            Status = "Pendente";
            DataAbertura = DateTime.UtcNow;
            CustoTotalProducao = 0;
        }

        public void IniciarProducao(string usuario)
        {
            if (Status != "Pendente")
            {
                AddNotification(nameof(Status), "A ordem de produção só pode ser iniciada se estiver com status Pendente.");
                return;
            }

            Status = "EmProducao";
            DataInicio = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }

        public void RegistrarApontamento(decimal quantidadeApontada, decimal quantidadeRefugada, string operador, string usuario)
        {
            if (Status != "EmProducao")
            {
                AddNotification(nameof(Status), "Só é possível registrar apontamentos em uma ordem de produção Em Execução.");
                return;
            }

            var apontamento = new ApontamentoProducao(Id, quantidadeApontada, quantidadeRefugada, operador, TenantId, usuario);
            if (!apontamento.IsValid)
            {
                AddNotifications(apontamento.Notifications);
                return;
            }

            Apontamentos.Add(apontamento);
            QuantidadeProduzida += quantidadeApontada;
            QuantidadeRefugada += quantidadeRefugada;
            MarcarAlterado(usuario);
        }

        public void EncerrarOrdemProducao(decimal custoTotalCalculado, string usuario)
        {
            if (Status != "EmProducao" && Status != "Pendente")
            {
                AddNotification(nameof(Status), "Somente ordens Pendentes ou Em Execução podem ser encerradas.");
                return;
            }

            Status = "Encerrada";
            DataFim = DateTime.UtcNow;
            CustoTotalProducao = custoTotalCalculado;
            MarcarAlterado(usuario);
        }

        public void CancelarOrdemProducao(string usuario)
        {
            if (Status == "Encerrada" || Status == "Cancelada")
            {
                AddNotification(nameof(Status), "Ordens de produção já encerradas ou canceladas não podem ser alteradas.");
                return;
            }

            Status = "Cancelada";
            DataFim = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }
    }
}
