using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Acesso de um usuário ao Portal vinculado a um fornecedor (EF Portal do Fornecedor §15.2
    /// `pfo_usuario_fornecedor`). Base do isolamento por fornecedor (PFO-002): o usuário só enxerga
    /// os dados do seu fornecedor. A autenticação externa reutiliza o padrão do Portal do Cliente (D6).
    /// </summary>
    public class PfoUsuarioFornecedor : EntidadeSaaSBase
    {
        public Guid FornecedorId { get; private set; }
        public Guid UsuarioId { get; private set; }
        public bool Ativo { get; private set; }
        public DateTime? UltimoAcessoEm { get; private set; }

        protected PfoUsuarioFornecedor() { }

        public PfoUsuarioFornecedor(Guid fornecedorId, Guid usuarioId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            FornecedorId = fornecedorId;
            UsuarioId = usuarioId;
            Ativo = true;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<PfoUsuarioFornecedor>()
                .Requires()
                .IsNotEmpty(FornecedorId, nameof(FornecedorId), "O fornecedor do acesso é obrigatório [PFO-002] [Origem: PfoUsuarioFornecedor]")
                .IsNotEmpty(UsuarioId, nameof(UsuarioId), "O usuário do acesso é obrigatório [PFO-003] [Origem: PfoUsuarioFornecedor]"));
        }

        public void RegistrarAcesso() => UltimoAcessoEm = DateTime.UtcNow;
        public void Desativar(string usuario) { Ativo = false; MarcarAlterado(usuario); }
    }
}
