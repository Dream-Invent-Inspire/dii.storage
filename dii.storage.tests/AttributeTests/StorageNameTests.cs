using dii.storage.Attributes;
using dii.storage.tests.Attributes;
using dii.storage.tests.Orderer;
using Xunit;

namespace dii.storage.tests.AttributeTests
{
    [Collection(nameof(StorageNameTests))]
    [TestCollectionPriorityOrder(304)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class StorageNameTests
    {
        [Fact, TestPriorityOrder(100)]
        public void StorageNameTests_Success()
        {
            var storageNameAttribute = new StorageNameAttribute("Test-Entity-Name");

            Assert.NotNull(storageNameAttribute);
            Assert.Equal("Test-Entity-Name", storageNameAttribute.Name);
        }
    }
}