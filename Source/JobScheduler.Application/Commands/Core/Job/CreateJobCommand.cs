using FluentResults;
using Framework.Mapper.Interfaces;
using MediatR;
using JobScheduler.Domain.Types;

namespace JobScheduler.Application.Commands.Core.Job
{
    public record CreateJobCommand(string Name, string Description, string CronSchedule, bool IsOneTime, JobStatus Status) : IRequest<Result<Guid>>, IOneWayMap<Domain.Entities.Job>;
}