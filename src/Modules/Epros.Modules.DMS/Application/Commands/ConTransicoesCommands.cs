using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.DMS.Application.Commands
{
    // ==================== CON-CRM — transições de Oportunidade ====================

    /// <summary>Avança a etapa do funil (D-02: expõe OportunidadeConcessionaria.AvancarEtapa).</summary>
    public record AvancarEtapaOportunidadeCommand(Guid OportunidadeId, string NovaEtapa) : ICommand;

    public class AvancarEtapaOportunidadeCommandValidator : AbstractValidator<AvancarEtapaOportunidadeCommand>
    {
        public AvancarEtapaOportunidadeCommandValidator()
        {
            RuleFor(c => c.OportunidadeId).NotEmpty();
            RuleFor(c => c.NovaEtapa).NotEmpty();
        }
    }

    /// <summary>Converte a oportunidade em venda (expõe Converter; idempotência de conversão na entidade).</summary>
    public record ConverterOportunidadeCommand(Guid OportunidadeId, Guid VendaId) : ICommand;

    public class ConverterOportunidadeCommandValidator : AbstractValidator<ConverterOportunidadeCommand>
    {
        public ConverterOportunidadeCommandValidator()
        {
            RuleFor(c => c.OportunidadeId).NotEmpty();
            RuleFor(c => c.VendaId).NotEmpty();
        }
    }

    /// <summary>Registra a perda da oportunidade com motivo (expõe RegistrarPerda).</summary>
    public record RegistrarPerdaOportunidadeCommand(Guid OportunidadeId, Guid MotivoPerdaId) : ICommand;

    public class RegistrarPerdaOportunidadeCommandValidator : AbstractValidator<RegistrarPerdaOportunidadeCommand>
    {
        public RegistrarPerdaOportunidadeCommandValidator()
        {
            RuleFor(c => c.OportunidadeId).NotEmpty();
            RuleFor(c => c.MotivoPerdaId).NotEmpty();
        }
    }

    // ==================== CON-VEN — transições de Proposta / Estoque / Reserva ====================

    /// <summary>Aceita a proposta (expõe PropostaVenda.Aceitar; só de emitida).</summary>
    public record AceitarPropostaVendaCommand(Guid PropostaId) : ICommand;

    public class AceitarPropostaVendaCommandValidator : AbstractValidator<AceitarPropostaVendaCommand>
    {
        public AceitarPropostaVendaCommandValidator() => RuleFor(c => c.PropostaId).NotEmpty();
    }

    public record ExpirarPropostaVendaCommand(Guid PropostaId) : ICommand;

    public class ExpirarPropostaVendaCommandValidator : AbstractValidator<ExpirarPropostaVendaCommand>
    {
        public ExpirarPropostaVendaCommandValidator() => RuleFor(c => c.PropostaId).NotEmpty();
    }

    /// <summary>Reserva a unidade em estoque (expõe EstoqueVeiculo.Reservar; só de "Livre").</summary>
    public record ReservarEstoqueVeiculoCommand(Guid EstoqueVeiculoId) : ICommand;

    public class ReservarEstoqueVeiculoCommandValidator : AbstractValidator<ReservarEstoqueVeiculoCommand>
    {
        public ReservarEstoqueVeiculoCommandValidator() => RuleFor(c => c.EstoqueVeiculoId).NotEmpty();
    }

    /// <summary>Libera a unidade em estoque (volta a "Livre").</summary>
    public record LiberarEstoqueVeiculoCommand(Guid EstoqueVeiculoId) : ICommand;

    public class LiberarEstoqueVeiculoCommandValidator : AbstractValidator<LiberarEstoqueVeiculoCommand>
    {
        public LiberarEstoqueVeiculoCommandValidator() => RuleFor(c => c.EstoqueVeiculoId).NotEmpty();
    }

    /// <summary>Cancela a reserva de veículo com motivo (expõe ReservaVeiculo.Cancelar).</summary>
    public record CancelarReservaVeiculoCommand(Guid ReservaId, string Motivo) : ICommand;

    public class CancelarReservaVeiculoCommandValidator : AbstractValidator<CancelarReservaVeiculoCommand>
    {
        public CancelarReservaVeiculoCommandValidator()
        {
            RuleFor(c => c.ReservaId).NotEmpty();
            RuleFor(c => c.Motivo).NotEmpty();
        }
    }

    // ==================== CON-FIN — transições de Jornada / Contrato ====================

    /// <summary>Encerra a jornada F&amp;I (expõe JornadaFin.Encerrar).</summary>
    public record EncerrarJornadaFinCommand(Guid JornadaId) : ICommand;

    public class EncerrarJornadaFinCommandValidator : AbstractValidator<EncerrarJornadaFinCommand>
    {
        public EncerrarJornadaFinCommandValidator() => RuleFor(c => c.JornadaId).NotEmpty();
    }

    /// <summary>Liquida o contrato de financiamento (expõe ContratoFin.Liquidar).</summary>
    public record LiquidarContratoFinCommand(Guid ContratoId) : ICommand;

    public class LiquidarContratoFinCommandValidator : AbstractValidator<LiquidarContratoFinCommand>
    {
        public LiquidarContratoFinCommandValidator() => RuleFor(c => c.ContratoId).NotEmpty();
    }

    /// <summary>Cancela o contrato de financiamento (expõe ContratoFin.Cancelar).</summary>
    public record CancelarContratoFinCommand(Guid ContratoId) : ICommand;

    public class CancelarContratoFinCommandValidator : AbstractValidator<CancelarContratoFinCommand>
    {
        public CancelarContratoFinCommandValidator() => RuleFor(c => c.ContratoId).NotEmpty();
    }

    // ==================== CON-GAR — julgamento de solicitação ====================

    /// <summary>Julga a solicitação de garantia (expõe SolicitacaoGarantia.Aprovar/Rejeitar; só de aberta).</summary>
    public record JulgarSolicitacaoGarantiaCommand(Guid SolicitacaoId, bool Aprovar) : ICommand;

    public class JulgarSolicitacaoGarantiaCommandValidator : AbstractValidator<JulgarSolicitacaoGarantiaCommand>
    {
        public JulgarSolicitacaoGarantiaCommandValidator() => RuleFor(c => c.SolicitacaoId).NotEmpty();
    }

    // ==================== CON-PES — cancelamento de reserva ====================

    /// <summary>Cancela a reserva de peça (expõe ReservaPeca.Cancelar).</summary>
    public record CancelarReservaPecaCommand(Guid ReservaId) : ICommand;

    public class CancelarReservaPecaCommandValidator : AbstractValidator<CancelarReservaPecaCommand>
    {
        public CancelarReservaPecaCommandValidator() => RuleFor(c => c.ReservaId).NotEmpty();
    }
}
