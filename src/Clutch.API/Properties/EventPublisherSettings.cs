namespace Clutch.API.Properties
{
    public class EventPublisherSettings
    {
        /// <summary>
        /// Retrieves or sets the number of times to retry an operation.
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// Retrieves or sets the interval between operation retries.
        /// </summary>
        public int RetryInterval { get; set; }

        /// <summary>
        /// Set to true to use exponential backoff for operation retries.
        /// </summary>
        public bool UseExponentialBackoff { get; set; }
    }
}
