using dii.storage.Attributes;
using dii.storage.tests.Attributes;
using dii.storage.tests.AttributeTests.Data;
using dii.storage.tests.Orderer;
using System;
using Xunit;

namespace dii.storage.tests.AttributeTests
{
    [Collection(nameof(EnableTimeToLiveAttributeTests))]
    [TestCollectionPriorityOrder(305)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class EnableTimeToLiveAttributeTests
    {
        [Theory, TestPriorityOrder(100), ClassData(typeof(EnableTimeToLiveData))]
        public void EnableTimeToLiveAttributeTests_Success(int timeToLiveInSeconds)
        {
            var enableTimeToLiveAttribute = new EnableTimeToLiveAttribute(timeToLiveInSeconds);

            Assert.NotNull(enableTimeToLiveAttribute);
            Assert.Equal(timeToLiveInSeconds, enableTimeToLiveAttribute.TimeToLiveInSeconds);
        }

        [Theory, TestPriorityOrder(101), ClassData(typeof(EnableTimeToLiveExceptionData))]
        public void EnableTimeToLiveAttributeTests_Exception(int timeToLiveInSeconds)
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => { new EnableTimeToLiveAttribute(timeToLiveInSeconds); });

            Assert.NotNull(exception);
            Assert.Equal("The time to live in seconds must be either a nonzero positive integer or '-1'. (Parameter 'timeToLiveInSeconds')", exception.Message);
        }
    }
}