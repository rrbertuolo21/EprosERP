using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Commands;
using MediatR;
using Quartz;

namespace Epros.Modules.GestaoClientes.Infrastructure.Jobs
{
    [DisallowConcurrentExecution]
    public class SincronizarGeografiaJob : IJob
    {
        private readonly IMediator _mediator;

        public SincronizarGeografiaJob(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _mediator.Send(new SincronizarGeografiaCommand());
        }
    }
}
