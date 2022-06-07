namespace dii.cosmos.Models.Interfaces
{
    public interface INoSqlDatabaseConfig
	{
		/// <summary>
		/// The Uri of the database.
		/// </summary>
		string Uri { get; set; }

		/// <summary>
		/// The Key for the Uri Connection.
		/// </summary>
		string Key { get; set; }

		/// <summary>
		/// The Id of the database.
		/// </summary>
		string DatabaseId { get; set; }

		/// <summary>
		/// Auto-create the database if it does not exist.
		/// </summary>
		bool AutoCreate { get; set; }

		/// <summary>
		/// The configuration set Max RU/sec.
		/// </summary>
		int MaxRUPerSecond { get; set; }

		/// <summary>
		/// If the Database Exists and the Max RU/sec is different
		/// alters the Max RU/sec to the configured value.
		/// </summary>
		bool AutoAdjustMaxRUPerSecond { get; set; }

		/// <summary>
		/// Allow autoscaling.
		/// </summary>
		bool AutoScaling { get; set; }
	}
}