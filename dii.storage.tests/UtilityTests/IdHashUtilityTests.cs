using dii.storage.tests.Attributes;
using dii.storage.tests.Orderer;
using dii.storage.tests.UtilityTests.Data;
using dii.storage.Utilities;
using System;
using Xunit;

namespace dii.storage.tests.UtilityTests
{
    [Collection(nameof(IdHashUtilityTests))]
    [TestCollectionPriorityOrder(100)]
    [TestCaseOrderer(TestPriorityOrderer.FullName, TestPriorityOrderer.AssemblyName)]
    public class IdHashUtilityTests
    {
        [Theory, TestPriorityOrder(100), ClassData(typeof(ValidateIdHashData))]
        public void IdHashUtilityTests_ValidateIdHash(string idHash, bool expectedOutcome)
        {
            var result = IdHashUtility.ValidateIdHash(idHash);

            if (expectedOutcome)
            {
                Assert.True(result);
            }
            else
            {
                Assert.False(result);
            }
        }

        [Fact, TestPriorityOrder(101)]
        public void IdHashUtilityTests_NewId()
        {
            var id = IdHashUtility.NewId();

            Assert.NotNull(id);
        }

        [Theory, TestPriorityOrder(102), ClassData(typeof(ToIdHashData))]
        public void IdHashUtilityTests_ToIdHash(object value)
        {
            string id;

            switch (value)
            {
                case string s:
                    var dateTime = DateTime.Parse(s);

                    id = dateTime.ToIdHash();
                    break;
                case DateTime dt:
                    id = dt.ToIdHash();
                    break;
                case DateTimeOffset dto:
                    id = dto.ToIdHash();
                    break;
                case long l:
                    id = l.ToIdHash();
                    break;
                case ulong ul:
                    id = ul.ToIdHash();
                    break;
                default:
                    throw new ArgumentException("Testing unsupported type for IdHashUtility.ToIdHash().");
            }

            Assert.Equal("gBNEpi.Y2Qg", id);
        }

        [Theory, TestPriorityOrder(103), ClassData(typeof(ToIdHashData))]
        public void IdHashUtilityTests_AsIdHash(object value)
        {
            string id;

            switch (value)
            {
                case string s:
                    var dateTime = DateTime.Parse(s);

                    id = IdHashUtility.AsIdHash(dateTime);
                    break;
                case DateTime dt:
                    id = IdHashUtility.AsIdHash(dt);
                    break;
                case DateTimeOffset dto:
                    id = IdHashUtility.AsIdHash(dto);
                    break;
                case long l:
                    id = IdHashUtility.AsIdHash(l);
                    break;
                case ulong ul:
                    id = IdHashUtility.AsIdHash(ul);
                    break;
                default:
                    throw new ArgumentException("Testing unsupported type for IdHashUtility.AsIdHash().");
            }

            Assert.Equal("gBNEpi.Y2Qg", id);
        }
    }
}