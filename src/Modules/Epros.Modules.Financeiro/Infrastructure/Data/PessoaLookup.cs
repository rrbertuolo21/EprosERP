using System;

namespace Epros.Modules.Financeiro.Infrastructure.Data
{
    public class PessoaLookup
    {
        public Guid Id { get; set; }
        public bool EhFornecedor { get; set; }
        public bool EhCliente { get; set; }
        public int Status { get; set; }
        public int TipoPessoa { get; set; }
        public string TenantId { get; set; } = string.Empty;
        public string CriadoPor { get; set; } = "system";
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? DeletadoEm { get; set; }
    }

    public class PessoaJuridicaLookup
    {
        public Guid PessoaId { get; set; }
        public string Cnpj { get; set; } = string.Empty;
        public string RazaoSocial { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string CriadoPor { get; set; } = "system";
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? DeletadoEm { get; set; }
    }

    public class PessoaFisicaLookup
    {
        public Guid PessoaId { get; set; }
        public string Cpf { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string CriadoPor { get; set; } = "system";
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? DeletadoEm { get; set; }
    }
}
