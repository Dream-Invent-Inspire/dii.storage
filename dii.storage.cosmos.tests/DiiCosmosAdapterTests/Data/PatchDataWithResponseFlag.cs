using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using Xunit;

namespace dii.storage.cosmos.tests.DiiCosmosAdapterTests.Data
{
    public class PatchDataWithResponseFlag : TheoryData<List<(PatchOperation, string)>, bool>
    {
        public PatchDataWithResponseFlag()
        {
            var newValue1 = "fakeEntity: UPDATED";

            var patchOperations1 = new List<(PatchOperation, string)>()
            {
                (PatchOperation.Replace("/string", newValue1), newValue1)
            };

            Add(patchOperations1, true);

            var newValue2 = "fakeEntity: UPDATED AGAIN";

            var patchOperations2 = new List<(PatchOperation, string)>()
            {
                (PatchOperation.Replace("/string", newValue2), newValue2)
            };

            Add(patchOperations2, false);
        }
    }
}