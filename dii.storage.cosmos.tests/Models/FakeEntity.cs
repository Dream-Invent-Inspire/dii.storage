﻿using dii.storage.Attributes;
using dii.storage.cosmos.Models;
using dii.storage.Models.Interfaces;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using static dii.storage.cosmos.tests.Models.Enums;

namespace dii.storage.cosmos.tests.Models
{
    [StorageName("Test-FakeEntity")]
    [EnableTimeToLive(_defaultTimeToLiveInSeconds)]
    public class FakeEntity : DiiCosmosEntity, IDiiTimeToLiveEntity
    {
        internal const int _defaultTimeToLiveInSeconds = 3600;

        /// <summary>
        /// Initalizes an instance of <see cref="FakeEntity" />.
        /// </summary>
        public FakeEntity()
        {
            TimeToLiveInSeconds = _defaultTimeToLiveInSeconds;
        }

        /// <summary>
        /// The Unique Id for the <see cref="FakeEntity"/>.
        /// </summary>
        [PartitionKey(typeof(PartitionKey))]
		public string FakeEntityId { get; set; }

		[Id()]
		public string Id { get { return FakeEntityId; } set { FakeEntityId = value; } }

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

        /// <summary>
        /// A <see cref="string"/> Cosmos primitive to be searched.
        /// </summary>
        [Searchable("_self")]
		public string SearchableCosmosPrimitive { get; set; }

		/// <summary>
		/// An <see cref="int"/> value to be searched.
		/// </summary>
		[Searchable("int")]
		public int SearchableIntegerValue { get; set; }

		/// <summary>
		/// A <see cref="decimal"/> value to be searched.
		/// </summary>
		[Searchable("decimal")]
		public decimal SearchableDecimalValue { get; set; }

		/// <summary>
		/// A <see cref="string"/> value to be searched.
		/// </summary>
		[Searchable("string")]
		public string SearchableStringValue { get; set; }

		/// <summary>
		/// A <see cref="Guid"/> value to be searched.
		/// </summary>
		[Searchable("guid")]
		public Guid SearchableGuidValue { get; set; }

		/// <summary>
		/// A <see cref="List{T}"/> of <see cref="string"/> values to be searched.
		/// </summary>
		[Searchable("list")]
		public List<string> SearchableListValue { get; set; }

		/// <summary>
		/// A <see cref="DateTime"/> value to be searched.
		/// </summary>
		[Searchable("datetime")]
		public DateTime SearchableDateTimeValue { get; set; }

		/// <summary>
		/// A <see cref="enum"/> value to be searched.
		/// </summary>
		[Searchable("enum")]
		public FakeEnum SearchableEnumValue { get; set; }

		/// <summary>
		/// An object to be packed and compressed.
		/// </summary>
		[Compress(0)]
		public FakeMessagePackEntity CompressedPackedEntity { get; set; }

		/// <summary>
		/// An <see cref="int"/> value to be compressed.
		/// </summary>
		[Compress(1)]
		public int CompressedIntegerValue { get; set; }

		/// <summary>
		/// A <see cref="decimal"/> value to be compressed.
		/// </summary>
		[Compress(2)]
		public decimal CompressedDecimalValue { get; set; }

		/// <summary>
		/// A <see cref="string"/> value to be compressed.
		/// </summary>
		[Compress(3)]
		public string CompressedStringValue { get; set; }

		/// <summary>
		/// A <see cref="Guid"/> value to be compressed.
		/// </summary>
		[Compress(4)]
		public Guid CompressedGuidValue { get; set; }

		/// <summary>
		/// A <see cref="List{T}"/> of <see cref="string"/> values to be compressed.
		/// </summary>
		[Compress(5)]
		public List<string> CompressedListValue { get; set; }

		/// <summary>
		/// A <see cref="DateTime"/> value to be compressed.
		/// </summary>
		[Compress(6)]
		public DateTime CompressedDateTimeValue { get; set; }

		/// <summary>
		/// A <see cref="enum"/> value to be compressed.
		/// </summary>
		[Compress(7)]
		public FakeEnum CompressedEnumValue { get; set; }

		/// <summary>
		/// A value to test complex recursive nesting.
		/// </summary>
		[Searchable("complex")]
		public FakeSearchableEntity ComplexSearchable { get; set; }
	}
}