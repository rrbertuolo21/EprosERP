using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Domain.Entities.Plataforma.Iot;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Plataforma.Iot
{
    /// <summary>
    /// PLT · IoT — cadastro de dispositivo/sensor, vínculo a ativo e ingestão de leitura (série
    /// temporal). Leitura fora da faixa emite <c>plt.iot.leitura_fora_faixa</c> e
    /// <c>plt.iot.condicao_operacional_detectada</c> (condição p/ Manutenção preditiva — não é ordem).
    /// </summary>
    public class RegistrarDispositivoIotCommandHandler : ICommandHandler<RegistrarDispositivoIotCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public RegistrarDispositivoIotCommandHandler(ContextAplicativo context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
        }

        public async Task<CommandResult> Handle(RegistrarDispositivoIotCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            if (await _context.DispositivosIot.AnyAsync(d => d.Codigo == request.Codigo, ct))
                return CommandResult.Falha("Já existe dispositivo com este código.");

            var disp = new DispositivoIot(request.Codigo, request.Nome, request.Tipo, request.Protocolo,
                request.AtivoVinculadoTipo, request.AtivoVinculadoId, tenantId, usuario);
            if (!disp.IsValid) return CommandResult.Falha(disp.Notifications.Select(n => n.Message));

            _context.DispositivosIot.Add(disp);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Dispositivo registrado.", new { disp.Id });
        }
    }

    public class RegistrarSensorIotCommandHandler : ICommandHandler<RegistrarSensorIotCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public RegistrarSensorIotCommandHandler(ContextAplicativo context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
        }

        public async Task<CommandResult> Handle(RegistrarSensorIotCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            if (!await _context.DispositivosIot.AnyAsync(d => d.Id == request.DispositivoId, ct))
                return CommandResult.Falha("Dispositivo não encontrado.");
            if (await _context.SensoresIot.AnyAsync(s => s.DispositivoId == request.DispositivoId && s.Codigo == request.Codigo, ct))
                return CommandResult.Falha("Já existe sensor com este código no dispositivo.");

            var sensor = new SensorIot(request.DispositivoId, request.Codigo, request.Grandeza, request.Unidade,
                request.LimiteMin, request.LimiteMax, request.RetencaoDias, tenantId, usuario);
            if (!sensor.IsValid) return CommandResult.Falha(sensor.Notifications.Select(n => n.Message));

            _context.SensoresIot.Add(sensor);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Sensor registrado.", new { sensor.Id });
        }
    }

    public class VincularDispositivoAtivoCommandHandler : ICommandHandler<VincularDispositivoAtivoCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ICurrentUser _user;

        public VincularDispositivoAtivoCommandHandler(ContextAplicativo context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context;
            _user = user;
        }

        public async Task<CommandResult> Handle(VincularDispositivoAtivoCommand request, CancellationToken ct)
        {
            var usuario = _user.GetUserId() ?? "system";
            var disp = await _context.DispositivosIot.FirstOrDefaultAsync(d => d.Id == request.DispositivoId, ct);
            if (disp == null) return CommandResult.Falha("Dispositivo não encontrado.");
            if (string.IsNullOrWhiteSpace(request.AtivoTipo) || string.IsNullOrWhiteSpace(request.AtivoId))
                return CommandResult.Falha("Tipo e id do ativo são obrigatórios.");

            disp.VincularAtivo(request.AtivoTipo, request.AtivoId, usuario);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Dispositivo vinculado ao ativo.");
        }
    }

    public class IngestarLeituraCommandHandler : ICommandHandler<IngestarLeituraCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public IngestarLeituraCommandHandler(ContextAplicativo context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
        }

        public async Task<CommandResult> Handle(IngestarLeituraCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var sensor = await _context.SensoresIot.FirstOrDefaultAsync(s => s.Id == request.SensorId, ct);
            if (sensor == null) return CommandResult.Falha("Sensor não encontrado.");

            var medidoEm = request.MedidoEm ?? DateTime.UtcNow;
            var foraFaixa = sensor.EstaForaFaixa(request.Valor);

            var leitura = new LeituraSensor(sensor.Id, request.Valor, medidoEm, foraFaixa, tenantId, usuario);
            _context.LeiturasSensor.Add(leitura);

            var disp = await _context.DispositivosIot.FirstOrDefaultAsync(d => d.Id == sensor.DispositivoId, ct);
            disp?.RegistrarLeitura(medidoEm, usuario);

            if (foraFaixa)
            {
                var payload = JsonSerializer.Serialize(new
                {
                    sensor.Id, sensor.Codigo, sensor.Grandeza, request.Valor,
                    sensor.LimiteMin, sensor.LimiteMax,
                    AtivoTipo = disp?.AtivoVinculadoTipo, AtivoId = disp?.AtivoVinculadoId
                });
                _context.OutboxMessages.Add(new OutboxMessage(tenantId,
                    CatalogoEventosIntegracao.Plataforma.IotLeituraForaFaixa, payload));
                // Condição operacional p/ Manutenção preditiva — NÃO é ordem definitiva (o dono decide).
                _context.OutboxMessages.Add(new OutboxMessage(tenantId,
                    CatalogoEventosIntegracao.Plataforma.IotCondicaoOperacionalDetectada, payload));
            }

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Leitura ingerida.", new { leitura.Id, ForaFaixa = foraFaixa });
        }
    }

    // ===================== Queries =====================

    public class ObterDispositivosIotQueryHandler : IQueryHandler<ObterDispositivosIotQuery, IReadOnlyList<DispositivoIotDto>>
    {
        private readonly ContextAplicativo _context;
        public ObterDispositivosIotQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<IReadOnlyList<DispositivoIotDto>> Handle(ObterDispositivosIotQuery request, CancellationToken ct)
        {
            var q = _context.DispositivosIot.AsNoTracking().AsQueryable();
            if (request.ApenasAtivos) q = q.Where(d => d.Ativo);
            return await q.OrderBy(d => d.Nome)
                .Select(d => new DispositivoIotDto(d.Id, d.Codigo, d.Nome, d.Tipo, d.Protocolo,
                    d.AtivoVinculadoTipo, d.AtivoVinculadoId, d.Ativo, d.UltimaLeituraEm)).ToListAsync(ct);
        }
    }

    public class ObterSensoresQueryHandler : IQueryHandler<ObterSensoresQuery, IReadOnlyList<SensorIotDto>>
    {
        private readonly ContextAplicativo _context;
        public ObterSensoresQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<IReadOnlyList<SensorIotDto>> Handle(ObterSensoresQuery request, CancellationToken ct)
            => await _context.SensoresIot.AsNoTracking().Where(s => s.DispositivoId == request.DispositivoId)
                .OrderBy(s => s.Codigo)
                .Select(s => new SensorIotDto(s.Id, s.DispositivoId, s.Codigo, s.Grandeza, s.Unidade,
                    s.LimiteMin, s.LimiteMax, s.RetencaoDias)).ToListAsync(ct);
    }

    public class ObterLeiturasQueryHandler : IQueryHandler<ObterLeiturasQuery, IReadOnlyList<LeituraSensorDto>>
    {
        private readonly ContextAplicativo _context;
        public ObterLeiturasQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<IReadOnlyList<LeituraSensorDto>> Handle(ObterLeiturasQuery request, CancellationToken ct)
        {
            var q = _context.LeiturasSensor.AsNoTracking().Where(l => l.SensorId == request.SensorId);
            if (request.Desde.HasValue) q = q.Where(l => l.MedidoEm >= request.Desde.Value);
            if (request.ApenasForaFaixa) q = q.Where(l => l.ForaFaixa);
            return await q.OrderBy(l => l.MedidoEm)
                .Select(l => new LeituraSensorDto(l.Id, l.SensorId, l.Valor, l.MedidoEm, l.ForaFaixa)).ToListAsync(ct);
        }
    }

    public class ObterLeiturasVencidasQueryHandler : IQueryHandler<ObterLeiturasVencidasQuery, IReadOnlyList<LeituraVencidaDto>>
    {
        private readonly ContextAplicativo _context;
        public ObterLeiturasVencidasQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<IReadOnlyList<LeituraVencidaDto>> Handle(ObterLeiturasVencidasQuery request, CancellationToken ct)
        {
            var agora = DateTime.UtcNow;
            var dados = await (from l in _context.LeiturasSensor.AsNoTracking()
                               join s in _context.SensoresIot.AsNoTracking() on l.SensorId equals s.Id
                               select new { l.Id, l.SensorId, l.MedidoEm, s.RetencaoDias }).ToListAsync(ct);
            return dados
                .Select(x => new { x.Id, x.SensorId, x.MedidoEm, x.RetencaoDias, VenceEm = x.MedidoEm.AddDays(x.RetencaoDias) })
                .Where(x => x.VenceEm < agora)
                .Select(x => new LeituraVencidaDto(x.Id, x.SensorId, x.MedidoEm, x.RetencaoDias, x.VenceEm))
                .ToList();
        }
    }
}
