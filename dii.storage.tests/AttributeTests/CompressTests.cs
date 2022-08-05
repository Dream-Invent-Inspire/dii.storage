using dii.storage.Attributes;
using dii.storage.Exceptions;
using dii.storage.tests.Attributes;
using dii.storage.tests.AttributeTests.Data;
using dii.storage.tests.Orderer;
using Xunit;

namespace dii.storage.tests.AttributeTests
{
    [Collection(nameof(CompressTests))]
    [TestCollectionPriorityOrder(303)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class CompressTests
    {
        //[Fact, TestPriorityOrder(100)]
        //public void CompressTests_Prep()
        //{
        //}

        [Theory, TestPriorityOrder(101), ClassData(typeof(CompressData))]
        public void CompressTests_Success(int order)
        {
            var compressAttribute = new CompressAttribute(order);

            Assert.NotNull(compressAttribute);
            Assert.Equal(order, compressAttribute.Order);
        }

        [Theory, TestPriorityOrder(102), ClassData(typeof(CompressExceptionData))]
        public void CompressTests_Exception(int order)
        {
            // Normally the property name would be derived from actual Attribute usage.
            var propertyName = "PropertyA";
            var exception = Assert.Throws<DiiNegativeCompressOrderException>(() => { new CompressAttribute(order, propertyName); });

            Assert.NotNull(exception);
            Assert.Equal($"The [Compress(order)] order on {propertyName} cannot be negative.", exception.Message);
        }

        #region Teardown
        //[Fact, TestPriorityOrder(int.MaxValue)]
        //public void Teardown()
        //{
        //}
        #endregion
    }
}