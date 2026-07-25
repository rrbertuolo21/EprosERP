using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities
{
    public class CustomPage : EntidadeSaaSBase
    {
        public string Slug { get; private set; } = string.Empty;
        public string Conteudo { get; private set; } = string.Empty;
        public string Status { get; private set; } = "Rascunho"; // Publicada, Rascunho

        protected CustomPage() { } // EF Core

        public CustomPage(string slug, string conteudo, string status, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<CustomPage>()
                .Requires()
                .IsNotNullOrEmpty(slug, nameof(Slug), "O slug da página é obrigatório")
                .IsNotNullOrEmpty(status, nameof(Status), "O status da página é obrigatório")
            );

            if (slug != null && !System.Text.RegularExpressions.Regex.IsMatch(slug, "^[a-z0-9]+(?:-[a-z0-9]+)*$"))
            {
                AddNotification(nameof(Slug), "O slug deve conter apenas letras minúsculas, números e hífens");
            }

            if (status != null && status != "Publicada" && status != "Rascunho")
            {
                AddNotification(nameof(Status), "O status da página deve ser Publicada ou Rascunho");
            }

            Slug = slug ?? string.Empty;
            Conteudo = conteudo ?? string.Empty;
            Status = status ?? string.Empty;
        }

        public void Atualizar(string conteudo, string status, string alteradoPor)
        {
            AddNotifications(new Contract<CustomPage>()
                .Requires()
                .IsNotNullOrEmpty(status, nameof(Status), "O status da página é obrigatório")
            );

            if (status != null && status != "Publicada" && status != "Rascunho")
            {
                AddNotification(nameof(Status), "O status da página deve ser Publicada ou Rascunho");
            }

            if (IsValid)
            {
                Conteudo = conteudo ?? string.Empty;
                Status = status ?? string.Empty;
                MarcarAlterado(alteradoPor);
            }
        }

        public void Publicar(string alteradoPor)
        {
            if (Status != "Rascunho")
            {
                AddNotification(nameof(Status), "A página só pode ser publicada se estiver em Rascunho");
            }

            if (IsValid)
            {
                Status = "Publicada";
                MarcarAlterado(alteradoPor);
            }
        }

        public void DefinirComoRascunho(string alteradoPor)
        {
            if (Status != "Publicada")
            {
                AddNotification(nameof(Status), "A página só pode ser definida como rascunho se estiver Publicada");
            }

            if (IsValid)
            {
                Status = "Rascunho";
                MarcarAlterado(alteradoPor);
            }
        }
    }
}
