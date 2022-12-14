namespace dii.storage.Models
{
    /// <summary>
    /// The configuration necessary to create and connect to a NoSQL database with read/write access.
    /// </summary>
    public class DatabaseConfig
    {
        /// <summary>
		/// The key for the read/write Connection.
		/// </summary>
		public string Key { get; set; }

        /// <summary>
        /// The id(s) of the database(s) to provide read/write access to.
        /// </summary>
        public string[] DatabaseIds { get; set; }

        /// <summary>
        /// Auto-creates the database and/or containers if they do not exist.
        /// </summary>
        /// <remarks>
        /// When set to <see langword="false" />, the <see cref="MaxRUPerSecond" />,
        /// <see cref="AutoAdjustMaxRUPerSecond" />, and <see cref="AutoScaling" /> properties
        /// are ignored.
        /// </remarks>
        public bool AutoCreate { get; set; }

        /// <summary>
        /// The max RU/sec.
        /// </summary>
        public int MaxRUPerSecond { get; set; }

        /// <summary>
        /// When true, the <see cref="DiiContext"/> should attempt to change
        /// the existing database RU/sec throughput provisioning if it is different
        /// than what is set in <see cref="MaxRUPerSecond"/>.
        /// </summary>
        public bool AutoAdjustMaxRUPerSecond { get; set; }

        /// <summary>
        /// When true, RU throughput provisioning will be set to autoscale. 
        /// </summary>
        /// <remarks>
        /// This must be modified via Azure Portal once the database is created. Attempting to 
        /// toggle autoscale once the database exists via dii.storage will cause
        /// an exception to be thrown.
        /// </remarks>
        public bool AutoScaling { get; set; }
    }
}