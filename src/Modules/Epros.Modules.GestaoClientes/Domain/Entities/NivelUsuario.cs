using System;
using System.Collections.Generic;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>Nível/plano de usuário com quotas (APP-TEN-003 11.11).</summary>
    public class NivelUsuario : EntidadeSaaSBase
    {
        public int LevelId { get; private set; }
        public string Label { get; private set; } = string.Empty;
        public bool CanUpload { get; private set; }
        public int WaitBetweenDownloads { get; private set; }
        public int DownloadSpeed { get; private set; }
        public long MaxStorageBytes { get; private set; }
        public bool ShowSiteAdverts { get; private set; }
        public bool ShowUpgradeScreen { get; private set; }
        public int DaysToKeepInactiveFiles { get; private set; }
        public int ConcurrentUploads { get; private set; }
        public int ConcurrentDownloads { get; private set; }
        public int DownloadsPer24Hours { get; private set; }
        public long MaxDownloadFilesizeAllowed { get; private set; }
        public int MaxRemoteDownloadUrls { get; private set; }
        public long MaxUploadSize { get; private set; }
        public ENivelUsuarioTipo LevelType { get; private set; }
        public bool OnUpgradePage { get; private set; }

        public List<PrecoNivelUsuario> Precos { get; private set; } = new();

        protected NivelUsuario() { } // EF Core

        public NivelUsuario(
            int levelId,
            string label,
            bool canUpload,
            int waitBetweenDownloads,
            int downloadSpeed,
            long maxStorageBytes,
            bool showSiteAdverts,
            bool showUpgradeScreen,
            int daysToKeepInactiveFiles,
            int concurrentUploads,
            int concurrentDownloads,
            int downloadsPer24Hours,
            long maxDownloadFilesizeAllowed,
            int maxRemoteDownloadUrls,
            long maxUploadSize,
            ENivelUsuarioTipo levelType,
            bool onUpgradePage,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            LevelId = levelId;
            Label = label;
            CanUpload = canUpload;
            WaitBetweenDownloads = waitBetweenDownloads;
            DownloadSpeed = downloadSpeed;
            MaxStorageBytes = maxStorageBytes;
            ShowSiteAdverts = showSiteAdverts;
            ShowUpgradeScreen = showUpgradeScreen;
            DaysToKeepInactiveFiles = daysToKeepInactiveFiles;
            ConcurrentUploads = concurrentUploads;
            ConcurrentDownloads = concurrentDownloads;
            DownloadsPer24Hours = downloadsPer24Hours;
            MaxDownloadFilesizeAllowed = maxDownloadFilesizeAllowed;
            MaxRemoteDownloadUrls = maxRemoteDownloadUrls;
            MaxUploadSize = maxUploadSize;
            LevelType = levelType;
            OnUpgradePage = onUpgradePage;
            Validar();
        }

        public void Alterar(
            string label,
            bool canUpload,
            long maxStorageBytes,
            int concurrentUploads,
            int concurrentDownloads,
            long maxUploadSize,
            ENivelUsuarioTipo levelType,
            bool onUpgradePage,
            string alteradoPor)
        {
            Label = label;
            CanUpload = canUpload;
            MaxStorageBytes = maxStorageBytes;
            ConcurrentUploads = concurrentUploads;
            ConcurrentDownloads = concurrentDownloads;
            MaxUploadSize = maxUploadSize;
            LevelType = levelType;
            OnUpgradePage = onUpgradePage;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<NivelUsuario>()
                .Requires()
                .IsGreaterThan(LevelId, 0, nameof(LevelId), "LevelId deve ser maior que zero [Origem: NivelUsuario]")
                .IsNotNullOrEmpty(Label, nameof(Label), "Label é obrigatório [Origem: NivelUsuario]")
                .HasMaxLen(Label ?? string.Empty, 20, nameof(Label), "Label deve ter no máximo 20 caracteres [Origem: NivelUsuario]")
                .IsTrue(Enum.IsDefined(typeof(ENivelUsuarioTipo), LevelType), nameof(LevelType), "LevelType não consta na lista [Origem: NivelUsuario]")
            );
        }
    }
}
