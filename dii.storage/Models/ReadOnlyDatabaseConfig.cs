namespace dii.storage.Models
{
    /// <summary>
    /// The configuration necessary to create and connect to a NoSQL database with read-only access.
    /// </summary>
    public class ReadOnlyDatabaseConfig
    {
        /// <summary>
		/// The key for the read-only Connection.
		/// </summary>
		public string Key { get; set; }

        /// <summary>
        /// The id(s) of the database(s) to provide read-only access to.
        /// </summary>
        public string[] DatabaseIds { get; set; }
    }
}