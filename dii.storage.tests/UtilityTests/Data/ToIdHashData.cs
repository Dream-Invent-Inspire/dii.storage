using System;
using Xunit;

namespace dii.storage.tests.UtilityTests.Data
{
    public class ToIdHashData : TheoryData<object>
    {
        public ToIdHashData()
        {
            Add("2021-10-26 03:21:07");
            Add(new DateTime(2021, 10, 26, 03, 21, 07));
            Add(new DateTimeOffset(2021, 10, 26, 03, 21, 07, TimeSpan.Zero));
            Add(637708152670000000L);
            Add(637708152670000000UL);
        }
    }
}