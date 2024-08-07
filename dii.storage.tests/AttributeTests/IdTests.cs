﻿using dii.storage.tests.Attributes;
using dii.storage.tests.AttributeTests.Data;
using dii.storage.tests.Models;
using dii.storage.tests.Orderer;
using dii.storage.tests.Utilities;
using Xunit;

namespace dii.storage.tests.AttributeTests
{
    [Collection(nameof(IdTests))]
    [TestCollectionPriorityOrder(301)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class IdTests
    {
        public IdTests()
        {
            _ = Optimizer.Init("Db1", typeof(MultipleIdEntity), typeof(FirstIdSeparatorWinsEntity));

            TestHelpers.AssertOptimizerIsInitialized();
        }

        [Theory, TestPriorityOrder(100), ClassData(typeof(MultipleIdEntityData))]
        public void Id_Success(MultipleIdEntity multipleIdEntity, string expected)
        {
            var optimizer = Optimizer.Get();

            var entity = (dynamic)optimizer.ToEntity(multipleIdEntity);

            Assert.NotNull(entity);

            var id = entity.id as string;

            Assert.Equal(expected, id);
        }

        [Theory, TestPriorityOrder(101), ClassData(typeof(FirstIdSeparatorWinsEntityData))]
        public void Id_FirstSeparatorWins(FirstIdSeparatorWinsEntity firstIdSeparatorWinsEntity, string expected)
        {
            var optimizer = Optimizer.Get();

            var entity = (dynamic)optimizer.ToEntity(firstIdSeparatorWinsEntity);

            Assert.NotNull(entity);

            var id = entity.id as string;

            Assert.Equal(expected, id);
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