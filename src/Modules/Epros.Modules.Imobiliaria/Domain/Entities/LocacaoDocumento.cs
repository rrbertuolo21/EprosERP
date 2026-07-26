using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Imobiliaria.Domain.Entities
{
    /// <summary>
    /// Documento anexo da locacao (EF GESTAO_IMOBILIARIA 11.11, tabela imo_locacao_documento, RN-014).
    /// </summary>
    public class LocacaoDocumento : EntidadeSaaSBase
    {
        public Guid LocacaoId { get; private set; }
        public byte[] Conteudo { get; private set; } = Array.Empty<byte>();
        public string? NomeArquivo { get; private set; }
        public string? ContentType { get; private set; }

        protected LocacaoDocumento() { } // EF Core

        public LocacaoDocumento(byte[] conteudo, string? nomeArquivo, string? contentType, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Conteudo = conteudo ?? Array.Empty<byte>();
            NomeArquivo = nomeArquivo;
            ContentType = contentType;
            Validar();
        }

        internal void VincularALocacao(Guid locacaoId) => LocacaoId = locacaoId;

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<LocacaoDocumento>()
                .Requires()
                .IsGreaterThan(Conteudo.Length, 0, nameof(Conteudo),
                    "O documento nao pode ser vazio. [Origem: LocacaoDocumento]"));
        }
    }
}
