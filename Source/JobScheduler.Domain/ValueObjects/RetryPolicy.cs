namespace JobScheduler.Domain.ValueObjects
{
    public class RetryPolicy
    {
        public int MaxRetries { get; private set; }
        public int DelayBetweenRetriesSeconds { get; private set; }

        protected RetryPolicy()
        {
        }

        public RetryPolicy(int maxRetries, int delayBetweenRetriesSeconds)
        {
            MaxRetries = maxRetries;
            DelayBetweenRetriesSeconds = delayBetweenRetriesSeconds;
        }
    }
}