using dii.storage.Attributes;
using dii.storage.Models.Interfaces;
using System;
using System.Reflection;

namespace dii.storage.tests.Models
{
    /// <inheritdoc/>
    public abstract class FakeDiiTimeToLiveEntity : FakeDiiEntity, IDiiEntity, IDiiTimeToLiveEntity
    {
        /// <summary>
        /// Initalizes an instance of <see cref="DiiTimeToLiveCosmosEntity" />.
        /// </summary>
        public FakeDiiTimeToLiveEntity()
        {
            // Declaring the ttl property without having a valid value (-1, null or a positive integer) causes 
            // Cosmos DB to throw an exception. Lets try to fetch the value from the derived class.
            var timeToLiveInSeconds = this.GetType()?.GetCustomAttribute<EnableTimeToLiveAttribute>()?.TimeToLiveInSeconds;
            if (timeToLiveInSeconds.HasValue && (timeToLiveInSeconds == -1 || timeToLiveInSeconds > 0))
            {
                TimeToLiveInSeconds = timeToLiveInSeconds.Value;
            }
        }

        /// <inheritdoc/>
		[Searchable("_ts")]
        public long LastUpdated { get; set; }

        /// <inheritdoc/>
        [Searchable("ttl")]
        public int TimeToLiveInSeconds { get; set; }

        /// <inheritdoc/>
        public DateTimeOffset? TimeToLiveDecayDateTime
        {
            get
            {
                if (LastUpdated > 0)
                {
                    return DateTimeOffset.FromUnixTimeSeconds(LastUpdated);
                }

                return null;
            }
        }
    }
}