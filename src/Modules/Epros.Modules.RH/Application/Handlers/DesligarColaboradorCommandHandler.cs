using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.RH.Application.Commands;
using Epros.Modules.RH.Domain.Entities;
using Epros.Modules.RH.Domain.Folha.Calculo;
using Epros.Modules.RH.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.RH.Application.Handlers
{
    public class DesligarColaboradorCommandHandler : ICommandHandler<DesligarColaboradorCommand>
    {
        private readonly ContextRH _context;
        private readonly ICurrentUser _currentUser;

        public DesligarColaboradorCommandHandler(
            ContextRH context,
            ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DesligarColaboradorCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var colaborador = await _context.Colaboradores
                .FirstOrDefaultAsync(c => c.Id == request.ColaboradorId, cancellationToken);

            if (colaborador == null)
            {
                return CommandResult.Falha("Colaborador não encontrado.");
            }

            colaborador.Desligar(request.DataDemissao, usuario);

            if (!colaborador.IsValid)
            {
                return CommandResult.Falha(colaborador.Notifications.Select(n => n.Message));
            }

            // Sem tipo de desligamento informado => desligamento simples (compatível com o fluxo legado).
            if (request.TipoDesligamento is not { } tipo)
            {
                await _context.SaveChangesAsync(cancellationToken);
                return CommandResult.Ok("Desligamento registrado com sucesso!", new { ColaboradorId = colaborador.Id, Status = colaborador.Status });
            }

            // Regra #0: apurar rescisão só com tabela do ano confirmada; sem tabela, o desligamento
            // ainda é registrado, mas a rescisão fica pendente (valida-contador), sem inventar valores.
            TabelasFolha tabelas;
            try
            {
                tabelas = TabelasFolha.Vigente(request.DataDemissao.Year);
            }
            catch (InvalidOperationException ex)
            {
                await _context.SaveChangesAsync(cancellationToken);
                return CommandResult.Ok(
                    $"Desligamento registrado. Rescisão NÃO apurada: {ex.Message}",
                    new { ColaboradorId = colaborador.Id, Status = colaborador.Status, RescisaoApurada = false });
            }

            var entrada = new EntradaRescisao(
                Tipo: tipo,
                SalarioMensal: colaborador.SalarioBase,
                DataAdmissao: colaborador.DataAdmissao,
                DataDesligamento: request.DataDemissao,
                DiasTrabalhadosNoMes: request.DataDemissao.Day,
                SaldoFgtsDepositado: request.SaldoFgtsDepositado,
                TemFeriasVencidas: request.TemFeriasVencidas,
                RemuneracaoFeriasVencidas: request.RemuneracaoFeriasVencidas,
                MediaVariaveis: request.MediaVariaveis,
                NumDependentes: request.NumDependentes,
                PensaoAlimenticia: request.PensaoAlimenticia);

            var r = MotorRescisao.Calcular(entrada, tabelas);

            // Persistir o cabeçalho da rescisão (entidade dedicada RH-FOL) com os valores apurados
            // pelo motor. Códigos eSocial/SEFIP ficam como rascunho (valida-contador) até o fechamento.
            var rescisao = new FolRescisao(
                colaboradorId: colaborador.Id,
                dataDemissao: request.DataDemissao,
                dataPagamento: null,
                motivo: tipo.ToString(),
                dataAvisoPrevio: null,
                diasAvisoPrevio: r.DiasAvisoPrevio,
                comprovouNovoEmprego: "N",          // valida-contador
                dispensouEmpregado: "N",            // valida-contador
                pensaoAlimenticia: request.PensaoAlimenticia > 0 ? request.PensaoAlimenticia : (decimal?)null,
                pensaoAlimenticiaFgts: null,
                fgtsValorRescisao: r.MultaFgts,
                fgtsSaldoBanco: request.SaldoFgtsDepositado > 0 ? request.SaldoFgtsDepositado : (decimal?)null,
                fgtsComplementoSaldo: null,
                fgtsCodigoAfastamento: "PENDENTE",  // valida-contador — código eSocial por tipo
                fgtsCodigoSaque: "PENDENTE",        // valida-contador — código de saque FGTS
                tenantId: colaborador.TenantId,
                criadoPor: usuario);

            if (!rescisao.IsValid)
            {
                return CommandResult.Falha(rescisao.Notifications.Select(n => n.Message));
            }

            _context.FolRescisaos.Add(rescisao);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok(
                $"Desligamento e rescisão apurados pelo motor ({tipo}). Líquido rescisório: {r.Liquido:N2}.",
                new
                {
                    ColaboradorId = colaborador.Id,
                    Status = colaborador.Status,
                    RescisaoId = rescisao.Id,
                    RescisaoApurada = true,
                    r.DiasAvisoPrevio,
                    r.AvosDecimoTerceiro,
                    r.AvosFerias,
                    r.TotalProventos,
                    r.Inss,
                    r.Irrf,
                    r.MultaFgts,
                    r.TotalDescontos,
                    r.Liquido,
                    r.TemDireitoSeguroDesemprego,
                    Verbas = r.Verbas.Select(v => new { v.Codigo, v.Descricao, v.Valor, v.Natureza })
                });
        }
    }
}
