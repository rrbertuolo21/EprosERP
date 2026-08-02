using System;
using Epros.Modules.Agricultor.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Agricultor.Domain.Entities
{
    /// <summary>
    /// Cultura (catálogo). Reaproveitado do Agris (crops.json, 250 itens EN). AGR-D03: entra em produção
    /// com catálogo curado BR; crop_id 1 = "N/A" (default do Agris). Curadoria é dado-mestre.
    /// </summary>
    public class Cultura : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string? Codigo { get; private set; }

        protected Cultura() { }

        public Cultura(string nome, string? codigo, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            Codigo = codigo;
            AddNotifications(new Contract<Cultura>().Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome da cultura é obrigatório. [Origem: Cultura]"));
        }
    }

    /// <summary>Categoria de despesa (Agris: expense categories). Ex.: Insumos, Folha/Mão de obra, Combustível.</summary>
    public class CategoriaDespesa : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string? Codigo { get; private set; }
        /// <summary>Marca a categoria de mão de obra (AGR-D02): Q100 TIPO_DOC=5 / TIPO_LANC=2.</summary>
        public bool EhFolhaMaoDeObra { get; private set; }

        protected CategoriaDespesa() { }

        public CategoriaDespesa(string nome, string? codigo, bool ehFolhaMaoDeObra, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            Codigo = codigo;
            EhFolhaMaoDeObra = ehFolhaMaoDeObra;
            AddNotifications(new Contract<CategoriaDespesa>().Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome da categoria é obrigatório. [Origem: CategoriaDespesa]"));
        }
    }

    /// <summary>Fornecedor (Agris: suppliers). ID_PARTIC candidato do Q100 quando pessoa jurídica/física.</summary>
    public class Fornecedor : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string? CpfCnpj { get; private set; }
        public string? Codigo { get; private set; }

        protected Fornecedor() { }

        public Fornecedor(string nome, string? cpfCnpj, string? codigo, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            CpfCnpj = cpfCnpj;
            Codigo = codigo;
            AddNotifications(new Contract<Fornecedor>().Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do fornecedor é obrigatório. [Origem: Fornecedor]"));
        }
    }

    /// <summary>
    /// Safra: talhão × cultura × período de cultivo. Entidade NOVA (não existe no Agris). O modelo Epros
    /// privilegia a safra sobre a cultura fixa do talhão. Custeia despesas e produz receitas por ciclo.
    /// </summary>
    public class Safra : EntidadeSaaSBase
    {
        public Guid TalhaoId { get; private set; }
        public Guid CulturaId { get; private set; }
        public DateTime? DataPlantio { get; private set; }
        public DateTime? DataColheita { get; private set; }
        public EStatusSafra Status { get; private set; }

        protected Safra() { }

        public Safra(Guid talhaoId, Guid culturaId, DateTime? dataPlantio, DateTime? dataColheita,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            TalhaoId = talhaoId;
            CulturaId = culturaId;
            DataPlantio = dataPlantio;
            DataColheita = dataColheita;
            Status = EStatusSafra.Planejada;
            Validar();
        }

        public void Iniciar(string usuario)
        {
            if (Status == EStatusSafra.Encerrada)
            { AddNotification(nameof(Status), "Safra encerrada não pode ser reiniciada. [Origem: Safra]"); return; }
            Status = EStatusSafra.EmAndamento;
            MarcarAlterado(usuario);
        }

        public void RegistrarColheita(DateTime dataColheita, string usuario)
        {
            DataColheita = dataColheita;
            Status = EStatusSafra.Colhida;
            MarcarAlterado(usuario);
        }

        public void Encerrar(string usuario)
        {
            Status = EStatusSafra.Encerrada;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            if (TalhaoId == Guid.Empty)
                AddNotification(nameof(TalhaoId), "A safra exige um talhão. [Origem: Safra]");
            if (CulturaId == Guid.Empty)
                AddNotification(nameof(CulturaId), "A safra exige uma cultura. [Origem: Safra]");
            if (DataPlantio.HasValue && DataColheita.HasValue && DataColheita < DataPlantio)
                AddNotification(nameof(DataColheita), "A colheita não pode ser anterior ao plantio. [Origem: Safra]");
        }
    }

    /// <summary>
    /// Colaborador da propriedade (Agris: staff = User + role). AGR-D02: em V1 é só cadastro/acesso;
    /// o custo de mão de obra entra como Despesa (categoria Folha), sem folha de pagamento própria.
    /// </summary>
    public class Colaborador : EntidadeSaaSBase
    {
        public Guid PropriedadeId { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string? Email { get; private set; }
        public EPapelColaborador Papel { get; private set; }

        protected Colaborador() { }

        public Colaborador(Guid propriedadeId, string nome, string? email, EPapelColaborador papel,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            PropriedadeId = propriedadeId;
            Nome = nome;
            Email = email;
            Papel = papel;
            AddNotifications(new Contract<Colaborador>().Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do colaborador é obrigatório. [Origem: Colaborador]"));
        }
    }

    /// <summary>Anotação de campo (Agris: fieldnotes). Ligada opcionalmente a um talhão e a um responsável.</summary>
    public class AnotacaoCampo : EntidadeSaaSBase
    {
        public Guid? TalhaoId { get; private set; }
        public Guid? DesignadoA { get; private set; }
        public string Titulo { get; private set; } = string.Empty;
        public string? Descricao { get; private set; }
        public string? Tipo { get; private set; }
        public string? LatLng { get; private set; }
        public EStatusAnotacao Status { get; private set; }
        public bool IsPublic { get; private set; }

        protected AnotacaoCampo() { }

        public AnotacaoCampo(string titulo, string? descricao, string? tipo, Guid? talhaoId, Guid? designadoA,
            string? latLng, bool isPublic, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            Titulo = titulo;
            Descricao = descricao;
            Tipo = tipo;
            TalhaoId = talhaoId;
            DesignadoA = designadoA;
            LatLng = latLng;
            IsPublic = isPublic;
            Status = EStatusAnotacao.Aberta;
            AddNotifications(new Contract<AnotacaoCampo>().Requires()
                .IsNotNullOrEmpty(titulo, nameof(Titulo), "O título da anotação é obrigatório. [Origem: AnotacaoCampo]"));
        }

        public void MudarStatus(EStatusAnotacao status, string usuario)
        {
            Status = status;
            MarcarAlterado(usuario);
        }
    }
}
