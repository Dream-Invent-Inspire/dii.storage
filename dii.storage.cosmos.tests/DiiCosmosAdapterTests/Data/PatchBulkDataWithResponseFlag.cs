using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using Xunit;

namespace dii.storage.cosmos.tests.DiiCosmosAdapterTests.Data
{
    public class PatchBulkDataWithResponseFlag : TheoryData<List<(List<PatchOperation>, string)>, bool>
    {
        public PatchBulkDataWithResponseFlag()
        {
            var newValue100 = "fakeEntity1: UPDATED";
            var newValue101 = "fakeEntity2: UPDATED";
            var newValue102 = "fakeEntity3: UPDATED";

            var patchOperations100 = new List<(List<PatchOperation>, string)>()
            {
                (new List<PatchOperation> { PatchOperation.Replace("/string", newValue100) }, newValue100),
                (new List<PatchOperation> { PatchOperation.Replace("/string", newValue101) }, newValue101),
                (new List<PatchOperation> { PatchOperation.Replace("/string", newValue102) }, newValue102)
            };

            Add(patchOperations100, true);

            var newValue200 = "fakeEntity1: UPDATED AGAIN";
            var newValue201 = "fakeEntity2: UPDATED AGAIN";
            var newValue202 = "fakeEntity3: UPDATED AGAIN";

            var patchOperations200 = new List<(List<PatchOperation>, string)>()
            {
                (new List<PatchOperation> { PatchOperation.Replace("/string", newValue200) }, newValue200),
                (new List<PatchOperation> { PatchOperation.Replace("/string", newValue201) }, newValue201),
                (new List<PatchOperation> { PatchOperation.Replace("/string", newValue202) }, newValue202)
            };

            Add(patchOperations200, false);
        }
    }
}