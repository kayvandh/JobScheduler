using Framework.FluentResultsAddOn;

namespace FluentResults
{
    public class FluentError : IError
    {
        public FluentError(ApplicationErrorCode errorCode, string errorMessage, Dictionary<string, object>? metadata = null)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            Metadata = metadata;
        }

        public ApplicationErrorCode ErrorCode { get; }
        public string ErrorMessage { get; }
        public string Message => $"{ErrorCode}: {ErrorMessage}";
        public List<IError> Reasons => new();
        public Dictionary<string, object>? Metadata { get; private set; }

        public void WithMetaData(Dictionary<string, object> metadata)
        {
            Metadata = metadata;
        }

        public static FluentError Raise(ApplicationErrorCode errorCode, string errorMessage, Dictionary<string, object>? metadata = null)
        {
            return new FluentError(errorCode, errorMessage, metadata);
        }
    }
}