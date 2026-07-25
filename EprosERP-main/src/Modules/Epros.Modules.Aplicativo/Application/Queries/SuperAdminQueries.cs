using System.Collections.Generic;
using Epros.Modules.Aplicativo.Application.Dtos;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Aplicativo.Application.Queries
{
    public record ObterDashboardGlobalQuery() : IQuery<DashboardGlobalDto>;

    public record ListarUsuariosInternosQuery() : IQuery<IEnumerable<UsuarioInternoDto>>;

    public record ListarSystemSettingsQuery() : IQuery<IEnumerable<SystemSettingDto>>;

    public record ListarExecucoesMassaGlobalQuery() : IQuery<IEnumerable<ExecucaoMassaGlobalDto>>;

    public record ListarCustomPagesQuery() : IQuery<IEnumerable<CustomPageDto>>;

    public record ListarNewsletterSubscribersQuery() : IQuery<IEnumerable<NewsletterSubscriberDto>>;

    public record ListarClientesQuery() : IQuery<IEnumerable<SuperAdminClienteDto>>;
}
