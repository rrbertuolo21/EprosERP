using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.DMS.Application.Commands
{
    // ==================== CON-MNT (oficina) ====================

    public record AprovarOrcamentoManutencaoCommand(Guid OrcamentoId) : ICommand;
    public class AprovarOrcamentoManutencaoCommandValidator : AbstractValidator<AprovarOrcamentoManutencaoCommand>
    {
        public AprovarOrcamentoManutencaoCommandValidator() => RuleFor(c => c.OrcamentoId).NotEmpty();
    }

    public record RejeitarOrcamentoManutencaoCommand(Guid OrcamentoId) : ICommand;
    public class RejeitarOrcamentoManutencaoCommandValidator : AbstractValidator<RejeitarOrcamentoManutencaoCommand>
    {
        public RejeitarOrcamentoManutencaoCommandValidator() => RuleFor(c => c.OrcamentoId).NotEmpty();
    }

    public record AtualizarStatusOsManutencaoCommand(Guid OrdemServicoId, string Status) : ICommand;
    public class AtualizarStatusOsManutencaoCommandValidator : AbstractValidator<AtualizarStatusOsManutencaoCommand>
    {
        public AtualizarStatusOsManutencaoCommandValidator()
        {
            RuleFor(c => c.OrdemServicoId).NotEmpty();
            RuleFor(c => c.Status).NotEmpty();
        }
    }

    public record EncerrarOsManutencaoCommand(Guid OrdemServicoId) : ICommand;
    public class EncerrarOsManutencaoCommandValidator : AbstractValidator<EncerrarOsManutencaoCommand>
    {
        public EncerrarOsManutencaoCommandValidator() => RuleFor(c => c.OrdemServicoId).NotEmpty();
    }

    // ==================== CON-PES ====================

    public record AtenderDemandaPecaCommand(Guid DemandaId) : ICommand;
    public class AtenderDemandaPecaCommandValidator : AbstractValidator<AtenderDemandaPecaCommand>
    {
        public AtenderDemandaPecaCommandValidator() => RuleFor(c => c.DemandaId).NotEmpty();
    }

    public record CancelarDemandaPecaCommand(Guid DemandaId) : ICommand;
    public class CancelarDemandaPecaCommandValidator : AbstractValidator<CancelarDemandaPecaCommand>
    {
        public CancelarDemandaPecaCommandValidator() => RuleFor(c => c.DemandaId).NotEmpty();
    }

    // ==================== CON-CRM ====================

    public record RealizarTestDriveCommand(Guid TestDriveId, string Resultado) : ICommand;
    public class RealizarTestDriveCommandValidator : AbstractValidator<RealizarTestDriveCommand>
    {
        public RealizarTestDriveCommandValidator()
        {
            RuleFor(c => c.TestDriveId).NotEmpty();
            RuleFor(c => c.Resultado).NotEmpty();
        }
    }

    public record CancelarTestDriveCommand(Guid TestDriveId) : ICommand;
    public class CancelarTestDriveCommandValidator : AbstractValidator<CancelarTestDriveCommand>
    {
        public CancelarTestDriveCommandValidator() => RuleFor(c => c.TestDriveId).NotEmpty();
    }

    // ==================== CON-VEN (ciclo da venda mestre) ====================

    public record FaturarVendaVeiculoCommand(Guid VendaId) : ICommand;
    public class FaturarVendaVeiculoCommandValidator : AbstractValidator<FaturarVendaVeiculoCommand>
    {
        public FaturarVendaVeiculoCommandValidator() => RuleFor(c => c.VendaId).NotEmpty();
    }

    public record EntregarVendaVeiculoCommand(Guid VendaId) : ICommand;
    public class EntregarVendaVeiculoCommandValidator : AbstractValidator<EntregarVendaVeiculoCommand>
    {
        public EntregarVendaVeiculoCommandValidator() => RuleFor(c => c.VendaId).NotEmpty();
    }

    // ==================== CON-DEV (rede) ====================

    public record EncerrarContratoRedeCommand(Guid ContratoId) : ICommand;
    public class EncerrarContratoRedeCommandValidator : AbstractValidator<EncerrarContratoRedeCommand>
    {
        public EncerrarContratoRedeCommandValidator() => RuleFor(c => c.ContratoId).NotEmpty();
    }

    public record CancelarContratoRedeCommand(Guid ContratoId) : ICommand;
    public class CancelarContratoRedeCommandValidator : AbstractValidator<CancelarContratoRedeCommand>
    {
        public CancelarContratoRedeCommandValidator() => RuleFor(c => c.ContratoId).NotEmpty();
    }

    // ==================== CON-GAR ====================

    public record EncerrarVeiculoGarantiaCommand(Guid VeiculoGarantiaId) : ICommand;
    public class EncerrarVeiculoGarantiaCommandValidator : AbstractValidator<EncerrarVeiculoGarantiaCommand>
    {
        public EncerrarVeiculoGarantiaCommandValidator() => RuleFor(c => c.VeiculoGarantiaId).NotEmpty();
    }
}
