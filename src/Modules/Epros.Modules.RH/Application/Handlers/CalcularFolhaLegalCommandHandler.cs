using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.RH.Application.Commands;
using Epros.Modules.RH.Domain.Folha.Calculo;
using Epros.Modules.RH.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.RH.Application.Handlers
{
    /// <summary>
    /// Apura a folha mensal pelo MOTOR LEGAL (INSS/IRRF/FGTS, ordem de apuração da skill folha).
    /// Não persiste: devolve o holerite calculado (simulação/apuração). O salário-base vem do
    /// cadastro ativo (Colaborador). Regra #0: ano sem tabela confirmada => falha explícita (não inventa).
    /// </summary>
    public class CalcularFolhaLegalCommandHandler : ICommandHandler<CalcularFolhaLegalCommand>
    {
        private readonly ContextRH _context;

        public CalcularFolhaLegalCommandHandler(ContextRH context) => _context = context;

        public async Task<CommandResult> Handle(CalcularFolhaLegalCommand request, CancellationToken cancellationToken)
        {
            var colaborador = await _context.Colaboradores
                .FirstOrDefaultAsync(c => c.Id == request.ColaboradorId, cancellationToken);

            if (colaborador == null)
                return CommandResult.Falha("Colaborador não encontrado.");
            if (colaborador.Status != "Ativo")
                return CommandResult.Falha("Não é possível apurar folha para um colaborador desligado.");
            if (colaborador.SalarioBase <= 0)
                return CommandResult.Falha("Colaborador sem salário-base cadastrado.");

            TabelasFolha tabelas;
            try
            {
                tabelas = TabelasFolha.Vigente(request.AnoCompetencia);
            }
            catch (InvalidOperationException ex)
            {
                // Regra #0: não inventar tabela de ano não confirmado.
                return CommandResult.Falha(ex.Message);
            }

            var proventos = request.Proventos?
                .Select(p => new ItemProvento(p.Codigo, p.Descricao, p.Valor, p.IncideInss, p.IncideIrrf, p.IncideFgts))
                .ToList() ?? new List<ItemProvento>();
            var descontos = request.Descontos?
                .Select(d => new ItemDesconto(d.Codigo, d.Descricao, d.Valor))
                .ToList() ?? new List<ItemDesconto>();

            var entrada = new EntradaFolhaMensal(
                SalarioBase: colaborador.SalarioBase,
                ProventosAdicionais: proventos,
                DescontosDiversos: descontos,
                NumDependentes: request.NumDependentes,
                PensaoAlimenticiaBaseIrrf: request.PensaoAlimenticia,
                DescontoValeTransporte: request.DescontoValeTransporte,
                Aprendiz: request.Aprendiz);

            var r = MotorFolhaMensal.Calcular(entrada, tabelas);

            var holerite = new
            {
                ColaboradorId = colaborador.Id,
                NomeColaborador = colaborador.Nome,
                request.MesCompetencia,
                request.AnoCompetencia,
                AnoTabela = tabelas.Ano,
                Rubricas = r.Rubricas.Select(x => new { x.Codigo, x.Descricao, x.Tipo, x.Valor }),
                r.TotalProventos,
                r.BaseInss,
                r.Inss,
                r.BaseIrrf,
                r.Irrf,
                r.BaseFgts,
                r.Fgts,
                r.TotalDescontos,
                r.Liquido
            };

            return CommandResult.Ok(
                $"Folha apurada pelo motor legal (tabelas {tabelas.Ano}). Líquido: {r.Liquido:N2}.",
                holerite);
        }
    }
}
