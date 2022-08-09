using MessagePack;
using System;
using System.Collections.Generic;
using static dii.storage.cosmos.tests.Models.Enums;

namespace dii.storage.cosmos.tests.Models
{
    [MessagePackObject()]
	public class FakeMessagePackEntity
	{
		/// <summary>
		/// An <see cref="int"/> value to be packed.
		/// </summary>
		[Key(0)]
		public int PackedIntegerValue { get; set; }

		/// <summary>
		/// A <see cref="decimal"/> value to be packed.
		/// </summary>
		[Key(1)]
		public decimal PackedDecimalValue { get; set; }

		/// <summary>
		/// A <see cref="string"/> value to be packed.
		/// </summary>
		[Key(2)]
		public string PackedStringValue { get; set; }

		/// <summary>
		/// A <see cref="Guid"/> value to be packed.
		/// </summary>
		[Key(3)]
		public Guid PackedGuidValue { get; set; }

		/// <summary>
		/// A <see cref="List{T}"/> of <see cref="string"/> values to be packed.
		/// </summary>
		[Key(4)]
		public List<string> PackedListValue { get; set; }

		/// <summary>
		/// A <see cref="DateTime"/> value to be packed.
		/// </summary>
		[Key(5)]
		public DateTime PackedDateTimeValue { get; set; }

		/// <summary>
		/// A <see cref="enum"/> value to be packed.
		/// </summary>
		[Key(6)]
		public FakeEnum PackedEnumValue { get; set; }
	}
}