using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Agricultor.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Agricultor.Domain.Entities
{
    /// <summary>
    /// Imóvel rural do produtor (agregado raiz do PAINEL). Entidade NOVA — o Agris usava tenant de
    /// "restaurante" (AGR-D01/AGR-D07). Carrega o conjunto mínimo que fecha o registro 0040 do LCDPR:
    /// CAD_ITR/CAFIR (N8 c/ DV, obrig. se zona rural + PAIS=BR — RN33), CAEPF (N14, condicional — RN34),
    /// NOME_IMOVEL, TIPO_EXPLORACAO (1..6), PARTICIPACAO (N5,2), endereço UF/COD_MUN/CEP (tabela SPED).
    /// // valida-contador: obrigatoriedade CAFIR/CAEPF é regra fiscal — estrutura pronta, regra citada à norma.
    /// </summary>
    public class PropriedadeRural : EntidadeSaaSBase
    {
        public string NomeImovel { get; private set; } = string.Empty;
        public string? Matricula { get; private set; }
        public string? Car { get; private set; }               // Cadastro Ambiental Rural
        public string? CadItrCafir { get; private set; }       // N8 com DV (AGR-D17)
        public string? Caepf { get; private set; }             // N14 condicional
        public ETipoExploracao TipoExploracao { get; private set; }
        public decimal Participacao { get; private set; }      // N5,2 percentual

        // Endereço (domínios validados contra tabela SPED — AGR-D16)
        public string? Uf { get; private set; }
        public string? CodigoMunicipioSped { get; private set; }
        public string? Cep { get; private set; }
        public string? Endereco { get; private set; }

        // Operacional não-fiscal
        public decimal AreaTotalM2 { get; private set; }

        private readonly List<Talhao> _talhoes = new();
        public IReadOnlyCollection<Talhao> Talhoes => _talhoes.AsReadOnly();

        protected PropriedadeRural() { } // EF Core

        public PropriedadeRural(
            string nomeImovel,
            string? matricula,
            string? car,
            string? cadItrCafir,
            string? caepf,
            ETipoExploracao tipoExploracao,
            decimal participacao,
            string? uf,
            string? codigoMunicipioSped,
            string? cep,
            string? endereco,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            NomeImovel = nomeImovel;
            Matricula = matricula;
            Car = car;
            CadItrCafir = cadItrCafir;
            Caepf = caepf;
            TipoExploracao = tipoExploracao;
            Participacao = participacao;
            Uf = uf;
            CodigoMunicipioSped = codigoMunicipioSped;
            Cep = cep;
            Endereco = endereco;
            Validar();
        }

        public void AdicionarTalhao(Talhao talhao)
        {
            talhao.VincularAPropriedade(Id);
            _talhoes.Add(talhao);
        }

        public void AtualizarDados(
            string nomeImovel,
            string? matricula,
            string? car,
            string? cadItrCafir,
            string? caepf,
            ETipoExploracao tipoExploracao,
            decimal participacao,
            string? uf,
            string? codigoMunicipioSped,
            string? cep,
            string? endereco,
            string usuario)
        {
            NomeImovel = nomeImovel;
            Matricula = matricula;
            Car = car;
            CadItrCafir = cadItrCafir;
            Caepf = caepf;
            TipoExploracao = tipoExploracao;
            Participacao = participacao;
            Uf = uf;
            CodigoMunicipioSped = codigoMunicipioSped;
            Cep = cep;
            Endereco = endereco;
            Validar();
            if (!IsValid) return;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<PropriedadeRural>()
                .Requires()
                .IsNotNullOrEmpty(NomeImovel, nameof(NomeImovel),
                    "O nome do imóvel é obrigatório (alimenta o 0040 do LCDPR). [Origem: PropriedadeRural] (AGR-D01)")
                .IsGreaterOrEqualsThan(Participacao, 0m, nameof(Participacao),
                    "A participação não pode ser negativa. [Origem: PropriedadeRural] (AGR-D01)")
                .IsLowerOrEqualsThan(Participacao, 100m, nameof(Participacao),
                    "A participação não pode exceder 100%. [Origem: PropriedadeRural] (AGR-D01)"));

            if (!Enum.IsDefined(typeof(ETipoExploracao), TipoExploracao))
                AddNotification(nameof(TipoExploracao),
                    "Tipo de exploração fora do domínio 1..6 do 0040. [Origem: PropriedadeRural] (AGR-D01)");

            // CAD_ITR/CAFIR: N8 com dígito verificador quando informado (AGR-D17). A obrigatoriedade
            // (zona rural + PAIS=BR) é regra fiscal do overlay — // valida-contador.
            if (!string.IsNullOrWhiteSpace(CadItrCafir) && (CadItrCafir!.Length > 8 || !CadItrCafir.All(char.IsDigit)))
                AddNotification(nameof(CadItrCafir),
                    "CAD_ITR/CAFIR deve ser numérico com no máximo 8 dígitos (N8 c/ DV). [Origem: PropriedadeRural] (AGR-D17)");

            if (!string.IsNullOrWhiteSpace(Caepf) && (Caepf!.Length > 14 || !Caepf.All(char.IsDigit)))
                AddNotification(nameof(Caepf),
                    "CAEPF deve ser numérico com no máximo 14 dígitos (N14). [Origem: PropriedadeRural] (AGR-D17)");

            foreach (var t in _talhoes) AddNotifications(t);
        }
    }
}
