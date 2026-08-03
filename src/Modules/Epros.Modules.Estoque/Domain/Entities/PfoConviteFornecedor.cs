using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Convite de fornecedor ao Portal (EF Portal do Fornecedor §15.1 `pfo_convite_fornecedor`).
    /// PFO-001/002: toda operação carrega tenant e é isolada por fornecedor. Referencia o cadastro
    /// mestre do fornecedor (Cadastros Base) — não o duplica.
    /// </summary>
    public class PfoConviteFornecedor : EntidadeSaaSBase
    {
        public Guid FornecedorId { get; private set; }
        public string EmailConvite { get; private set; } = string.Empty;
        public EStatusConviteFornecedor Status { get; private set; } = EStatusConviteFornecedor.Rascunho;
        public DateTime? DataEnvio { get; private set; }
        public DateTime? DataExpiracao { get; private set; }

        protected PfoConviteFornecedor() { }

        public PfoConviteFornecedor(Guid fornecedorId, string emailConvite, DateTime? dataExpiracao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            FornecedorId = fornecedorId;
            EmailConvite = emailConvite ?? string.Empty;
            DataExpiracao = dataExpiracao;
            Status = EStatusConviteFornecedor.Rascunho;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<PfoConviteFornecedor>()
                .Requires()
                .IsNotEmpty(FornecedorId, nameof(FornecedorId), "O fornecedor do convite é obrigatório [PFO-002] [Origem: PfoConviteFornecedor]")
                .IsNotNullOrEmpty(EmailConvite, nameof(EmailConvite), "O e-mail do convite é obrigatório [Origem: PfoConviteFornecedor]"));
        }

        public void Enviar(string usuario) { Status = EStatusConviteFornecedor.Enviado; DataEnvio = DateTime.UtcNow; MarcarAlterado(usuario); }
        public void Aceitar(string usuario) { Status = EStatusConviteFornecedor.Aceito; MarcarAlterado(usuario); }
        public void Cancelar(string usuario) { Status = EStatusConviteFornecedor.Cancelado; MarcarAlterado(usuario); }
        public bool PodeAceitar() => Status == EStatusConviteFornecedor.Enviado;
    }
}
