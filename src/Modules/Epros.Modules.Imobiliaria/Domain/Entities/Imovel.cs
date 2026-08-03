using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Imobiliaria.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Imobiliaria.Domain.Entities
{
    /// <summary>
    /// Mestre imobiliario (EF GESTAO_IMOBILIARIA 11.1, tabela imo_imovel).
    /// Agregado raiz de proprietarios, imagens, custos e vistorias.
    /// RN-001: descricao e proprietario obrigatorios. RN-002/RN-006: persistencia atomica do conjunto.
    /// </summary>
    public class Imovel : EntidadeSaaSBase
    {
        public string Descricao { get; private set; } = string.Empty;
        public Guid? MunicipioId { get; private set; }
        public string? Cep { get; private set; }
        public string? Logradouro { get; private set; }
        public string? Numero { get; private set; }
        public string? Complemento { get; private set; }
        public string? Bairro { get; private set; }
        // Campos preservados do material original (EF 11.1); finalidade funcional nao informada.
        public string? Test1 { get; private set; }
        public string? Test2 { get; private set; }
        public EStatusImovel Status { get; private set; }

        private readonly List<ImovelProprietario> _proprietarios = new();
        private readonly List<ImovelImagem> _imagens = new();
        private readonly List<ImovelCusto> _custos = new();
        private readonly List<ImovelVistoria> _vistorias = new();

        public IReadOnlyCollection<ImovelProprietario> Proprietarios => _proprietarios.AsReadOnly();
        public IReadOnlyCollection<ImovelImagem> Imagens => _imagens.AsReadOnly();
        public IReadOnlyCollection<ImovelCusto> Custos => _custos.AsReadOnly();
        public IReadOnlyCollection<ImovelVistoria> Vistorias => _vistorias.AsReadOnly();

        protected Imovel() { } // EF Core

        public Imovel(
            string descricao,
            Guid? municipioId,
            string? cep,
            string? logradouro,
            string? numero,
            string? complemento,
            string? bairro,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Descricao = descricao;
            MunicipioId = municipioId;
            Cep = cep;
            Logradouro = logradouro;
            Numero = numero;
            Complemento = complemento;
            Bairro = bairro;
            Status = EStatusImovel.EmCadastro;
            Validar();
        }

        public void AdicionarProprietario(ImovelProprietario proprietario)
        {
            proprietario.VincularAoImovel(Id);
            _proprietarios.Add(proprietario);
        }

        public void AdicionarImagem(ImovelImagem imagem)
        {
            imagem.VincularAoImovel(Id);
            _imagens.Add(imagem);
        }

        public void AdicionarCusto(ImovelCusto custo)
        {
            custo.VincularAoImovel(Id);
            _custos.Add(custo);
        }

        public void AdicionarVistoria(ImovelVistoria vistoria)
        {
            vistoria.VincularAoImovel(Id);
            _vistorias.Add(vistoria);
        }

        /// <summary>
        /// Atualiza os dados cadastrais do imovel (ID3/PRD-03). Auditoria antes→depois via T8.
        /// </summary>
        public void AtualizarDados(
            string descricao,
            Guid? municipioId,
            string? cep,
            string? logradouro,
            string? numero,
            string? complemento,
            string? bairro,
            string usuario)
        {
            Descricao = descricao;
            MunicipioId = municipioId;
            Cep = cep;
            Logradouro = logradouro;
            Numero = numero;
            Complemento = complemento;
            Bairro = bairro;
            Validar();
            if (!IsValid) return;
            MarcarAlterado(usuario);
        }

        // ===== Ciclo de vida do imovel (ID1/PRD-01) — mapeado ao status canonico T3 =====

        /// <summary>EmCadastro/Inativo → Disponivel. Exige cadastro valido (RN-001).</summary>
        public void Disponibilizar(string usuario)
        {
            if (Status != EStatusImovel.EmCadastro && Status != EStatusImovel.Inativo)
            {
                AddNotification(nameof(Status),
                    "Apenas imoveis em cadastro ou inativos podem ser disponibilizados.");
                return;
            }
            Validar();
            if (!IsValid) return;
            Status = EStatusImovel.Disponivel;
            MarcarAlterado(usuario);
        }

        /// <summary>Disponivel/EmCadastro → Inativo. Bloqueado se locado.</summary>
        public void Inativar(string usuario)
        {
            if (Status == EStatusImovel.Locado)
            {
                AddNotification(nameof(Status), "Imovel locado nao pode ser inativado.");
                return;
            }
            if (Status == EStatusImovel.Inativo) return; // idempotente
            Status = EStatusImovel.Inativo;
            MarcarAlterado(usuario);
        }

        /// <summary>Inativo → Disponivel (reativacao governada).</summary>
        public void Reativar(string usuario) => Disponibilizar(usuario);

        /// <summary>
        /// Efeito colateral de locacao vigente (ID1): Disponivel → Locado.
        /// Chamado pelo fluxo de formalizacao da locacao.
        /// </summary>
        public void MarcarLocado(string usuario)
        {
            if (Status == EStatusImovel.Locado) return; // idempotente
            if (Status == EStatusImovel.Inativo)
            {
                AddNotification(nameof(Status), "Imovel inativo nao pode ser locado.");
                return;
            }
            Status = EStatusImovel.Locado;
            MarcarAlterado(usuario);
        }

        /// <summary>
        /// Efeito colateral de encerramento/cancelamento da locacao (ID1): Locado → Disponivel.
        /// </summary>
        public void LiberarLocacao(string usuario)
        {
            if (Status != EStatusImovel.Locado) return; // idempotente/sem efeito
            Status = EStatusImovel.Disponivel;
            MarcarAlterado(usuario);
        }

        /// <summary>Mapa do ciclo proprio ao status canonico da plataforma (T3).</summary>
        public Epros.Shared.Domain.Enums.ESituacaoCanonica SituacaoCanonica => Status switch
        {
            EStatusImovel.EmCadastro => Epros.Shared.Domain.Enums.ESituacaoCanonica.Rascunho,
            EStatusImovel.Disponivel => Epros.Shared.Domain.Enums.ESituacaoCanonica.Ativo,
            EStatusImovel.Locado => Epros.Shared.Domain.Enums.ESituacaoCanonica.Ativo,
            EStatusImovel.Inativo => Epros.Shared.Domain.Enums.ESituacaoCanonica.Inativo,
            _ => Epros.Shared.Domain.Enums.ESituacaoCanonica.Rascunho
        };

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<Imovel>()
                .Requires()
                .IsNotNullOrEmpty(Descricao, nameof(Descricao),
                    "A descricao do imovel e obrigatoria. [Origem: Imovel] (RN-001)"));

            // RN-001: ao menos um proprietario deve estar vinculado.
            if (!_proprietarios.Any())
            {
                AddNotification(nameof(Proprietarios),
                    "O imovel exige ao menos um proprietario. [Origem: Imovel] (RN-001)");
            }

            foreach (var p in _proprietarios) AddNotifications(p);
            foreach (var i in _imagens) AddNotifications(i);
            foreach (var c in _custos) AddNotifications(c);
            foreach (var v in _vistorias) AddNotifications(v);
        }
    }
}
