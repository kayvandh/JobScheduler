using Framework.Domain;

namespace JobScheduler.Domain.ValueObjects
{
    /// <summary>
    /// Represents a key-value parameter used in Step definitions (Header, QueryString, Body).
    /// </summary>
    public class StepParameter : ValueObject
    {
        public string Key { get; private set; } = string.Empty;
        public string? Value { get; private set; }

        /// <summary>
        /// True if this parameter should get its value dynamically from a previous step's output.
        /// </summary>
        public bool IsDynamic { get; private set; }

        /// <summary>
        /// The source path or expression used to extract the dynamic value from a previous step output.
        /// For example: "Step1.Token" or "PreviousStep.Data.Token".
        /// Null if the parameter is static.
        /// </summary>
        public string? Source { get; private set; }

        protected StepParameter()
        {
        }

        public StepParameter(string key, string? value = null, bool isDynamic = false, string? source = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Parameter key cannot be null or empty.", nameof(key));

            Key = key;
            Value = value;
            IsDynamic = isDynamic;

            if (isDynamic && string.IsNullOrWhiteSpace(source))
                throw new ArgumentException("Dynamic parameters must define a source path.", nameof(source));

            Source = source;
        }

        /// <summary>
        /// Updates the static value of the parameter.
        /// </summary>
        public void UpdateValue(string? newValue)
        {
            Value = newValue;
        }

        /// <summary>
        /// Marks the parameter as dynamic and assigns its source path.
        /// </summary>
        public void SetDynamic(string sourcePath)
        {
            IsDynamic = true;
            Source = sourcePath ?? throw new ArgumentNullException(nameof(sourcePath));
        }

        /// <summary>
        /// Marks the parameter as static.
        /// </summary>
        public void SetStaticValue(string value)
        {
            IsDynamic = false;
            Source = null;
            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Key;
            yield return Value ?? string.Empty;
            yield return IsDynamic;
            yield return Source ?? string.Empty;
        }
    }
}