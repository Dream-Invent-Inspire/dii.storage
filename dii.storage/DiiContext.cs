﻿using dii.storage.Models;
using dii.storage.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace dii.storage
{
    /// <summary>
    /// An contract intended to represent the necessary components of a DiiContext.
    /// <para>
    /// This is intended to be used with the singleton pattern.
    /// </para>
    /// </summary>
    public abstract class DiiContext
	{
		#region Public Properties
		/// <summary>
		/// The <see cref="INoSqlDatabaseConfig"/> configuration to be used by this context.
		/// </summary>
		public INoSqlContextConfig Config { get; protected set; }

		/// <summary>
		/// The max RU/sec.
		/// </summary>
		public int? DbThroughput { get; protected set; }

		/// <summary>
		/// Indicates that this instance of the <see cref="DiiContext"/> was responsible
		/// for the creation of the database.
		/// </summary>
		public bool DatabaseCreatedThisContext { get; protected set; }

		/// <summary>
		/// Represents the initialized table mappings within this instance of the <see cref="DiiContext"/>.
		/// </summary>
		public Dictionary<Type, TableMetaData> TableMappings { get; protected set; }
		#endregion Public Properties

		#region Public Methods
		/// <summary>
		/// Checks if the database exists and creates if it does not.
		/// </summary>
		/// <returns>Should always return true or throw an exception.</returns>
		public abstract Task<bool> DoesDatabaseExistAsync();

		/// <summary>
		/// Initializes the tables using the provided <see cref="TableMetaData"/>.
		/// </summary>
		/// <param name="tableMetaDatas">The <see cref="TableMetaData"/> generated by the <see cref="Optimizer"/>.</param>
		/// <returns>A task for async completion.</returns>
		public abstract Task InitTablesAsync(string dbid, ICollection<TableMetaData> tableMetaDatas, bool autoCreate, Optimizer optimizer = null);
		#endregion Public Methods
	}
}