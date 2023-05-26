using dii.storage.Attributes;
using dii.storage.Exceptions;
using dii.storage.tests.Attributes;
using dii.storage.tests.AttributeTests.Data;
using dii.storage.tests.Orderer;
using Xunit;

namespace dii.storage.tests.AttributeTests
{
    [Collection(nameof(SearchableTests))]
    [TestCollectionPriorityOrder(302)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class SearchableTests
    {
        [Theory, TestPriorityOrder(100), ClassData(typeof(SearchableData))]
        public void SearchableTests_Success(string abbreviation)
        {
            var searchableAttribute = new SearchableAttribute(abbreviation);

            Assert.NotNull(searchableAttribute);
            Assert.Equal(abbreviation, searchableAttribute.Abbreviation);
        }

        [Theory, TestPriorityOrder(101), ClassData(typeof(SearchableExceptionData))]
        public void SearchableTests_Exception(string abbreviation)
        {
            // Normally the property name would be derived from actual Attribute usage.
            var propertyName = "PropertyA";
            var exception = Assert.Throws<DiiNullSearchableKeyException>(() => { new SearchableAttribute(abbreviation, propertyName); });

            Assert.NotNull(exception);
            Assert.Equal($"The [Searchable(key)] key on {propertyName} cannot be null, empty or whitespace.", exception.Message);
        }
    }
}