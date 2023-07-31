﻿using System;

namespace dii.storage.Models.Interfaces
{
	/// <summary>
	/// The contract to ensure clean interaction with <see cref="Optimizer"/>.
	/// </summary>
    public interface IDiiEntity
	{
		/// <summary>
		/// Each version of this object can be stored and fetched in storage.
		/// This version is NOT intended to be overriden in sub-types. Inherited classes can create
		/// their own version for its needs.
		/// <para>
		/// Major changes involving property removal or desire to shuffle the order of compressed properties
		/// will require a major version increment and this version archived for reading past documents.
		/// </para>
		/// <para>
		/// Minor changes involving property addition can version the minor version and reading these values
		/// will not be impacted.
		/// </para>
		/// </summary>
		/// <remarks>
		/// This is a placeholder for a future feature.
		/// </remarks>
		Version SchemaVersion { get; }

		/// <summary>
		/// The computed version of the stored object. Necessary for optimistic concurrency.
		/// </summary>
		/// <remarks>
		/// This value should be generated by the storage engine automatically and should not be manually populated.
		/// </remarks>
		string DataVersion { get; set; }
    }
}