using dii.storage.Exceptions;
using dii.storage.tests.Attributes;
using dii.storage.tests.Models;
using dii.storage.tests.Orderer;
using dii.storage.tests.Utilities;
using System;
using System.Collections.Generic;
using Xunit;

namespace dii.storage.tests.OptimizerTests
{
    [Collection(nameof(PackageToJsonTests))]
    [TestCollectionPriorityOrder(208)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class PackageToJsonTests
    {
        [Fact, TestPriorityOrder(100)]
        public void PackageToJson_Prep()
        {
            _ = Optimizer.Init(typeof(FakeEntityTwo));

            TestHelpers.AssertOptimizerIsInitialized();
        }

        [Fact, TestPriorityOrder(101)]
        public void PackageToJson_Success()
        {
            Optimizer optimizer;
            try
            {
                optimizer = Optimizer.Get();
            }
            catch
            {
                PackageToJson_Prep();
                optimizer = Optimizer.Get();
            }

            var fakeEntityTwo = new FakeEntityTwo
            {
                Id = Guid.Parse("FFFF0000-0000-0000-0000-000000000000").ToString(),
                FakeEntityTwoId = Guid.Parse("DDDD0000-0000-0000-0000-000000000000").ToString(),
                CompressedStringValue = $"fakeEntityTwo: {nameof(FakeEntityTwo.CompressedStringValue)}"
            };

            var entity = optimizer.PackageToJson(fakeEntityTwo);

            Assert.NotNull(entity);
            var expectedEntity = $"{{\"string\":null,\"long\":0,\"complex\":null,\"collection_complex\":null,\"_etag\":null,\"PK\":\"dddd0000-0000-0000-0000-000000000000\",\"id\":\"ffff0000-0000-0000-0000-000000000000\",\"p\":\"kdkkZmFrZUVudGl0eVR3bzogQ29tcHJlc3NlZFN0cmluZ1ZhbHVl\"}}";
            Assert.Equal(expectedEntity, entity);
        }

        [Fact, TestPriorityOrder(102)]
        public void PackageToJson_CollectionComplex_Success()
        {
            Optimizer optimizer;
            try
            {
                optimizer = Optimizer.Get();
            }
            catch (DiiNotInitializedException ex)
            {
                _ = Optimizer.Init(typeof(FakeEntityTwo));
                TestHelpers.AssertOptimizerIsInitialized();
                optimizer = Optimizer.Get();
            }

            var fakeEntityTwo = new FakeEntityTwo
            {
                Id = Guid.NewGuid().ToString(),
                FakeEntityTwoId = Guid.NewGuid().ToString(),
                CollectionComplexSearchable = new List<FakeSearchableEntity>
                {
                    new FakeSearchableEntity
                    {
                        SearchableStringValue = "Searchable",
                        CompressedStringValue = "Compressed",
                        ComplexSearchable = new FakeSearchableEntityTwo
                        {
                            SearchableStringValue = "Searchable",
                            CompressedStringValue = "Compressed"
                        }
                    }
                }
            };

            var entity = optimizer.PackageToJson(fakeEntityTwo);

            Assert.NotNull(entity);
        }

        [Fact, TestPriorityOrder(102)]
        public void PackageToJson_UnregisteredType()
        {
            var optimizer = Optimizer.Get();

            var unregisteredEntity = new InvalidSearchableKeyEntity
            {
                InvalidSearchableKeyEntityId = Guid.NewGuid().ToString(),
                InvalidSearchableKeyStringPValue = $"fakeInvalidEntity: {nameof(InvalidSearchableKeyEntity.InvalidSearchableKeyStringPValue)}"
            };

            var entity = optimizer.PackageToJson(unregisteredEntity);

            Assert.Null(entity);
        }

        [Fact, TestPriorityOrder(103)]
        public void PackageToJson_Null()
        {
            var optimizer = Optimizer.Get();

            var exception = Assert.Throws<ArgumentNullException>(() => { optimizer.PackageToJson<FakeEntityTwo>(null); });
            Assert.NotNull(exception);
            Assert.Equal("Value cannot be null. (Parameter 'obj')", exception.Message);
        }

        #region Teardown
        [Fact, TestPriorityOrder(int.MaxValue)]
        public void Teardown()
        {
            TestHelpers.ResetOptimizerInstance();
        }
        #endregion
    }
}