using dii.storage.Models;
using System.Collections.Generic;
using Xunit;

namespace dii.storage.cosmos.tests.DiiCosmosContextTests.Data
{
    public class ContextEmptyInitData : TheoryData<List<TableMetaData>>
    {
        public ContextEmptyInitData()
        {
			Add(null);
            Add(new List<TableMetaData>());
            Add(new List<TableMetaData> { null });
        }
    }
}