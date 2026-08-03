using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Agricultor.Domain.Entities;
using Epros.Modules.Agricultor.Domain.Enums;

namespace Epros.Modules.Agricultor.Domain.Services
{
    public sealed record AchadoLcdpr(string Codigo, string Mensagem);

    public sealed record ResultadoValidacaoLcdpr(IReadOnlyList<AchadoLcdpr> Bloqueantes, IReadOnlyList<AchadoLcdpr> Alertas)
    {
        public bool Valido => Bloqueantes.Count == 0;
    }

    /// <summary>
    /// AGR-D20 — Validador próprio do LCDPR (a RFB NÃO disponibiliza validador — AGR-D13; não citar "PVA").
    /// Reproduz as checagens do leiaute 1.3 (EF §16; skill RN31–47). Bloqueantes impedem exportar; alertas
    /// apenas sinalizam. O que é política tributária do cliente (uso de 000/999, plano de contas) permanece
    /// // valida-contador — aqui validamos a ESTRUTURA e os domínios da norma.
    /// </summary>
    public class ValidadorLcdprService
    {
        private readonly GeradorArquivoLcdprService _gerador;

        public ValidadorLcdprService(GeradorArquivoLcdprService gerador) => _gerador = gerador;

        public ResultadoValidacaoLcdpr Validar(LcdprEscrituracao esc, decimal? limiteObrigatoriedadeAno = null)
        {
            var bloq = new List<AchadoLcdpr>();
            var alerta = new List<AchadoLcdpr>();

            // 0000: CPF do produtor (11 dígitos). COD_VER=0013 é garantido pelo gerador.
            if (string.IsNullOrWhiteSpace(esc.Cpf) || esc.Cpf.Length != 11 || !esc.Cpf.All(char.IsDigit))
                bloq.Add(new("RN31", "0000: CPF do declarante deve ter 11 dígitos numéricos."));

            // >=1 imóvel (0040) e >=1 conta (0050).
            if (!esc.Imoveis.Any())
                bloq.Add(new("EF16", "É obrigatório ao menos um imóvel (0040)."));
            if (!esc.Contas.Any())
                bloq.Add(new("EF16", "É obrigatório ao menos uma conta (0050)."));

            var codsImovel = esc.Imoveis.Select(i => i.CodImovel).ToHashSet();
            var codsConta = esc.Contas.Select(c => c.CodConta).ToHashSet();

            foreach (var l in esc.Lancamentos)
            {
                // Q100.COD_IMOVEL existe no 0040 (RN-05).
                if (!codsImovel.Contains(l.CodImovel))
                    bloq.Add(new("RN-05", $"Q100: COD_IMOVEL {l.CodImovel:000} não existe no 0040."));

                // Q100.COD_CONTA ∈ {000,999} ∪ 0050 (RN-06).
                var contaOk = l.CodConta == LcdprLancamento.CodContaEspecie
                    || l.CodConta == LcdprLancamento.CodContaTransito
                    || codsConta.Contains(l.CodConta);
                if (!contaOk)
                    bloq.Add(new("RN-06", $"Q100: COD_CONTA {l.CodConta:000} fora de {{000,999}} ∪ 0050."));

                // TIPO_LANC ∈ {1,2,3} (AGR-D08).
                if (!Enum.IsDefined(typeof(ETipoLancamentoLcdpr), l.TipoLanc))
                    bloq.Add(new("RN-07", "Q100: TIPO_LANC fora do domínio {1,2,3} (código 6 bloqueado — AGR-D08)."));

                // AGR-D12: receita nunca em COD_IMOVEL 000.
                if (l.VlEntrada > 0 && l.CodImovel == 0)
                    bloq.Add(new("AGR-D12", "Q100: receita não pode usar COD_IMOVEL '000'."));

                // Alerta: lançamento sem NUM_DOC.
                if (string.IsNullOrWhiteSpace(l.NumDoc))
                    alerta.Add(new("A-NUMDOC", $"Q100 em {l.Data:dd/MM/yyyy}: sem NUM_DOC."));

                // Alerta: uso de numerário em trânsito (999).
                if (l.CodConta == LcdprLancamento.CodContaTransito)
                    alerta.Add(new("A-999", $"Q100 em {l.Data:dd/MM/yyyy}: uso de COD_CONTA '999' (numerário em trânsito)."));
            }

            // 0045 obrigatório e soma de participação = 100% quando exigido (RN35).
            foreach (var imovel in esc.Imoveis)
            {
                var exigeTerceiros = imovel.Participacao < 100m || imovel.TipoExploracao != ETipoExploracao.Individual;
                if (exigeTerceiros)
                {
                    if (!imovel.Terceiros.Any())
                    {
                        bloq.Add(new("RN35", $"0045: imóvel {imovel.CodImovel:000} exige terceiros (participação < 100% ou exploração ≠ individual)."));
                    }
                    else
                    {
                        var soma = imovel.Participacao + imovel.Terceiros.Sum(t => t.PercContraparte);
                        if (Math.Abs(soma - 100m) > 0.01m)
                            bloq.Add(new("RN35", $"0045: soma das participações do imóvel {imovel.CodImovel:000} = {soma:0.00}%, deve ser 100%."));
                    }
                }
            }

            // Conciliação Q100 × Q200 (anual e mês a mês) — RN42 (bloqueante).
            var resumo = _gerador.DerivarResumoMensal(esc.Lancamentos);
            var totalEntradaQ100 = esc.Lancamentos.Sum(l => l.VlEntrada);
            var totalSaidaQ100 = esc.Lancamentos.Sum(l => l.VlSaida);
            var totalEntradaQ200 = resumo.Sum(r => r.VlEntrada);
            var totalSaidaQ200 = resumo.Sum(r => r.VlSaida);
            if (totalEntradaQ100 != totalEntradaQ200 || totalSaidaQ100 != totalSaidaQ200)
                bloq.Add(new("RN42", "Conciliação Q100×Q200 anual divergente."));

            // Alerta de obrigatoriedade (AGR-D10): receita próxima/acima do limite do ano. // valida-contador.
            if (limiteObrigatoriedadeAno.HasValue && limiteObrigatoriedadeAno.Value > 0)
            {
                var receitaBruta = totalEntradaQ100;
                if (receitaBruta > limiteObrigatoriedadeAno.Value)
                    alerta.Add(new("A-LIMITE", $"Receita bruta {receitaBruta:N2} acima do limite de obrigatoriedade do ano ({limiteObrigatoriedadeAno.Value:N2})."));
                else if (receitaBruta > limiteObrigatoriedadeAno.Value * 0.9m)
                    alerta.Add(new("A-LIMITE", "Receita bruta próxima do limite de obrigatoriedade do ano."));
            }

            return new ResultadoValidacaoLcdpr(bloq, alerta);
        }
    }
}
