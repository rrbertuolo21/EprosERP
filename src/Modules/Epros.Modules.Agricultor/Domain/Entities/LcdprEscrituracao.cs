using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Agricultor.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Agricultor.Domain.Entities
{
    /// <summary>
    /// Agregado raiz do submódulo LCDPR: 1 escrituração por CPF + ano (registros 0000 + 0010). Espelha o
    /// arquivo digital: 0030 (cadastrais), 0040 (imóveis, +0045), 0050 (contas), Q100 (lançamentos).
    /// Q200 (resumo mensal) e 9999 (encerramento) são DERIVADOS na geração (não persistidos). Chave de
    /// unicidade: CPF + DT_FIN. COD_VER do arquivo = 0013 (leiaute 1.3).
    /// </summary>
    public class LcdprEscrituracao : EntidadeSaaSBase
    {
        public string Cpf { get; private set; } = string.Empty;
        public string Nome { get; private set; } = string.Empty;
        public DateTime DtIni { get; private set; }
        public DateTime DtFin { get; private set; }
        public int IndSituacaoInicioPeriodo { get; private set; } // 0000 IND_SIT_INI_PER
        public int SituacaoEspecial { get; private set; }         // 0000 SIT_ESPECIAL
        public EFormaApuracao FormaApuracao { get; private set; } // 0010 FORMA_APUR
        public EStatusEscrituracaoLcdpr Status { get; private set; }

        // Encerramento 9999 — identificação do contador/responsável (preenchida ao fechar).
        public string? IdentificacaoNome { get; private set; }
        public string? IdentificacaoCpfCnpj { get; private set; }

        private readonly List<LcdprImovel> _imoveis = new();
        private readonly List<LcdprConta> _contas = new();
        private readonly List<LcdprLancamento> _lancamentos = new();

        public IReadOnlyCollection<LcdprImovel> Imoveis => _imoveis.AsReadOnly();
        public IReadOnlyCollection<LcdprConta> Contas => _contas.AsReadOnly();
        public IReadOnlyCollection<LcdprLancamento> Lancamentos => _lancamentos.AsReadOnly();

        public LcdprDadosCadastrais? DadosCadastrais { get; private set; }

        protected LcdprEscrituracao() { }

        public LcdprEscrituracao(string cpf, string nome, DateTime dtIni, DateTime dtFin,
            int indSituacaoInicioPeriodo, int situacaoEspecial, EFormaApuracao formaApuracao,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            Cpf = cpf;
            Nome = nome;
            DtIni = dtIni;
            DtFin = dtFin;
            IndSituacaoInicioPeriodo = indSituacaoInicioPeriodo;
            SituacaoEspecial = situacaoEspecial;
            FormaApuracao = formaApuracao;
            Status = EStatusEscrituracaoLcdpr.Aberta;
            Validar();
        }

        public void DefinirDadosCadastrais(LcdprDadosCadastrais dados)
        {
            dados.Vincular(Id);
            DadosCadastrais = dados;
        }

        public void AdicionarImovel(LcdprImovel imovel)
        {
            if (Status != EStatusEscrituracaoLcdpr.Aberta && Status != EStatusEscrituracaoLcdpr.Retificadora)
            { AddNotification(nameof(Status), "Escrituração não está aberta para edição. [Origem: LcdprEscrituracao]"); return; }
            if (_imoveis.Any(i => i.CodImovel == imovel.CodImovel))
            { AddNotification(nameof(imovel.CodImovel), $"COD_IMOVEL {imovel.CodImovel} duplicado no 0040. [Origem: LcdprEscrituracao]"); return; }
            imovel.Vincular(Id);
            _imoveis.Add(imovel);
        }

        public void AdicionarConta(LcdprConta conta)
        {
            if (_contas.Any(c => c.CodConta == conta.CodConta))
            { AddNotification(nameof(conta.CodConta), $"COD_CONTA {conta.CodConta} duplicado no 0050. [Origem: LcdprEscrituracao]"); return; }
            conta.Vincular(Id);
            _contas.Add(conta);
        }

        /// <summary>
        /// Adiciona um Q100, validando as referências contra 0040/0050 (RN-05/RN-06):
        /// COD_IMOVEL deve existir no 0040; COD_CONTA ∈ {000,999} ∪ contas do 0050.
        /// </summary>
        public void AdicionarLancamento(LcdprLancamento lanc)
        {
            if (Status != EStatusEscrituracaoLcdpr.Aberta && Status != EStatusEscrituracaoLcdpr.Retificadora)
            { AddNotification(nameof(Status), "Escrituração não está aberta para lançamentos. [Origem: LcdprEscrituracao]"); return; }

            lanc.Validar();
            if (!lanc.IsValid)
            { foreach (var n in lanc.Notifications) AddNotification(n.Key, n.Message); return; }

            if (_imoveis.All(i => i.CodImovel != lanc.CodImovel))
            { AddNotification(nameof(lanc.CodImovel), $"Q100: COD_IMOVEL {lanc.CodImovel} não existe no 0040. [Origem: LcdprEscrituracao] (RN-05, bloqueante)"); return; }

            var contaValida = lanc.CodConta == LcdprLancamento.CodContaEspecie
                || lanc.CodConta == LcdprLancamento.CodContaTransito
                || _contas.Any(c => c.CodConta == lanc.CodConta);
            if (!contaValida)
            { AddNotification(nameof(lanc.CodConta), $"Q100: COD_CONTA {lanc.CodConta} não está em {{000,999}} ∪ 0050. [Origem: LcdprEscrituracao] (RN-06, bloqueante)"); return; }

            lanc.Vincular(Id);
            _lancamentos.Add(lanc);
        }

        /// <summary>Fecha a escrituração (pronta para exportar). Exige >=1 0040 e >=1 0050 (EF §16).</summary>
        public void Fechar(string identificacaoNome, string identificacaoCpfCnpj, string usuario)
        {
            if (!_imoveis.Any())
            { AddNotification(nameof(Imoveis), "Escrituração exige ao menos um imóvel (0040). [Origem: LcdprEscrituracao] (bloqueante)"); return; }
            if (!_contas.Any())
            { AddNotification(nameof(Contas), "Escrituração exige ao menos uma conta (0050). [Origem: LcdprEscrituracao] (bloqueante)"); return; }

            IdentificacaoNome = identificacaoNome;
            IdentificacaoCpfCnpj = identificacaoCpfCnpj;
            Status = EStatusEscrituracaoLcdpr.Fechada;
            MarcarAlterado(usuario);
        }

        public void MarcarExportada(string usuario)
        {
            Status = EStatusEscrituracaoLcdpr.Exportada;
            MarcarAlterado(usuario);
        }

        public void ReabrirComoRetificadora(string usuario)
        {
            // AGR-D20: retificadora regenera o arquivo íntegro e completo (não é delta).
            Status = EStatusEscrituracaoLcdpr.Retificadora;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<LcdprEscrituracao>().Requires()
                .IsNotNullOrEmpty(Cpf, nameof(Cpf), "0000: CPF do declarante é obrigatório. [Origem: LcdprEscrituracao]")
                .IsNotNullOrEmpty(Nome, nameof(Nome), "0000: NOME é obrigatório. [Origem: LcdprEscrituracao]"));
            if (DtFin < DtIni)
                AddNotification(nameof(DtFin), "0000: DT_FIN não pode ser anterior a DT_INI. [Origem: LcdprEscrituracao]");
            if (!string.IsNullOrWhiteSpace(Cpf) && (Cpf.Length != 11 || !Cpf.All(char.IsDigit)))
                AddNotification(nameof(Cpf), "0000: CPF deve ter 11 dígitos numéricos. [Origem: LcdprEscrituracao]");
        }
    }

    /// <summary>
    /// AGR-D10 — motor de obrigatoriedade parametrizado por ANO-CALENDÁRIO. O limite NUNCA é hardcoded:
    /// vive nesta tabela (ex.: > R$ 4.800.000,00 vigente; R$ 7.200.000,00 no AC 2019). A comparação é
    /// sobre a receita bruta rural de TODAS as unidades do CPF (RN03). Valores = // valida-contador.
    /// </summary>
    public class LcdprParamObrigatoriedade : EntidadeSaaSBase
    {
        public int Ano { get; private set; }
        public decimal LimiteValor { get; private set; }
        public string? Origem { get; private set; } // norma vigente / AC 2019

        protected LcdprParamObrigatoriedade() { }

        public LcdprParamObrigatoriedade(int ano, decimal limiteValor, string? origem, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Ano = ano;
            LimiteValor = limiteValor;
            Origem = origem;
            AddNotifications(new Contract<LcdprParamObrigatoriedade>().Requires()
                .IsGreaterThan(limiteValor, 0m, nameof(LimiteValor), "O limite de obrigatoriedade deve ser > 0. [Origem: LcdprParamObrigatoriedade] (AGR-D10)")
                .IsGreaterThan(ano, 2000, nameof(Ano), "Ano-calendário inválido. [Origem: LcdprParamObrigatoriedade]"));
        }

        /// <summary>Obrigado a escriturar quando a receita bruta rural anual excede o limite do ano (RN03).</summary>
        public bool ObrigadoAEscriturar(decimal receitaBrutaRuralAnual) => receitaBrutaRuralAnual > LimiteValor;
    }
}
