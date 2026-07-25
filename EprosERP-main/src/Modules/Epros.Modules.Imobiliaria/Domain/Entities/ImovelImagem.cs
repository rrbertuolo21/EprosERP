using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Imobiliaria.Domain.Entities
{
    /// <summary>
    /// Imagem binaria vinculada ao imovel (EF GESTAO_IMOBILIARIA 11.3, tabela imo_imovel_imagem, RN-029).
    /// </summary>
    public class ImovelImagem : EntidadeSaaSBase
    {
        public Guid ImovelId { get; private set; }
        public byte[] Conteudo { get; private set; } = Array.Empty<byte>();
        public string? NomeArquivo { get; private set; }
        public string? ContentType { get; private set; }

        protected ImovelImagem() { } // EF Core

        public ImovelImagem(byte[] conteudo, string? nomeArquivo, string? contentType, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Conteudo = conteudo ?? Array.Empty<byte>();
            NomeArquivo = nomeArquivo;
            ContentType = contentType;
            Validar();
        }

        internal void VincularAoImovel(Guid imovelId) => ImovelId = imovelId;

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ImovelImagem>()
                .Requires()
                .IsGreaterThan(Conteudo.Length, 0, nameof(Conteudo),
                    "A imagem nao pode ser vazia. [Origem: ImovelImagem]"));
        }
    }
}
