using System;

namespace dii.storage.Attributes
{
    /// <summary>
    /// Denotes that a class or struct should be stored in the data store with
	/// the time to live configuration provided.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class EnableTimeToLiveAttribute : Attribute
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="EnableTimeToLiveAttribute"/> class with the
        /// time to live configurations with which the instance should be initalized.
        /// </summary>
        /// <param name="timeToLiveInSeconds">Used to provide the time to live for like entities.</param>
        /// <remarks>
        /// The unit of measurement is seconds. The maximum allowed value is 2147483647. A valid value must be either a nonzero positive integer or '-1'.
        /// By default, TimeToLiveInSeconds is set to -1 meaning all the items never expire for the container.
        /// <para>
        /// Each item within the container can have their individual time to live in seconds which overrides this setting.
        /// </para>
        /// The time to live in seconds is how many seconds after the last updated timestamp (_ts) has passed.
        /// </remarks>
        public EnableTimeToLiveAttribute(int timeToLiveInSeconds = -1)
		{
            if (timeToLiveInSeconds < -1 || timeToLiveInSeconds == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeToLiveInSeconds), "The time to live in seconds must be either a nonzero positive integer or '-1'.");
            }

            TimeToLiveInSeconds = timeToLiveInSeconds;
		}

        /// <summary>
        /// Gets the default time to live in seconds for item in a container from the Azure Cosmos service.
        /// </summary>
        /// <remarks>
        /// The unit of measurement is seconds. The maximum allowed value is 2147483647. A valid value must be either a nonzero positive integer or '-1'.
        /// By default, TimeToLiveInSeconds is set to -1 meaning all the items never expire for the container.
        /// <para>
        /// Each item within the container can have their individual time to live in seconds which overrides this setting.
        /// </para>
        /// </remarks>
        public int TimeToLiveInSeconds { get; init; }
	}
}