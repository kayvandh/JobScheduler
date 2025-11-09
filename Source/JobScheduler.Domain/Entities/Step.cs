using JobScheduler.Domain.Common;
using JobScheduler.Domain.Types;
using JobScheduler.Domain.ValueObjects;

namespace JobScheduler.Domain.Entities
{
    public class Step : AuditableEntity
    {
        public Guid Id { get; private set; }
        public Guid JobId { get; set; }
        public int Order { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ApiEndpoint { get; set; } = string.Empty;
        public HttpMethodType HttpMethod { get; set; }
        public AuthenticationType AuthenticationType { get; set; }
        public AuthenticationInfo? AuthenticationInfo { get; set; }
        public RetryPolicy? RetryPolicy { get; set; }
        public int TimeoutInSeconds { get; set; }
        public List<StepParameter> Headers { get; set; } = [];
        public List<StepParameter> QueryParameters { get; set; } = [];
        public List<StepParameter> BodyParameters { get; set; } = [];
        public List<ConditionalFlow> ConditionalFlows { get; set; } = [];
        public StepStatus Status { get; set; }
        public string? Output { get; set; }

        public Step()
        {
            Id = Guid.NewGuid();
        }

        #region Behavior helpers

        public void AddHeader(StepParameter parameter)
        {
            ArgumentNullException.ThrowIfNull(parameter);
            Headers.Add(parameter);
        }

        public void AddQueryParameter(StepParameter parameter)
        {
            ArgumentNullException.ThrowIfNull(parameter);
            QueryParameters.Add(parameter);
        }

        public void AddBodyParameter(StepParameter parameter)
        {
            ArgumentNullException.ThrowIfNull(parameter);
            BodyParameters.Add(parameter);
        }

        public void AddConditionalFlow(ConditionalFlow conditionalFlow)
        {
            ArgumentNullException.ThrowIfNull(conditionalFlow);
            ConditionalFlows.Add(conditionalFlow);
        }

        public void SetOutput(string output)
        {
            Output = output;
            Status = StepStatus.Completed;
        }

        public void MarkRunning()
        {
            Status = StepStatus.Running;
        }

        public void MarkFailed(string? error = null)
        {
            Status = StepStatus.Failed;
            Output = error;
        }

        #endregion Behavior helpers
    }
}