using System;

namespace Epros.Shared.Application.Models
{
    public interface ISyncable
    {
        Guid SyncId { get; }
        string TenantId { get; }
        int SyncVersion { get; }
        DateTime CriadoEm { get; }
        DateTime? AlteradoEm { get; }
    }
}
