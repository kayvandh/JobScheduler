using AutoMapper;
using FluentResults;
using Framework.FluentResultsAddOn;
using MediatR;
using JobScheduler.Application.Common.Interfaces.Persistence;

namespace JobScheduler.Application.Commands.Core.Job
{
    public class CreateJobCommandHandler : IRequestHandler<CreateJobCommand, Result<Guid>>
    {
        private readonly IJobSchedulerUnitOfWork JobSchedulerUnitOfWork;
        private readonly IMapper mapper;

        public CreateJobCommandHandler(IJobSchedulerUnitOfWork JobSchedulerUnitOfWork, IMapper mapper)
        {
            this.JobSchedulerUnitOfWork = JobSchedulerUnitOfWork;
            this.mapper = mapper;
        }

        public async Task<Result<Guid>> Handle(CreateJobCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var job = mapper.Map<Domain.Entities.Job>(request);
                await JobSchedulerUnitOfWork.Jops.AddAsync(job);
                await JobSchedulerUnitOfWork.SaveChangesAsync(cancellationToken);
                return Result.Ok(job.Id);
            }
            catch (Exception ex)
            {
                return Result.Fail(FluentError.Raise(ApplicationErrorCode.GeneralError, ex.Message));
            }
        }
    }
}