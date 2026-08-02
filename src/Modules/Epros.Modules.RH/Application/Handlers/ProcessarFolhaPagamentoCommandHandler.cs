using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Epros.Modules.RH.Application.Commands;
using Epros.Modules.RH.Domain.Entities;
using Epros.Modules.RH.Domain.Folha.Calculo;
using Epros.Modules.RH.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.RH.Application.Handlers
{
    /// <summary>
    /// Processa e PERSISTE a folha mensal pelo MOTOR LEGAL (INSS/IRRF/FGTS na ordem de apuração da
    /// skill departamento-pessoal/folha). Antes, este handler gravava líquido = bruto − verbas manuais,
    /// IGNORANDO o motor — cálculo inventado. Agora as verbas informadas (proventos/descontos) alimentam
    /// o <see cref="MotorFolhaMensal"/>, que apura INSS/IRRF/FGTS; as rubricas calculadas são persistidas
    /// e o líquido gravado é o do motor. Regra #0: ano sem tabela confirmada => falha (não inventa).
    /// </summary>
    public class ProcessarFolhaPagamentoCommandHandler : ICommandHandler<ProcessarFolhaPagamentoCommand>
    {
        private readonly ContextRH _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ProcessarFolhaPagamentoCommandHandler(
            ContextRH context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ProcessarFolhaPagamentoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // Buscar colaborador
            var colaborador = await _context.Colaboradores
                .FirstOrDefaultAsync(c => c.Id == request.ColaboradorId, cancellationToken);

            if (colaborador == null)
            {
                return CommandResult.Falha("Colaborador não encontrado.");
            }

            if (colaborador.Status != "Ativo")
            {
                return CommandResult.Falha("Não é possível processar folha de pagamento para um colaborador desligado.");
            }

            if (colaborador.SalarioBase <= 0)
            {
                return CommandResult.Falha("Colaborador sem salário-base cadastrado.");
            }

            // Idempotência: uma folha por colaborador/competência.
            var folhaExiste = await _context.FolhasPagamento
                .AnyAsync(f => f.ColaboradorId == request.ColaboradorId
                            && f.MesCompetencia == request.MesCompetencia
                            && f.AnoCompetencia == request.AnoCompetencia, cancellationToken);

            if (folhaExiste)
            {
                return CommandResult.Falha($"Já existe uma folha de pagamento processada para este colaborador na competência {request.MesCompetencia:D2}/{request.AnoCompetencia}.");
            }

            // Regra #0: não inventar tabela de ano não confirmado.
            TabelasFolha tabelas;
            try
            {
                tabelas = TabelasFolha.Vigente(request.AnoCompetencia);
            }
            catch (InvalidOperationException ex)
            {
                return CommandResult.Falha(ex.Message);
            }

            // As verbas informadas alimentam o motor: proventos habituais compõem as bases
            // (INSS/IRRF/FGTS incidem por padrão); descontos manuais entram como diversos.
            var proventosAdicionais = new List<ItemProvento>();
            var descontosDiversos = new List<ItemDesconto>();
            int seqProv = 1, seqDesc = 1;
            if (request.Verbas != null)
            {
                foreach (var verba in request.Verbas)
                {
                    if (verba.Tipo == "Provento")
                        proventosAdicionais.Add(new ItemProvento($"P{seqProv++:D3}", verba.Descricao, verba.Valor));
                    else if (verba.Tipo == "Desconto")
                        descontosDiversos.Add(new ItemDesconto($"D{seqDesc++:D3}", verba.Descricao, verba.Valor));
                }
            }

            var entrada = new EntradaFolhaMensal(
                SalarioBase: colaborador.SalarioBase,
                ProventosAdicionais: proventosAdicionais,
                DescontosDiversos: descontosDiversos);

            var resultado = MotorFolhaMensal.Calcular(entrada, tabelas);

            // Persistir a folha: bruto semeado com o salário-base; as demais rubricas (adicionais,
            // INSS/IRRF, VT, diversos e o encargo FGTS) são adicionadas a partir do resultado do motor.
            var folha = new FolhaPagamento(
                colaborador.Id,
                request.MesCompetencia,
                request.AnoCompetencia,
                colaborador.SalarioBase,
                tenantId,
                usuario);

            foreach (var rubrica in resultado.Rubricas)
            {
                // "001" (salário base) já é o valor semeado no bruto — não duplicar.
                if (rubrica.Codigo == "001") continue;
                folha.AdicionarVerba(rubrica.Descricao, rubrica.Tipo, rubrica.Valor, usuario);
            }

            if (!folha.IsValid)
            {
                return CommandResult.Falha(folha.Notifications.Select(n => n.Message));
            }

            _context.FolhasPagamento.Add(folha);

            // Enfileirar evento de folha processada para integração de contas a pagar no Financeiro.
            var payload = new
            {
                FolhaPagamentoId = folha.Id,
                ColaboradorId = colaborador.Id,
                NomeColaborador = colaborador.Nome,
                CpfColaborador = colaborador.Cpf,
                MesCompetencia = folha.MesCompetencia,
                AnoCompetencia = folha.AnoCompetencia,
                SalarioLiquido = folha.SalarioLiquido,
                TenantId = tenantId
            };

            var payloadJson = JsonSerializer.Serialize(payload);
            var outboxMessage = new OutboxMessage(tenantId, "FolhaProcessada", payloadJson);
            _context.OutboxMessages.Add(outboxMessage);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok(
                $"Folha de pagamento processada pelo motor legal (tabelas {tabelas.Ano}). Líquido: {folha.SalarioLiquido:N2}.",
                new
                {
                    FolhaPagamentoId = folha.Id,
                    folha.SalarioBruto,
                    folha.Descontos,
                    folha.SalarioLiquido,
                    resultado.Inss,
                    resultado.Irrf,
                    resultado.Fgts
                });
        }
    }
}
