using System;

namespace Epros.Modules.RH.Domain.Entities
{
    // Regras de negocio do submodulo Folha de Pagamento e Beneficios (RH-FP).
    // Estados da competencia: Rascunho -> Processando -> Concluido -> Fechado | Cancelado.

    public partial class FolCompetencia
    {
        public const string StRascunho = "Rascunho";
        public const string StProcessando = "Processando";
        public const string StConcluido = "Concluido";
        public const string StFechado = "Fechado";
        public const string StCancelado = "Cancelado";

        public bool EstaFechada => Status == StFechado || Status == StCancelado;

        public void IniciarProcessamento(string usuario)
        {
            if (Status != StRascunho)
            {
                AddNotification(nameof(Status), "So e possivel processar competencia em Rascunho.");
                return;
            }
            Status = StProcessando;
            MarcarAlterado(usuario);
        }

        public void Concluir(string usuario)
        {
            if (Status != StProcessando)
            {
                AddNotification(nameof(Status), "So e possivel concluir competencia em Processando.");
                return;
            }
            Status = StConcluido;
            MarcarAlterado(usuario);
        }

        public void Fechar(string usuario)
        {
            if (Status != StConcluido)
            {
                AddNotification(nameof(Status), "So e possivel fechar competencia Concluida.");
                return;
            }
            Status = StFechado;
            MarcarAlterado(usuario);
        }

        public void Cancelar(string usuario)
        {
            if (Status == StFechado)
            {
                AddNotification(nameof(Status), "Competencia fechada nao pode ser cancelada por fluxo ordinario.");
                return;
            }
            Status = StCancelado;
            MarcarAlterado(usuario);
        }

        // RF-FOL-014 / criterio de aceite 6/8: pagamento agregado.
        public void MarcarComoPaga(string usuario)
        {
            StatusPagamento = "Pago";
            MarcarAlterado(usuario);
        }
    }

    public partial class FolLancamento
    {
        // RF-FOL-017: liquido a partir de salario base, proventos e descontos.
        public void RecalcularTotais()
        {
            var basee = SalarioBase ?? 0m;
            ValorBruto = basee + TotalProventos + (TotalHorasExtras ?? 0m);
            ValorLiquido = ValorBruto.Value - TotalDescontos - (TotalEmprestimos ?? 0m);
        }

        // RF-FOL-014: pagamento individual muda status.
        public void MarcarComoPago(string usuario)
        {
            if (Status == "Pago")
            {
                AddNotification(nameof(Status), "Lancamento ja esta pago.");
                return;
            }
            Status = "Pago";
            MarcarAlterado(usuario);
        }
    }

    public partial class FolRubrica
    {
        public const string TipoProvento = "Provento";
        public const string TipoDesconto = "Desconto";
        public const string TipoInformativo = "Informativo";

        // RF-FOL-004: rubrica deve ter exatamente um tipo valido.
        public static bool TipoValido(string tipo)
            => tipo == TipoProvento || tipo == TipoDesconto || tipo == TipoInformativo;
    }
}
