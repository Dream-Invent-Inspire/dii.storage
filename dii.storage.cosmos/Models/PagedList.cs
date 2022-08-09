using System.Collections.Generic;

namespace dii.storage.cosmos.Models
{
	/// <summary>
	/// <inheritdoc cref="List{T}" path="//summary" />
	/// Also provides paging properties
	/// </summary>
	/// <typeparam name="T"><inheritdoc cref="List{T}" path="//typeparam" /></typeparam>
	public class PagedList<T> : List<T>
	{
		/// <summary>
		/// The continuation token used to fetch the next set of results.
		/// </summary>
		public string ContinuationToken { get; set; }

		/// <summary>
		/// Indicates whether or not this data is the last page of the results.
		/// </summary>
		public bool IsLastPage { get => string.IsNullOrEmpty(ContinuationToken); }

		/// <summary>
		/// Cost of operation in RU/sec.
		/// </summary>
		public double RequestCharge { get; set; }
	}
}