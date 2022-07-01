using dii.cosmos.tests.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace dii.cosmos.tests.Orderer
{
    public class TestPriorityOrderer : ITestCaseOrderer
    {
        public const string FullName = "dii.cosmos.tests.Orderer.TestPriorityOrderer";
        public const string AssemblyName = "dii.cosmos.tests";

        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
            where TTestCase : ITestCase
        {
            if (testCases == null)
            {
                throw new ArgumentNullException(nameof(testCases), "Cannot use TestPriorityOrderer without at least 1 test case.");
            }

            if (testCases.Count() == 1)
            {
                yield return testCases.First();
                yield break;
            }

            var sortedMethods = new SortedDictionary<int, List<TTestCase>>();
            var comparison = new Comparison<TTestCase>((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.TestMethod.Method.Name, y.TestMethod.Method.Name));
            var assemblyQualifiedName = typeof(TestPriorityOrderAttribute).AssemblyQualifiedName;

            foreach (var testCase in testCases)
            {
                var priority = GetTestCasePriority(testCase.TestMethod.Method.GetCustomAttributes(assemblyQualifiedName));

                GetOrCreate(sortedMethods, priority).Add(testCase);
            }

            foreach (var list in sortedMethods.Keys.Select(x => sortedMethods[x]))
            {
                list.Sort(comparison);

                foreach (var testCase in list)
                {
                    yield return testCase;
                }
            }
        }

        private static int GetTestCasePriority(IEnumerable<IAttributeInfo> attributeInfos)
        {
            var priority = 0;

            foreach (var attr in attributeInfos)
            {
                priority = attr.GetNamedArgument<int>(nameof(TestPriorityOrderAttribute.Priority));
            }

            return priority;
        }

        private static TValue GetOrCreate<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key)
            where TValue : new()
        {
            if (dictionary.TryGetValue(key, out TValue result))
            {
                return result;
            }

            result = new TValue();
            dictionary[key] = result;

            return result;
        }
    }
}