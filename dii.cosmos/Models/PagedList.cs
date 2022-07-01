using System.Collections.Generic;

namespace dii.cosmos.Models
{
    public class PagedList<T> : List<T>
	{
		public string ContinuationToken { get; set; }
		public bool IsLastPage { get => string.IsNullOrEmpty(ContinuationToken); }
		public double RequestCharge { get; set; }
	}
}