using dii.storage.Attributes;
using System;

namespace dii.storage.tests.Models
{
    public class InvalidSelfReferenceEntity : FakeDiiEntity
	{
		/// <summary>
		/// The Unique Id for the <see cref="InvalidSelfReferenceEntity"/>.
		/// </summary>
		[PartitionKey(typeof(Guid))]
		public string InvalidSelfReferenceEntityId { get; set; }

		[Id()]
		public string Id { get; set; }

		/// <summary>
		/// A <see cref="string"/> value to be searched.
		/// </summary>
		[Searchable("string")]
		public string SearchableStringValue { get; set; }

		/// <summary>
		/// A <see cref="string"/> value to be compressed.
		/// </summary>
		[Compress(0)]
		public string CompressedStringValue { get; set; }

        /// <summary>
        /// A test object for nesting of self to be searchable.
        /// </summary>
        [Searchable("selfreference")]
        public InvalidSelfReferenceEntity NestedSameEntity { get; set; }
    }
}