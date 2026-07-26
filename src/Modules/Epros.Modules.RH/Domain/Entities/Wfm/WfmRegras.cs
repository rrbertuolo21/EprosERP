using System;

namespace Epros.Modules.RH.Domain.Entities
{
    // Regras de negocio do submodulo Gestao da Forca de Trabalho (RH-WFM).

    public partial class WfmColaborador
    {
        // Secao 18: dominio de status do colaborador.
        public const string StRascunho = "Rascunho";
        public const string StAtivo = "Ativo";
        public const string StAfastado = "Afastado";
        public const string StTransferido = "Transferido";
        public const string StDemitido = "Demitido";
        public const string StInativo = "Inativo";

        public static bool StatusValido(string status)
            => status == StRascunho || status == StAtivo || status == StAfastado
               || status == StTransferido || status == StDemitido || status == StInativo;

        // Secao 20.9: demissao bloqueia folha normal e aciona rescisao.
        public bool BloqueiaFolhaNormal() => Status == StDemitido;

        public void Demitir(string usuario)
        {
            Status = StDemitido;
            Ativo = false;
            MarcarAlterado(usuario);
        }

        public void AlterarStatus(string status, string usuario)
        {
            if (!StatusValido(status))
            {
                AddNotification(nameof(Status), "Status de colaborador invalido.");
                return;
            }
            Status = status;
            MarcarAlterado(usuario);
        }
    }

    public partial class WfmComissaoColaborador
    {
        // Secao 20.7/20.8: percentual maximo 100; tipo de cargo de dominio fechado.
        public const string CargoOperador = "Operador";
        public const string CargoVendedor = "Vendedor";
        public const string CargoSupervisor = "Supervisor";
        public const string CargoGerente = "Gerente";

        public static bool TipoCargoValido(string tipo)
            => tipo == CargoOperador || tipo == CargoVendedor || tipo == CargoSupervisor || tipo == CargoGerente;

        public static bool PercentualValido(decimal percentual) => percentual >= 0m && percentual <= 100m;

        public void ValidarRegras()
        {
            if (!TipoCargoValido(TipoCargo))
                AddNotification(nameof(TipoCargo), "Tipo de cargo invalido (Operador, Vendedor, Supervisor, Gerente).");
            if (!PercentualValido(ValorPercentualComissao))
                AddNotification(nameof(ValorPercentualComissao), "O percentual de comissao deve estar entre 0 e 100.");
        }
    }

    public partial class WfmTransferencia
    {
        public const string StPendente = "Pendente";
        public const string StAprovado = "Aprovado";
        public const string StEmAndamento = "Em andamento";
        public const string StRejeitado = "Rejeitado";
        public const string StCancelado = "Cancelado";

        public static bool StatusValido(string status)
            => status == StPendente || status == StAprovado || status == StEmAndamento
               || status == StRejeitado || status == StCancelado;
    }
}
