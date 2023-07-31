using dii.storage.tests.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace dii.storage.tests.Orderer
{
    public class TestCollectionPriorityOrderer : ITestCollectionOrderer
    {
        public const string FullName = "dii.storage.tests.Orderer.TestCollectionPriorityOrderer";
        public const string AssemblyName = "dii.storage.tests";

        public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> testCollections)
        {
            if (testCollections == null)
            {
                throw new ArgumentNullException(nameof(testCollections), "Cannot use TestCollectionPriorityOrderer without at least 1 test collection.");
            }

            if (testCollections.Count() == 1)
            {
                yield return testCollections.First();
                yield break;
            }

            var sortedCollections = new SortedDictionary<int, List<ITestCollection>>();
            var comparison = new Comparison<ITestCollection>((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.DisplayName, y.DisplayName));
            var assemblyTypes = Assembly.GetExecutingAssembly().GetExportedTypes();

            foreach (var testCollection in testCollections)
            {
                var priority = GetTestCollectionPriority(assemblyTypes, testCollection.DisplayName);

                GetOrCreate(sortedCollections, priority).Add(testCollection);
            }

            foreach (var list in sortedCollections.Keys.Select(x => sortedCollections[x]))
            {
                list.Sort(comparison);

                foreach (var testCollection in list)
                {
                    yield return testCollection;
                }
            }
        }

        private static int GetTestCollectionPriority(Type[] assemblyTypes, string name)
        {
            var testCollectionType = assemblyTypes.FirstOrDefault(x => x.Name == name);

            var priorityAttribute = testCollectionType.GetCustomAttribute(typeof(TestCollectionPriorityOrderAttribute)) as TestCollectionPriorityOrderAttribute;

            return priorityAttribute.Priority;
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